// Controllers/InventoryController.cs
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
				.Include(i => i.MaterialInventories).ThenInclude(mi => mi.Material)
				.OrderBy(i => i.InventoryName)
				.ToListAsync();

			return View(inventories);
		}

		// ── DETAILS ───────────────────────────────────────────────
		public async Task<IActionResult> Details(int id)
		{
			if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

			var inventory = await _context.Inventories
				.Include(i => i.MaterialInventories).ThenInclude(mi => mi.Material)
				.FirstOrDefaultAsync(i => i.InventoryID == id);

			if (inventory == null) return NotFound();

			if (inventory.Capacity.HasValue && inventory.Capacity > 0)
			{
				decimal usedVolume = inventory.MaterialInventories
					.Sum(mi => mi.QuantityAvailable * (mi.Material?.VolumeFactorM3 ?? 1.0m));

				decimal remaining = inventory.Capacity.Value - usedVolume;
				decimal fillRate = Math.Round((usedVolume / inventory.Capacity.Value) * 100, 1);

				ViewBag.UsedVolumeM3 = Math.Round(usedVolume, 2);
				ViewBag.RemainingCapacityM3 = Math.Round(remaining, 2);
				ViewBag.FillRatePercent = fillRate;
				ViewBag.ShowWarning = fillRate >= 85;
				// ── NEW: flag when inventory is at or beyond 100% ──
				ViewBag.IsFull = remaining <= 0;
			}
			else
			{
				ViewBag.IsFull = false;
			}

			if (IsAdmin())
			{
				var existingMaterialIds = inventory.MaterialInventories
					.Select(mi => mi.MaterialID).ToList();
				ViewBag.AvailableMaterials = await _context.Materials
					.Where(m => !existingMaterialIds.Contains(m.MaterialID))
					.OrderBy(m => m.MaterialName)
					.ToListAsync();

				// ── NEW: list other inventories that still have space ──
				ViewBag.OtherInventories = await _context.Inventories
					.Include(i => i.MaterialInventories).ThenInclude(mi => mi.Material)
					.Where(i => i.InventoryID != id)
					.OrderBy(i => i.InventoryName)
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

			TempData["SuccessMessageKey"] = "msg_inventory_created";
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

			TempData["SuccessMessageKey"] = "msg_inventory_updated";
			return RedirectToAction(nameof(Index));
		}

		// ── DELETE GET ────────────────────────────────────────────
		[HttpGet]
		public async Task<IActionResult> Delete(int id)
		{
			if (!IsAdmin()) return RedirectToAction("Index", "Home");

			var inventory = await _context.Inventories.FindAsync(id);
			if (inventory == null) return NotFound();
			return View(inventory);
		}

		// ── DELETE POST ───────────────────────────────────────────
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			if (!IsAdmin()) return RedirectToAction("Index", "Home");

			var inventory = await _context.Inventories
				.Include(i => i.MaterialInventories)
				.FirstOrDefaultAsync(i => i.InventoryID == id);

			if (inventory == null) return NotFound();

			_context.MaterialInventories.RemoveRange(inventory.MaterialInventories);
			_context.Inventories.Remove(inventory);
			await _context.SaveChangesAsync();

			TempData["SuccessMessageKey"] = "msg_inventory_deleted";
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

			// Always load the inventory first
			var inv = await _context.Inventories
				.Include(i => i.MaterialInventories).ThenInclude(mi => mi.Material)
				.FirstOrDefaultAsync(i => i.InventoryID == inventoryId);

			if (inv == null) return NotFound();

			bool already = inv.MaterialInventories.Any(mi => mi.MaterialID == materialId);

			if (already)
			{
				TempData["ErrorMessageKey"] = "err_material_exists_warehouse";
				return RedirectToAction(nameof(Details), new { id = inventoryId });
			}

			var material = await _context.Materials.FindAsync(materialId);
			if (material == null) return NotFound();

			// ── CAPACITY CHECK (always enforced when capacity is set) ──
			if (inv.Capacity.HasValue && inv.Capacity.Value > 0)
			{
				decimal currentUsed = inv.MaterialInventories
					.Sum(mi => mi.QuantityAvailable * (mi.Material?.VolumeFactorM3 ?? 1.0m));
				decimal newVolume = quantityAvailable * material.VolumeFactorM3;
				decimal remaining = inv.Capacity.Value - currentUsed;

				// ── FULL: no space at all ──
				if (remaining <= 0)
				{
					TempData["ErrorMessageKey"] = "err_warehouse_full";
					TempData["ShowInventoryRedirect"] = true;
					return RedirectToAction(nameof(Details), new { id = inventoryId });
				}

				// ── EXCEEDS REMAINING SPACE ──
				if (currentUsed + newVolume > inv.Capacity.Value)
				{
					TempData["ErrorMessageKey"] = "err_warehouse_capacity";
					return RedirectToAction(nameof(Details), new { id = inventoryId });
				}
			}

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

			inv.LastUpdatedDate = DateTime.Now;
			await _context.SaveChangesAsync();
			TempData["SuccessMessageKey"] = "msg_material_added_warehouse";

			return RedirectToAction(nameof(Details), new { id = inventoryId });
		}

		// ── UPDATE STOCK ──────────────────────────────────────────
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdateStock(int inventoryId, int materialId,
													  decimal newQuantity, string notes)
		{
			if (!IsAdmin()) return Forbid();

			var entry = await _context.MaterialInventories
				.FirstOrDefaultAsync(mi => mi.InventoryID == inventoryId && mi.MaterialID == materialId);
			if (entry == null) return NotFound();

			var inv = await _context.Inventories
				.Include(i => i.MaterialInventories).ThenInclude(mi => mi.Material)
				.FirstOrDefaultAsync(i => i.InventoryID == inventoryId);
			var material = await _context.Materials.FindAsync(materialId);

			if (inv?.Capacity.HasValue == true && inv.Capacity.Value > 0 && material != null)
			{
				decimal otherVolume = inv.MaterialInventories
					.Where(mi => mi.MaterialID != materialId)
					.Sum(mi => mi.QuantityAvailable * (mi.Material?.VolumeFactorM3 ?? 1.0m));
				decimal newVolume = newQuantity * material.VolumeFactorM3;
				decimal maxAllowed = inv.Capacity.Value - otherVolume;

				if (otherVolume + newVolume > inv.Capacity.Value)
				{
					TempData["ErrorMessageKey"] = "err_warehouse_capacity";
					return RedirectToAction(nameof(Details), new { id = inventoryId });
				}
			}

			entry.TransactionQuantity = newQuantity - entry.QuantityAvailable;
			entry.QuantityAvailable = newQuantity;
			entry.TransactionDate = DateTime.Now;
			if (!string.IsNullOrWhiteSpace(notes)) entry.Notes = notes;

			if (inv != null) inv.LastUpdatedDate = DateTime.Now;
			await _context.SaveChangesAsync();
			TempData["SuccessMessageKey"] = "msg_quantity_updated";
			return RedirectToAction(nameof(Details), new { id = inventoryId });
		}

		// ── REMOVE MATERIAL FROM INVENTORY ────────────────────────
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> RemoveMaterial(int inventoryId, int materialId)
		{
			if (!IsAdmin()) return Forbid();

			var entry = await _context.MaterialInventories
				.FirstOrDefaultAsync(mi => mi.InventoryID == inventoryId && mi.MaterialID == materialId);
			if (entry != null)
			{
				_context.MaterialInventories.Remove(entry);
				await _context.SaveChangesAsync();
				TempData["SuccessMessageKey"] = "msg_material_removed_warehouse";
			}
			return RedirectToAction(nameof(Details), new { id = inventoryId });
		}
	}
}