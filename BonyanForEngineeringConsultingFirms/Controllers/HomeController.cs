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

            if (role == "Admin")
            {
                ViewBag.EmployeeCount = _context.Employees.Count();
                ViewBag.TaskCount = _context.Tasks.Count();
                ViewBag.InvoiceCount = _context.Invoices.Count();
                ViewBag.DocumentCount = _context.Documents.Count();
                ViewBag.MaterialCount = _context.Materials.Count();
                ViewBag.SupplierCount = _context.Suppliers.Count();
                ViewBag.SiteVisitCount = _context.SiteVisits.Count();

                ViewBag.ProjectCount = _context.Projects.Count();
                ViewBag.ProjectsInProgress = _context.Projects.Count(p => p.Status == ProjectStatus.InProgress);
                ViewBag.ProjectsCompleted = _context.Projects.Count(p => p.Status == ProjectStatus.Completed);
                ViewBag.ProjectsPlanning = _context.Projects.Count(p => p.Status == ProjectStatus.Planning);
                ViewBag.ProjectsOnHold = _context.Projects.Count(p => p.Status == ProjectStatus.OnHold);
                ViewBag.RecentProjects = _context.Projects.OrderByDescending(p => p.StartDate).Take(5).ToList();

                ViewBag.RecentTasks = _context.Tasks.OrderByDescending(t => t.CreatedAt).Take(5).ToList();
                ViewBag.RecentEmployees = _context.Employees.OrderByDescending(e => e.HireDate).Take(5).ToList();
            }
            else
            {
                var myProjectIds = _context.EmployeeProjects
                    .Where(ep => ep.EmployeeId == employeeId)
                    .Select(ep => ep.ProjectId)
                    .ToList();

                var myProjects = _context.Projects
                    .Where(p => myProjectIds.Contains(p.ProjectId))
                    .ToList();

                ViewBag.ProjectCount = myProjects.Count;
                ViewBag.ProjectsInProgress = myProjects.Count(p => p.Status == ProjectStatus.InProgress);
                ViewBag.ProjectsCompleted = myProjects.Count(p => p.Status == ProjectStatus.Completed);
                ViewBag.ProjectsPlanning = myProjects.Count(p => p.Status == ProjectStatus.Planning);
                ViewBag.ProjectsOnHold = myProjects.Count(p => p.Status == ProjectStatus.OnHold);
                ViewBag.RecentProjects = myProjects.OrderByDescending(p => p.StartDate).Take(5).ToList();

                ViewBag.TaskCount = _context.Tasks.Count(t => myProjectIds.Contains(t.ProjectId));
                ViewBag.InvoiceCount = _context.Invoices.Count(i => myProjectIds.Contains(i.ProjectId));
                ViewBag.DocumentCount = _context.Documents.Count(d => myProjectIds.Contains(d.ProjectId));
                ViewBag.SiteVisitCount = _context.SiteVisits.Count(s => myProjectIds.Contains(s.ProjId));

                ViewBag.EmployeeCount = 0;
                ViewBag.MaterialCount = 0;
                ViewBag.SupplierCount = 0;

                ViewBag.RecentTasks = _context.Tasks
                    .Where(t => myProjectIds.Contains(t.ProjectId))
                    .OrderByDescending(t => t.CreatedAt).Take(5).ToList();
                ViewBag.RecentEmployees = new List<Bonyan.DAL.Models.Employee>();
            }

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

        // ════════════════════════════════════════════════
        //  NAVBAR NOTIFICATIONS  (real system data)
        //  Surfaces: overdue tasks, overdue unpaid invoices,
        //  and critical site-visit safety alerts.
        // ════════════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            if (HttpContext.Session.GetString("Email") == null)
                return Json(new List<object>());

            var role = HttpContext.Session.GetString("Role");
            var employeeId = HttpContext.Session.GetInt32("EmployeeId");
            var now = DateTime.Now;

            IQueryable<Bonyan.DAL.Models.Task> taskQ = _context.Tasks.AsNoTracking();
            IQueryable<Bonyan.DAL.Models.Invoice> invoiceQ = _context.Invoices.AsNoTracking();
            IQueryable<Bonyan.DAL.Models.SiteVisit> visitQ = _context.SiteVisits.AsNoTracking();

            if (role != "Admin" && employeeId.HasValue)
            {
                var myProjectIds = _context.EmployeeProjects
                    .AsNoTracking()
                    .Where(ep => ep.EmployeeId == employeeId.Value)
                    .Select(ep => ep.ProjectId);

                taskQ = taskQ.Where(t => myProjectIds.Contains(t.ProjectId));
                invoiceQ = invoiceQ.Where(i => myProjectIds.Contains(i.ProjectId));
                visitQ = visitQ.Where(v => myProjectIds.Contains(v.ProjId));
            }

            string TimeAgo(DateTime dt)
            {
                var span = now - dt;
                if (span.TotalDays >= 1) return $"منذ {(int)span.TotalDays} يوم";
                if (span.TotalHours >= 1) return $"منذ {(int)span.TotalHours} ساعة";
                if (span.TotalMinutes >= 1) return $"منذ {(int)span.TotalMinutes} دقيقة";
                return "الآن";
            }

            var overdueTasks = await taskQ
                .Where(t => t.DueDate != null && t.DueDate < now
                         && t.Status != TasksStatus.Completed
                         && t.Status != TasksStatus.Cancelled)
                .OrderBy(t => t.DueDate)
                .Take(5)
                .Select(t => new
                {
                    id = "task-" + t.TaskId,
                    type = "danger",
                    title = "مهمة متأخرة: " + t.Task_Name,
                    sub = t.Project != null ? t.Project.ProjectName : "—",
                    due = t.DueDate,
                    url = Url.Action("Details", "Task", new { id = t.TaskId })
                })
                .ToListAsync();

            var overdueInvoices = await invoiceQ
                .Where(i => i.Due_Date != null && i.Due_Date < now
                         && i.Invoice_Status == InvoiceStatus.Unpaid)
                .OrderBy(i => i.Due_Date)
                .Take(5)
                .Select(i => new
                {
                    id = "invoice-" + i.Invoice_ID,
                    type = "warning",
                    title = "فاتورة متأخرة السداد",
                    sub = i.Project != null ? i.Project.ProjectName : "—",
                    due = i.Due_Date,
                    url = Url.Action("Details", "Invoice", new { id = i.Invoice_ID })
                })
                .ToListAsync();

            var criticalVisits = await visitQ
                .Where(v => v.SafetyStatus == SafetyStatus.Critical)
                .OrderByDescending(v => v.VisitDate)
                .Take(3)
                .Select(v => new
                {
                    id = "visit-" + v.VisitId,
                    type = "danger",
                    title = "تنبيه سلامة حرج في زيارة موقع",
                    sub = v.Project != null ? v.Project.ProjectName : "—",
                    due = (DateTime?)v.VisitDate,
                    url = Url.Action("Details", "SiteVisit", new { id = v.VisitId })
                })
                .ToListAsync();

            var all = overdueTasks
                .Concat(overdueInvoices)
                .Concat(criticalVisits)
                .Select(n => new
                {
                    n.id,
                    n.type,
                    n.title,
                    n.sub,
                    time = n.due.HasValue ? TimeAgo(n.due.Value) : "الآن",
                    n.url,
                    isRead = false
                })
                .OrderByDescending(n => n.type == "danger")
                .Take(12)
                .ToList();

            return Json(all);
        }

        // ════════════════════════════════════════════════
        //  ABOUT PAGE
        // ════════════════════════════════════════════════
        public IActionResult About()
        {
            return View();
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