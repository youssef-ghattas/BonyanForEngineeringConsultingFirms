using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Bonyan.DAL.Models
{
	[PrimaryKey(nameof(MaterialID), nameof(SupplierID))]
	public class MaterialSupplier
	{
		public int MaterialID { get; set; }
		public int SupplierID { get; set; }

		[Column(TypeName = "decimal(12, 2)")]
		[Range(0, double.MaxValue)]
		public decimal? SupplyPrice { get; set; }

		[DataType(DataType.Date)]
		public DateTime? LastSupplyDate { get; set; }

		[ForeignKey("MaterialID")]
		public virtual Material Material { get; set; }

		[ForeignKey("SupplierID")]
		public virtual Supplier Supplier { get; set; }
	}
}