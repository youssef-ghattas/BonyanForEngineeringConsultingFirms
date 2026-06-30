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
	[Index(nameof(AdminId), IsUnique = true)]
	public class AdminAccount
	{
		[Key]
		public int AdminAccountId { get; set; }

		[Required]
		public int AdminId { get; set; }

		[Required]
		[StringLength(255)]
		public string Password { get; set; }

		[Required]
		public DateTime CreatedAt { get; set; } = DateTime.Now;

		[Required]
		public bool IsFirstLogin { get; set; } = true;

		[ForeignKey("AdminId")]
		public virtual Admin Admin { get; set; }
	}
}
