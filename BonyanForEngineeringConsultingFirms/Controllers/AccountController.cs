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

		// ── Secret code lives here, changes every time app restarts ──
		// In production you'd store this in a database or config
		private static string _adminSecretCode = GenerateSecretCode();

		private static string GenerateSecretCode()
		{
			const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
			var random = new Random();
			return new string(Enumerable.Range(0, 8)
				.Select(_ => chars[random.Next(chars.Length)])
				.ToArray());
		}

		public AccountController(BonyanDbContext context)
		{
			_context = context;

			// ── Print secret code to Output window on startup ──
			System.Diagnostics.Debug.WriteLine("=================================");
			System.Diagnostics.Debug.WriteLine($"ADMIN SECRET CODE: {_adminSecretCode}");
			System.Diagnostics.Debug.WriteLine("=================================");
		}

		// ════════════════════════════════════════════════
		//  EMPLOYEE LOGIN
		// ════════════════════════════════════════════════

		public IActionResult Login()
		{
			if (HttpContext.Session.GetString("Email") != null)
				return RedirectToAction("Index", "Home");
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Login(string email, string password)
		{
			// Check employee login
			var user = _context.UserAccounts
				.Include(u => u.Employee)
				.FirstOrDefault(u => u.Employee.Email == email
								  && u.Password == password);

			if (user != null)
			{
				HttpContext.Session.SetString("Email", user.Employee.Email);
				HttpContext.Session.SetString("Role", "Employee");
				HttpContext.Session.SetString("FullName",
					user.Employee.FirstName + " " + user.Employee.LastName);
				HttpContext.Session.SetInt32("UserId", user.UserId);
				HttpContext.Session.SetInt32("EmployeeId", user.EmployeeId);
				return RedirectToAction("Index", "Home");
			}

			// Check admin login
			var admin = _context.AdminAccounts
				.Include(a => a.Admin)
				.FirstOrDefault(a => a.Admin.Email == email
								  && a.Password == password);

			if (admin != null)
			{
				HttpContext.Session.SetString("Email", admin.Admin.Email);
				HttpContext.Session.SetString("Role", "Admin");
				HttpContext.Session.SetString("FullName",
					admin.Admin.FirstName + " " + admin.Admin.LastName);
				HttpContext.Session.SetInt32("AdminId", admin.AdminId);
				return RedirectToAction("Index", "Home");
			}

			ViewBag.Error = "البريد الإلكتروني أو كلمة المرور غير صحيحة";
			return View();
		}

		// ════════════════════════════════════════════════
		//  EMPLOYEE REGISTER (kept from before)
		// ════════════════════════════════════════════════

		public IActionResult Register()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Register(
			string firstName, string lastName,
			string email, string phoneNum,
			string ssn, string password,
			Gender gender, Specialization specialization,
			decimal salary, UserRole role)
		{
			if (_context.Employees.Any(e => e.Email == email))
			{
				ViewBag.Error = "البريد الإلكتروني مسجل مسبقاً";
				return View();
			}

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
			_context.SaveChanges();

			var userAccount = new UserAccount
			{
				EmployeeId = employee.EmployeeId,
				Password = password,
				Role = role
			};

			_context.UserAccounts.Add(userAccount);
			_context.SaveChanges();

			TempData["Success"] = "تم إنشاء الحساب بنجاح، قم بتسجيل الدخول";
			return RedirectToAction("Login");
		}

		// ════════════════════════════════════════════════
		//  ADMIN REGISTER
		// ════════════════════════════════════════════════

		public IActionResult AdminRegister()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult AdminRegister(
			string firstName, string lastName,
			string email, string phoneNum,
			string password, string secretCode)
		{
			// ── Check Secret Code ─────────────────────
			if (secretCode != _adminSecretCode)
			{
				ViewBag.Error = "الكود السري غير صحيح";
				return View();
			}

			// ── Check email not already used ──────────
			if (_context.Admins.Any(a => a.Email == email))
			{
				ViewBag.Error = "البريد الإلكتروني مسجل مسبقاً";
				return View();
			}

			// ── Create Admin ──────────────────────────
			var admin = new Admin
			{
				FirstName = firstName,
				LastName = lastName,
				Email = email,
				PhoneNum = phoneNum,
				CreatedAt = DateTime.Now
			};

			_context.Admins.Add(admin);
			_context.SaveChanges();

			var adminAccount = new AdminAccount
			{
				AdminId = admin.AdminId,
				Password = password,
				CreatedAt = DateTime.Now
			};

			_context.AdminAccounts.Add(adminAccount);
			_context.SaveChanges();

			// ── Regenerate secret code after use ──────
			_adminSecretCode = GenerateSecretCode();

			TempData["Success"] = "تم إنشاء حساب الأدمن بنجاح";
			return RedirectToAction("Login");
		}

		// ── Show current secret code (protect this in production!) ──
		// Only call this from Visual Studio or a protected admin page
		public IActionResult GetSecretCode()
		{
			// Remove this action or protect it before deploying!
			return Content($"Current Admin Secret Code: {_adminSecretCode}");
		}

		// ════════════════════════════════════════════════
		//  LOGOUT
		// ════════════════════════════════════════════════

		public IActionResult Logout()
		{
			HttpContext.Session.Clear();
			return RedirectToAction("Login");
		}
	}
}