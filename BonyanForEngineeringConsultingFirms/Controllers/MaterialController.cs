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
		public async Task<IActionResult> Create(Material material)
		{
			if (!IsAdmin()) return RedirectToAction("Index", "Home");

			if (_context.Materials.Any(m => m.MaterialName == material.MaterialName))
				ModelState.AddModelError("MaterialName", "هذه المادة مسجلة مسبقاً");

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

			if (material.VolumeFactorM3 <= 0)
				material.VolumeFactorM3 = GetDefaultVolumeFactor(material.MaterialName);

			_context.Materials.Add(material);
			await _context.SaveChangesAsync();

			// ── AUTO-CREATE MATERIAL INVOICE ──────────────────────
			if (material.PreferredSupplierID.HasValue &&
				material.Quantity.HasValue && material.Quantity > 0 &&
				material.UnitPrice.HasValue && material.UnitPrice > 0)
			{
				var total = material.Quantity.Value * material.UnitPrice.Value;
				_context.MaterialInvoices.Add(new MaterialInvoice
				{
					MaterialID = material.MaterialID,
					SupplierID = material.PreferredSupplierID.Value,
					Quantity = material.Quantity.Value,
					UnitPrice = material.UnitPrice.Value,
					TotalAmount = total,
					TaxPercent = 0,
					DiscountPercent = 0,
					FinalAmount = total,
					InvoiceDate = DateTime.Now,
					Status = InvoiceStatus.Unpaid,
					Notes = $"فاتورة تلقائية — {material.MaterialName}"
				});
			}

			// ── AUTO-ADD TO INVENTORY ─────────────────────────────
			if (material.TargetInventoryID.HasValue &&
				material.Quantity.HasValue && material.Quantity > 0)
			{
				bool already = await _context.MaterialInventories
					.AnyAsync(mi => mi.InventoryID == material.TargetInventoryID.Value &&
									mi.MaterialID == material.MaterialID);
				if (!already)
				{
					_context.MaterialInventories.Add(new MaterialInventory
					{
						InventoryID = material.TargetInventoryID.Value,
						MaterialID = material.MaterialID,
						QuantityAvailable = material.Quantity.Value,
						TransactionQuantity = material.Quantity.Value,
						TransactionDate = DateTime.Now,
						Notes = "إضافة تلقائية عند إنشاء المادة"
					});
					var inv = await _context.Inventories.FindAsync(material.TargetInventoryID.Value);
					if (inv != null) inv.LastUpdatedDate = DateTime.Now;
				}
			}

			await _context.SaveChangesAsync();

			TempData["SuccessMessage"] = "تمت إضافة المادة وإنشاء الفاتورة بنجاح";
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

			TempData["SuccessMessage"] = "تم تحديث بيانات المادة بنجاح";
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
				TempData["ErrorMessage"] = "لا يمكن حذف المادة لأنها مرتبطة بطلبات مهام.";
				return RedirectToAction(nameof(Index));
			}

			_context.MaterialSuppliers.RemoveRange(material.MaterialSuppliers);
			_context.MaterialInventories.RemoveRange(material.MaterialInventories);
			_context.MaterialInvoices.RemoveRange(material.MaterialInvoices);
			_context.Materials.Remove(material);
			await _context.SaveChangesAsync();

			TempData["SuccessMessage"] = "تم حذف المادة بنجاح";
			return RedirectToAction(nameof(Index));
		}

		// ── GET SUPPLIERS FOR MATERIAL TYPE (AJAX) ────────────────
		[HttpGet]
		public async Task<IActionResult> GetSuppliersForType(string materialName)
		{
			var typeKey = GetMaterialTypeKey(materialName ?? "");

			var allSuppliers = await _context.Suppliers
				.Where(s => s.SuppliedMaterialTypes != null)
				.ToListAsync();

			var matching = allSuppliers
				.Where(s => s.SuppliedMaterialTypes!
					.Split(',', StringSplitOptions.RemoveEmptyEntries)
					.Any(t => t == typeKey ||
							  (typeKey == "Others" && t.StartsWith("Others"))))
				.Select(s => new { s.SupplierID, s.SupplierName, s.Phone })
				.ToList();

			return Json(matching);
		}
	}
}