using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Bonyan.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace Bonyan.DAL.Models
{
	[Table("UserAccounts")]
	[Index(nameof(EmployeeId), IsUnique = true)]
	public class UserAccount
	{
		[Key]
		public int UserId { get; set; }

		[Required]
		public int EmployeeId { get; set; }

		[Required]
		public UserRole Role { get; set; } = UserRole.Engineer;

		[Required]
		[StringLength(255)]
		public string Password { get; set; }

		[Required]
		public DateTime CreatedAt { get; set; } = DateTime.Now;

		[ForeignKey("EmployeeId")]
		public virtual Employee Employee { get; set; }

		public virtual ICollection<Bonyan.DAL.Models.Task> CreatedTasks { get; set; }
			= new List<Bonyan.DAL.Models.Task>();
	}
}
