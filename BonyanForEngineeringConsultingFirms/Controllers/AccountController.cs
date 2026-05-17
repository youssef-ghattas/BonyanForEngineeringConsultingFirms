using Bonyan.DAL.Context;
using Bonyan.DAL.Enums;
using Bonyan.DAL.Models;
using Bonyan.PL.ViewModels;
using BonyanForEngineeringConsultingFirms.Helpers;
using Bonyan.PL.ViewModels;
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
	}
}