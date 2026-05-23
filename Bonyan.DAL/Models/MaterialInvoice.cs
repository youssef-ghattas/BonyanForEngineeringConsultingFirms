using Bonyan.DAL.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonyan.DAL.Models
{
	public class MaterialInvoice
	{
		[Key]
		public int MaterialInvoiceID { get; set; }

		[Required]
		public int MaterialID { get; set; }

		[Required]
		public int SupplierID { get; set; }

		[Required]
		[Column(TypeName = "decimal(15, 3)")]
		[Range(0, double.MaxValue)]
		public decimal Quantity { get; set; }

		[Required]
		[Column(TypeName = "decimal(15, 2)")]
		[Range(0, double.MaxValue)]
		public decimal UnitPrice { get; set; }

		// Auto-calculated: Quantity * UnitPrice
		[Column(TypeName = "decimal(18, 2)")]
		public decimal TotalAmount { get; set; }

		[Column(TypeName = "decimal(5, 2)")]
		[Range(0, 100)]
		public decimal? TaxPercent { get; set; } = 0;

		[Column(TypeName = "decimal(5, 2)")]
		[Range(0, 100)]
		public decimal? DiscountPercent { get; set; } = 0;

		// Final = TotalAmount + Tax - Discount
		[Column(TypeName = "decimal(18, 2)")]
		public decimal FinalAmount { get; set; }

		[Required]
		[DataType(DataType.Date)]
		public DateTime InvoiceDate { get; set; } = DateTime.Now;

		[DataType(DataType.Date)]
		public DateTime? DueDate { get; set; }

		[Required]
		public InvoiceStatus Status { get; set; } = InvoiceStatus.Unpaid;

		[StringLength(500)]
		public string Notes { get; set; }

		// ── Navigation ───────────────────────────────────────────
		[ForeignKey("MaterialID")]
		public virtual Material Material { get; set; }

		[ForeignKey("SupplierID")]
		public virtual Supplier Supplier { get; set; }
	}
}
