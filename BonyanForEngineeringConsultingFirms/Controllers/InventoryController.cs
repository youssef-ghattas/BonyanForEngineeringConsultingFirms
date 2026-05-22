using Bonyan.DAL.Context;
using Bonyan.DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BonyanForEngineeringConsultingFirms.Controllers
{
    public class InventoryController : Controller
    {
        private readonly BonyanDbContext _context;

        public InventoryController(BonyanDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";
        private bool IsLoggedIn() => HttpContext.Session.GetString("Role") != null;

        // ── INDEX ─────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var inventories = await _context.Inventories
                .Include(i => i.MaterialInventories)
                    .ThenInclude(mi => mi.Material)
                .OrderBy(i => i.InventoryName)
                .ToListAsync();

            return View(inventories);
        }

        // ── DETAILS ───────────────────────────────────────────────
        public async Task<IActionResult> Details(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var inventory = await _context.Inventories
                .Include(i => i.MaterialInventories)
                    .ThenInclude(mi => mi.Material)
                .FirstOrDefaultAsync(i => i.InventoryID == id);

            if (inventory == null) return NotFound();

            // For AddMaterial: materials not yet in this inventory
            if (IsAdmin())
            {
                var existingMaterialIds = inventory.MaterialInventories
                    .Select(mi => mi.MaterialID).ToList();
                ViewBag.AvailableMaterials = await _context.Materials
                    .Where(m => !existingMaterialIds.Contains(m.MaterialID))
                    .OrderBy(m => m.MaterialName)
                    .ToListAsync();
            }

            return View(inventory);
        }

        // ── CREATE GET ────────────────────────────────────────────
        public IActionResult Create()
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Home");
            return View();
        }

        // ── CREATE POST ───────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Inventory inventory)
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid) return View(inventory);

            inventory.LastUpdatedDate = DateTime.Now;
            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تمت إضافة المخزن بنجاح";
            return RedirectToAction(nameof(Index));
        }

        // ── EDIT GET ──────────────────────────────────────────────
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Home");

            var inventory = await _context.Inventories.FindAsync(id);
            if (inventory == null) return NotFound();
            return View(inventory);
        }

        // ── EDIT POST ─────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Inventory inventory)
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Home");
            if (!ModelState.IsValid) return View(inventory);

            var existing = await _context.Inventories.FindAsync(inventory.InventoryID);
            if (existing == null) return NotFound();

            existing.InventoryName = inventory.InventoryName;
            existing.Location = inventory.Location;
            existing.Capacity = inventory.Capacity;
            existing.LastUpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم تحديث بيانات المخزن بنجاح";
            return RedirectToAction(nameof(Index));
        }

        // ── DELETE POST ───────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Home");

            var inventory = await _context.Inventories
                .Include(i => i.MaterialInventories)
                .FirstOrDefaultAsync(i => i.InventoryID == id);

            if (inventory == null) return NotFound();

            _context.MaterialInventories.RemoveRange(inventory.MaterialInventories);
            _context.Inventories.Remove(inventory);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم حذف المخزن بنجاح";
            return RedirectToAction(nameof(Index));
        }

        // ── ADD MATERIAL TO INVENTORY ─────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMaterial(int inventoryId, int materialId,
                                                      decimal quantityAvailable,
                                                      string storageLocation, string notes)
        {
            if (!IsAdmin()) return Forbid();

            bool already = await _context.MaterialInventories
                .AnyAsync(mi => mi.InventoryID == inventoryId && mi.MaterialID == materialId);

            if (!already)
            {
                _context.MaterialInventories.Add(new MaterialInventory
                {
                    InventoryID = inventoryId,
                    MaterialID = materialId,
                    QuantityAvailable = quantityAvailable,
                    StorageLocation = storageLocation ?? "",
                    Notes = notes ?? "",
                    TransactionDate = DateTime.Now,
                    TransactionQuantity = quantityAvailable
                });

                var inv = await _context.Inventories.FindAsync(inventoryId);
                if (inv != null) inv.LastUpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تمت إضافة المادة للمخزن بنجاح";
            }
            else
            {
                TempData["ErrorMessage"] = "هذه المادة موجودة مسبقاً في هذا المخزن";
            }

            return RedirectToAction(nameof(Details), new { id = inventoryId });
        }

        // ── UPDATE STOCK ──────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStock(int inventoryId, int materialInventoryId,
                                                      decimal newQuantity, string notes)
        {
            if (!IsAdmin()) return Forbid();

            var entry = await _context.MaterialInventories.FindAsync(materialInventoryId);
            if (entry == null) return NotFound();

            entry.TransactionQuantity = newQuantity - entry.QuantityAvailable;
            entry.QuantityAvailable = newQuantity;
            entry.TransactionDate = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(notes)) entry.Notes = notes;

            var inv = await _context.Inventories.FindAsync(inventoryId);
            if (inv != null) inv.LastUpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "تم تحديث الكمية بنجاح";
            return RedirectToAction(nameof(Details), new { id = inventoryId });
        }

        // ── REMOVE MATERIAL FROM INVENTORY ────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMaterial(int inventoryId, int materialInventoryId)
        {
            if (!IsAdmin()) return Forbid();

            var entry = await _context.MaterialInventories.FindAsync(materialInventoryId);
            if (entry != null)
            {
                _context.MaterialInventories.Remove(entry);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم إزالة المادة من المخزن";
            }

            return RedirectToAction(nameof(Details), new { id = inventoryId });
        }
    }
}
