using Bonyan.DAL.Context;
using Bonyan.DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bonyan.DAL.Enums;

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
            // if already logged in go to dashboard
            if (HttpContext.Session.GetString("Username") != null)
                return RedirectToAction("Index", "Home");

            return View();
        }

        // ── Login POST ────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string username, string password)
        {
            var user = _context.UserAccounts
                .Include(u => u.Employee)
                .FirstOrDefault(u => u.Username == username
                                  && u.Password == password);

            if (user == null)
            {
                ViewBag.Error = "اسم المستخدم أو كلمة المرور غير صحيحة";
                return View();
            }

            // ── Save in Session ───────────────────────
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role.ToString());
            HttpContext.Session.SetString("FullName",
                user.Employee.FirstName + " " + user.Employee.LastName);
            HttpContext.Session.SetInt32("UserId", user.UserId);

            return RedirectToAction("Index", "Home");
        }

        // ── Logout ────────────────────────────────────
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
        // ── Register GET ──────────────────────────────────
        public IActionResult Register()
        {
            if (HttpContext.Session.GetString("Username") != null)
                return RedirectToAction("Index", "Home");

            return View();
        }

        // ── Register POST ─────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(Employee employee, string username, string password)
        {
            // Check if email already exists
            if (_context.Employees.Any(e => e.Email == employee.Email))
            {
                ViewBag.Error = "البريد الإلكتروني مستخدم بالفعل";
                return View(employee);
            }

            // Check if SSN already exists
            if (_context.Employees.Any(e => e.SSN == employee.SSN))
            {
                ViewBag.Error = "رقم الهوية مستخدم بالفعل";
                return View(employee);
            }

            // Check if username already exists
            if (_context.UserAccounts.Any(u => u.Username == username))
            {
                ViewBag.Error = "اسم المستخدم مستخدم بالفعل";
                return View(employee);
            }

            // Save Employee first
            _context.Employees.Add(employee);
            _context.SaveChanges();

            // Then create UserAccount linked to employee
            var userAccount = new UserAccount
            {
                EmployeeId = employee.EmployeeId,
                Username = username,
                Password = password,
                Role = Bonyan.DAL.Enums.UserRole.Engineer // default role
            };

            _context.UserAccounts.Add(userAccount);
            _context.SaveChanges();

            ViewBag.Success = "تم إنشاء الحساب بنجاح! يمكنك تسجيل الدخول الآن.";
            return RedirectToAction("Login");
        }
    }
}