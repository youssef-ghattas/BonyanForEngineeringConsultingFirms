using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bonyan.DAL.Models
{
	public class Drawing
	{
		[Key]
		public int DrawingId { get; set; }

		public int? TaskId { get; set; }

		[Required]
		public int ProjectId { get; set; }   // ← NEW

		[Required]
		public int EmployeeId { get; set; }

		[Required]
		[StringLength(200)]
		public string DrawingTitle { get; set; }

		[StringLength(500)]
		public string FilePath { get; set; }

		[Required]
		public DateTime UploadDate { get; set; } = DateTime.Now;

		public string Notes { get; set; }

		[ForeignKey("TaskId")]
		public virtual Task Task { get; set; }

		[ForeignKey("ProjectId")]
		public virtual Project Project { get; set; }   // ← NEW

		[ForeignKey("EmployeeId")]
		public virtual Employee Employee { get; set; }
	}
}
