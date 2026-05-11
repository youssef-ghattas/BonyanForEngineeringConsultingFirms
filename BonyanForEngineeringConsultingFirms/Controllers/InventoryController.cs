using Bonyan.BLL.Services;
using Bonyan.DAL.Models;
using Microsoft.AspNetCore.Mvc;

namespace BonyanForEngineeringConsultingFirms.Controllers
{
	public class InventoryController : Controller
	{
		private readonly IService<Inventory> _inventoryService;

		public InventoryController(IService<Inventory> inventoryService)
		{
			_inventoryService = inventoryService;
		}

		public IActionResult Index()
		{
			var inventories = _inventoryService.GetAll();
			return View(inventories);
		}

		public IActionResult Details(int id)
		{
			var inventory = _inventoryService.GetById(id);
			if (inventory == null) return NotFound();
			return View(inventory);
		}

		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Create(Inventory inventory)
		{
			if (ModelState.IsValid)
			{
				_inventoryService.Add(inventory);
				return RedirectToAction(nameof(Index));
			}
			return View(inventory);
		}

		public IActionResult Edit(int id)
		{
			var inventory = _inventoryService.GetById(id);
			if (inventory == null) return NotFound();
			return View(inventory);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Edit(Inventory inventory)
		{
			if (ModelState.IsValid)
			{
				_inventoryService.Update(inventory);
				return RedirectToAction(nameof(Index));
			}
			return View(inventory);
		}

		public IActionResult Delete(int id)
		{
			var inventory = _inventoryService.GetById(id);
			if (inventory == null) return NotFound();
			return View(inventory);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public IActionResult DeleteConfirmed(int id)
		{
			_inventoryService.Delete(id);
			return RedirectToAction(nameof(Index));
		}
	}
}