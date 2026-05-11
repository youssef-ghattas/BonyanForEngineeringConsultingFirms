using Bonyan.BLL.Services;
using Microsoft.AspNetCore.Mvc;
using Task = Bonyan.DAL.Models.Task;

namespace BonyanForEngineeringConsultingFirms.Controllers
{
    public class TaskController : Controller
    {
        private readonly IService<Task> _taskService;

        public TaskController(IService<Task> taskService)
        {
            _taskService = taskService;
        }

        public IActionResult Index()
        {
            var tasks = _taskService.GetAll();
            return View(tasks);
        }

        public IActionResult Details(int id)
        {
            var task = _taskService.GetById(id);
            if (task == null) return NotFound();
            return View(task);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Task task)
        {
            if (ModelState.IsValid)
            {
                _taskService.Add(task);
                return RedirectToAction(nameof(Index));
            }
            return View(task);
        }

        public IActionResult Edit(int id)
        {
            var task = _taskService.GetById(id);
            if (task == null) return NotFound();
            return View(task);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Task task)
        {
            if (ModelState.IsValid)
            {
                _taskService.Update(task);
                return RedirectToAction(nameof(Index));
            }
            return View(task);
        }

        public IActionResult Delete(int id)
        {
            var task = _taskService.GetById(id);
            if (task == null) return NotFound();
            return View(task);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _taskService.Delete(id);
            return RedirectToAction(nameof(Index));
        }
    }
}