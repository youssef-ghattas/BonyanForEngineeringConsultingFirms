using Bonyan.DAL.Context;
using Bonyan.DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BonyanForEngineeringConsultingFirms.Controllers
{
    public class DocumentController : Controller
    {
        private readonly BonyanDbContext _context;
        private readonly IWebHostEnvironment _env;

        public DocumentController(BonyanDbContext context,
                                   IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ─────────────────────────────────────────────
        // INDEX — list all docs (admin) or assigned (engineer)
        // ─────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var role = HttpContext.Session.GetString("Role");
            var employeeId = HttpContext.Session.GetInt32("EmployeeId");

            List<Document> docs;

            if (role == "Admin")
            {
                docs = await _context.Documents
                    .Include(d => d.Project)
                    .Include(d => d.Employee)
                    .OrderByDescending(d => d.UploadDate)
                    .ToListAsync();
            }
            else
            {
                if (employeeId == null)
                    return RedirectToAction("Login", "Account");

                // Engineer sees only docs from projects they are assigned to
                docs = await _context.Documents
                    .Include(d => d.Project)
                    .Include(d => d.Employee)
                    .Where(d => d.Project.EmployeeProjects
                                 .Any(ep => ep.EmployeeId == employeeId))
                    .OrderByDescending(d => d.UploadDate)
                    .ToListAsync();
            }

            return View(docs);
        }

        // ─────────────────────────────────────────────
        // CREATE (GET) — called with ?projectId=X
        // ─────────────────────────────────────────────
        public IActionResult Create(int projectId)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role == null) return RedirectToAction("Login", "Account");

            // Pass projectId so the form knows which project
            ViewBag.ProjectId = projectId;

            // Load project name for display
            var project = _context.Projects.Find(projectId);
            ViewBag.ProjectName = project?.ProjectName ?? "—";

            return View();
        }

        // ─────────────────────────────────────────────
        // CREATE (POST) — upload file + save record
        // ─────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Document document,
                                                 IFormFile uploadedFile)
        {
            var role = HttpContext.Session.GetString("Role");
            var employeeId = HttpContext.Session.GetInt32("EmployeeId");

            if (role == null) return RedirectToAction("Login", "Account");

            // Remove nav props from validation
            ModelState.Remove("Project");
            ModelState.Remove("Task");
            ModelState.Remove("Employee");
            ModelState.Remove("FilePath");

            if (uploadedFile == null || uploadedFile.Length == 0)
                ModelState.AddModelError("uploadedFile", "يرجى اختيار ملف للرفع");

            if (ModelState.IsValid)
            {
                // Save file to wwwroot/uploads/documents/
                var uploadsFolder = Path.Combine(_env.WebRootPath,
                                                  "uploads", "documents");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueName = Guid.NewGuid().ToString()
                               + Path.GetExtension(uploadedFile!.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await uploadedFile.CopyToAsync(stream);

                document.FilePath = "/uploads/documents/" + uniqueName;
                document.FileType = Path.GetExtension(uploadedFile.FileName)
                                           .TrimStart('.').ToUpper();
                document.UploadDate = DateTime.Now;
                document.EmployeeId = employeeId ?? 0;

                _context.Documents.Add(document);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "تم رفع المستند بنجاح";
                return RedirectToAction("Details", "Project",
                                        new { id = document.ProjectId });
            }

            ViewBag.ProjectId = document.ProjectId;
            var proj = _context.Projects.Find(document.ProjectId);
            ViewBag.ProjectName = proj?.ProjectName ?? "—";
            return View(document);
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

            var doc = await _context.Documents.FindAsync(id);
            if (doc == null) return NotFound();

            // Delete physical file
            if (!string.IsNullOrEmpty(doc.FilePath))
            {
                var fullPath = Path.Combine(_env.WebRootPath,
                                             doc.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);
            }

            int projectId = doc.ProjectId;
            _context.Documents.Remove(doc);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم حذف المستند بنجاح";
            return RedirectToAction("Details", "Project",
                                    new { id = projectId });
        }
    }
}