using Bonyan.DAL.Context;
using Bonyan.DAL.Enums;
using Bonyan.DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BonyanForEngineeringConsultingFirms.Controllers
{
    public class MaterialController : Controller
    {
        private readonly BonyanDbContext _context;

        public MaterialController(BonyanDbContext context)
        {
            _context = context;
        }

        // ── Auth Helper ───────────────────────────────────────────
        private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";
        private bool IsLoggedIn() => HttpContext.Session.GetString("Role") != null;

        // ── INDEX ─────────────────────────────────────────────────
        public async Task<IActionResult> Index(string search = "")
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var query = _context.Materials
                .Include(m => m.MaterialSuppliers)
                    .ThenInclude(ms => ms.Supplier)
                .Include(m => m.MaterialInventories)
                    .ThenInclude(mi => mi.Inventory)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(m => m.MaterialName.Contains(search) ||
                                          m.Description.Contains(search));

            ViewBag.Search = search;
            return View(await query.OrderBy(m => m.MaterialName).ToListAsync());
        }

        // ── DETAILS ───────────────────────────────────────────────
        public async Task<IActionResult> Details(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var material = await _context.Materials
                .Include(m => m.MaterialSuppliers)
                    .ThenInclude(ms => ms.Supplier)
                .Include(m => m.MaterialInventories)
                    .ThenInclude(mi => mi.Inventory)
                .Include(m => m.MaterialTasks)
                    .ThenInclude(mt => mt.Task)
                        .ThenInclude(t => t.Project)
                .FirstOrDefaultAsync(m => m.MaterialID == id);

            if (material == null) return NotFound();

            // All suppliers not yet linked to this material (for Add Supplier form)
            if (IsAdmin())
            {
                var linkedIds = material.MaterialSuppliers.Select(ms => ms.SupplierID).ToList();
                ViewBag.AvailableSuppliers = await _context.Suppliers
                    .Where(s => !linkedIds.Contains(s.SupplierID))
                    .OrderBy(s => s.SupplierName)
                    .ToListAsync();
            }

            return View(material);
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
        public async Task<IActionResult> Create(Material material)
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Home");

            if (_context.Materials.Any(m => m.MaterialName == material.MaterialName))
            {
                ModelState.AddModelError("MaterialName", "هذه المادة مسجلة مسبقاً");
                return View(material);
            }

            if (!ModelState.IsValid) return View(material);

            _context.Materials.Add(material);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تمت إضافة المادة بنجاح";
            return RedirectToAction(nameof(Index));
        }

        // ── EDIT GET ──────────────────────────────────────────────
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Home");

            var material = await _context.Materials.FindAsync(id);
            if (material == null) return NotFound();
            return View(material);
        }

        // ── EDIT POST ─────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Material material)
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Home");

            if (_context.Materials.Any(m => m.MaterialName == material.MaterialName &&
                                            m.MaterialID != material.MaterialID))
            {
                ModelState.AddModelError("MaterialName", "هذا الاسم مستخدم لمادة أخرى");
                return View(material);
            }

            if (!ModelState.IsValid) return View(material);

            var existing = await _context.Materials.FindAsync(material.MaterialID);
            if (existing == null) return NotFound();

            existing.MaterialName = material.MaterialName;
            existing.Unit = material.Unit;
            existing.UnitPrice = material.UnitPrice;
            existing.Description = material.Description;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم تحديث بيانات المادة بنجاح";
            return RedirectToAction(nameof(Index));
        }

        // ── DELETE POST ───────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Home");

            var material = await _context.Materials
                .Include(m => m.MaterialSuppliers)
                .Include(m => m.MaterialInventories)
                .Include(m => m.MaterialTasks)
                .FirstOrDefaultAsync(m => m.MaterialID == id);

            if (material == null) return NotFound();

            if (material.MaterialTasks.Any())
            {
                TempData["ErrorMessage"] = "لا يمكن حذف المادة لأنها مرتبطة بطلبات مهام.";
                return RedirectToAction(nameof(Index));
            }

            _context.MaterialSuppliers.RemoveRange(material.MaterialSuppliers);
            _context.MaterialInventories.RemoveRange(material.MaterialInventories);
            _context.Materials.Remove(material);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم حذف المادة بنجاح";
            return RedirectToAction(nameof(Index));
        }

        // ── ADD SUPPLIER LINK ─────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSupplier(int materialId, int supplierId,
                                                      decimal? supplyPrice, DateTime? lastSupplyDate)
        {
            if (!IsAdmin()) return Forbid();

            bool already = await _context.MaterialSuppliers
                .AnyAsync(ms => ms.MaterialID == materialId && ms.SupplierID == supplierId);
            if (!already)
            {
                _context.MaterialSuppliers.Add(new MaterialSupplier
                {
                    MaterialID = materialId,
                    SupplierID = supplierId,
                    SupplyPrice = supplyPrice,
                    LastSupplyDate = lastSupplyDate
                });
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تمت إضافة المورد للمادة بنجاح";
            }

            return RedirectToAction(nameof(Details), new { id = materialId });
        }

        // ── REMOVE SUPPLIER LINK ──────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveSupplier(int materialId, int supplierId)
        {
            if (!IsAdmin()) return Forbid();

            var link = await _context.MaterialSuppliers
                .FirstOrDefaultAsync(ms => ms.MaterialID == materialId && ms.SupplierID == supplierId);
            if (link != null)
            {
                _context.MaterialSuppliers.Remove(link);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم إزالة المورد من المادة";
            }

            return RedirectToAction(nameof(Details), new { id = materialId });
        }
    }
}
