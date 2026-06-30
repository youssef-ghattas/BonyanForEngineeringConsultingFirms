using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Bonyan.DAL.Models
{
	public class Supplier
	{
		[Key]
		public int SupplierID { get; set; }

		[Required]
		[StringLength(150)]
		public string SupplierName { get; set; }

		[StringLength(100)]
		public string ContactPerson { get; set; }

		[Phone]
		[StringLength(20)]
		public string Phone { get; set; }

		[EmailAddress]
		[StringLength(100)]
		public string Email { get; set; }

		[StringLength(300)]
		public string Address { get; set; }

		// Comma-separated list of material types this supplier provides
		// e.g. "Steel,Cement,Others:Gravel"
		[StringLength(500)]
		public string SuppliedMaterialTypes { get; set; }

		public virtual ICollection<MaterialSupplier> SuppliedMaterials { get; set; } = new List<MaterialSupplier>();
	}
}