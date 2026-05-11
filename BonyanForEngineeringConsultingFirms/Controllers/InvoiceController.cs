using Bonyan.BLL.Services;
using Bonyan.DAL.Models;
using Microsoft.AspNetCore.Mvc;

namespace BonyanForEngineeringConsultingFirms.Controllers
{
	public class InvoiceController : Controller
	{
		private readonly IService<Invoice> _invoiceService;

		public InvoiceController(IService<Invoice> invoiceService)
		{
			_invoiceService = invoiceService;
		}

		public IActionResult Index()
		{
			var invoices = _invoiceService.GetAll();
			return View(invoices);
		}

		public IActionResult Details(int id)
		{
			var invoice = _invoiceService.GetById(id);
			if (invoice == null) return NotFound();
			return View(invoice);
		}

		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Create(Invoice invoice)
		{
			if (ModelState.IsValid)
			{
				_invoiceService.Add(invoice);
				return RedirectToAction(nameof(Index));
			}
			return View(invoice);
		}

		public IActionResult Edit(int id)
		{
			var invoice = _invoiceService.GetById(id);
			if (invoice == null) return NotFound();
			return View(invoice);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Edit(Invoice invoice)
		{
			if (ModelState.IsValid)
			{
				_invoiceService.Update(invoice);
				return RedirectToAction(nameof(Index));
			}
			return View(invoice);
		}

		public IActionResult Delete(int id)
		{
			var invoice = _invoiceService.GetById(id);
			if (invoice == null) return NotFound();
			return View(invoice);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public IActionResult DeleteConfirmed(int id)
		{
			_invoiceService.Delete(id);
			return RedirectToAction(nameof(Index));
		}
	}
}