using Bonyan.BLL.Services;
using Bonyan.DAL.Models;
using Microsoft.AspNetCore.Mvc;

namespace BonyanForEngineeringConsultingFirms.Controllers
{
	public class SiteVisitController : Controller
	{
		private readonly IService<SiteVisit> _siteVisitService;

		public SiteVisitController(IService<SiteVisit> siteVisitService)
		{
			_siteVisitService = siteVisitService;
		}

		public IActionResult Index()
		{
			var siteVisits = _siteVisitService.GetAll();
			return View(siteVisits);
		}

		public IActionResult Details(int id)
		{
			var siteVisit = _siteVisitService.GetById(id);
			if (siteVisit == null) return NotFound();
			return View(siteVisit);
		}

		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Create(SiteVisit siteVisit)
		{
			if (ModelState.IsValid)
			{
				_siteVisitService.Add(siteVisit);
				return RedirectToAction(nameof(Index));
			}
			return View(siteVisit);
		}

		public IActionResult Edit(int id)
		{
			var siteVisit = _siteVisitService.GetById(id);
			if (siteVisit == null) return NotFound();
			return View(siteVisit);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Edit(SiteVisit siteVisit)
		{
			if (ModelState.IsValid)
			{
				_siteVisitService.Update(siteVisit);
				return RedirectToAction(nameof(Index));
			}
			return View(siteVisit);
		}

		public IActionResult Delete(int id)
		{
			var siteVisit = _siteVisitService.GetById(id);
			if (siteVisit == null) return NotFound();
			return View(siteVisit);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public IActionResult DeleteConfirmed(int id)
		{
			_siteVisitService.Delete(id);
			return RedirectToAction(nameof(Index));
		}
	}
}