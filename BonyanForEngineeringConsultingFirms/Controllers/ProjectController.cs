using Bonyan.BLL.Services;
using Bonyan.DAL.Models;
using Microsoft.AspNetCore.Mvc;

namespace BonyanForEngineeringConsultingFirms.Controllers
{
    public class ProjectController : Controller
    {
        private readonly IService<Project> _projectService;

        public ProjectController(IService<Project> projectService)
        {
            _projectService = projectService;
        }

        public IActionResult Index()
        {
            var projects = _projectService.GetAll();
            return View(projects);
        }

        public IActionResult Details(int id)
        {
            var project = _projectService.GetById(id);
            if (project == null) return NotFound();
            return View(project);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Project project)
        {
            if (ModelState.IsValid)
            {
                _projectService.Add(project);
                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }

        public IActionResult Edit(int id)
        {
            var project = _projectService.GetById(id);
            if (project == null) return NotFound();
            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Project project)
        {
            if (ModelState.IsValid)
            {
                _projectService.Update(project);
                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }

        public IActionResult Delete(int id)
        {
            var project = _projectService.GetById(id);
            if (project == null) return NotFound();
            return View(project);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _projectService.Delete(id);
            return RedirectToAction(nameof(Index));
        }
    }
}