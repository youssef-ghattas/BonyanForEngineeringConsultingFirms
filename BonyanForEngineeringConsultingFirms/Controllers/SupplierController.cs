// Controllers/SupplierController.cs
using Bonyan.BLL.Services;
using Bonyan.DAL.Context;
using Bonyan.DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BonyanForEngineeringConsultingFirms.Controllers
{
	public class SupplierController : Controller
	{
		private readonly IService<Supplier> _supplierService;
		private readonly BonyanDbContext _context;

		public SupplierController(IService<Supplier> supplierService, BonyanDbContext context)
		{
			_supplierService = supplierService;
			_context = context;
		}

		private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";
		private bool IsLoggedIn() => HttpContext.Session.GetString("Role") != null;

		// ── INDEX ─────────────────────────────────────────────────
		public async Task<IActionResult> Index(string search = "")
		{
			if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

			var query = _context.Suppliers
				.Include(s => s.SuppliedMaterials)
				.AsQueryable();

			if (!string.IsNullOrWhiteSpace(search))
				query = query.Where(s => s.SupplierName.Contains(search) ||
										 (s.Email != null && s.Email.Contains(search)) ||
										 (s.Phone != null && s.Phone.Contains(search)));

			ViewBag.Search = search;
			var suppliers = await query.OrderBy(s => s.SupplierName).ToListAsync();
			return View(suppliers);
		}

		// ── DETAILS ───────────────────────────────────────────────
		public async Task<IActionResult> Details(int id)
		{
			if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

			var supplier = await _context.Suppliers
				.Include(s => s.SuppliedMaterials).ThenInclude(ms => ms.Material)
				.FirstOrDefaultAsync(s => s.SupplierID == id);

			if (supplier == null) return NotFound();
			return View(supplier);
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
		public async Task<IActionResult> Create(
	Supplier supplier,
	List<string> selectedMaterialTypes,
	string othersText)
		{
			if (!IsAdmin()) return RedirectToAction("Index", "Home");

			ModelState.Remove("SuppliedMaterials");
			ModelState.Remove("SuppliedMaterialTypes");
			ModelState.Remove("Phone");
			ModelState.Remove("Email");
			ModelState.Remove("ContactPerson");
			ModelState.Remove("Address");
			ModelState.Remove("othersText");
			ModelState.Remove("selectedMaterialTypes");

			if (!ModelState.IsValid)
			{
				ViewBag.ValidationErrors = ModelState
					.Where(x => x.Value.Errors.Count > 0)
					.Select(x => $"{x.Key}: {string.Join(", ", x.Value.Errors.Select(e => e.ErrorMessage))}")
					.ToList();
				return View(supplier);
			}

			var types = new List<string>();
			if (selectedMaterialTypes != null && selectedMaterialTypes.Count > 0)
			{
				foreach (var t in selectedMaterialTypes)
				{
					if (t == "Others")
						types.Add(!string.IsNullOrWhiteSpace(othersText)
							? "Others:" + othersText.Trim()
							: "Others");
					else
						types.Add(t);
				}
			}
			supplier.SuppliedMaterialTypes = types.Count > 0
				? string.Join(",", types)
				: null;

			try
			{
				_context.Suppliers.Add(supplier);
				await _context.SaveChangesAsync();
				TempData["SuccessMessage"] = "تمت إضافة المورد بنجاح";
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("", "حدث خطأ أثناء الحفظ: " + ex.Message);
				return View(supplier);
			}
		}

		// ── EDIT GET ──────────────────────────────────────────────
		public async Task<IActionResult> Edit(int id)
		{
			if (!IsAdmin()) return RedirectToAction("Index", "Home");
			var supplier = await _context.Suppliers.FindAsync(id);
			if (supplier == null) return NotFound();
			return View(supplier);
		}

		// ── EDIT POST ─────────────────────────────────────────────
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(
	Supplier supplier,
	List<string> selectedMaterialTypes,
	string othersText)
		{
			if (!IsAdmin()) return RedirectToAction("Index", "Home");

			ModelState.Remove("SuppliedMaterials");
			ModelState.Remove("SuppliedMaterialTypes");
			ModelState.Remove("Phone");
			ModelState.Remove("Email");
			ModelState.Remove("ContactPerson");
			ModelState.Remove("Address");
			ModelState.Remove("othersText");
			ModelState.Remove("selectedMaterialTypes");

			if (!ModelState.IsValid)
			{
				ViewBag.ValidationErrors = ModelState
					.Where(x => x.Value.Errors.Count > 0)
					.Select(x => $"{x.Key}: {string.Join(", ", x.Value.Errors.Select(e => e.ErrorMessage))}")
					.ToList();
				return View(supplier);
			}

			var existing = await _context.Suppliers.FindAsync(supplier.SupplierID);
			if (existing == null) return NotFound();

			existing.SupplierName = supplier.SupplierName;
			existing.ContactPerson = supplier.ContactPerson;
			existing.Email = supplier.Email;
			existing.Phone = supplier.Phone;
			existing.Address = supplier.Address;

			var types = new List<string>();
			if (selectedMaterialTypes != null && selectedMaterialTypes.Count > 0)
			{
				foreach (var t in selectedMaterialTypes)
				{
					if (t == "Others")
						types.Add(!string.IsNullOrWhiteSpace(othersText)
							? "Others:" + othersText.Trim()
							: "Others");
					else
						types.Add(t);
				}
			}
			existing.SuppliedMaterialTypes = types.Count > 0
				? string.Join(",", types)
				: null;

			try
			{
				await _context.SaveChangesAsync();
				TempData["SuccessMessage"] = "تم تحديث بيانات المورد بنجاح";
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("", "حدث خطأ أثناء الحفظ: " + ex.Message);
				return View(supplier);
			}
		}

		// ── DELETE GET ────────────────────────────────────────────
		public async Task<IActionResult> Delete(int id)
		{
			if (!IsAdmin()) return RedirectToAction("Index", "Home");
			var supplier = await _context.Suppliers.FindAsync(id);
			if (supplier == null) return NotFound();
			return View(supplier);
		}

		// ── DELETE POST ───────────────────────────────────────────
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			if (!IsAdmin()) return RedirectToAction("Index", "Home");

			var supplier = await _context.Suppliers
				.Include(s => s.SuppliedMaterials)
				.FirstOrDefaultAsync(s => s.SupplierID == id);

			if (supplier == null) return NotFound();

			// Remove junction records first
			_context.MaterialSuppliers.RemoveRange(supplier.SuppliedMaterials);
			_context.Suppliers.Remove(supplier);
			await _context.SaveChangesAsync();

			TempData["SuccessMessage"] = "تم حذف المورد بنجاح";
			return RedirectToAction(nameof(Index));
		}
	}
}