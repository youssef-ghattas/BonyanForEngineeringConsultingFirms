// Controllers/HomeController.cs
using Bonyan.DAL.Context;
using Bonyan.DAL.Enums;
using BonyanForEngineeringConsultingFirms.ViewModels;
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

        // ════════════════════════════════════════════════
        //  HOME INDEX  (existing, unchanged)
        // ════════════════════════════════════════════════
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Email") == null)
                return RedirectToAction("Login", "Account");

            var role = HttpContext.Session.GetString("Role");
            var employeeId = HttpContext.Session.GetInt32("EmployeeId");

            ViewBag.EmployeeCount = _context.Employees.Count();
            ViewBag.TaskCount = _context.Tasks.Count();
            ViewBag.InvoiceCount = _context.Invoices.Count();
            ViewBag.DocumentCount = _context.Documents.Count();
            ViewBag.MaterialCount = _context.Materials.Count();
            ViewBag.SupplierCount = _context.Suppliers.Count();
            ViewBag.SiteVisitCount = _context.SiteVisits.Count();

            if (role == "Admin")
            {
                ViewBag.ProjectCount = _context.Projects.Count();
                ViewBag.ProjectsInProgress = _context.Projects.Count(p => p.Status == ProjectStatus.InProgress);
                ViewBag.ProjectsCompleted = _context.Projects.Count(p => p.Status == ProjectStatus.Completed);
                ViewBag.ProjectsPlanning = _context.Projects.Count(p => p.Status == ProjectStatus.Planning);
                ViewBag.ProjectsOnHold = _context.Projects.Count(p => p.Status == ProjectStatus.OnHold);
                ViewBag.RecentProjects = _context.Projects.OrderByDescending(p => p.StartDate).Take(5).ToList();
            }
            else
            {
                var myProjects = _context.EmployeeProjects
                    .Where(ep => ep.EmployeeId == employeeId)
                    .Include(ep => ep.Project)
                    .Select(ep => ep.Project)
                    .ToList();

                ViewBag.ProjectCount = myProjects.Count;
                ViewBag.ProjectsInProgress = myProjects.Count(p => p.Status == ProjectStatus.InProgress);
                ViewBag.ProjectsCompleted = myProjects.Count(p => p.Status == ProjectStatus.Completed);
                ViewBag.ProjectsPlanning = myProjects.Count(p => p.Status == ProjectStatus.Planning);
                ViewBag.ProjectsOnHold = myProjects.Count(p => p.Status == ProjectStatus.OnHold);
                ViewBag.RecentProjects = myProjects.OrderByDescending(p => p.StartDate).Take(5).ToList();
            }

            ViewBag.RecentTasks = _context.Tasks.OrderByDescending(t => t.CreatedAt).Take(5).ToList();
            ViewBag.RecentEmployees = _context.Employees.OrderByDescending(e => e.HireDate).Take(5).ToList();
            ViewBag.IsAdmin = role == "Admin";

            return View();
        }

        // ════════════════════════════════════════════════
        //  DASHBOARD  (new analytics page)
        // ════════════════════════════════════════════════
        public async Task<IActionResult> Dashboard()
        {
            if (HttpContext.Session.GetString("Email") == null)
                return RedirectToAction("Login", "Account");

            var role = HttpContext.Session.GetString("Role");
            var employeeId = HttpContext.Session.GetInt32("EmployeeId");
            var now = DateTime.Now;

            // ── Base queryables (scoped by role) ─────────────────────
            IQueryable<Bonyan.DAL.Models.Project> projectQ = _context.Projects.AsNoTracking();
            IQueryable<Bonyan.DAL.Models.Task> taskQ = _context.Tasks.AsNoTracking();

            if (role != "Admin" && employeeId.HasValue)
            {
                var myProjectIds = _context.EmployeeProjects
                    .AsNoTracking()
                    .Where(ep => ep.EmployeeId == employeeId.Value)
                    .Select(ep => ep.ProjectId);

                projectQ = projectQ.Where(p => myProjectIds.Contains(p.ProjectId));
                taskQ = taskQ.Where(t => myProjectIds.Contains(t.ProjectId));
            }

            // ── 1. Top metric cards ───────────────────────────────────
            var totalProjects = await projectQ.CountAsync();
            var activeTasks = await taskQ.CountAsync(t => t.Status == TasksStatus.InProgress);
            var totalDocs = await _context.Documents.AsNoTracking().CountAsync()
                              + await _context.Drawings.AsNoTracking().CountAsync();
            var totalBudget = await projectQ.SumAsync(p => (decimal?)p.Budget) ?? 0m;

            // Total distinct clients (Client_Name column on Project)
            var totalClients = await projectQ
                .Where(p => p.Client_Name != null && p.Client_Name != "")
                .Select(p => p.Client_Name)
                .Distinct()
                .CountAsync();

            // ── 2. Projects by status (bar chart) ────────────────────
            var projectsByStatus = await projectQ
                .GroupBy(p => p.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            int projPlanning = projectsByStatus.FirstOrDefault(x => x.Status == ProjectStatus.Planning)?.Count ?? 0;
            int projInProgress = projectsByStatus.FirstOrDefault(x => x.Status == ProjectStatus.InProgress)?.Count ?? 0;
            int projOnHold = projectsByStatus.FirstOrDefault(x => x.Status == ProjectStatus.OnHold)?.Count ?? 0;
            int projCompleted = projectsByStatus.FirstOrDefault(x => x.Status == ProjectStatus.Completed)?.Count ?? 0;
            int projCancelled = projectsByStatus.FirstOrDefault(x => x.Status == ProjectStatus.Cancelled)?.Count ?? 0;

            // ── 3. Tasks by status (doughnut chart) ──────────────────
            var tasksByStatus = await taskQ
                .GroupBy(t => t.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            int taskPending = tasksByStatus.FirstOrDefault(x => x.Status == TasksStatus.Pending)?.Count ?? 0;
            int taskInProgress = tasksByStatus.FirstOrDefault(x => x.Status == TasksStatus.InProgress)?.Count ?? 0;
            int taskUnderReview = tasksByStatus.FirstOrDefault(x => x.Status == TasksStatus.UnderReview)?.Count ?? 0;
            int taskCompleted = tasksByStatus.FirstOrDefault(x => x.Status == TasksStatus.Completed)?.Count ?? 0;
            int taskCancelled = tasksByStatus.FirstOrDefault(x => x.Status == TasksStatus.Cancelled)?.Count ?? 0;

            // ── 4. Recent projects table (projection only) ────────────
            var recentProjects = await projectQ
                .OrderByDescending(p => p.StartDate)
                .Take(6)
                .Select(p => new RecentProjectRow
                {
                    ProjectId = p.ProjectId,
                    ProjectName = p.ProjectName,
                    ClientName = p.Client_Name ?? "—",
                    Status = p.Status,
                    TaskCount = p.Tasks.Count()
                })
                .ToListAsync();

            // ── 5. Overdue tasks (projection only) ───────────────────
            var overdueTasks = await taskQ
                .Where(t => t.DueDate != null
                         && t.DueDate < now
                         && t.Status != TasksStatus.Completed
                         && t.Status != TasksStatus.Cancelled)
                .OrderBy(t => t.DueDate)
                .Take(6)
                .Select(t => new OverdueTaskRow
                {
                    TaskId = t.TaskId,
                    TaskName = t.Task_Name,
                    ProjectName = t.Project != null ? t.Project.ProjectName : "—",
                    DueDate = t.DueDate!.Value
                })
                .ToListAsync();

            // ── Build ViewModel ───────────────────────────────────────
            var vm = new DashboardViewModel
            {
                TotalProjects = totalProjects,
                ActiveTasks = activeTasks,
                TotalClients = totalClients,
                TotalDocuments = totalDocs,
                TotalBudget = totalBudget,

                ProjPlanning = projPlanning,
                ProjInProgress = projInProgress,
                ProjOnHold = projOnHold,
                ProjCompleted = projCompleted,
                ProjCancelled = projCancelled,

                TaskPending = taskPending,
                TaskInProgress = taskInProgress,
                TaskUnderReview = taskUnderReview,
                TaskCompleted = taskCompleted,
                TaskCancelled = taskCancelled,

                RecentProjects = recentProjects,
                OverdueTasks = overdueTasks,

                IsAdmin = role == "Admin"
            };

            return View(vm);
        }

        // ── Landing & Error (unchanged) ───────────────────────────────
        public async Task<IActionResult> Landing()
        {
            int totalProjects = await _context.Projects.CountAsync();
            int totalOffices = await _context.Admins.CountAsync();
            int completedProjects = await _context.Projects.CountAsync(p => p.Status == ProjectStatus.Completed);
            int totalDocuments = await _context.Documents.CountAsync() + await _context.Drawings.CountAsync();

            int planningCount = await _context.Projects.CountAsync(p => p.Status == ProjectStatus.Planning);
            int inProgressCount = await _context.Projects.CountAsync(p => p.Status == ProjectStatus.InProgress);
            int onHoldCount = await _context.Projects.CountAsync(p => p.Status == ProjectStatus.OnHold);

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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new BonyanForEngineeringConsultingFirms.Models.ErrorViewModel
            {
                RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}