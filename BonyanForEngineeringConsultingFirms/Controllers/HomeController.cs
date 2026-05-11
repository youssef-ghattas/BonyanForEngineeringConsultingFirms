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

		public IActionResult Index()
		{
			// redirect to login if not logged in
			if (HttpContext.Session.GetString("Email") == null)
				return RedirectToAction("Login", "Account");

			// ── Counts ────────────────────────────────────
			ViewBag.EmployeeCount = _context.Employees.Count();
			ViewBag.ProjectCount = _context.Projects.Count();
			ViewBag.TaskCount = _context.Tasks.Count();
			ViewBag.InvoiceCount = _context.Invoices.Count();
			ViewBag.DocumentCount = _context.Documents.Count();
			ViewBag.MaterialCount = _context.Materials.Count();
			ViewBag.SupplierCount = _context.Suppliers.Count();
			ViewBag.SiteVisitCount = _context.SiteVisits.Count();

			// ── Project Status Breakdown ──────────────────
			ViewBag.ProjectsInProgress = _context.Projects
				.Count(p => p.Status == ProjectStatus.InProgress);
			ViewBag.ProjectsCompleted = _context.Projects
				.Count(p => p.Status == ProjectStatus.Completed);
			ViewBag.ProjectsPlanning = _context.Projects
				.Count(p => p.Status == ProjectStatus.Planning);
			ViewBag.ProjectsOnHold = _context.Projects
				.Count(p => p.Status == ProjectStatus.OnHold);

			// ── Recent Data ───────────────────────────────
			ViewBag.RecentProjects = _context.Projects
				.OrderByDescending(p => p.StartDate)
				.Take(5)
				.ToList();

			ViewBag.RecentTasks = _context.Tasks
				.OrderByDescending(t => t.CreatedAt)
				.Take(5)
				.ToList();

			ViewBag.RecentEmployees = _context.Employees
				.OrderByDescending(e => e.HireDate)
				.Take(5)
				.ToList();

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