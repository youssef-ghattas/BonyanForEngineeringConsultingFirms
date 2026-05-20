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
	public class Invoice
	{
		[Key]
		public int Invoice_ID { get; set; }

		
		public int? TaskId { get; set; }

		// ── NEW ──────────────────────────────────────────
		[Required]
		public int ProjectId { get; set; }   // nullable so existing rows are not broken
											  // ─────────────────────────────────────────────────

		[Required]
		[DataType(DataType.Date)]
		public DateTime Invoice_Date { get; set; } = DateTime.Now;

		[DataType(DataType.Date)]
		public DateTime? Due_Date { get; set; }

		
		[Column(TypeName = "decimal(15, 2)")]
		[Range(0, double.MaxValue)]
		public decimal? Total_Amount { get; set; }

		[Required]
		public InvoiceStatus Invoice_Status { get; set; } = InvoiceStatus.Unpaid;

		[Column(TypeName = "decimal(5, 2)")]
		[Range(0, double.MaxValue)]
		public decimal? Tax { get; set; } = 0;

		[Column(TypeName = "decimal(5, 2)")]
		[Range(0, double.MaxValue)]
		public decimal? Discount { get; set; } = 0;

		[ForeignKey("TaskId")]
		public virtual Task Task { get; set; }

		// ── NEW ──────────────────────────────────────────
		[ForeignKey("ProjectId")]
		public virtual Project Project { get; set; }
		// ─────────────────────────────────────────────────

		public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
	}
}