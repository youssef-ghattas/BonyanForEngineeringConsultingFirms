using Bonyan.BLL.Services;
using Bonyan.DAL.Models;
using Microsoft.AspNetCore.Mvc;

namespace BonyanForEngineeringConsultingFirms.Controllers
{
    public class DrawingController : Controller
    {
        private readonly IService<Drawing> _drawingService;

        public DrawingController(IService<Drawing> drawingService)
        {
            _drawingService = drawingService;
        }

        public IActionResult Index()
        {
            var drawings = _drawingService.GetAll();
            return View(drawings);
        }

        public IActionResult Details(int id)
        {
            var drawing = _drawingService.GetById(id);
            if (drawing == null) return NotFound();
            return View(drawing);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Drawing drawing)
        {
            if (ModelState.IsValid)
            {
                _drawingService.Add(drawing);
                return RedirectToAction(nameof(Index));
            }
            return View(drawing);
        }

        public IActionResult Edit(int id)
        {
            var drawing = _drawingService.GetById(id);
            if (drawing == null) return NotFound();
            return View(drawing);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Drawing drawing)
        {
            if (ModelState.IsValid)
            {
                _drawingService.Update(drawing);
                return RedirectToAction(nameof(Index));
            }
            return View(drawing);
        }

        public IActionResult Delete(int id)
        {
            var drawing = _drawingService.GetById(id);
            if (drawing == null) return NotFound();
            return View(drawing);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _drawingService.Delete(id);
            return RedirectToAction(nameof(Index));
        }
    }
}