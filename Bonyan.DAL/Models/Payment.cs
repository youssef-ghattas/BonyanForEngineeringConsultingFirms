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
	public class Payment
	{
		[Key]
		public int PaymentID { get; set; }

		[Required]
		public int InvoiceID { get; set; }

		[Required]
		[DataType(DataType.Date)]
		public DateTime PaymentDate { get; set; } = DateTime.Now;

		[Required]
		[Column(TypeName = "decimal(15, 2)")]
		[Range(0.01, double.MaxValue, ErrorMessage = "Payment must be greater than 0.")]
		public decimal PaymentAmount { get; set; }

		[Required]
		public PaymentMethod PaymentMethod { get; set; }

		[Required]
		public PaymentStatus Payment_Status { get; set; } = PaymentStatus.Pending;

		[StringLength(100)]
		public string TransactionReference { get; set; }

		[ForeignKey("InvoiceID")]
		public virtual Invoice Invoice { get; set; }
	}
}