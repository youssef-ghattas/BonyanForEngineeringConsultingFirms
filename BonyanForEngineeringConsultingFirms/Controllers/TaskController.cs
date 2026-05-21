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

            IQueryable<Task> query = _context.Tasks
                .Include(t => t.Project);

            if (projectId.HasValue)
                query = query.Where(t => t.ProjectId == projectId.Value);

            var tasks = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();

            ViewBag.ProjectId = projectId;
            if (projectId.HasValue)
                ViewBag.ProjectName = (await _context.Projects.FindAsync(projectId.Value))?.ProjectName ?? "—";

            return View(tasks);
        }

        public async Task<IActionResult> Details(int id)
        {
            var task = await _context.Tasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.TaskId == id);
            if (task == null) return NotFound();
            return View(task);
        }

        // Called from Project Details page with ?projectId=X
        public IActionResult Create(int projectId)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return Forbid();

            ViewBag.ProjectId = projectId;
            ViewBag.ProjectName = _context.Projects.Find(projectId)?.ProjectName ?? "—";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Task task)
        {
            var role = HttpContext.Session.GetString("Role");
            var userId = HttpContext.Session.GetInt32("UserId");
            if (role != "Admin") return Forbid();

            ModelState.Remove("Project");
            ModelState.Remove("Creator");
            ModelState.Remove("Documents");
            ModelState.Remove("Drawings");
            ModelState.Remove("Invoices");
            ModelState.Remove("MaterialTasks");

            if (!ModelState.IsValid)
            {
                ViewBag.ProjectId = task.ProjectId;
                ViewBag.ProjectName = _context.Projects.Find(task.ProjectId)?.ProjectName ?? "—";
                return View(task);
            }

            task.CreatedAt = DateTime.Now;
            task.CreatedBy_UserID = userId ?? 0;

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تمت إضافة المهمة بنجاح";
            return RedirectToAction("Details", "Project", new { id = task.ProjectId });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return Forbid();

            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            ViewBag.ProjectName = _context.Projects.Find(task.ProjectId)?.ProjectName ?? "—";
            return View(task);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Task task)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return Forbid();

            ModelState.Remove("Project");
            ModelState.Remove("Creator");
            ModelState.Remove("Documents");
            ModelState.Remove("Drawings");
            ModelState.Remove("Invoices");
            ModelState.Remove("MaterialTasks");

            if (!ModelState.IsValid)
            {
                ViewBag.ProjectName = _context.Projects.Find(task.ProjectId)?.ProjectName ?? "—";
                return View(task);
            }

            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم تحديث المهمة بنجاح";
            return RedirectToAction("Details", "Project", new { id = task.ProjectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return Forbid();

            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            int projectId = task.ProjectId;
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم حذف المهمة";
            return RedirectToAction("Details", "Project", new { id = projectId });
        }
    }
}