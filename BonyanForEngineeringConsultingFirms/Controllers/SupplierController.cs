using Bonyan.DAL.Context;
using Bonyan.DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BonyanForEngineeringConsultingFirms.Controllers
{
    public class SupplierController : Controller
    {
        private readonly BonyanDbContext _context;

        public SupplierController(BonyanDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";
        private bool IsLoggedIn() => HttpContext.Session.GetString("Role") != null;

        // ── INDEX ─────────────────────────────────────────────────
        public async Task<IActionResult> Index(string search = "")
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var query = _context.Suppliers
                .Include(s => s.SuppliedMaterials)
                    .ThenInclude(ms => ms.Material)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(s => s.SupplierName.Contains(search) ||
                                         s.ContactPerson.Contains(search) ||
                                         s.Email.Contains(search));

            ViewBag.Search = search;
            return View(await query.OrderBy(s => s.SupplierName).ToListAsync());
        }

        // ── DETAILS ───────────────────────────────────────────────
        public async Task<IActionResult> Details(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var supplier = await _context.Suppliers
                .Include(s => s.SuppliedMaterials)
                    .ThenInclude(ms => ms.Material)
                .FirstOrDefaultAsync(s => s.SupplierID == id);

            if (supplier == null) return NotFound();
            return View(supplier);
        }

        // ── CREATE GET ────────────────────────────────────────────
        public IActionResult Create()
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Home");
            return View();
        }

        // ── CREATE POST ───────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Supplier supplier)
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Home");

            if (_context.Suppliers.Any(s => s.Email == supplier.Email))
            {
                ModelState.AddModelError("Email", "البريد الإلكتروني مسجل مسبقاً لمورد آخر");
                return View(supplier);
            }

            if (!ModelState.IsValid) return View(supplier);

            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تمت إضافة المورد بنجاح";
            return RedirectToAction(nameof(Index));
        }

        // ── EDIT GET ──────────────────────────────────────────────
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Home");

            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null) return NotFound();
            return View(supplier);
        }

        // ── EDIT POST ─────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Supplier supplier)
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Home");

            if (_context.Suppliers.Any(s => s.Email == supplier.Email &&
                                            s.SupplierID != supplier.SupplierID))
            {
                ModelState.AddModelError("Email", "البريد الإلكتروني مستخدم لمورد آخر");
                return View(supplier);
            }

            if (!ModelState.IsValid) return View(supplier);

            var existing = await _context.Suppliers.FindAsync(supplier.SupplierID);
            if (existing == null) return NotFound();

            existing.SupplierName = supplier.SupplierName;
            existing.ContactPerson = supplier.ContactPerson;
            existing.Email = supplier.Email;
            existing.Phone = supplier.Phone;
            existing.Address = supplier.Address;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم تحديث بيانات المورد بنجاح";
            return RedirectToAction(nameof(Index));
        }

        // ── DELETE POST ───────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Home");

            var supplier = await _context.Suppliers
                .Include(s => s.SuppliedMaterials)
                .FirstOrDefaultAsync(s => s.SupplierID == id);

            if (supplier == null) return NotFound();

            // Only remove links, keep the materials
            _context.MaterialSuppliers.RemoveRange(supplier.SuppliedMaterials);
            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم حذف المورد بنجاح";
            return RedirectToAction(nameof(Index));
        }
    }
}
