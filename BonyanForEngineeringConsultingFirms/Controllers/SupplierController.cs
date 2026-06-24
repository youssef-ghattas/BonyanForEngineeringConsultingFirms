// Controllers/SupplierController.cs
using Bonyan.BLL.Services;
using Bonyan.DAL.Context;
using Bonyan.DAL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace BonyanForEngineeringConsultingFirms.Controllers
{
	public class SupplierController : Controller
	{
		private readonly IService<Supplier> _supplierService;
		private readonly BonyanDbContext _context;

		public SupplierController(IService<Supplier> supplierService, BonyanDbContext context)
		{
			_supplierService = supplierService;
			_context = context;
		}

		private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";
		private bool IsLoggedIn() => HttpContext.Session.GetString("Role") != null;

		private static string GetMaterialTypeKey(string materialName)
		{
			if (string.IsNullOrWhiteSpace(materialName)) return "Others";
			var n = materialName.ToLower();
			if (n.Contains("steel") || n.Contains("حديد")) return "Steel";
			if (n.Contains("sand") || n.Contains("رمل")) return "Sand";
			if (n.Contains("cement") || n.Contains("أسمنت") || n.Contains("اسمنت")) return "Cement";
			if (n.Contains("brick") || n.Contains("طوب")) return "Bricks";
			return "Others";
		}

		private async Task AutoLinkMaterials(Supplier supplier, List<string> types)
		{
			if (types == null || types.Count == 0) return;
			var allMaterials = await _context.Materials.ToListAsync();
			var existingIds = (supplier.SuppliedMaterials ?? new List<MaterialSupplier>())
				.Select(ms => ms.MaterialID).ToHashSet();
			var toAdd = new List<MaterialSupplier>();

			foreach (var type in types)
			{
				if (type.StartsWith("Others:"))
				{
					var custom = type.Substring("Others:".Length).Trim().ToLower();
					if (string.IsNullOrEmpty(custom)) continue;
					var matches = allMaterials.Where(m =>
						!existingIds.Contains(m.MaterialID) &&
						(m.MaterialName.ToLower().Contains(custom) || GetMaterialTypeKey(m.MaterialName) == "Others"));
					foreach (var m in matches)
						toAdd.Add(new MaterialSupplier { SupplierID = supplier.SupplierID, MaterialID = m.MaterialID });
				}
				else
				{
					var matches = allMaterials.Where(m =>
						!existingIds.Contains(m.MaterialID) &&
						GetMaterialTypeKey(m.MaterialName) == type);
					foreach (var m in matches)
						toAdd.Add(new MaterialSupplier { SupplierID = supplier.SupplierID, MaterialID = m.MaterialID });
				}
			}

			if (toAdd.Count > 0)
			{
				_context.MaterialSuppliers.AddRange(toAdd);
				await _context.SaveChangesAsync();
			}
		}

		// ── INDEX ─────────────────────────────────────────────────
		public async Task<IActionResult> Index(string search = "")
		{
			if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

			var query = _context.Suppliers
				.Include(s => s.SuppliedMaterials)
				.AsQueryable();

			if (!string.IsNullOrWhiteSpace(search))
				query = query.Where(s => s.SupplierName.Contains(search) ||
										 (s.Email != null && s.Email.Contains(search)) ||
										 (s.Phone != null && s.Phone.Contains(search)));

			ViewBag.Search = search;
			var suppliers = await query.OrderBy(s => s.SupplierName).ToListAsync();
			return View(suppliers);
		}

		// ── DETAILS ───────────────────────────────────────────────
		public async Task<IActionResult> Details(int id)
		{
			if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

			var supplier = await _context.Suppliers
				.Include(s => s.SuppliedMaterials).ThenInclude(ms => ms.Material)
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
		public async Task<IActionResult> Create(
	Supplier supplier,
	List<string> selectedMaterialTypes,
	string othersText)
		{
			if (!IsAdmin()) return RedirectToAction("Index", "Home");

			ModelState.Remove("SuppliedMaterials");
			ModelState.Remove("SuppliedMaterialTypes");
			ModelState.Remove("Phone");
			ModelState.Remove("Email");
			ModelState.Remove("ContactPerson");
			ModelState.Remove("Address");
			ModelState.Remove("othersText");
			ModelState.Remove("selectedMaterialTypes");

			if (!ModelState.IsValid)
			{
				ViewBag.ValidationErrors = ModelState
					.Where(x => x.Value.Errors.Count > 0)
					.Select(x => $"{x.Key}: {string.Join(", ", x.Value.Errors.Select(e => e.ErrorMessage))}")
					.ToList();
				return View(supplier);
			}

			var types = new List<string>();
			if (selectedMaterialTypes != null && selectedMaterialTypes.Count > 0)
			{
				foreach (var t in selectedMaterialTypes)
				{
					if (t == "Others")
						types.Add(!string.IsNullOrWhiteSpace(othersText)
							? "Others:" + othersText.Trim()
							: "Others");
					else
						types.Add(t);
				}
			}
			supplier.SuppliedMaterialTypes = types.Count > 0
				? string.Join(",", types)
				: null;

			try
			{
				_context.Suppliers.Add(supplier);
				await _context.SaveChangesAsync();

				if (types.Count > 0)
					await AutoLinkMaterials(supplier, types);

				TempData["SuccessMessageKey"] = "msg_supplier_created";
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("", "حدث خطأ أثناء الحفظ: " + ex.Message);
				return View(supplier);
			}
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
		public async Task<IActionResult> Edit(
	Supplier supplier,
	List<string> selectedMaterialTypes,
	string othersText)
		{
			if (!IsAdmin()) return RedirectToAction("Index", "Home");

			ModelState.Remove("SuppliedMaterials");
			ModelState.Remove("SuppliedMaterialTypes");
			ModelState.Remove("Phone");
			ModelState.Remove("Email");
			ModelState.Remove("ContactPerson");
			ModelState.Remove("Address");
			ModelState.Remove("othersText");
			ModelState.Remove("selectedMaterialTypes");

			if (!ModelState.IsValid)
			{
				ViewBag.ValidationErrors = ModelState
					.Where(x => x.Value.Errors.Count > 0)
					.Select(x => $"{x.Key}: {string.Join(", ", x.Value.Errors.Select(e => e.ErrorMessage))}")
					.ToList();
				return View(supplier);
			}

			var existing = await _context.Suppliers
				.Include(s => s.SuppliedMaterials)
				.FirstOrDefaultAsync(s => s.SupplierID == supplier.SupplierID);
			if (existing == null) return NotFound();

			existing.SupplierName = supplier.SupplierName;
			existing.ContactPerson = supplier.ContactPerson;
			existing.Email = supplier.Email;
			existing.Phone = supplier.Phone;
			existing.Address = supplier.Address;

			var types = new List<string>();
			if (selectedMaterialTypes != null && selectedMaterialTypes.Count > 0)
			{
				foreach (var t in selectedMaterialTypes)
				{
					if (t == "Others")
						types.Add(!string.IsNullOrWhiteSpace(othersText)
							? "Others:" + othersText.Trim()
							: "Others");
					else
						types.Add(t);
				}
			}
			existing.SuppliedMaterialTypes = types.Count > 0
				? string.Join(",", types)
				: null;

			try
			{
				await _context.SaveChangesAsync();

				if (types.Count > 0)
					await AutoLinkMaterials(existing, types);

				TempData["SuccessMessageKey"] = "msg_supplier_updated";
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("", "حدث خطأ أثناء الحفظ: " + ex.Message);
				return View(supplier);
			}
		}

		// ── DELETE GET ────────────────────────────────────────────
		public async Task<IActionResult> Delete(int id)
		{
			if (!IsAdmin()) return RedirectToAction("Index", "Home");
			var supplier = await _context.Suppliers.FindAsync(id);
			if (supplier == null) return NotFound();
			return View(supplier);
		}

		// ── DELETE POST ───────────────────────────────────────────
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			if (!IsAdmin()) return RedirectToAction("Index", "Home");

			var supplier = await _context.Suppliers
				.Include(s => s.SuppliedMaterials)
				.FirstOrDefaultAsync(s => s.SupplierID == id);

			if (supplier == null) return NotFound();

			// Remove junction records first
			_context.MaterialSuppliers.RemoveRange(supplier.SuppliedMaterials);
			_context.Suppliers.Remove(supplier);
			await _context.SaveChangesAsync();

			TempData["SuccessMessageKey"] = "msg_supplier_deleted";
			return RedirectToAction(nameof(Index));
		}
	}
}