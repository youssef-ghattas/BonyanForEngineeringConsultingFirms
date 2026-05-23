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

			// ── VOLUME-BASED CAPACITY CALCULATION ─────────────────
			// Each material has a VolumeFactorM3: how many m³ does 1 unit occupy
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
			}

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
				// Check if adding this quantity would exceed capacity
				var inv = await _context.Inventories
					.Include(i => i.MaterialInventories).ThenInclude(mi => mi.Material)
					.FirstOrDefaultAsync(i => i.InventoryID == inventoryId);

				var material = await _context.Materials.FindAsync(materialId);

				if (inv?.Capacity.HasValue == true && material != null)
				{
					decimal currentUsed = inv.MaterialInventories
						.Sum(mi => mi.QuantityAvailable * (mi.Material?.VolumeFactorM3 ?? 1.0m));
					decimal newVolume = quantityAvailable * material.VolumeFactorM3;
					decimal fillRate = ((currentUsed + newVolume) / inv.Capacity.Value) * 100;

					if (currentUsed + newVolume > inv.Capacity.Value)
					{
						TempData["ErrorMessage"] = $"لا يمكن إضافة هذه الكمية! الحجم المطلوب ({newVolume:N2} م³) يتجاوز السعة المتبقية ({(inv.Capacity.Value - currentUsed):N2} م³).";
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
		public async Task<IActionResult> UpdateStock(int inventoryId, int materialId,
													  decimal newQuantity, string notes)
		{
			if (!IsAdmin()) return Forbid();

			var entry = await _context.MaterialInventories
				.FirstOrDefaultAsync(mi => mi.InventoryID == inventoryId && mi.MaterialID == materialId);
			if (entry == null) return NotFound();

			// Volume check
			var inv = await _context.Inventories
				.Include(i => i.MaterialInventories).ThenInclude(mi => mi.Material)
				.FirstOrDefaultAsync(i => i.InventoryID == inventoryId);
			var material = await _context.Materials.FindAsync(materialId);

			if (inv?.Capacity.HasValue == true && material != null)
			{
				decimal otherVolume = inv.MaterialInventories
					.Where(mi => mi.MaterialID != materialId)
					.Sum(mi => mi.QuantityAvailable * (mi.Material?.VolumeFactorM3 ?? 1.0m));
				decimal newVolume = newQuantity * material.VolumeFactorM3;

				if (otherVolume + newVolume > inv.Capacity.Value)
				{
					TempData["ErrorMessage"] = $"لا يمكن تحديث الكمية! الحجم الكلي ({otherVolume + newVolume:N2} م³) يتجاوز سعة المخزن ({inv.Capacity.Value:N2} م³).";
					return RedirectToAction(nameof(Details), new { id = inventoryId });
				}
			}

			entry.TransactionQuantity = newQuantity - entry.QuantityAvailable;
			entry.QuantityAvailable = newQuantity;
			entry.TransactionDate = DateTime.Now;
			if (!string.IsNullOrWhiteSpace(notes)) entry.Notes = notes;

			if (inv != null) inv.LastUpdatedDate = DateTime.Now;
			await _context.SaveChangesAsync();
			TempData["SuccessMessage"] = "تم تحديث الكمية بنجاح";
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
				TempData["SuccessMessage"] = "تم إزالة المادة من المخزن";
			}
			return RedirectToAction(nameof(Details), new { id = inventoryId });
		}
	}
}