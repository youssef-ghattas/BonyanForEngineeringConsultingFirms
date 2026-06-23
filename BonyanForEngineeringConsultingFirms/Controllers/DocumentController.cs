// Controllers/DocumentController.cs
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

        public DocumentController(BonyanDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index(int? projectId)
        {
            var role = HttpContext.Session.GetString("Role");
            var employeeId = HttpContext.Session.GetInt32("EmployeeId");

            if (role == null) return RedirectToAction("Login", "Account");

            IQueryable<Document> query = _context.Documents
                .Include(d => d.Project)
                .Include(d => d.Employee);

            if (role != "Admin" && employeeId != null)
            {
                query = query.Where(d => d.Project.EmployeeProjects
                                          .Any(ep => ep.EmployeeId == employeeId));
            }

            if (projectId.HasValue)
                query = query.Where(d => d.ProjectId == projectId.Value);

            var docs = await query.OrderByDescending(d => d.UploadDate).ToListAsync();

            // Load all projects for filter dropdown
            if (role == "Admin")
            {
                ViewBag.Projects = await _context.Projects
                    .OrderBy(p => p.ProjectName).ToListAsync();
            }
            else if (employeeId != null)
            {
                ViewBag.Projects = await _context.Projects
                    .Where(p => p.EmployeeProjects.Any(ep => ep.EmployeeId == employeeId))
                    .OrderBy(p => p.ProjectName).ToListAsync();
            }

            ViewBag.SelectedProjectId = projectId;
            return View(docs);
        }

        public IActionResult Create(int projectId)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role == null) return RedirectToAction("Login", "Account");
            if (role == "Admin") return Forbid(); // Admin cannot upload documents

            ViewBag.ProjectId = projectId;
            ViewBag.ProjectName = _context.Projects.Find(projectId)?.ProjectName ?? "—";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Document document, IFormFile uploadedFile)
        {
            var role = HttpContext.Session.GetString("Role");
            var employeeId = HttpContext.Session.GetInt32("EmployeeId");

            if (role == null) return RedirectToAction("Login", "Account");
            if (role == "Admin") return Forbid();

            if (employeeId == null)
            {
                TempData["ErrorMessageKey"] = "err_user_not_found";
                return RedirectToAction("Login", "Account");
            }

            ModelState.Remove("Project");
            ModelState.Remove("Task");
            ModelState.Remove("Employee");
            ModelState.Remove("FilePath");
            ModelState.Remove("FileType");

            if (uploadedFile == null || uploadedFile.Length == 0)
                ModelState.AddModelError("uploadedFile", "يرجى اختيار ملف للرفع");

            if (!ModelState.IsValid)
            {
                ViewBag.ProjectId = document.ProjectId;
                ViewBag.ProjectName = _context.Projects.Find(document.ProjectId)?.ProjectName ?? "—";
                return View(document);
            }

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "documents");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueName = Guid.NewGuid().ToString() + Path.GetExtension(uploadedFile.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await uploadedFile.CopyToAsync(stream);

            document.FilePath = "/uploads/documents/" + uniqueName;
            document.FileType = Path.GetExtension(uploadedFile.FileName).TrimStart('.').ToUpper();
            document.UploadDate = DateTime.Now;
            document.EmployeeId = employeeId.Value; // exact employee, no fallback

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            TempData["SuccessMessageKey"] = "msg_document_uploaded";
            return RedirectToAction("Details", "Project", new { id = document.ProjectId });
        }

        // ── DELETE GET ────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role == null) return RedirectToAction("Login", "Account");

            var doc = await _context.Documents.FindAsync(id);
            if (doc == null) return NotFound();
            return View(doc);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role == null) return RedirectToAction("Login", "Account");

            var doc = await _context.Documents.FindAsync(id);
            if (doc == null) return NotFound();

            if (!string.IsNullOrEmpty(doc.FilePath))
            {
                var fullPath = Path.Combine(_env.WebRootPath, doc.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);
            }

            int projectId = doc.ProjectId;
            _context.Documents.Remove(doc);
            await _context.SaveChangesAsync();

            TempData["SuccessMessageKey"] = "msg_document_deleted";
            return RedirectToAction("Details", "Project", new { id = projectId });
        }
    }
}