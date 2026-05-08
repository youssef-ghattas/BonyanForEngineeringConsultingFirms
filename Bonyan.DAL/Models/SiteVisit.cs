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
	public class SiteVisit
	{
		[Key]
		public int VisitId { get; set; }

		[Required]
		public int ProjId { get; set; }

		[Required]
		public int EmployeeId { get; set; }

		[Required]
		[DataType(DataType.Date)]
		public DateTime VisitDate { get; set; } = DateTime.Now;

		[StringLength(500)]
		public string VisitPhotos { get; set; }

		public string Report { get; set; }

		[Required]
		public SafetyStatus SafetyStatus { get; set; }

		[ForeignKey("ProjId")]
		public virtual Project Project { get; set; }

		[ForeignKey("EmployeeId")]
		public virtual Employee Employee { get; set; }
	}
}