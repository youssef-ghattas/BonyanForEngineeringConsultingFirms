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
		public BonyanDbContext(DbContextOptions<BonyanDbContext> options) : base(options)
		{
		}

		// ── DbSets ──────────────────────────────────────────
		public DbSet<Employee> Employees { get; set; }
		public DbSet<UserAccount> UserAccounts { get; set; }
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

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// ── Composite Primary Keys ───────────────────────
			modelBuilder.Entity<EmployeeProject>()
				.HasKey(ep => new { ep.EmployeeId, ep.ProjectId });

			modelBuilder.Entity<MaterialSupplier>()
				.HasKey(ms => new { ms.MaterialID, ms.SupplierID });

			// ── MaterialInventory Unique Index ───────────────
			modelBuilder.Entity<MaterialInventory>()
				.HasIndex(mi => new { mi.InventoryID, mi.MaterialID })
				.IsUnique();

			// ── Employee → UserAccount (one to one) ──────────
			modelBuilder.Entity<Employee>()
				.HasOne(e => e.UserAccount)
				.WithOne(u => u.Employee)
				.HasForeignKey<UserAccount>(u => u.EmployeeId)
				.OnDelete(DeleteBehavior.Restrict);

			// ── UserAccount → Tasks ──────────────────────────
			modelBuilder.Entity<Task>()
				.HasOne(t => t.Creator)
				.WithMany(u => u.CreatedTasks)
				.HasForeignKey(t => t.CreatedBy_UserID)
				.OnDelete(DeleteBehavior.Restrict);

			// ── Project → Tasks ──────────────────────────────
			modelBuilder.Entity<Task>()
				.HasOne(t => t.Project)
				.WithMany(p => p.Tasks)
				.HasForeignKey(t => t.ProjectId)
				.OnDelete(DeleteBehavior.Restrict);

			// ── Task → Documents ─────────────────────────────
			modelBuilder.Entity<Document>()
				.HasOne(d => d.Task)
				.WithMany(t => t.Documents)
				.HasForeignKey(d => d.TaskId)
				.OnDelete(DeleteBehavior.Restrict);

			// ── Task → Drawings ──────────────────────────────
			modelBuilder.Entity<Drawing>()
				.HasOne(d => d.Task)
				.WithMany(t => t.Drawings)
				.HasForeignKey(d => d.TaskId)
				.OnDelete(DeleteBehavior.Restrict);

			// ── Task → Invoices ──────────────────────────────
			modelBuilder.Entity<Invoice>()
				.HasOne(i => i.Task)
				.WithMany(t => t.Invoices)
				.HasForeignKey(i => i.TaskId)
				.OnDelete(DeleteBehavior.Restrict);

			// ── Invoice → Payments ───────────────────────────
			modelBuilder.Entity<Payment>()
				.HasOne(p => p.Invoice)
				.WithMany(i => i.Payments)
				.HasForeignKey(p => p.InvoiceID)
				.OnDelete(DeleteBehavior.Restrict);

			// ── Task → MaterialTasks ─────────────────────────
			modelBuilder.Entity<MaterialTask>()
				.HasOne(mt => mt.Task)
				.WithMany(t => t.MaterialTasks)
				.HasForeignKey(mt => mt.TaskID)
				.OnDelete(DeleteBehavior.Restrict);

			// ── Material → MaterialTasks ─────────────────────
			modelBuilder.Entity<MaterialTask>()
				.HasOne(mt => mt.Material)
				.WithMany(m => m.MaterialTasks)
				.HasForeignKey(mt => mt.MaterialID)
				.OnDelete(DeleteBehavior.Restrict);

			// ── Material → MaterialSuppliers ─────────────────
			modelBuilder.Entity<MaterialSupplier>()
				.HasOne(ms => ms.Material)
				.WithMany(m => m.MaterialSuppliers)
				.HasForeignKey(ms => ms.MaterialID)
				.OnDelete(DeleteBehavior.Restrict);

			// ── Supplier → MaterialSuppliers ─────────────────
			modelBuilder.Entity<MaterialSupplier>()
				.HasOne(ms => ms.Supplier)
				.WithMany(s => s.SuppliedMaterials)
				.HasForeignKey(ms => ms.SupplierID)
				.OnDelete(DeleteBehavior.Restrict);

			// ── Material → MaterialInventories ───────────────
			modelBuilder.Entity<MaterialInventory>()
				.HasOne(mi => mi.Material)
				.WithMany(m => m.MaterialInventories)
				.HasForeignKey(mi => mi.MaterialID)
				.OnDelete(DeleteBehavior.Restrict);

			// ── Inventory → MaterialInventories ──────────────
			modelBuilder.Entity<MaterialInventory>()
				.HasOne(mi => mi.Inventory)
				.WithMany(i => i.MaterialInventories)
				.HasForeignKey(mi => mi.InventoryID)
				.OnDelete(DeleteBehavior.Restrict);

			// ── SiteVisit → Project ───────────────────────────
			modelBuilder.Entity<SiteVisit>()
				.HasOne(sv => sv.Project)
				.WithMany(p => p.SiteVisits)
				.HasForeignKey(sv => sv.ProjId)
				.OnDelete(DeleteBehavior.Restrict);

			// ── SiteVisit → Employee ──────────────────────────
			modelBuilder.Entity<SiteVisit>()
				.HasOne(sv => sv.Employee)
				.WithMany(e => e.SiteVisits)
				.HasForeignKey(sv => sv.EmployeeId)
				.OnDelete(DeleteBehavior.Restrict);

			// ── Document → Employee ───────────────────────────
			modelBuilder.Entity<Document>()
				.HasOne(d => d.Employee)
				.WithMany(e => e.UploadedDocuments)
				.HasForeignKey(d => d.EmployeeId)
				.OnDelete(DeleteBehavior.Restrict);

			// ── Drawing → Employee ────────────────────────────
			modelBuilder.Entity<Drawing>()
				.HasOne(d => d.Employee)
				.WithMany(e => e.UploadedDrawings)
				.HasForeignKey(d => d.EmployeeId)
				.OnDelete(DeleteBehavior.Restrict);

			// ── EmployeeProject → Employee ────────────────────
			modelBuilder.Entity<EmployeeProject>()
				.HasOne(ep => ep.Employee)
				.WithMany(e => e.EmployeeProjects)
				.HasForeignKey(ep => ep.EmployeeId)
				.OnDelete(DeleteBehavior.Restrict);

			// ── EmployeeProject → Project ─────────────────────
			modelBuilder.Entity<EmployeeProject>()
				.HasOne(ep => ep.Project)
				.WithMany(p => p.EmployeeProjects)
				.HasForeignKey(ep => ep.ProjectId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
