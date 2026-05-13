using Bonyan.DAL.Context;
using Bonyan.DAL.Enums;
using Bonyan.DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BonyanForEngineeringConsultingFirms.Controllers
{
	public class EmployeeController : Controller
	{
		private readonly BonyanDbContext _context;

		public EmployeeController(BonyanDbContext context)
		{
			_context = context;
		}

		// ── Index ─────────────────────────────────────
		public IActionResult Index()
		{
			// redirect if not logged in
			if (HttpContext.Session.GetString("Email") == null)
				return RedirectToAction("Login", "Account");

			var employees = _context.Employees
				.Include(e => e.UserAccount)
				.ToList();

			return View(employees);
		}

		// ── Details ───────────────────────────────────
		public IActionResult Details(int id)
		{
			var employee = _context.Employees
				.Include(e => e.UserAccount)
				.Include(e => e.EmployeeProjects)
					.ThenInclude(ep => ep.Project)
				.FirstOrDefault(e => e.EmployeeId == id);

			if (employee == null) return NotFound();
			return View(employee);
		}

		// ── Create GET ────────────────────────────────
		public IActionResult Create()
		{
			// only admin can add employees
			if (HttpContext.Session.GetString("Role") != "Admin")
				return RedirectToAction("Index", "Home");

			return View();
		}

		// ── Create POST ───────────────────────────────
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Create(
			Employee employee,
			string Password,
			UserRole Role)
		{
			// only admin can add employees
			if (HttpContext.Session.GetString("Role") != "Admin")
				return RedirectToAction("Index", "Home");

			// remove UserAccount from ModelState
			// because we handle it manually
			ModelState.Remove("UserAccount");
			ModelState.Remove("Password");

			// check email not already used
			if (_context.Employees.Any(e => e.Email == employee.Email))
			{
				ModelState.AddModelError("Email",
					"البريد الإلكتروني مسجل مسبقاً");
				return View(employee);
			}

			// check SSN not already used
			if (_context.Employees.Any(e => e.SSN == employee.SSN))
			{
				ModelState.AddModelError("SSN",
					"الرقم القومي مسجل مسبقاً");
				return View(employee);
			}

			if (!ModelState.IsValid)
				return View(employee);

			// ── Step 1: Save Employee ─────────────────
			_context.Employees.Add(employee);
			_context.SaveChanges();

			// ── Step 2: Create UserAccount ────────────
			var userAccount = new UserAccount
			{
				EmployeeId = employee.EmployeeId,
				Password = Password,
				Role = Role,
				CreatedAt = DateTime.Now
			};

			_context.UserAccounts.Add(userAccount);
			_context.SaveChanges();

			TempData["Success"] = "تم إضافة الموظف بنجاح";
			return RedirectToAction(nameof(Index));
		}

		// ── Edit GET ──────────────────────────────────
		public IActionResult Edit(int id)
		{
			if (HttpContext.Session.GetString("Role") != "Admin")
				return RedirectToAction("Index", "Home");

			var employee = _context.Employees
				.Include(e => e.UserAccount)
				.FirstOrDefault(e => e.EmployeeId == id);

			if (employee == null) return NotFound();

			// pass current role to view
			ViewBag.CurrentRole = employee.UserAccount?.Role;

			return View(employee);
		}

		// ── Edit POST ─────────────────────────────────
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Edit(
			Employee employee,
			UserRole Role,
			string NewPassword)
		{
			if (HttpContext.Session.GetString("Role") != "Admin")
				return RedirectToAction("Index", "Home");

			ModelState.Remove("UserAccount");
			ModelState.Remove("NewPassword");

			// check email not used by another employee
			if (_context.Employees.Any(e =>
				e.Email == employee.Email &&
				e.EmployeeId != employee.EmployeeId))
			{
				ModelState.AddModelError("Email",
					"البريد الإلكتروني مسجل مسبقاً");
				ViewBag.CurrentRole = Role;
				return View(employee);
			}

			if (!ModelState.IsValid)
			{
				ViewBag.CurrentRole = Role;
				return View(employee);
			}

			// ── Update Employee ───────────────────────
			var existing = _context.Employees
				.Include(e => e.UserAccount)
				.FirstOrDefault(e => e.EmployeeId == employee.EmployeeId);

			if (existing == null) return NotFound();

			existing.FirstName = employee.FirstName;
			existing.LastName = employee.LastName;
			existing.Email = employee.Email;
			existing.PhoneNum = employee.PhoneNum;
			existing.SSN = employee.SSN;
			existing.Gender = employee.Gender;
			existing.Specialization = employee.Specialization;
			existing.Salary = employee.Salary;
			existing.HireDate = employee.HireDate;

			// ── Update UserAccount ────────────────────
			if (existing.UserAccount != null)
			{
				existing.UserAccount.Role = Role;

				// only update password if admin entered a new one
				if (!string.IsNullOrWhiteSpace(NewPassword))
					existing.UserAccount.Password = NewPassword;
			}

			_context.SaveChanges();

			TempData["Success"] = "تم تعديل بيانات الموظف بنجاح";
			return RedirectToAction(nameof(Index));
		}

		// ── Delete GET ────────────────────────────────
		public IActionResult Delete(int id)
		{
			if (HttpContext.Session.GetString("Role") != "Admin")
				return RedirectToAction("Index", "Home");

			var employee = _context.Employees
				.FirstOrDefault(e => e.EmployeeId == id);

			if (employee == null) return NotFound();
			return View(employee);
		}

		// ── Delete POST ───────────────────────────────
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public IActionResult DeleteConfirmed(int id)
		{
			if (HttpContext.Session.GetString("Role") != "Admin")
				return RedirectToAction("Index", "Home");

			var employee = _context.Employees
				.Include(e => e.UserAccount)
				.Include(e => e.EmployeeProjects)
				.FirstOrDefault(e => e.EmployeeId == id);

			if (employee == null) return NotFound();

			// delete UserAccount first (FK constraint)
			if (employee.UserAccount != null)
				_context.UserAccounts.Remove(employee.UserAccount);

			// delete EmployeeProject assignments
			if (employee.EmployeeProjects.Any())
				_context.EmployeeProjects.RemoveRange(employee.EmployeeProjects);

			_context.Employees.Remove(employee);
			_context.SaveChanges();

			TempData["Success"] = "تم حذف الموظف بنجاح";
			return RedirectToAction(nameof(Index));
		}
	}
}