using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bonyan.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Task = Bonyan.DAL.Models.Task;

namespace Bonyan.DAL.Context
{
	public class BonyanDbContext : DbContext
	{
		public BonyanDbContext(DbContextOptions<BonyanDbContext> options)
			: base(options) { }

		// ── DbSets ───────────────────────────────────────
		public DbSet<Employee> Employees { get; set; }
		public DbSet<UserAccount> UserAccounts { get; set; }
		public DbSet<Admin> Admins { get; set; }
		public DbSet<AdminAccount> AdminAccounts { get; set; }
		public DbSet<Project> Projects { get; set; }
		public DbSet<EmployeeProject> EmployeeProjects { get; set; }
		public DbSet<Task> Tasks { get; set; }
		public DbSet<Document> Documents { get; set; }
		public DbSet<Drawing> Drawings { get; set; }
		public DbSet<SiteVisit> SiteVisits { get; set; }
		public DbSet<Invoice> Invoices { get; set; }
		public DbSet<Payment> Payments { get; set; }
		public DbSet<Material> Materials { get; set; }
		public DbSet<MaterialInventory> MaterialInventories { get; set; }
		public DbSet<MaterialSupplier> MaterialSuppliers { get; set; }
		public DbSet<MaterialTask> MaterialTasks { get; set; }
		public DbSet<Supplier> Suppliers { get; set; }
		public DbSet<Inventory> Inventories { get; set; }
		public DbSet<MaterialInvoice> MaterialInvoices { get; set; }  // ← NEW

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// ── Composite Keys ───────────────────────────
			modelBuilder.Entity<EmployeeProject>()
				.HasKey(ep => new { ep.EmployeeId, ep.ProjectId });

			modelBuilder.Entity<MaterialSupplier>()
				.HasKey(ms => new { ms.MaterialID, ms.SupplierID });

			modelBuilder.Entity<MaterialInventory>()
				.HasKey(mi => new { mi.InventoryID, mi.MaterialID });

			// ── Admin → AdminAccount (one-to-one) ────────
			modelBuilder.Entity<Admin>()
				.HasOne(a => a.AdminAccount)
				.WithOne(aa => aa.Admin)
				.HasForeignKey<AdminAccount>(aa => aa.AdminId)
				.OnDelete(DeleteBehavior.Restrict);

			// ── Employee → UserAccount (one-to-one) ──────
			modelBuilder.Entity<Employee>()
				.HasOne(e => e.UserAccount)
				.WithOne(u => u.Employee)
				.HasForeignKey<UserAccount>(u => u.EmployeeId)
				.OnDelete(DeleteBehavior.Restrict);

			// ── Task → Creator ────────────────────────────
			modelBuilder.Entity<Task>()
				.HasOne(t => t.Creator)
				.WithMany(u => u.CreatedTasks)
				.HasForeignKey(t => t.CreatedBy_UserID)
				.OnDelete(DeleteBehavior.Restrict)
				.IsRequired(false);

			// ── Task → Project ────────────────────────────
			modelBuilder.Entity<Task>()
				.HasOne(t => t.Project)
				.WithMany(p => p.Tasks)
				.HasForeignKey(t => t.ProjectId)
				.OnDelete(DeleteBehavior.Restrict);

			// ── Task → Assigned employee ──────────────────
			modelBuilder.Entity<Task>()
				.HasOne(t => t.AssignedToEmployee)
				.WithMany()
				.HasForeignKey(t => t.AssignedToEmployeeId)
				.OnDelete(DeleteBehavior.SetNull)
				.IsRequired(false);

			// ── EmployeeProject ───────────────────────────
			modelBuilder.Entity<EmployeeProject>()
				.HasOne(ep => ep.Employee)
				.WithMany(e => e.EmployeeProjects)
				.HasForeignKey(ep => ep.EmployeeId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<EmployeeProject>()
				.HasOne(ep => ep.Project)
				.WithMany(p => p.EmployeeProjects)
				.HasForeignKey(ep => ep.ProjectId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<EmployeeProject>()
				.HasOne(ep => ep.AssignedByAdmin)
				.WithMany()
				.HasForeignKey(ep => ep.AssignedByAdminId)
				.OnDelete(DeleteBehavior.SetNull);

			// ── Document ──────────────────────────────────
			modelBuilder.Entity<Document>()
				.HasOne(d => d.Task)
				.WithMany(t => t.Documents)
				.HasForeignKey(d => d.TaskId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Document>()
				.HasOne(d => d.Employee)
				.WithMany(e => e.UploadedDocuments)
				.HasForeignKey(d => d.EmployeeId)
				.OnDelete(DeleteBehavior.Restrict);

			// Material → PreferredSupplier
			modelBuilder.Entity<Material>()
				.HasOne(m => m.PreferredSupplier).WithMany()
				.HasForeignKey(m => m.PreferredSupplierID)
				.OnDelete(DeleteBehavior.SetNull).IsRequired(false);

			// Material → TargetInventory
			modelBuilder.Entity<Material>()
				.HasOne(m => m.TargetInventory).WithMany()
				.HasForeignKey(m => m.TargetInventoryID)
				.OnDelete(DeleteBehavior.SetNull).IsRequired(false);

			// MaterialInventory composite
			modelBuilder.Entity<MaterialInventory>()
				.HasOne(mi => mi.Material).WithMany(m => m.MaterialInventories)
				.HasForeignKey(mi => mi.MaterialID).OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<MaterialInventory>()
				.HasOne(mi => mi.Inventory).WithMany(i => i.MaterialInventories)
				.HasForeignKey(mi => mi.InventoryID).OnDelete(DeleteBehavior.Cascade);

			// MaterialInvoice
			modelBuilder.Entity<MaterialInvoice>()
				.HasOne(mi => mi.Material).WithMany(m => m.MaterialInvoices)
				.HasForeignKey(mi => mi.MaterialID).OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<MaterialInvoice>()
				.HasOne(mi => mi.Supplier).WithMany()
				.HasForeignKey(mi => mi.SupplierID).OnDelete(DeleteBehavior.Restrict);
		}
	}
}
