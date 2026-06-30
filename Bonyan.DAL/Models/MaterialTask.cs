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
	public class MaterialTask
	{
		[Key]
		public int MaterialTaskID { get; set; }

		[Required]
		public int TaskID { get; set; }

		[Required]
		public int MaterialID { get; set; }

		[Required]
		[Column(TypeName = "decimal(15, 3)")]
		[Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
		public decimal QuantityRequired { get; set; }

		[Column(TypeName = "decimal(15, 3)")]
		[Range(0, double.MaxValue)]
		public decimal? QuantityUsed { get; set; } = 0;

		[Required]
		[DataType(DataType.Date)]
		public DateTime RequestDate { get; set; } = DateTime.Now;

		[Required]
		public MaterialTaskStatus Status { get; set; } = MaterialTaskStatus.Requested;

		[ForeignKey("TaskID")]
		public virtual Task Task { get; set; }

		[ForeignKey("MaterialID")]
		public virtual Material Material { get; set; }
	}
}
