using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Bonyan.DAL.Models
{
	public class Document
	{
		[Key]
		public int DocId { get; set; }

		[Required]
		public int TaskId { get; set; }

		[Required]
		public int EmployeeId { get; set; }

		[Required]
		[StringLength(200)]
		public string DocTitle { get; set; }

		[StringLength(50)]
		public string FileType { get; set; }

		[Required]
		[StringLength(500)]
		public string FilePath { get; set; }

		[Required]
		public DateTime UploadDate { get; set; } = DateTime.Now;

		[StringLength(100)]
		public string Category { get; set; }

		public string Notes { get; set; }

		[ForeignKey("TaskId")]
		public virtual Task Task { get; set; }

		[ForeignKey("EmployeeId")]
		public virtual Employee Employee { get; set; }
	}
}
