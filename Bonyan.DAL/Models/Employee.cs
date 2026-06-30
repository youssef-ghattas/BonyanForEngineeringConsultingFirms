using Bonyan.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Bonyan.DAL.Models
{
	[Index(nameof(Email), IsUnique = true)]
	[Index(nameof(SSN), IsUnique = true)]
	public class Employee
	{
		[Key]
		public int EmployeeId { get; set; }

		[Required(ErrorMessage = "First Name is required.")]
		[StringLength(50)]
		[Display(Name = "First Name")]
		public string FirstName { get; set; }

		[Required(ErrorMessage = "Last Name is required.")]
		[StringLength(50)]
		[Display(Name = "Last Name")]
		public string LastName { get; set; }

		[Required(ErrorMessage = "Email is required.")]
		[EmailAddress(ErrorMessage = "Invalid Email Address.")]
		[StringLength(100)]
		public string Email { get; set; }

		[Phone(ErrorMessage = "Invalid Phone Number.")]
		[StringLength(11)]
		[Display(Name = "Phone Number")]
		public string PhoneNum { get; set; }

		[Required(ErrorMessage = "SSN is required.")]
		[StringLength(14)]
		public string SSN { get; set; }

		[Required(ErrorMessage = "Specialization is required.")]
		public Specialization Specialization { get; set; }

		[Required(ErrorMessage = "Salary is required.")]
		[Column(TypeName = "decimal(18, 10)")]  // ← no rounding, stores exactly what you enter
		[Range(0.0000000001, double.MaxValue, ErrorMessage = "Salary cannot be negative or zero.")]
		public decimal Salary { get; set; }

		[Required(ErrorMessage = "Gender is required.")]
		public Gender Gender { get; set; }

		[Required]
		[DataType(DataType.Date)]
		[Display(Name = "Hire Date")]
		public DateTime HireDate { get; set; } = DateTime.Now;

		// Navigation Properties
		public virtual UserAccount UserAccount { get; set; }
		public virtual ICollection<EmployeeProject> EmployeeProjects { get; set; } = new List<EmployeeProject>();
		public virtual ICollection<SiteVisit> SiteVisits { get; set; } = new List<SiteVisit>();
		public virtual ICollection<Document> UploadedDocuments { get; set; } = new List<Document>();
		public virtual ICollection<Drawing> UploadedDrawings { get; set; } = new List<Drawing>();
	}
}