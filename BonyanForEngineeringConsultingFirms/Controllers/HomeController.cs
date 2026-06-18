using Bonyan.DAL.Context;
using Bonyan.DAL.Enums;
using BonyanForEngineeringConsultingFirms.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace BonyanForEngineeringConsultingFirms.Controllers
{
	public class HomeController : Controller
	{
		private readonly BonyanDbContext _context;

		public HomeController(BonyanDbContext context)
		{
			_context = context;
		}

public async Task<IActionResult> Landing()
    {
        // ── Global system-wide counts ─────────────────────────────────────────────
        int totalProjects = await _context.Projects.CountAsync();
        int totalOffices = await _context.Admins.CountAsync();   // each Admin = one subscribed office
        int completedProjects = await _context.Projects
                                    .CountAsync(p => p.Status == ProjectStatus.Completed);
        int totalDocuments = await _context.Documents.CountAsync()
                             + await _context.Drawings.CountAsync(); // combined doc + drawing count

        // ── Status breakdown for progress bars ───────────────────────────────────
        int planningCount = await _context.Projects.CountAsync(p => p.Status == ProjectStatus.Planning);
        int inProgressCount = await _context.Projects.CountAsync(p => p.Status == ProjectStatus.InProgress);
        int onHoldCount = await _context.Projects.CountAsync(p => p.Status == ProjectStatus.OnHold);

        // ── Calculate percentages (guard against divide-by-zero) ─────────────────
        double planningPct = totalProjects > 0 ? Math.Round((double)planningCount / totalProjects * 100, 1) : 0;
        double inProgressPct = totalProjects > 0 ? Math.Round((double)inProgressCount / totalProjects * 100, 1) : 0;
        double completedPct = totalProjects > 0 ? Math.Round((double)completedProjects / totalProjects * 100, 1) : 0;
        double onHoldPct = totalProjects > 0 ? Math.Round((double)onHoldCount / totalProjects * 100, 1) : 0;

        var vm = new LandingStatsViewModel
        {
            TotalSubscribedOffices = totalOffices,
            TotalCompletedProjects = completedProjects,
            TotalProjects = totalProjects,
            TotalDocuments = totalDocuments,
            PlanningPercent = planningPct,
            InProgressPercent = inProgressPct,
            CompletedPercent = completedPct,
            OnHoldPercent = onHoldPct,
            ProjectsPlanning = planningCount,
            ProjectsInProgress = inProgressCount,
            ProjectsOnHold = onHoldCount
        };

        return View(vm);
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