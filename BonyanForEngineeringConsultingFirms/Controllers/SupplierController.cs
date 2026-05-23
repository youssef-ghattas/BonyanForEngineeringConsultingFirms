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

		public IActionResult Index()
		{
			if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
			var suppliers = _supplierService.GetAll();
			return View(suppliers);
		}

		public IActionResult Details(int id)
		{
			if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
			var supplier = _supplierService.GetById(id);
			if (supplier == null) return NotFound();
			return View(supplier);
		}

		public IActionResult Create()
		{
			if (!IsAdmin()) return RedirectToAction("Index", "Home");
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(
			Supplier supplier,
			List<string> selectedMaterialTypes,
			string othersText)
		{
			if (!IsAdmin()) return RedirectToAction("Index", "Home");

			// Remove navigation properties from ModelState so they don't block validation
			ModelState.Remove("SuppliedMaterials");
			ModelState.Remove("SuppliedMaterialTypes");

			if (!ModelState.IsValid)
				return View(supplier);

			// Build the SuppliedMaterialTypes comma-separated string
			var types = new List<string>();
			if (selectedMaterialTypes != null && selectedMaterialTypes.Count > 0)
			{
				foreach (var t in selectedMaterialTypes)
				{
					if (t == "Others")
					{
						if (!string.IsNullOrWhiteSpace(othersText))
							types.Add("Others:" + othersText.Trim());
						else
							types.Add("Others");
					}
					else
					{
						types.Add(t);
					}
				}
			}
			supplier.SuppliedMaterialTypes = types.Count > 0
				? string.Join(",", types)
				: null;

			_context.Suppliers.Add(supplier);
			await _context.SaveChangesAsync();

			TempData["SuccessMessage"] = "تمت إضافة المورد بنجاح";
			return RedirectToAction(nameof(Index));
		}

		public IActionResult Edit(int id)
		{
			if (!IsAdmin()) return RedirectToAction("Index", "Home");
			var supplier = _supplierService.GetById(id);
			if (supplier == null) return NotFound();
			return View(supplier);
		}

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

			if (!ModelState.IsValid)
				return View(supplier);

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
					{
						if (!string.IsNullOrWhiteSpace(othersText))
							types.Add("Others:" + othersText.Trim());
						else
							types.Add("Others");
					}
					else
					{
						types.Add(t);
					}
				}
			}
			existing.SuppliedMaterialTypes = types.Count > 0
				? string.Join(",", types)
				: null;

			await _context.SaveChangesAsync();

			TempData["SuccessMessage"] = "تم تحديث بيانات المورد بنجاح";
			return RedirectToAction(nameof(Index));
		}

		public IActionResult Delete(int id)
		{
			if (!IsAdmin()) return RedirectToAction("Index", "Home");
			var supplier = _supplierService.GetById(id);
			if (supplier == null) return NotFound();
			return View(supplier);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public IActionResult DeleteConfirmed(int id)
		{
			if (!IsAdmin()) return RedirectToAction("Index", "Home");
			_supplierService.Delete(id);
			TempData["SuccessMessage"] = "تم حذف المورد بنجاح";
			return RedirectToAction(nameof(Index));
		}
	}
}
