using Bonyan.DAL.Context;
using Bonyan.DAL.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task = Bonyan.DAL.Models.Task;

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
            // ── Redirect to login if not logged in ────
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            // ── Stat Counts ───────────────────────────
            ViewBag.EmployeeCount = _context.Employees.Count();
            ViewBag.ProjectCount = _context.Projects.Count();
            ViewBag.TaskCount = _context.Tasks.Count();
            ViewBag.DocumentCount = _context.Documents.Count();
            ViewBag.DrawingCount = _context.Drawings.Count();
            ViewBag.InvoiceCount = _context.Invoices.Count();
            ViewBag.MaterialCount = _context.Materials.Count();
            ViewBag.SupplierCount = _context.Suppliers.Count();

            // ── Active Projects ───────────────────────
            ViewBag.ActiveProjects = _context.Projects
                .Where(p => p.Status == ProjectStatus.InProgress)
                .Count();

            // ── Pending Tasks ─────────────────────────
            ViewBag.PendingTasks = _context.Tasks
                .Where(t => t.Status == TasksStatus.Pending)
                .Count();

            // ── Unpaid Invoices ───────────────────────
            ViewBag.UnpaidInvoices = _context.Invoices
                .Where(i => i.Invoice_Status == InvoiceStatus.Unpaid)
                .Count();

            // ── Recent Projects ───────────────────────
            ViewBag.RecentProjects = _context.Projects
                .OrderByDescending(p => p.StartDate)
                .Take(5)
                .ToList();

            // ── Recent Tasks ──────────────────────────
            ViewBag.RecentTasks = _context.Tasks
                .Include(t => t.Project)
                .OrderByDescending(t => t.CreatedAt)
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