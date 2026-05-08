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

		[StringLength(300)]
		public string Description { get; set; }

		public virtual ICollection<MaterialSupplier> MaterialSuppliers { get; set; } = new List<MaterialSupplier>();
		public virtual ICollection<MaterialTask> MaterialTasks { get; set; } = new List<MaterialTask>();
		public virtual ICollection<MaterialInventory> MaterialInventories { get; set; } = new List<MaterialInventory>();
	}
}