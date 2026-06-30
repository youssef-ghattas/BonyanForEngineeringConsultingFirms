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
	[Index(nameof(Email), IsUnique = true)]
	public class Admin
	{
		[Key]
		public int AdminId { get; set; }

		[Required(ErrorMessage = "First Name is required.")]
		[StringLength(50)]
		public string FirstName { get; set; }

		[Required(ErrorMessage = "Last Name is required.")]
		[StringLength(50)]
		public string LastName { get; set; }

		[Required(ErrorMessage = "Email is required.")]
		[EmailAddress]
		[StringLength(100)]
		public string Email { get; set; }

		[StringLength(20)]
		public string PhoneNum { get; set; }

		[Required]
		public DateTime CreatedAt { get; set; } = DateTime.Now;

		// Navigation
		public virtual AdminAccount AdminAccount { get; set; }
	}
}
