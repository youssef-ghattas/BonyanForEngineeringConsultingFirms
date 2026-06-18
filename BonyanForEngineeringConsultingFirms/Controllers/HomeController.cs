using Bonyan.DAL.Context;
using Bonyan.DAL.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BonyanForEngineeringConsultingFirms.Controllers
{
	public class HomeController : Controller
	{
		private readonly BonyanDbContext _context;

		public HomeController(BonyanDbContext context)
		{
			_context = context;
		}

        public IActionResult Landing()
        {
            return View();
        }
        public IActionResult Index()
		{
			if (HttpContext.Session.GetString("Email") == null)
				return RedirectToAction("Login", "Account");

			var role = HttpContext.Session.GetString("Role");

			// ── Counts (same for everyone) ────────────────
			ViewBag.EmployeeCount = _context.Employees.Count();
			ViewBag.TaskCount = _context.Tasks.Count();
			ViewBag.InvoiceCount = _context.Invoices.Count();
			ViewBag.DocumentCount = _context.Documents.Count();
			ViewBag.MaterialCount = _context.Materials.Count();
			ViewBag.SupplierCount = _context.Suppliers.Count();
			ViewBag.SiteVisitCount = _context.SiteVisits.Count();

			if (role == "Admin")
			{
				// Admin sees ALL projects
				ViewBag.ProjectCount = _context.Projects.Count();

				ViewBag.ProjectsInProgress = _context.Projects
					.Count(p => p.Status == ProjectStatus.InProgress);
				ViewBag.ProjectsCompleted = _context.Projects
					.Count(p => p.Status == ProjectStatus.Completed);
				ViewBag.ProjectsPlanning = _context.Projects
					.Count(p => p.Status == ProjectStatus.Planning);
				ViewBag.ProjectsOnHold = _context.Projects
					.Count(p => p.Status == ProjectStatus.OnHold);

				ViewBag.RecentProjects = _context.Projects
					.OrderByDescending(p => p.StartDate)
					.Take(5).ToList();
			}
			else
			{
				// Employee sees ONLY assigned projects
				var employeeId = HttpContext.Session.GetInt32("EmployeeId");

				var myProjects = _context.EmployeeProjects
					.Where(ep => ep.EmployeeId == employeeId)
					.Include(ep => ep.Project)
					.Select(ep => ep.Project)
					.ToList();

				ViewBag.ProjectCount = myProjects.Count;

				ViewBag.ProjectsInProgress = myProjects
					.Count(p => p.Status == ProjectStatus.InProgress);
				ViewBag.ProjectsCompleted = myProjects
					.Count(p => p.Status == ProjectStatus.Completed);
				ViewBag.ProjectsPlanning = myProjects
					.Count(p => p.Status == ProjectStatus.Planning);
				ViewBag.ProjectsOnHold = myProjects
					.Count(p => p.Status == ProjectStatus.OnHold);

				ViewBag.RecentProjects = myProjects
					.OrderByDescending(p => p.StartDate)
					.Take(5).ToList();
			}

			ViewBag.RecentTasks = _context.Tasks
				.OrderByDescending(t => t.CreatedAt)
				.Take(5).ToList();

			ViewBag.RecentEmployees = _context.Employees
				.OrderByDescending(e => e.HireDate)
				.Take(5).ToList();

			ViewBag.IsAdmin = role == "Admin";

			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new BonyanForEngineeringConsultingFirms.Models.ErrorViewModel
			{
				RequestId = System.Diagnostics.Activity.Current?.Id
							?? HttpContext.TraceIdentifier
			});
		}
	}
}