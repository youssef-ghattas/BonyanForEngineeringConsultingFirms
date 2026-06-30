using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bonyan.DAL.Models
{
	public class Material
	{
		[Key]
		public int MaterialID { get; set; }

		[Required]
		[StringLength(150)]
		public string MaterialName { get; set; }

		[StringLength(50)]
		public string Unit { get; set; }

		[Column(TypeName = "decimal(15, 2)")]
		[Range(0, double.MaxValue)]
		public decimal? UnitPrice { get; set; }

		// Quantity being ordered/requested
		[Column(TypeName = "decimal(15, 3)")]
		[Range(0, double.MaxValue)]
		public decimal? Quantity { get; set; }

		// Which supplier we chose to request from
		public int? PreferredSupplierID { get; set; }

		// Target inventory to store this material
		public int? TargetInventoryID { get; set; }

		// Volume Factor: how many m³ does 1 unit of this material occupy in warehouse
		// Sand=1.0, Cement(per ton)=0.85, Steel(per ton)=0.55, Bricks(per 1000)=1.25
		[Column(TypeName = "decimal(8, 4)")]
		[Range(0, double.MaxValue)]
		public decimal VolumeFactorM3 { get; set; } = 1.0m;

		[StringLength(300)]
		public string Description { get; set; }

		[ForeignKey("PreferredSupplierID")]
		public virtual Supplier PreferredSupplier { get; set; }

		[ForeignKey("TargetInventoryID")]
		public virtual Inventory TargetInventory { get; set; }

		public virtual ICollection<MaterialSupplier> MaterialSuppliers { get; set; } = new List<MaterialSupplier>();
		public virtual ICollection<MaterialTask> MaterialTasks { get; set; } = new List<MaterialTask>();
		public virtual ICollection<MaterialInventory> MaterialInventories { get; set; } = new List<MaterialInventory>();
		public virtual ICollection<MaterialInvoice> MaterialInvoices { get; set; } = new List<MaterialInvoice>();
	}
}