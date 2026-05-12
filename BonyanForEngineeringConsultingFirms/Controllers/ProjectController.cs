using Bonyan.DAL.Context;
using Bonyan.DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bonyan.Web.Controllers
{
    public class ProjectController : Controller
    {
        private readonly BonyanDbContext _context;

        public ProjectController(BonyanDbContext context)
        {
            _context = context;
        }

        // 1. عرض قائمة المشاريع (Index)
        public async Task<IActionResult> Index()
        {
            var projects = await _context.Projects.Include(p => p.EmployeeProjects).ToListAsync();
            return View(projects);
        }

        // 2. صفحة إضافة مشروع جديد (GET)
        public IActionResult Create()
        {
            return View();
        }

        // 3. حفظ المشروع الجديد (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Project project)
        {
            if (ModelState.IsValid)
            {
                _context.Add(project);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }

        // 4. صفحة التعديل (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects.FindAsync(id);
            if (project == null) return NotFound();

            return View(project);
        }

        // 5. حفظ التعديلات (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Project project)
        {
            if (id != project.ProjectId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(project);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.ProjectId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }

        // 6. صفحة تأكيد الحذف (GET)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects
                .FirstOrDefaultAsync(m => m.ProjectId == id);
            if (project == null) return NotFound();

            return View(project);
        }

        // 7. تنفيذ الحذف الفعلي (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();

                // إرسال رسالة نجاح تظهر لمرة واحدة
                TempData["SuccessMessage"] = "تم حذف المشروع بنجاح";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.ProjectId == id);
        }
    }
}