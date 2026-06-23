// BonyanForEngineeringConsultingFirms/Controllers/SiteVisitController.cs
using Bonyan.DAL.Context;
using Bonyan.DAL.Enums;
using Bonyan.DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BonyanForEngineeringConsultingFirms.Controllers
{
    public class SiteVisitController : Controller
    {
        private readonly BonyanDbContext _context;
        private readonly IWebHostEnvironment _env;

        public SiteVisitController(BonyanDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ── INDEX — Admin sees all (with project filter), employees see their project visits
        public async Task<IActionResult> Index(int? projectId)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role == null) return RedirectToAction("Login", "Account");

            var employeeId = HttpContext.Session.GetInt32("EmployeeId");

            IQueryable<SiteVisit> query = _context.SiteVisits
                .Include(sv => sv.Project)
                .Include(sv => sv.Employee);

            // Non-admin: only their assigned projects
            if (role != "Admin" && employeeId != null)
                query = query.Where(sv => sv.Project.EmployeeProjects
                                            .Any(ep => ep.EmployeeId == employeeId));

            // Filter by project
            if (projectId.HasValue)
                query = query.Where(sv => sv.ProjId == projectId.Value);

            var visits = await query.OrderByDescending(sv => sv.VisitDate).ToListAsync();

            // Projects list for filter dropdown
            IQueryable<Project> projectsQuery = role == "Admin"
                ? _context.Projects.OrderBy(p => p.ProjectName)
                : _context.Projects
                    .Where(p => p.EmployeeProjects.Any(ep => ep.EmployeeId == employeeId))
                    .OrderBy(p => p.ProjectName);

            ViewBag.Projects = await projectsQuery.ToListAsync();
            ViewBag.SelectedProjectId = projectId;

            return View(visits);
        }

        // ── DETAILS
        public async Task<IActionResult> Details(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role == null) return RedirectToAction("Login", "Account");

            var siteVisit = await _context.SiteVisits
                .Include(sv => sv.Project)
                .Include(sv => sv.Employee)
                .FirstOrDefaultAsync(sv => sv.VisitId == id);

            if (siteVisit == null) return NotFound();
            return View(siteVisit);
        }

        // ── CREATE GET — Only ProjectManager or Engineer (NOT Admin)
        public IActionResult Create(int projectId)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role == null) return RedirectToAction("Login", "Account");
            if (role == "Admin") return Forbid(); // Admin cannot add site visits

            var employeeId = HttpContext.Session.GetInt32("EmployeeId");

            // Must be assigned to this project
            bool assigned = _context.EmployeeProjects
                .Any(ep => ep.ProjectId == projectId && ep.EmployeeId == employeeId);
            if (!assigned) return Forbid();

            ViewBag.ProjectId = projectId;
            ViewBag.ProjectName = _context.Projects.Find(projectId)?.ProjectName ?? "—";
            return View();
        }

        // ── CREATE POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SiteVisit siteVisit, IFormFile? visitPhoto)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role == null) return RedirectToAction("Login", "Account");
            if (role == "Admin") return Forbid();

            var employeeId = HttpContext.Session.GetInt32("EmployeeId");
            if (employeeId == null) return RedirectToAction("Login", "Account");

            // Must be assigned
            bool assigned = _context.EmployeeProjects
                .Any(ep => ep.ProjectId == siteVisit.ProjId && ep.EmployeeId == employeeId);
            if (!assigned) return Forbid();

            ModelState.Remove("Project");
            ModelState.Remove("Employee");
            ModelState.Remove("VisitPhotos");

            // Handle photo upload
            string photoPath = "";
            if (visitPhoto != null && visitPhoto.Length > 0)
            {
                var allowedExts = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var ext = Path.GetExtension(visitPhoto.FileName).ToLower();
                if (!allowedExts.Contains(ext))
                    ModelState.AddModelError("visitPhoto", "يُسمح فقط برفع ملفات الصور");
                else
                {
                    var folder = Path.Combine(_env.WebRootPath, "uploads", "sitevisits");
                    Directory.CreateDirectory(folder);
                    var uniqueName = Guid.NewGuid() + ext;
                    var fullPath = Path.Combine(folder, uniqueName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                        await visitPhoto.CopyToAsync(stream);
                    photoPath = "/uploads/sitevisits/" + uniqueName;
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.ProjectId = siteVisit.ProjId;
                ViewBag.ProjectName = _context.Projects.Find(siteVisit.ProjId)?.ProjectName ?? "—";
                return View(siteVisit);
            }

            siteVisit.EmployeeId = employeeId.Value;
            siteVisit.VisitDate = DateTime.Now;
            siteVisit.VisitPhotos = photoPath;

            _context.SiteVisits.Add(siteVisit);
            await _context.SaveChangesAsync();

            TempData["SuccessMessageKey"] = "msg_sitevisit_created";
            return RedirectToAction("Details", "Project", new { id = siteVisit.ProjId });
        }

        // ── DELETE GET ────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return Forbid();

            var sv = await _context.SiteVisits.FindAsync(id);
            if (sv == null) return NotFound();
            return View(sv);
        }

        // ── DELETE POST — Only Admin
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return Forbid();

            var sv = await _context.SiteVisits.FindAsync(id);
            if (sv == null) return NotFound();

            int projectId = sv.ProjId;

            if (!string.IsNullOrEmpty(sv.VisitPhotos))
            {
                var fullPath = Path.Combine(_env.WebRootPath, sv.VisitPhotos.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);
            }

            _context.SiteVisits.Remove(sv);
            await _context.SaveChangesAsync();

            TempData["SuccessMessageKey"] = "msg_sitevisit_deleted";
            return RedirectToAction("Index");
        }
    }
}