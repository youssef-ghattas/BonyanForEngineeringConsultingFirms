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

        public DrawingController(BonyanDbContext context,
                                  IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ─────────────────────────────────────────────
        // INDEX
        // ─────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var role = HttpContext.Session.GetString("Role");
            var employeeId = HttpContext.Session.GetInt32("EmployeeId");

            List<Drawing> drawings;

            if (role == "Admin")
            {
                drawings = await _context.Drawings
                    .Include(d => d.Project)
                    .Include(d => d.Employee)
                    .OrderByDescending(d => d.UploadDate)
                    .ToListAsync();
            }
            else
            {
                if (employeeId == null)
                    return RedirectToAction("Login", "Account");

                drawings = await _context.Drawings
                    .Include(d => d.Project)
                    .Include(d => d.Employee)
                    .Where(d => d.Project.EmployeeProjects
                                 .Any(ep => ep.EmployeeId == employeeId))
                    .OrderByDescending(d => d.UploadDate)
                    .ToListAsync();
            }

            return View(drawings);
        }

        // ─────────────────────────────────────────────
        // CREATE (GET)
        // ─────────────────────────────────────────────
        public IActionResult Create(int projectId)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role == null) return RedirectToAction("Login", "Account");

            ViewBag.ProjectId = projectId;
            var project = _context.Projects.Find(projectId);
            ViewBag.ProjectName = project?.ProjectName ?? "—";

            return View();
        }

        // ─────────────────────────────────────────────
        // CREATE (POST)
        // ─────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Drawing drawing,
                                                 IFormFile uploadedFile)
        {
            var role = HttpContext.Session.GetString("Role");
            var employeeId = HttpContext.Session.GetInt32("EmployeeId");

            if (role == null) return RedirectToAction("Login", "Account");

            ModelState.Remove("Project");
            ModelState.Remove("Task");
            ModelState.Remove("Employee");
            ModelState.Remove("FilePath");

            if (uploadedFile == null || uploadedFile.Length == 0)
                ModelState.AddModelError("uploadedFile", "يرجى اختيار صورة للرفع");

            // Validate image types only
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png",
                                            ".gif", ".bmp", ".webp", ".svg" };
            if (uploadedFile != null)
            {
                var ext = Path.GetExtension(uploadedFile.FileName).ToLower();
                if (!allowedExtensions.Contains(ext))
                    ModelState.AddModelError("uploadedFile",
                        "يُسمح فقط برفع ملفات الصور (jpg, png, gif, ...)");
            }

            if (ModelState.IsValid)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath,
                                                  "uploads", "drawings");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueName = Guid.NewGuid().ToString()
                               + Path.GetExtension(uploadedFile!.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await uploadedFile.CopyToAsync(stream);

                drawing.FilePath = "/uploads/drawings/" + uniqueName;
                drawing.UploadDate = DateTime.Now;
                drawing.EmployeeId = employeeId ?? 0;

                _context.Drawings.Add(drawing);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "تم رفع الصورة بنجاح";
                return RedirectToAction("Details", "Project",
                                        new { id = drawing.ProjectId });
            }

            ViewBag.ProjectId = drawing.ProjectId;
            var proj = _context.Projects.Find(drawing.ProjectId);
            ViewBag.ProjectName = proj?.ProjectName ?? "—";
            return View(drawing);
        }

        // ─────────────────────────────────────────────
        // DELETE (POST)
        // ─────────────────────────────────────────────
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
                var fullPath = Path.Combine(_env.WebRootPath,
                                             drawing.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);
            }

            int projectId = drawing.ProjectId;
            _context.Drawings.Remove(drawing);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم حذف الصورة بنجاح";
            return RedirectToAction("Details", "Project",
                                    new { id = projectId });
        }
    }
}