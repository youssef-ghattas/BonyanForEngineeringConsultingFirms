// Controllers/MaterialController.cs
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

		private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";
		private bool IsLoggedIn() => HttpContext.Session.GetString("Role") != null;

		private static string GetMaterialTypeKey(string materialName)
		{
			if (string.IsNullOrWhiteSpace(materialName)) return "Others";
			var n = materialName.ToLower();
			if (n == "steel") return "Steel";
			if (n == "sand") return "Sand";
			if (n == "cement") return "Cement";
			if (n == "bricks") return "Bricks";
			return "Others";
		}

		private static decimal GetDefaultVolumeFactor(string materialName)
		{
			return GetMaterialTypeKey(materialName) switch
			{
				"Sand" => 1.0m,
				"Cement" => 0.85m,
				"Steel" => 0.55m,
				"Bricks" => 1.25m,
				_ => 1.0m
			};
		}

		// ── INDEX ─────────────────────────────────────────────────
		public async Task<IActionResult> Index(string search = "")
		{
			if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

			var query = _context.Materials
				.Include(m => m.PreferredSupplier)
				.Include(m => m.TargetInventory)
				.Include(m => m.MaterialInventories).ThenInclude(mi => mi.Inventory)
				.AsQueryable();

			if (!string.IsNullOrWhiteSpace(search))
				query = query.Where(m => m.MaterialName.Contains(search) ||
										 (m.Description != null && m.Description.Contains(search)));

			ViewBag.Search = search;
			return View(await query.OrderBy(m => m.MaterialName).ToListAsync());
		}

		// ── DETAILS ───────────────────────────────────────────────
		public async Task<IActionResult> Details(int id)
		{
			if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

			var material = await _context.Materials
				.Include(m => m.MaterialSuppliers).ThenInclude(ms => ms.Supplier)
				.Include(m => m.MaterialInventories).ThenInclude(mi => mi.Inventory)
				.Include(m => m.MaterialTasks)
				.Include(m => m.PreferredSupplier)
				.Include(m => m.TargetInventory)
				.FirstOrDefaultAsync(m => m.MaterialID == id);

			if (material == null) return NotFound();
			return View(material);
		}

		// ── CREATE GET ────────────────────────────────────────────
		public async Task<IActionResult> Create()
		{
			if (!IsAdmin()) return RedirectToAction("Index", "Home");

			ViewBag.AllSuppliers = await _context.Suppliers
				.OrderBy(s => s.SupplierName).ToListAsync();
			ViewBag.Inventories = await _context.Inventories
				.OrderBy(i => i.InventoryName).ToListAsync();
			return View();
		}

		// ── CREATE POST ───────────────────────────────────────────
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(
	Material material,
	string inventoryDecision,
	int? secondaryInventoryId,
	decimal? splitQtyPrimary,
	decimal? splitQtySecondary)
		{
			if (!IsAdmin()) return RedirectToAction("Index", "Home");

			ModelState.Remove("PreferredSupplier");
			ModelState.Remove("TargetInventory");
			ModelState.Remove("MaterialSuppliers");
			ModelState.Remove("MaterialTasks");
			ModelState.Remove("MaterialInventories");
			ModelState.Remove("MaterialInvoices");
			ModelState.Remove("Description");

			if (!ModelState.IsValid)
			{
				ViewBag.AllSuppliers = await _context.Suppliers.OrderBy(s => s.SupplierName).ToListAsync();
				ViewBag.Inventories = await _context.Inventories.OrderBy(i => i.InventoryName).ToListAsync();
				return View(material);
			}

			if (material.VolumeFactorM3 <= 0)
				material.VolumeFactorM3 = GetDefaultVolumeFactor(material.MaterialName);

			_context.Materials.Add(material);
			await _context.SaveChangesAsync();

			// ── INVENTORY STORAGE ─────────────────────────────────────
			if (material.TargetInventoryID.HasValue &&
				material.Quantity.HasValue && material.Quantity > 0)
			{
				decimal totalQty = material.Quantity.Value;
				decimal volFactor = material.VolumeFactorM3;

				if (inventoryDecision == "split" &&
					secondaryInventoryId.HasValue &&
					splitQtyPrimary.HasValue && splitQtySecondary.HasValue)
				{
					// Part in primary inventory
					if (splitQtyPrimary.Value > 0)
					{
						var primaryInv = await _context.Inventories
							.FindAsync(material.TargetInventoryID.Value);
						_context.MaterialInventories.Add(new MaterialInventory
						{
							InventoryID = material.TargetInventoryID.Value,
							MaterialID = material.MaterialID,
							QuantityAvailable = splitQtyPrimary.Value,
							TransactionQuantity = splitQtyPrimary.Value,
							TransactionDate = DateTime.Now,
							StorageLocation = "غير محدد",
							Notes = "تخزين جزئي - القسم الأول"
						});
						if (primaryInv != null) primaryInv.LastUpdatedDate = DateTime.Now;
					}

					// Remainder in secondary inventory
					if (splitQtySecondary.Value > 0)
					{
						bool alreadyThere = await _context.MaterialInventories
							.AnyAsync(mi => mi.InventoryID == secondaryInventoryId.Value
										 && mi.MaterialID == material.MaterialID);
						if (!alreadyThere)
						{
							var secondaryInv = await _context.Inventories
								.FindAsync(secondaryInventoryId.Value);
							_context.MaterialInventories.Add(new MaterialInventory
							{
								InventoryID = secondaryInventoryId.Value,
								MaterialID = material.MaterialID,
								QuantityAvailable = splitQtySecondary.Value,
								TransactionQuantity = splitQtySecondary.Value,
								TransactionDate = DateTime.Now,
								StorageLocation = "غير محدد",
								Notes = "تخزين جزئي - القسم الثاني"
							});
							if (secondaryInv != null) secondaryInv.LastUpdatedDate = DateTime.Now;
						}
					}
					await _context.SaveChangesAsync();
				}
				else if (inventoryDecision == "move" && secondaryInventoryId.HasValue)
				{
					// Full quantity in secondary inventory only
					bool alreadyThere = await _context.MaterialInventories
						.AnyAsync(mi => mi.InventoryID == secondaryInventoryId.Value
									 && mi.MaterialID == material.MaterialID);
					if (!alreadyThere)
					{
						var secondaryInv = await _context.Inventories
							.FindAsync(secondaryInventoryId.Value);
						if (secondaryInv != null)
						{
							_context.MaterialInventories.Add(new MaterialInventory
							{
								InventoryID = secondaryInventoryId.Value,
								MaterialID = material.MaterialID,
								QuantityAvailable = totalQty,
								TransactionQuantity = totalQty,
								TransactionDate = DateTime.Now,
								StorageLocation = "غير محدد",
								Notes = "تم التخزين في مخزن بديل لعدم توفر مساحة"
							});
							secondaryInv.LastUpdatedDate = DateTime.Now;
							await _context.SaveChangesAsync();
						}
					}
				}
				else
				{
					// "ok" — normal: store everything in the selected inventory
					var targetInv = await _context.Inventories
						.Include(i => i.MaterialInventories).ThenInclude(mi => mi.Material)
						.FirstOrDefaultAsync(i => i.InventoryID == material.TargetInventoryID.Value);

					bool already = targetInv?.MaterialInventories
						.Any(mi => mi.MaterialID == material.MaterialID) ?? false;

					if (!already && targetInv != null)
					{
						_context.MaterialInventories.Add(new MaterialInventory
						{
							InventoryID = material.TargetInventoryID.Value,
							MaterialID = material.MaterialID,
							QuantityAvailable = totalQty,
							TransactionQuantity = totalQty,
							TransactionDate = DateTime.Now,
							StorageLocation = "غير محدد",
							Notes = "إضافة تلقائية عند إنشاء المادة"
						});
						targetInv.LastUpdatedDate = DateTime.Now;
						await _context.SaveChangesAsync();
					}
				}
			}

			// ── REDIRECT TO MATERIAL INVOICE ──────────────────────────
			if (material.PreferredSupplierID.HasValue &&
				material.Quantity.HasValue && material.Quantity > 0 &&
				material.UnitPrice.HasValue && material.UnitPrice > 0)
			{
				return RedirectToAction("Create", "MaterialInvoice", new
				{
					materialId = material.MaterialID,
					supplierId = material.PreferredSupplierID.Value,
					quantity = material.Quantity.Value,
					unitPrice = material.UnitPrice.Value
				});
			}

			TempData["SuccessMessageKey"] = "msg_material_created";
			return RedirectToAction(nameof(Index));
		}
		// ── EDIT GET ──────────────────────────────────────────────
		public async Task<IActionResult> Edit(int id)
		{
			if (!IsAdmin()) return RedirectToAction("Index", "Home");
			var material = await _context.Materials.FindAsync(id);
			if (material == null) return NotFound();

			ViewBag.AllSuppliers = await _context.Suppliers.OrderBy(s => s.SupplierName).ToListAsync();
			ViewBag.Inventories = await _context.Inventories.OrderBy(i => i.InventoryName).ToListAsync();
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
				ModelState.AddModelError("MaterialName", "هذا الاسم مستخدم لمادة أخرى");

			ModelState.Remove("PreferredSupplier");
			ModelState.Remove("TargetInventory");
			ModelState.Remove("MaterialSuppliers");
			ModelState.Remove("MaterialTasks");
			ModelState.Remove("MaterialInventories");
			ModelState.Remove("MaterialInvoices");

			if (!ModelState.IsValid)
			{
				ViewBag.AllSuppliers = await _context.Suppliers.OrderBy(s => s.SupplierName).ToListAsync();
				ViewBag.Inventories = await _context.Inventories.OrderBy(i => i.InventoryName).ToListAsync();
				return View(material);
			}

			var existing = await _context.Materials.FindAsync(material.MaterialID);
			if (existing == null) return NotFound();

			existing.MaterialName = material.MaterialName;
			existing.Unit = material.Unit;
			existing.UnitPrice = material.UnitPrice;
			existing.Quantity = material.Quantity;
			existing.Description = material.Description;
			existing.PreferredSupplierID = material.PreferredSupplierID;
			existing.TargetInventoryID = material.TargetInventoryID;
			existing.VolumeFactorM3 = material.VolumeFactorM3 > 0
				? material.VolumeFactorM3
				: GetDefaultVolumeFactor(material.MaterialName);

			await _context.SaveChangesAsync();

			TempData["SuccessMessageKey"] = "msg_material_updated";
			return RedirectToAction(nameof(Index));
		}

		// ── DELETE POST ───────────────────────────────────────────
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			if (!IsAdmin()) return RedirectToAction("Index", "Home");

			var material = await _context.Materials
				.Include(m => m.MaterialSuppliers)
				.Include(m => m.MaterialInventories)
				.Include(m => m.MaterialTasks)
				.Include(m => m.MaterialInvoices)
				.FirstOrDefaultAsync(m => m.MaterialID == id);

			if (material == null) return NotFound();

			if (material.MaterialTasks.Any())
			{
				TempData["ErrorMessageKey"] = "err_material_delete_linked";
				return RedirectToAction(nameof(Index));
			}

			_context.MaterialSuppliers.RemoveRange(material.MaterialSuppliers);
			_context.MaterialInventories.RemoveRange(material.MaterialInventories);
			_context.MaterialInvoices.RemoveRange(material.MaterialInvoices);
			_context.Materials.Remove(material);
			await _context.SaveChangesAsync();

			TempData["SuccessMessageKey"] = "msg_material_deleted";
			return RedirectToAction(nameof(Index));
		}

		// ── GET SUPPLIERS FOR MATERIAL TYPE (AJAX) ────────────────
		[HttpGet]
		public async Task<IActionResult> GetSuppliersForType(string materialName)
		{
			var typeKey = GetMaterialTypeKey(materialName ?? "");

			// Load ALL suppliers — show them all but sort matching ones first
			var allSuppliers = await _context.Suppliers.ToListAsync();

			var result = allSuppliers
				.Select(s =>
				{
					bool matches = false;
					if (!string.IsNullOrWhiteSpace(s.SuppliedMaterialTypes))
					{
						var parts = s.SuppliedMaterialTypes
							.Split(',', StringSplitOptions.RemoveEmptyEntries);
						matches = parts.Any(t =>
							t == typeKey ||
							(typeKey == "Others" && t.StartsWith("Others")));
					}
					return new
					{
						s.SupplierID,
						s.SupplierName,
						s.Phone,
						IsMatch = matches
					};
				})
				.OrderByDescending(s => s.IsMatch)
				.ThenBy(s => s.SupplierName)
				.Select(s => new { s.SupplierID, s.SupplierName, s.Phone })
				.ToList();

			return Json(result);
		}
		// ── CHECK INVENTORY CAPACITY (AJAX) ──────────────────────────
		[HttpGet]
		public async Task<IActionResult> CheckInventoryCapacity(
			int inventoryId, decimal quantity, decimal volumeFactor)
		{
			var inv = await _context.Inventories
				.Include(i => i.MaterialInventories).ThenInclude(mi => mi.Material)
				.FirstOrDefaultAsync(i => i.InventoryID == inventoryId);

			if (inv == null)
				return Json(new { status = "error", message = "المخزن غير موجود" });

			// No capacity defined → unlimited, always OK
			if (!inv.Capacity.HasValue || inv.Capacity.Value <= 0)
				return Json(new
				{
					status = "ok",
					availableM3 = (decimal?)null,
					neededM3 = quantity * volumeFactor
				});

			decimal usedM3 = inv.MaterialInventories
				.Sum(mi => mi.QuantityAvailable * (mi.Material?.VolumeFactorM3 ?? 1.0m));
			decimal totalCap = inv.Capacity.Value;
			decimal availableM3 = totalCap - usedM3;
			decimal neededM3 = quantity * volumeFactor;

			if (neededM3 <= availableM3)
				return Json(new { status = "ok", availableM3, neededM3 });

			if (availableM3 <= 0)
				return Json(new
				{
					status = "full",
					availableM3 = 0m,
					neededM3,
					inventoryName = inv.InventoryName
				});

			// Partial: has some space but not enough
			decimal availableQty = volumeFactor > 0 ? availableM3 / volumeFactor : 0;
			decimal remainingQty = quantity - availableQty;

			return Json(new
			{
				status = "partial",
				availableM3,
				neededM3,
				availableQty = Math.Round(availableQty, 3),
				remainingQty = Math.Round(remainingQty, 3),
				inventoryName = inv.InventoryName
			});
		}

		// ── GET ALL INVENTORIES WITH CAPACITY INFO (AJAX) ─────────────
		[HttpGet]
		public async Task<IActionResult> GetInventoriesWithCapacity(
			decimal volumeFactor, decimal quantity)
		{
			var inventories = await _context.Inventories
				.Include(i => i.MaterialInventories).ThenInclude(mi => mi.Material)
				.ToListAsync();

			var result = inventories.Select(inv =>
			{
				decimal usedM3 = inv.MaterialInventories
					.Sum(mi => mi.QuantityAvailable * (mi.Material?.VolumeFactorM3 ?? 1.0m));
				decimal? availableM3 = inv.Capacity.HasValue && inv.Capacity.Value > 0
					? inv.Capacity.Value - usedM3
					: (decimal?)null;
				decimal neededM3 = quantity * volumeFactor;
				bool canFitAll = availableM3 == null || availableM3.Value >= neededM3;

				return new
				{
					inv.InventoryID,
					inv.InventoryName,
					AvailableM3 = availableM3,
					CanFitAll = canFitAll
				};
			})
			.OrderByDescending(i => i.CanFitAll)
			.ThenBy(i => i.InventoryName)
			.ToList();

			return Json(result);
		}
	}
}