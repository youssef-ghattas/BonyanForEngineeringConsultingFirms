using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Bonyan.DAL.Enums;

namespace Bonyan.DAL.Models
{
	public class Project
	{
		[Key]
		public int ProjectId { get; set; }

		[Required]
		[StringLength(150)]
		public string ProjectName { get; set; }

		[StringLength(500)]
		public string Location { get; set; }

		[DataType(DataType.Date)]
		public DateTime? StartDate { get; set; }

		[DataType(DataType.Date)]
		public DateTime? EndDate { get; set; }

		[Column(TypeName = "decimal(15, 2)")]
		[Range(0, double.MaxValue, ErrorMessage = "Budget cannot be negative.")]
		public decimal? Budget { get; set; }

		[Required]
		public ProjectStatus Status { get; set; } = ProjectStatus.Planning;

		[StringLength(150)]
		public string Client_Name { get; set; }

		// Navigation Properties
		public virtual ICollection<EmployeeProject> EmployeeProjects { get; set; } = new List<EmployeeProject>();
		public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
		public virtual ICollection<SiteVisit> SiteVisits { get; set; } = new List<SiteVisit>();
		// Add these navigation properties to the Project class:
		public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
		public virtual ICollection<Drawing> Drawings { get; set; } = new List<Drawing>();
		public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
	}
}
