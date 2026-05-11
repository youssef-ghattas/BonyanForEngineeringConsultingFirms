using Bonyan.BLL.Services;
using Bonyan.DAL.Models;
using Microsoft.AspNetCore.Mvc;

namespace BonyanForEngineeringConsultingFirms.Controllers
{
	public class MaterialController : Controller
	{
		private readonly IService<Material> _materialService;

		public MaterialController(IService<Material> materialService)
		{
			_materialService = materialService;
		}

		public IActionResult Index()
		{
			var materials = _materialService.GetAll();
			return View(materials);
		}

		public IActionResult Details(int id)
		{
			var material = _materialService.GetById(id);
			if (material == null) return NotFound();
			return View(material);
		}

		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Create(Material material)
		{
			if (ModelState.IsValid)
			{
				_materialService.Add(material);
				return RedirectToAction(nameof(Index));
			}
			return View(material);
		}

		public IActionResult Edit(int id)
		{
			var material = _materialService.GetById(id);
			if (material == null) return NotFound();
			return View(material);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Edit(Material material)
		{
			if (ModelState.IsValid)
			{
				_materialService.Update(material);
				return RedirectToAction(nameof(Index));
			}
			return View(material);
		}

		public IActionResult Delete(int id)
		{
			var material = _materialService.GetById(id);
			if (material == null) return NotFound();
			return View(material);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public IActionResult DeleteConfirmed(int id)
		{
			_materialService.Delete(id);
			return RedirectToAction(nameof(Index));
		}
	}
}