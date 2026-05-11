using Bonyan.BLL.Services;
using Bonyan.DAL.Models;
using Microsoft.AspNetCore.Mvc;

namespace BonyanForEngineeringConsultingFirms.Controllers
{
    public class DocumentController : Controller
    {
        private readonly IService<Document> _documentService;

        public DocumentController(IService<Document> documentService)
        {
            _documentService = documentService;
        }

        public IActionResult Index()
        {
            var documents = _documentService.GetAll();
            return View(documents);
        }

        public IActionResult Details(int id)
        {
            var document = _documentService.GetById(id);
            if (document == null) return NotFound();
            return View(document);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Document document)
        {
            if (ModelState.IsValid)
            {
                _documentService.Add(document);
                return RedirectToAction(nameof(Index));
            }
            return View(document);
        }

        public IActionResult Edit(int id)
        {
            var document = _documentService.GetById(id);
            if (document == null) return NotFound();
            return View(document);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Document document)
        {
            if (ModelState.IsValid)
            {
                _documentService.Update(document);
                return RedirectToAction(nameof(Index));
            }
            return View(document);
        }

        public IActionResult Delete(int id)
        {
            var document = _documentService.GetById(id);
            if (document == null) return NotFound();
            return View(document);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _documentService.Delete(id);
            return RedirectToAction(nameof(Index));
        }
    }
}