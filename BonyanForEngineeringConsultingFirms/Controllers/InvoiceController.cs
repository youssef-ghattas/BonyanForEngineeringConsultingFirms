using Bonyan.DAL.Context;
using Bonyan.DAL.Enums;
using Bonyan.DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BonyanForEngineeringConsultingFirms.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly BonyanDbContext _context;

        public InvoiceController(BonyanDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? projectId)
        {
            var role = HttpContext.Session.GetString("Role");
            var employeeId = HttpContext.Session.GetInt32("EmployeeId");
            if (role == null) return RedirectToAction("Login", "Account");

            IQueryable<Invoice> query = _context.Invoices.Include(i => i.Project);

            if (role != "Admin" && employeeId != null)
                query = query.Where(i => i.Project.EmployeeProjects
                                          .Any(ep => ep.EmployeeId == employeeId));

            if (projectId.HasValue)
                query = query.Where(i => i.ProjectId == projectId.Value);

            var invoices = await query.OrderByDescending(i => i.Invoice_Date).ToListAsync();

            var projectsQuery = role == "Admin"
                ? _context.Projects.OrderBy(p => p.ProjectName)
                : _context.Projects
                    .Where(p => p.EmployeeProjects.Any(ep => ep.EmployeeId == employeeId))
                    .OrderBy(p => p.ProjectName);

            ViewBag.Projects = await projectsQuery.ToListAsync();
            ViewBag.SelectedProjectId = projectId;
            return View(invoices);
        }

        public async Task<IActionResult> Details(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Project)
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.Invoice_ID == id);
            if (invoice == null) return NotFound();
            return View(invoice);
        }

        // ProjectManager and Engineer can create invoices
        public IActionResult Create(int projectId)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role == "Admin") return Forbid(); // Admin cannot add invoices

            var employeeId = HttpContext.Session.GetInt32("EmployeeId");

            // Make sure they belong to this project
            bool assigned = _context.EmployeeProjects
                .Any(ep => ep.ProjectId == projectId && ep.EmployeeId == employeeId);
            if (!assigned && role != "Admin") return Forbid();

            ViewBag.ProjectId = projectId;
            ViewBag.ProjectName = _context.Projects.Find(projectId)?.ProjectName ?? "—";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Invoice invoice)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role == "Admin") return Forbid();

            ModelState.Remove("Project");
            ModelState.Remove("Task");
            ModelState.Remove("Payments");

            if (!ModelState.IsValid)
            {
                ViewBag.ProjectId = invoice.ProjectId;
                ViewBag.ProjectName = _context.Projects.Find(invoice.ProjectId)?.ProjectName ?? "—";
                return View(invoice);
            }

			invoice.Invoice_Date = DateTime.Now;
			CalculateInvoicePayment(invoice);
			_context.Invoices.Add(invoice);
			await _context.SaveChangesAsync();

			TempData["SuccessMessageKey"] = "msg_invoice_created";
            return RedirectToAction("Details", "Project", new { id = invoice.ProjectId });
        }

        // Only Admin can edit/delete
        public async Task<IActionResult> Edit(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return Forbid();

            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null) return NotFound();

            ViewBag.ProjectName = _context.Projects.Find(invoice.ProjectId)?.ProjectName ?? "—";
            return View(invoice);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Invoice invoice)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return Forbid();

            ModelState.Remove("Project");
            ModelState.Remove("Task");
            ModelState.Remove("Payments");

            if (!ModelState.IsValid)
            {
                ViewBag.ProjectName = _context.Projects.Find(invoice.ProjectId)?.ProjectName ?? "—";
                return View(invoice);
            }

			CalculateInvoicePayment(invoice);
			_context.Invoices.Update(invoice);
			await _context.SaveChangesAsync();

			TempData["SuccessMessageKey"] = "msg_invoice_updated";
            return RedirectToAction("Details", "Project", new { id = invoice.ProjectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return Forbid();

            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null) return NotFound();

            int projectId = invoice.ProjectId;
            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();

            TempData["SuccessMessageKey"] = "msg_invoice_deleted";
            return RedirectToAction("Details", "Project", new { id = projectId });
        }
		private void CalculateInvoicePayment(Invoice invoice)
		{
			decimal total = invoice.Total_Amount ?? 0;
			decimal discAmt = total * (invoice.Discount ?? 0) / 100m;
			decimal discountedAmount = total - discAmt;
			decimal taxAmt = discountedAmount * (invoice.Tax ?? 0) / 100m;
			decimal finalAmount = discountedAmount + taxAmt;

			if (invoice.Invoice_Status == InvoiceStatus.PartiallyPaid)
			{
				var paid = invoice.Paid_Amount ?? 0;
				if (paid > finalAmount) paid = finalAmount;
				if (paid < 0) paid = 0;
				invoice.Paid_Amount = paid;
				invoice.RemainingAmount = finalAmount - paid;
			}
			else if (invoice.Invoice_Status == InvoiceStatus.Paid)
			{
				invoice.Paid_Amount = finalAmount;
				invoice.RemainingAmount = 0;
			}
			else // Unpaid
			{
				invoice.Paid_Amount = 0;
				invoice.RemainingAmount = finalAmount;
			}
		}
	}
}