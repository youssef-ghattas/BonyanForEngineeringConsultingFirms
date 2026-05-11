using Bonyan.BLL.Services;
using Bonyan.DAL.Models;
using Microsoft.AspNetCore.Mvc;

namespace BonyanForEngineeringConsultingFirms.Controllers
{
	public class SupplierController : Controller
	{
		private readonly IService<Supplier> _supplierService;

		public SupplierController(IService<Supplier> supplierService)
		{
			_supplierService = supplierService;
		}

		public IActionResult Index()
		{
			var suppliers = _supplierService.GetAll();
			return View(suppliers);
		}

		public IActionResult Details(int id)
		{
			var supplier = _supplierService.GetById(id);
			if (supplier == null) return NotFound();
			return View(supplier);
		}

		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Create(Supplier supplier)
		{
			if (ModelState.IsValid)
			{
				_supplierService.Add(supplier);
				return RedirectToAction(nameof(Index));
			}
			return View(supplier);
		}

		public IActionResult Edit(int id)
		{
			var supplier = _supplierService.GetById(id);
			if (supplier == null) return NotFound();
			return View(supplier);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Edit(Supplier supplier)
		{
			if (ModelState.IsValid)
			{
				_supplierService.Update(supplier);
				return RedirectToAction(nameof(Index));
			}
			return View(supplier);
		}

		public IActionResult Delete(int id)
		{
			var supplier = _supplierService.GetById(id);
			if (supplier == null) return NotFound();
			return View(supplier);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public IActionResult DeleteConfirmed(int id)
		{
			_supplierService.Delete(id);
			return RedirectToAction(nameof(Index));
		}
	}
}