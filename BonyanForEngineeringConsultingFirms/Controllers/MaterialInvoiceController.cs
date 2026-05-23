// Controllers/MaterialInvoiceController.cs
using Bonyan.DAL.Context;
using Bonyan.DAL.Enums;
using Bonyan.DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BonyanForEngineeringConsultingFirms.Controllers
{
	public class MaterialInvoiceController : Controller
	{
		private readonly BonyanDbContext _context;

		public MaterialInvoiceController(BonyanDbContext context)
		{
			_context = context;
		}

		private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";
		private bool IsLoggedIn() => HttpContext.Session.GetString("Role") != null;

		// ── INDEX (Admin only) ────────────────────────────────────
		public async Task<IActionResult> Index(int? materialId, int? supplierId, int? status)
		{
			if (!IsAdmin()) return RedirectToAction("Login", "Account");

			var query = _context.MaterialInvoices
				.Include(mi => mi.Material)
				.Include(mi => mi.Supplier)
				.AsQueryable();

			if (materialId.HasValue)
				query = query.Where(mi => mi.MaterialID == materialId.Value);
			if (supplierId.HasValue)
				query = query.Where(mi => mi.SupplierID == supplierId.Value);
			if (status.HasValue)
				query = query.Where(mi => (int)mi.Status == status.Value);

			ViewBag.Materials = await _context.Materials.OrderBy(m => m.MaterialName).ToListAsync();
			ViewBag.Suppliers = await _context.Suppliers.OrderBy(s => s.SupplierName).ToListAsync();
			ViewBag.SelectedMaterialId = materialId;
			ViewBag.SelectedSupplierId = supplierId;
			ViewBag.SelectedStatus = status;

			// Summary totals
			var all = await query.ToListAsync();
			ViewBag.TotalInvoices = all.Count;
			ViewBag.TotalAmount = all.Sum(i => i.FinalAmount);
			ViewBag.UnpaidAmount = all.Where(i => i.Status == InvoiceStatus.Unpaid).Sum(i => i.FinalAmount);

			return View(all.OrderByDescending(i => i.InvoiceDate).ToList());
		}

		// ── DETAILS ───────────────────────────────────────────────
		public async Task<IActionResult> Details(int id)
		{
			if (!IsAdmin()) return RedirectToAction("Login", "Account");

			var invoice = await _context.MaterialInvoices
				.Include(mi => mi.Material)
				.Include(mi => mi.Supplier)
				.FirstOrDefaultAsync(mi => mi.MaterialInvoiceID == id);

			if (invoice == null) return NotFound();
			return View(invoice);
		}

		// ── CREATE GET ────────────────────────────────────────────
		public async Task<IActionResult> Create(int? materialId)
		{
			if (!IsAdmin()) return RedirectToAction("Login", "Account");

			ViewBag.Materials = await _context.Materials
				.Include(m => m.MaterialSuppliers).ThenInclude(ms => ms.Supplier)
				.OrderBy(m => m.MaterialName).ToListAsync();
			ViewBag.Suppliers = await _context.Suppliers.OrderBy(s => s.SupplierName).ToListAsync();
			ViewBag.SelectedMaterialId = materialId;

			var invoice = new MaterialInvoice { InvoiceDate = DateTime.Now };
			if (materialId.HasValue)
			{
				invoice.MaterialID = materialId.Value;
				var mat = await _context.Materials.FindAsync(materialId.Value);
				if (mat != null)
				{
					invoice.UnitPrice = mat.UnitPrice ?? 0;
					invoice.Quantity = mat.Quantity ?? 1;
				}
			}
			return View(invoice);
		}

		// ── CREATE POST ───────────────────────────────────────────
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(MaterialInvoice invoice)
		{
			if (!IsAdmin()) return RedirectToAction("Login", "Account");

			ModelState.Remove("Material");
			ModelState.Remove("Supplier");

			if (!ModelState.IsValid)
			{
				ViewBag.Materials = await _context.Materials
					.Include(m => m.MaterialSuppliers).ThenInclude(ms => ms.Supplier)
					.OrderBy(m => m.MaterialName).ToListAsync();
				ViewBag.Suppliers = await _context.Suppliers.OrderBy(s => s.SupplierName).ToListAsync();
				return View(invoice);
			}

			// Calculate amounts
			invoice.TotalAmount = invoice.Quantity * invoice.UnitPrice;
			decimal taxAmount = invoice.TotalAmount * (invoice.TaxPercent ?? 0) / 100;
			decimal discountAmount = invoice.TotalAmount * (invoice.DiscountPercent ?? 0) / 100;
			invoice.FinalAmount = invoice.TotalAmount + taxAmount - discountAmount;

			_context.MaterialInvoices.Add(invoice);
			await _context.SaveChangesAsync();

			TempData["SuccessMessage"] = "تمت إضافة فاتورة المواد بنجاح";
			return RedirectToAction(nameof(Details), new { id = invoice.MaterialInvoiceID });
		}

		// ── EDIT GET ──────────────────────────────────────────────
		public async Task<IActionResult> Edit(int id)
		{
			if (!IsAdmin()) return RedirectToAction("Login", "Account");

			var invoice = await _context.MaterialInvoices.FindAsync(id);
			if (invoice == null) return NotFound();

			ViewBag.Materials = await _context.Materials.OrderBy(m => m.MaterialName).ToListAsync();
			ViewBag.Suppliers = await _context.Suppliers.OrderBy(s => s.SupplierName).ToListAsync();
			return View(invoice);
		}

		// ── EDIT POST ─────────────────────────────────────────────
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(MaterialInvoice invoice)
		{
			if (!IsAdmin()) return RedirectToAction("Login", "Account");

			ModelState.Remove("Material");
			ModelState.Remove("Supplier");

			if (!ModelState.IsValid)
			{
				ViewBag.Materials = await _context.Materials.OrderBy(m => m.MaterialName).ToListAsync();
				ViewBag.Suppliers = await _context.Suppliers.OrderBy(s => s.SupplierName).ToListAsync();
				return View(invoice);
			}

			var existing = await _context.MaterialInvoices.FindAsync(invoice.MaterialInvoiceID);
			if (existing == null) return NotFound();

			existing.MaterialID = invoice.MaterialID;
			existing.SupplierID = invoice.SupplierID;
			existing.Quantity = invoice.Quantity;
			existing.UnitPrice = invoice.UnitPrice;
			existing.TaxPercent = invoice.TaxPercent;
			existing.DiscountPercent = invoice.DiscountPercent;
			existing.TotalAmount = invoice.Quantity * invoice.UnitPrice;
			decimal taxAmount = existing.TotalAmount * (invoice.TaxPercent ?? 0) / 100;
			decimal discountAmount = existing.TotalAmount * (invoice.DiscountPercent ?? 0) / 100;
			existing.FinalAmount = existing.TotalAmount + taxAmount - discountAmount;
			existing.InvoiceDate = invoice.InvoiceDate;
			existing.DueDate = invoice.DueDate;
			existing.Status = invoice.Status;
			existing.Notes = invoice.Notes;

			await _context.SaveChangesAsync();

			TempData["SuccessMessage"] = "تم تحديث الفاتورة بنجاح";
			return RedirectToAction(nameof(Details), new { id = invoice.MaterialInvoiceID });
		}

		// ── DELETE POST ───────────────────────────────────────────
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(int id)
		{
			if (!IsAdmin()) return RedirectToAction("Login", "Account");

			var invoice = await _context.MaterialInvoices.FindAsync(id);
			if (invoice == null) return NotFound();

			_context.MaterialInvoices.Remove(invoice);
			await _context.SaveChangesAsync();

			TempData["SuccessMessage"] = "تم حذف الفاتورة";
			return RedirectToAction(nameof(Index));
		}

		// ── GET SUPPLIERS FOR MATERIAL (AJAX) ─────────────────────
		[HttpGet]
		public async Task<IActionResult> GetSuppliersForMaterial(int materialId)
		{
			var suppliers = await _context.MaterialSuppliers
				.Where(ms => ms.MaterialID == materialId)
				.Select(ms => new { ms.SupplierID, ms.Supplier.SupplierName, ms.SupplyPrice })
				.ToListAsync();
			return Json(suppliers);
		}

		// ── GET MATERIAL INFO (AJAX) ──────────────────────────────
		[HttpGet]
		public async Task<IActionResult> GetMaterialInfo(int materialId)
		{
			var mat = await _context.Materials.FindAsync(materialId);
			if (mat == null) return NotFound();
			return Json(new { unitPrice = mat.UnitPrice ?? 0, unit = mat.Unit ?? "", quantity = mat.Quantity ?? 1 });
		}
	}
}