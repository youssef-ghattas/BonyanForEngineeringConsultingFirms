using Bonyan.DAL.Context;
using Bonyan.DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BonyanForEngineeringConsultingFirms.Controllers
{
    public class DrawingController : Controller
    {
        private readonly BonyanDbContext _context;
        private readonly IWebHostEnvironment _env;

        public DrawingController(BonyanDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index(int? projectId)
        {
            var role = HttpContext.Session.GetString("Role");
            var employeeId = HttpContext.Session.GetInt32("EmployeeId");
            if (role == null) return RedirectToAction("Login", "Account");

            IQueryable<Drawing> query = _context.Drawings
                .Include(d => d.Project)
                .Include(d => d.Employee);

            if (role != "Admin" && employeeId != null)
                query = query.Where(d => d.Project.EmployeeProjects
                                          .Any(ep => ep.EmployeeId == employeeId));

            if (projectId.HasValue)
                query = query.Where(d => d.ProjectId == projectId.Value);

            var drawings = await query.OrderByDescending(d => d.UploadDate).ToListAsync();

            var projectsQuery = role == "Admin"
                ? _context.Projects.OrderBy(p => p.ProjectName)
                : _context.Projects
                    .Where(p => p.EmployeeProjects.Any(ep => ep.EmployeeId == employeeId))
                    .OrderBy(p => p.ProjectName);

            ViewBag.Projects = await projectsQuery.ToListAsync();
            ViewBag.SelectedProjectId = projectId;

            // ── Stats for the dashboard cards ─────────────────────────
            var now = DateTime.Now;
            ViewBag.TotalDrawings = drawings.Count;
            ViewBag.ThisMonthCount = drawings.Count(d => d.UploadDate.Year == now.Year && d.UploadDate.Month == now.Month);

            // Drawings that share a title with another drawing in the same project = multiple versions
            ViewBag.MultiVersionCount = drawings
                .GroupBy(d => new { d.ProjectId, Title = (d.DrawingTitle ?? "").Trim().ToLower() })
                .Count(g => g.Count() > 1);

            ViewBag.ActiveProjectsCount = drawings
                .Where(d => d.ProjectId != 0)
                .Select(d => d.ProjectId)
                .Distinct()
                .Count();

            // Distinct, non-empty categories for the filter dropdown
            ViewBag.Categories = drawings
                .Select(d => d.Category)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(c => c!.Trim())
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            return View(drawings);
        }

        public IActionResult Create(int projectId)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role == null) return RedirectToAction("Login", "Account");
            if (role == "Admin") return Forbid(); // Admin cannot upload drawings

            ViewBag.ProjectId = projectId;
            ViewBag.ProjectName = _context.Projects.Find(projectId)?.ProjectName ?? "—";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Drawing drawing, IFormFile uploadedFile)
        {
            var role = HttpContext.Session.GetString("Role");
            var employeeId = HttpContext.Session.GetInt32("EmployeeId");

            if (role == null) return RedirectToAction("Login", "Account");
            if (role == "Admin") return Forbid();

            if (employeeId == null)
            {
                TempData["ErrorMessage"] = "لم يتم العثور على بيانات المستخدم، يرجى تسجيل الدخول مجدداً.";
                return RedirectToAction("Login", "Account");
            }

            ModelState.Remove("Project");
            ModelState.Remove("Task");
            ModelState.Remove("Employee");
            ModelState.Remove("FilePath");

            if (uploadedFile == null || uploadedFile.Length == 0)
                ModelState.AddModelError("uploadedFile", "يرجى اختيار صورة للرفع");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg" };
            if (uploadedFile != null)
            {
                var ext = Path.GetExtension(uploadedFile.FileName).ToLower();
                if (!allowedExtensions.Contains(ext))
                    ModelState.AddModelError("uploadedFile", "يُسمح فقط برفع ملفات الصور");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.ProjectId = drawing.ProjectId;
                ViewBag.ProjectName = _context.Projects.Find(drawing.ProjectId)?.ProjectName ?? "—";
                return View(drawing);
            }

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "drawings");
            Directory.CreateDirectory(uploadsFolder);
            var uniqueName = Guid.NewGuid() + Path.GetExtension(uploadedFile!.FileName);
            var fullPath = Path.Combine(uploadsFolder, uniqueName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
                await uploadedFile.CopyToAsync(stream);

            drawing.FilePath = "/uploads/drawings/" + uniqueName;
            drawing.UploadDate = DateTime.Now;
            drawing.EmployeeId = employeeId.Value; // exact employee, no fallback

            _context.Drawings.Add(drawing);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم رفع الصورة بنجاح";
            return RedirectToAction("Details", "Project", new { id = drawing.ProjectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role == null) return RedirectToAction("Login", "Account");

            var drawing = await _context.Drawings.FindAsync(id);
            if (drawing == null) return NotFound();

            if (!string.IsNullOrEmpty(drawing.FilePath))
            {
                var fullPath = Path.Combine(_env.WebRootPath, drawing.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);
            }

            int projectId = drawing.ProjectId;
            _context.Drawings.Remove(drawing);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم حذف الصورة بنجاح";
            return RedirectToAction("Details", "Project", new { id = projectId });
        }
    }
}
