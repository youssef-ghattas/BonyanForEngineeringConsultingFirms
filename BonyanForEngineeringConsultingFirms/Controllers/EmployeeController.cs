using Bonyan.BLL.Services;
using Bonyan.DAL.Models;
using Microsoft.AspNetCore.Mvc;

namespace BonyanForEngineeringConsultingFirms.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IService<Employee> _employeeService;

        public EmployeeController(IService<Employee> employeeService)
        {
            _employeeService = employeeService;
        }

        // ── Index (List All) ─────────────────────────────
        public IActionResult Index()
        {
            var employees = _employeeService.GetAll();
            return View(employees);
        }

        // ── Details ──────────────────────────────────────
        public IActionResult Details(int id)
        {
            var employee = _employeeService.GetById(id);
            if (employee == null) return NotFound();
            return View(employee);
        }

        // ── Create GET ───────────────────────────────────
        public IActionResult Create()
        {
            return View();
        }

        // ── Create POST ──────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Employee employee)
        {
            if (ModelState.IsValid)
            {
                _employeeService.Add(employee);
                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }

        // ── Edit GET ─────────────────────────────────────
        public IActionResult Edit(int id)
        {
            var employee = _employeeService.GetById(id);
            if (employee == null) return NotFound();
            return View(employee);
        }

        // ── Edit POST ────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Employee employee)
        {
            if (ModelState.IsValid)
            {
                _employeeService.Update(employee);
                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }

        // ── Delete GET ───────────────────────────────────
        public IActionResult Delete(int id)
        {
            var employee = _employeeService.GetById(id);
            if (employee == null) return NotFound();
            return View(employee);
        }

        // ── Delete POST ──────────────────────────────────
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _employeeService.Delete(id);
            return RedirectToAction(nameof(Index));
        }
    }
}