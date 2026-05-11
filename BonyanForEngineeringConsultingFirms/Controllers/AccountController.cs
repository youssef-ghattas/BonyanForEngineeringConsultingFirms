using Bonyan.DAL.Context;
using Bonyan.DAL.Enums;
using Bonyan.DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BonyanForEngineeringConsultingFirms.Controllers
{
	public class AccountController : Controller
	{
		private readonly BonyanDbContext _context;

		public AccountController(BonyanDbContext context)
		{
			_context = context;
		}

		// ── Login GET ─────────────────────────────────
		public IActionResult Login()
		{
			if (HttpContext.Session.GetString("Email") != null)
				return RedirectToAction("Index", "Home");
			return View();
		}

		// ── Login POST ────────────────────────────────
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Login(string email, string password)
		{
			var user = _context.UserAccounts
				.Include(u => u.Employee)
				.FirstOrDefault(u => u.Employee.Email == email
								  && u.Password == password);

			if (user == null)
			{
				ViewBag.Error = "البريد الإلكتروني أو كلمة المرور غير صحيحة";
				return View();
			}

			HttpContext.Session.SetString("Email", user.Employee.Email);
			HttpContext.Session.SetString("Role", user.Role.ToString());
			HttpContext.Session.SetString("FullName",
				user.Employee.FirstName + " " + user.Employee.LastName);
			HttpContext.Session.SetInt32("UserId", user.UserId);

			return RedirectToAction("Index", "Home");
		}

		// ── Register GET ──────────────────────────────
		public IActionResult Register()
		{
			return View();
		}

		// ── Register POST ─────────────────────────────
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Register(
			string firstName, string lastName,
			string email, string phoneNum,
			string ssn, string password,
			Gender gender, Specialization specialization,
			decimal salary)
		{
			// Check if email already exists
			if (_context.Employees.Any(e => e.Email == email))
			{
				ViewBag.Error = "البريد الإلكتروني مسجل مسبقاً";
				return View();
			}

			// 1. Create Employee first
			var employee = new Employee
			{
				FirstName = firstName,
				LastName = lastName,
				Email = email,
				PhoneNum = phoneNum,
				SSN = ssn,
				Gender = gender,
				Specialization = specialization,
				Salary = salary,
				HireDate = DateTime.Now
			};

			_context.Employees.Add(employee);
			_context.SaveChanges(); // Save to get EmployeeId

			// 2. Create UserAccount linked to Employee
			var user = new UserAccount
			{
				EmployeeId = employee.EmployeeId,
				Username = email,            // use email as username internally
				Password = password,         // plain text for now
				Role = UserRole.Engineer // default role
			};

			_context.UserAccounts.Add(user);
			_context.SaveChanges();

			return RedirectToAction("Login");
		}

		// ── Logout ────────────────────────────────────
		public IActionResult Logout()
		{
			HttpContext.Session.Clear();
			return RedirectToAction("Login");
		}
	}
}