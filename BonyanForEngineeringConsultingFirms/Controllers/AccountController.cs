using Bonyan.DAL.Context;
using Bonyan.DAL.Enums;
using Bonyan.DAL.Models;
using Bonyan.PL.ViewModels;
using Bonyan.PL.ViewModels;
using BonyanForEngineeringConsultingFirms.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace BonyanForEngineeringConsultingFirms.Controllers
{
	public class AccountController : Controller
	{
		private readonly BonyanDbContext _context;
		private readonly Services.EmailService _emailService;
		private readonly IConfiguration _config;


		public AccountController(BonyanDbContext context,
						 Services.EmailService emailService,
						 IConfiguration config)
		{
			_context = context;
			_emailService = emailService;
			_config = config;
		}

		// ════════════════════════════════════════════════
		//  LOGIN
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
			var hashedPassword = PasswordHelper.HashMD5(password);

			// Check employee login
			var user = _context.UserAccounts
				.Include(u => u.Employee)
				.FirstOrDefault(u => u.Employee.Email == email
								  && u.Password == hashedPassword);

			if (user != null)
			{
				HttpContext.Session.SetString("Email", user.Employee.Email);
				HttpContext.Session.SetString("Role", "Employee");
				HttpContext.Session.SetString("FullName",
					user.Employee.FirstName + " " + user.Employee.LastName);
				HttpContext.Session.SetInt32("UserId", user.UserId);
				HttpContext.Session.SetInt32("EmployeeId", user.EmployeeId);

				// First login → force password change
				if (user.IsFirstLogin)
					return RedirectToAction("ChangePassword");

				return RedirectToAction("Index", "Home");
			}

			// Check admin login
			var admin = _context.AdminAccounts
				.Include(a => a.Admin)
				.FirstOrDefault(a => a.Admin.Email == email
								  && a.Password == hashedPassword);

			if (admin != null)
			{
				HttpContext.Session.SetString("Email", admin.Admin.Email);
				HttpContext.Session.SetString("Role", "Admin");
				HttpContext.Session.SetString("FullName",
					admin.Admin.FirstName + " " + admin.Admin.LastName);
				HttpContext.Session.SetInt32("AdminId", admin.AdminId);

				// First login → force password change
				if (admin.IsFirstLogin)
					return RedirectToAction("ChangePassword");

				return RedirectToAction("Index", "Home");
			}

			ViewBag.Error = "البريد الإلكتروني أو كلمة المرور غير صحيحة";
			return View();
		}

		// ════════════════════════════════════════════════
		//  CHANGE PASSWORD (forced on first login)
		// ════════════════════════════════════════════════

		public IActionResult ChangePassword()
		{
			if (HttpContext.Session.GetString("Email") == null)
				return RedirectToAction("Login");
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult ChangePassword(ChangePasswordViewModel model)
		{
			if (!ModelState.IsValid)
				return View(model);

			var email = HttpContext.Session.GetString("Email");
			var role = HttpContext.Session.GetString("Role");
			var hashedCurrent = PasswordHelper.HashMD5(model.CurrentPassword);
			var hashedNew = PasswordHelper.HashMD5(model.NewPassword);

			if (role == "Admin")
			{
				var adminAccount = _context.AdminAccounts
					.Include(a => a.Admin)
					.FirstOrDefault(a => a.Admin.Email == email);

				if (adminAccount == null || adminAccount.Password != hashedCurrent)
				{
					ModelState.AddModelError("CurrentPassword", "كلمة المرور الحالية غير صحيحة");
					return View(model);
				}

				adminAccount.Password = hashedNew;
				adminAccount.IsFirstLogin = false;
				_context.SaveChanges();
			}
			else // Employee
			{
				var userAccount = _context.UserAccounts
					.Include(u => u.Employee)
					.FirstOrDefault(u => u.Employee.Email == email);

				if (userAccount == null || userAccount.Password != hashedCurrent)
				{
					ModelState.AddModelError("CurrentPassword", "كلمة المرور الحالية غير صحيحة");
					return View(model);
				}

				userAccount.Password = hashedNew;
				userAccount.IsFirstLogin = false;
				_context.SaveChanges();
			}

			TempData["Success"] = "تم تغيير كلمة المرور بنجاح";
			return RedirectToAction("Index", "Home");
		}

		// ════════════════════════════════════════════════
		//  ADMIN REGISTER (called from Employee page button)
		// ════════════════════════════════════════════════

		public IActionResult AdminRegister()
		{
			// Only logged-in admins can create another admin
			if (HttpContext.Session.GetString("Role") != "Admin")
				return RedirectToAction("Login");
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult AdminRegister(
			string firstName, string lastName,
			string email, string phoneNum,
			string password)
		{
			if (HttpContext.Session.GetString("Role") != "Admin")
				return RedirectToAction("Login");

			if (_context.Admins.Any(a => a.Email == email))
			{
				ViewBag.Error = "البريد الإلكتروني مسجل مسبقاً";
				return View();
			}

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
				Password = PasswordHelper.HashMD5(password),
				CreatedAt = DateTime.Now,
				IsFirstLogin = true   // new admin must change password on first login
			};

			_context.AdminAccounts.Add(adminAccount);
			_context.SaveChanges();

			TempData["Success"] = "تم إنشاء حساب الأدمن بنجاح";
			return RedirectToAction("Index", "Employee");
		}

		// ════════════════════════════════════════════════
		//  LOGOUT
		// ════════════════════════════════════════════════

		public IActionResult Logout()
		{
			HttpContext.Session.Clear();
			return RedirectToAction("Login");
		}
		// ════════════════════════════════════════════════
		//  FORGOT PASSWORD — user enters their email
		// ════════════════════════════════════════════════

		public IActionResult ForgotPassword()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult ForgotPassword(Bonyan.PL.ViewModels.ForgotPasswordViewModel model)
		{
			if (!ModelState.IsValid)
				return View(model);

			// Check if email belongs to an employee
			string userFullName = null;
			string userEmail = null;

			var employee = _context.Employees
				.FirstOrDefault(e => e.Email == model.Email);

			if (employee != null)
			{
				userFullName = employee.FirstName + " " + employee.LastName;
				userEmail = employee.Email;
			}
			else
			{
				// Check if email belongs to an admin
				var admin = _context.Admins
					.FirstOrDefault(a => a.Email == model.Email);

				if (admin != null)
				{
					userFullName = admin.FirstName + " " + admin.LastName;
					userEmail = admin.Email;
				}
			}

			if (userFullName == null)
			{
				ModelState.AddModelError("Email", "هذا البريد الإلكتروني غير مسجل في النظام");
				return View(model);
			}

			// Send notification email to admin
			var adminEmail = _config["EmailSettings:AdminEmail"];
			var resetLink = Url.Action("ResetPassword", "Account",
								new { email = userEmail },
								Request.Scheme);

			var adminBody = Services.EmailTemplates.ForgotPasswordAdminNotification(
	userFullName, userEmail, resetLink);

			_emailService.SendEmail(adminEmail, "المدير", "طلب استعادة كلمة مرور", adminBody);

			TempData["Success"] = "تم إرسال طلبك إلى المدير. ستصلك كلمة المرور الجديدة على بريدك الإلكتروني قريباً.";
			return RedirectToAction("Login");
		}

		// ════════════════════════════════════════════════
		//  RESET PASSWORD — admin resets user's password
		// ════════════════════════════════════════════════

		public IActionResult ResetPassword(string email)
		{
			// Only logged-in admins can access this
			if (HttpContext.Session.GetString("Role") != "Admin")
				return RedirectToAction("Login");

			if (string.IsNullOrEmpty(email))
				return RedirectToAction("Index", "Employee");

			var model = new Bonyan.PL.ViewModels.ResetPasswordViewModel { Email = email };
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult ResetPassword(Bonyan.PL.ViewModels.ResetPasswordViewModel model)
		{
			if (HttpContext.Session.GetString("Role") != "Admin")
				return RedirectToAction("Login");

			if (!ModelState.IsValid)
				return View(model);

			var hashedNew = Helpers.PasswordHelper.HashMD5(model.NewPassword);
			string targetName = null;
			string targetEmail = model.Email;

			// Try employee first
			var userAccount = _context.UserAccounts
				.Include(u => u.Employee)
				.FirstOrDefault(u => u.Employee.Email == model.Email);

			if (userAccount != null)
			{
				targetName = userAccount.Employee.FirstName + " " + userAccount.Employee.LastName;
				userAccount.Password = hashedNew;
				userAccount.IsFirstLogin = true;
				_context.SaveChanges();
			}
			else
			{
				// Try admin
				var adminAccount = _context.AdminAccounts
					.Include(a => a.Admin)
					.FirstOrDefault(a => a.Admin.Email == model.Email);

				if (adminAccount != null)
				{
					targetName = adminAccount.Admin.FirstName + " " + adminAccount.Admin.LastName;
					adminAccount.Password = hashedNew;
					adminAccount.IsFirstLogin = true;
					_context.SaveChanges();
				}
			}

			if (targetName == null)
			{
				ViewBag.Error = "لم يتم العثور على المستخدم";
				return View(model);
			}

			// Send new password to the user by email
			var userBody = Services.EmailTemplates.NewPasswordNotification(
	targetName, model.NewPassword);

			_emailService.SendEmail(targetEmail, targetName, "كلمة المرور الجديدة - بنيان", userBody);

			TempData["Success"] = $"تم إعادة تعيين كلمة مرور {targetName} وإرسالها إليه بنجاح";
			return RedirectToAction("Index", "Employee");
		}
	}
}