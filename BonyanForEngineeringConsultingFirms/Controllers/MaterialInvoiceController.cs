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

			return View(all.OrderByDescending(i => i.InvoiceDate).ThenByDescending(i => i.MaterialInvoiceID).ToList());
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
		public async Task<IActionResult> Create(
			int? materialId,
			int? supplierId,
			decimal? quantity,
			decimal? unitPrice)
		{
			if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

			ViewBag.Materials = await _context.Materials.OrderBy(m => m.MaterialName).ToListAsync();
			ViewBag.Suppliers = await _context.Suppliers.OrderBy(s => s.SupplierName).ToListAsync();

			// Tell the view if we came from Material Create (so fields are locked)
			ViewBag.FromMaterial = materialId.HasValue;
			ViewBag.LockedMaterialId = materialId;
			ViewBag.LockedSupplierId = supplierId;
			ViewBag.LockedQuantity = quantity;
			ViewBag.LockedUnitPrice = unitPrice;

			var invoice = new MaterialInvoice
			{
				InvoiceDate = DateTime.Today,
				Status = InvoiceStatus.Unpaid
			};

			if (materialId.HasValue) invoice.MaterialID = materialId.Value;
			if (supplierId.HasValue) invoice.SupplierID = supplierId.Value;
			if (quantity.HasValue) invoice.Quantity = quantity.Value;
			if (unitPrice.HasValue) invoice.UnitPrice = unitPrice.Value;

			if (quantity.HasValue && unitPrice.HasValue)
			{
				invoice.TotalAmount = quantity.Value * unitPrice.Value;
				invoice.FinalAmount = invoice.TotalAmount;
			}

			return View(invoice);
		}

		// ── CREATE POST ───────────────────────────────────────────
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(MaterialInvoice invoice)
		{
			ModelState.Remove("Material");
			ModelState.Remove("Supplier");
			ModelState.Remove("Notes");

			if (!ModelState.IsValid)
			{
				ViewBag.Materials = await _context.Materials.OrderBy(m => m.MaterialName).ToListAsync();
				ViewBag.Suppliers = await _context.Suppliers.OrderBy(s => s.SupplierName).ToListAsync();
				ViewBag.FromMaterial = false;
				return View(invoice);
			}

			// Calculate totals
			invoice.TotalAmount = invoice.Quantity * invoice.UnitPrice;
			var taxAmt = invoice.TotalAmount * (invoice.TaxPercent ?? 0) / 100;
			var discAmt = invoice.TotalAmount * (invoice.DiscountPercent ?? 0) / 100;
			invoice.FinalAmount = invoice.TotalAmount + taxAmt - discAmt;

			// ── PARTIAL PAYMENT LOGIC ─────────────────────────────
			if (invoice.Status == InvoiceStatus.PartiallyPaid)
			{
				decimal paid = invoice.AmountPaid ?? 0;
				if (paid < 0) paid = 0;
				if (paid > invoice.FinalAmount) paid = invoice.FinalAmount;
				invoice.AmountPaid = paid;
				invoice.RemainingAmount = invoice.FinalAmount - paid;
			}
			else if (invoice.Status == InvoiceStatus.Paid)
			{
				invoice.AmountPaid = invoice.FinalAmount;
				invoice.RemainingAmount = 0;
			}
			else // Unpaid
			{
				invoice.AmountPaid = 0;
				invoice.RemainingAmount = invoice.FinalAmount;
			}

			_context.MaterialInvoices.Add(invoice);
			await _context.SaveChangesAsync();

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

			TempData["SuccessMessageKey"] = "msg_material_invoice_updated";
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

			TempData["SuccessMessageKey"] = "msg_material_invoice_deleted";
			return RedirectToAction(nameof(Index));
		}

		// ── CANCEL INVOICE & DELETE MATERIAL ─────────────────────────────────
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CancelAndDeleteMaterial(int materialId)
		{
			if (!IsAdmin()) return RedirectToAction("Login", "Account");

			var material = await _context.Materials
				.Include(m => m.MaterialInvoices)
				.Include(m => m.MaterialInventories)
				.Include(m => m.MaterialSuppliers)
				.FirstOrDefaultAsync(m => m.MaterialID == materialId);

			if (material == null) return NotFound();

			// Remove related records first
			_context.MaterialInvoices.RemoveRange(material.MaterialInvoices);
			_context.MaterialInventories.RemoveRange(material.MaterialInventories);
			_context.MaterialSuppliers.RemoveRange(material.MaterialSuppliers);
			_context.Materials.Remove(material);

			await _context.SaveChangesAsync();

			TempData["SuccessMessageKey"] = "msg_material_invoice_cancelled";
			return RedirectToAction("Index", "Material");
		}

		// ── GET SUPPLIERS FOR MATERIAL (AJAX) ─────────────────────
		[HttpGet]
		public async Task<IActionResult> GetSuppliersForMaterial(int materialId)
		{
			var suppliers = await _context.MaterialSuppliers
				.Where(ms => ms.MaterialID == materialId)
				.Include(ms => ms.Supplier)
				.Select(ms => new { ms.Supplier.SupplierID, ms.Supplier.SupplierName })
				.OrderBy(s => s.SupplierName)
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