using Bonyan.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonyan.DAL.Models
{
	[PrimaryKey(nameof(InventoryID), nameof(MaterialID))]
	public class MaterialInventory
	{
		[Required]
		public int InventoryID { get; set; }

		[Required]
		public int MaterialID { get; set; }

		[Required]
		[Column(TypeName = "decimal(15, 3)")]
		[Range(0, double.MaxValue)]
		public decimal QuantityAvailable { get; set; }

		[StringLength(200)]
		public string StorageLocation { get; set; }

		[Column(TypeName = "decimal(15, 3)")]
		[Range(0, double.MaxValue)]
		public decimal? TransactionQuantity { get; set; }

		[Required]
		[DataType(DataType.Date)]
		public DateTime TransactionDate { get; set; } = DateTime.Now;

		public string Notes { get; set; }

		[ForeignKey("MaterialID")]
		public virtual Material Material { get; set; }

		[ForeignKey("InventoryID")]
		public virtual Inventory Inventory { get; set; }
	}
}