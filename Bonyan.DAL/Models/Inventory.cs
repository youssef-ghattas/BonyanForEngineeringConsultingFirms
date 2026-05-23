using Bonyan.DAL.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonyan.DAL.Models
{
	public class Inventory
	{
		[Key]
		public int InventoryID { get; set; }

		[Required]
		[StringLength(150)]
		public string InventoryName { get; set; }

		[StringLength(300)]
		public string Location { get; set; }

		[Column(TypeName = "decimal(15, 2)")]
		public decimal? Capacity { get; set; }

		[Required]
		public DateTime LastUpdatedDate { get; set; } = DateTime.Now;

		public virtual ICollection<MaterialInventory> MaterialInventories { get; set; } = new List<MaterialInventory>();
	}
}