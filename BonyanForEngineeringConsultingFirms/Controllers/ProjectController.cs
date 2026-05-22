using Bonyan.DAL.Context;
using Bonyan.DAL.Models;
using Bonyan.DAL.Enums;
using Bonyan.PL.Helpers;
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

		// ────────────────────────────────────────────────────────
		// 1. INDEX — Admin sees all, Engineer sees only his projects
		// ────────────────────────────────────────────────────────
		public async Task<IActionResult> Index()
		{
			var role = HttpContext.Session.GetString("Role");
			var employeeId = HttpContext.Session.GetInt32("EmployeeId");

			List<Project> projects;

			if (role == "Admin")
			{
				projects = await _context.Projects
					.Include(p => p.EmployeeProjects)
					.ToListAsync();
			}
			else
			{
				if (employeeId == null)
					return RedirectToAction("Login", "Account");

				projects = await _context.Projects
					.Include(p => p.EmployeeProjects)
					.Where(p => p.EmployeeProjects
								 .Any(ep => ep.EmployeeId == employeeId))
					.ToListAsync();
			}

			return View(projects);
		}

        // ────────────────────────────────────────────────────────
        // 2. DETAILS — load everything for this project
        // ────────────────────────────────────────────────────────
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var role = HttpContext.Session.GetString("Role");
            var employeeId = HttpContext.Session.GetInt32("EmployeeId");

            // Load project with all related data
            var project = await _context.Projects
                .Include(p => p.EmployeeProjects)
                    .ThenInclude(ep => ep.Employee)
                    .ThenInclude(e => e.UserAccount)
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.AssignedToEmployee)
                .Include(p => p.Documents)
                    .ThenInclude(d => d.Employee)
                .Include(p => p.Drawings)
                    .ThenInclude(dr => dr.Employee)
                .Include(p => p.SiteVisits)
                    .ThenInclude(sv => sv.Employee)
                .FirstOrDefaultAsync(p => p.ProjectId == id);

            if (project == null) return NotFound();

            // Engineer can only open a project they are assigned to
            if (role != "Admin" && employeeId != null)
            {
                bool assigned = project.EmployeeProjects
                    .Any(ep => ep.EmployeeId == employeeId);
                if (!assigned) return Forbid();
            }

            // ── Load invoices (both direct + via tasks) ──────────
            var directInvoices = await _context.Invoices
                .Where(i => i.ProjectId == id)
                .ToListAsync();

            var taskInvoices = await _context.Invoices
                .Include(i => i.Task)
                .Where(i => i.TaskId != null && i.Task.ProjectId == id)
                .ToListAsync();

            var allInvoices = directInvoices
                .Union(taskInvoices, new Bonyan.PL.Helpers.InvoiceComparer())
                .ToList();

            ViewBag.ProjectInvoices = allInvoices;
            // ─────────────────────────────────────────────────────

            // Pass employee list so admin can assign from details page
            if (role == "Admin")
            {
                ViewBag.Employees = await _context.Employees
                    .Include(e => e.UserAccount)
                    .ToListAsync();
            }

            return View(project);
        }

        // ────────────────────────────────────────────────────────
        // 3. CREATE (GET) — load employees for checkbox dropdown
        // ────────────────────────────────────────────────────────
        public async Task<IActionResult> Create()
		{
			var role = HttpContext.Session.GetString("Role");
			if (role != "Admin") return Forbid();

			ViewBag.Employees = await _context.Employees
				.Include(e => e.UserAccount)
				.ToListAsync();

			return View();
		}

		// ────────────────────────────────────────────────────────
		// 4. CREATE (POST) — save project + assigned employees
		// ────────────────────────────────────────────────────────
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(Project project,
												List<int> selectedEmployees)
		{
			var role = HttpContext.Session.GetString("Role");
			if (role != "Admin") return Forbid();

			ModelState.Remove("EmployeeProjects");
			ModelState.Remove("Tasks");
			ModelState.Remove("SiteVisits");
			ModelState.Remove("Documents");
			ModelState.Remove("Drawings");
			ModelState.Remove("Invoices");

			if (ModelState.IsValid)
			{
				_context.Add(project);
				await _context.SaveChangesAsync(); // get the new ProjectId first

				if (selectedEmployees != null && selectedEmployees.Any())
				{
					foreach (var empId in selectedEmployees)
					{
						_context.EmployeeProjects.Add(new EmployeeProject
						{
							ProjectId = project.ProjectId,
							EmployeeId = empId,
							AssignmentDate = DateTime.Now
						});
					}
					await _context.SaveChangesAsync();
				}

				TempData["SuccessMessage"] = "تم إنشاء المشروع بنجاح";
				return RedirectToAction(nameof(Index));
			}

			// Reload on validation failure
			ViewBag.Employees = await _context.Employees
				.Include(e => e.UserAccount)
				.ToListAsync();

			return View(project);
		}

		// ────────────────────────────────────────────────────────
		// 5. EDIT (GET)
		// ────────────────────────────────────────────────────────
		public async Task<IActionResult> Edit(int? id)
		{
			var role = HttpContext.Session.GetString("Role");
			if (role != "Admin") return Forbid();

			if (id == null) return NotFound();

			var project = await _context.Projects.FindAsync(id);
			if (project == null) return NotFound();

			return View(project);
		}

		// ────────────────────────────────────────────────────────
		// 6. EDIT (POST)
		// ────────────────────────────────────────────────────────
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, Project project)
		{
			var role = HttpContext.Session.GetString("Role");
			if (role != "Admin") return Forbid();

			if (id != project.ProjectId) return NotFound();

			ModelState.Remove("EmployeeProjects");
			ModelState.Remove("Tasks");
			ModelState.Remove("SiteVisits");
			ModelState.Remove("Documents");
			ModelState.Remove("Drawings");
			ModelState.Remove("Invoices");

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

		// ────────────────────────────────────────────────────────
		// 7. ASSIGN EMPLOYEE (from Details page)
		// ────────────────────────────────────────────────────────
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AssignEmployee(int projectId,
														 int employeeId,
														 string roleInProject)
		{
			var role = HttpContext.Session.GetString("Role");
			if (role != "Admin") return Forbid();

			bool exists = await _context.EmployeeProjects
				.AnyAsync(ep => ep.ProjectId == projectId
							 && ep.EmployeeId == employeeId);

			if (!exists)
			{
				_context.EmployeeProjects.Add(new EmployeeProject
				{
					ProjectId = projectId,
					EmployeeId = employeeId,
					RoleInProject = roleInProject,
					AssignmentDate = DateTime.Now
				});
				await _context.SaveChangesAsync();
			}

			return RedirectToAction(nameof(Details), new { id = projectId });
		}

		// ────────────────────────────────────────────────────────
		// 8. REMOVE EMPLOYEE (from Details page)
		// ────────────────────────────────────────────────────────
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> RemoveEmployee(int projectId,
														 int employeeId)
		{
			var role = HttpContext.Session.GetString("Role");
			if (role != "Admin") return Forbid();

			var ep = await _context.EmployeeProjects
				.FirstOrDefaultAsync(e => e.ProjectId == projectId
									   && e.EmployeeId == employeeId);

			if (ep != null)
			{
				_context.EmployeeProjects.Remove(ep);
				await _context.SaveChangesAsync();
			}

			return RedirectToAction(nameof(Details), new { id = projectId });
		}

		// ────────────────────────────────────────────────────────
		// 9. DELETE (GET)
		// ────────────────────────────────────────────────────────
		public async Task<IActionResult> Delete(int? id)
		{
			var role = HttpContext.Session.GetString("Role");
			if (role != "Admin") return Forbid();

			if (id == null) return NotFound();

			var project = await _context.Projects
				.FirstOrDefaultAsync(m => m.ProjectId == id);

			if (project == null) return NotFound();

			return View(project);
		}

		// ────────────────────────────────────────────────────────
		// 10. DELETE (POST)
		// ────────────────────────────────────────────────────────
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var role = HttpContext.Session.GetString("Role");
			if (role != "Admin") return Forbid();

			var project = await _context.Projects.FindAsync(id);
			if (project != null)
			{
				_context.Projects.Remove(project);
				await _context.SaveChangesAsync();
				TempData["SuccessMessage"] = "تم حذف المشروع بنجاح";
			}

			return RedirectToAction(nameof(Index));
		}

		private bool ProjectExists(int id) =>
			_context.Projects.Any(e => e.ProjectId == id);
	}
}