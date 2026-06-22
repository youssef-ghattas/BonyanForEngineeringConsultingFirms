using Bonyan.DAL.Context;
using Bonyan.DAL.Enums;
using Bonyan.DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task = Bonyan.DAL.Models.Task;

namespace BonyanForEngineeringConsultingFirms.Controllers
{
    public class TaskController : Controller
    {
        private readonly BonyanDbContext _context;

        public TaskController(BonyanDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? projectId)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role == null) return RedirectToAction("Login", "Account");

            var employeeId = HttpContext.Session.GetInt32("EmployeeId");

            IQueryable<Task> query = _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.AssignedToEmployee);

            // Engineers and PMs only see tasks of their projects
            if (role != "Admin" && employeeId != null)
                query = query.Where(t => t.Project.EmployeeProjects
                                          .Any(ep => ep.EmployeeId == employeeId));

            if (projectId.HasValue)
                query = query.Where(t => t.ProjectId == projectId.Value);

            var tasks = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();

            if (role == "Admin")
            {
                ViewBag.Projects = await _context.Projects.ToListAsync();
            }
            else
            {
                ViewBag.Projects = await _context.Projects
                    .Where(p => p.EmployeeProjects.Any(ep => ep.EmployeeId == employeeId))
                    .ToListAsync();
            }

            ViewBag.ProjectId = projectId;
            if (projectId.HasValue)
            {
                ViewBag.ProjectName = (await _context.Projects.FindAsync(projectId.Value))?.ProjectName;
            }

            return View(tasks);
        }

        public async Task<IActionResult> Details(int id)
        {
            var task = await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.AssignedToEmployee)
                .FirstOrDefaultAsync(t => t.TaskId == id);
            if (task == null) return NotFound();
            return View(task);
        }

        // Only ProjectManager can create tasks
        public IActionResult Create(int projectId)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "ProjectManager") return Forbid();

            var employeeId = HttpContext.Session.GetInt32("EmployeeId");

            // Verify this PM is actually assigned to this project
            bool assigned = _context.EmployeeProjects
                .Any(ep => ep.ProjectId == projectId && ep.EmployeeId == employeeId);
            if (!assigned) return Forbid();

            ViewBag.ProjectId = projectId;
            ViewBag.ProjectName = _context.Projects.Find(projectId)?.ProjectName ?? "—";

            // Only engineers of THIS project (excluding the PM himself)
            var engineers = _context.EmployeeProjects
                .Where(ep => ep.ProjectId == projectId && ep.EmployeeId != employeeId)
                .Include(ep => ep.Employee)
                .Select(ep => ep.Employee)
                .ToList();
            ViewBag.Engineers = engineers;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Task task)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "ProjectManager") return Forbid();

            var userId = HttpContext.Session.GetInt32("UserId");
            var employeeId = HttpContext.Session.GetInt32("EmployeeId");

            ModelState.Remove("Project");
            ModelState.Remove("Creator");
            ModelState.Remove("AssignedToEmployee");
            ModelState.Remove("Documents");
            ModelState.Remove("Drawings");
            ModelState.Remove("Invoices");
            ModelState.Remove("MaterialTasks");

            if (!ModelState.IsValid)
            {
                ViewBag.ProjectId = task.ProjectId;
                ViewBag.ProjectName = _context.Projects.Find(task.ProjectId)?.ProjectName ?? "—";
                ViewBag.Engineers = _context.EmployeeProjects
                    .Where(ep => ep.ProjectId == task.ProjectId && ep.EmployeeId != employeeId)
                    .Include(ep => ep.Employee)
                    .Select(ep => ep.Employee)
                    .ToList();
                return View(task);
            }

            task.CreatedAt = DateTime.Now;
            task.CreatedBy_UserID = userId;

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            TempData["SuccessMessageKey"] = "msg_task_created";
            return RedirectToAction("Details", "Project", new { id = task.ProjectId });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "ProjectManager") return Forbid();

            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            var employeeId = HttpContext.Session.GetInt32("EmployeeId");

            ViewBag.ProjectName = _context.Projects.Find(task.ProjectId)?.ProjectName ?? "—";
            ViewBag.Engineers = _context.EmployeeProjects
                .Where(ep => ep.ProjectId == task.ProjectId && ep.EmployeeId != employeeId)
                .Include(ep => ep.Employee)
                .Select(ep => ep.Employee)
                .ToList();

            return View(task);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Task task)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "ProjectManager") return Forbid();

            var employeeId = HttpContext.Session.GetInt32("EmployeeId");

            ModelState.Remove("Project");
            ModelState.Remove("Creator");
            ModelState.Remove("AssignedToEmployee");
            ModelState.Remove("Documents");
            ModelState.Remove("Drawings");
            ModelState.Remove("Invoices");
            ModelState.Remove("MaterialTasks");

            if (!ModelState.IsValid)
            {
                ViewBag.ProjectName = _context.Projects.Find(task.ProjectId)?.ProjectName ?? "—";
                ViewBag.Engineers = _context.EmployeeProjects
                    .Where(ep => ep.ProjectId == task.ProjectId && ep.EmployeeId != employeeId)
                    .Include(ep => ep.Employee)
                    .Select(ep => ep.Employee)
                    .ToList();
                return View(task);
            }

            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();

            TempData["SuccessMessageKey"] = "msg_task_updated";
            return RedirectToAction("Details", "Project", new { id = task.ProjectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "ProjectManager") return Forbid();

            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            int projectId = task.ProjectId;
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            TempData["SuccessMessageKey"] = "msg_task_deleted";
            return RedirectToAction("Details", "Project", new { id = projectId });
        }
    }
}