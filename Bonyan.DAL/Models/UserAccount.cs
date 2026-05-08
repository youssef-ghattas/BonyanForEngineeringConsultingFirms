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
	[Table("User")]
	[Index(nameof(Username), IsUnique = true)]
	[Index(nameof(EmployeeId), IsUnique = true)]
	public class UserAccount
	{
		[Key]
		public int UserId { get; set; }

		[Required]
		public int EmployeeId { get; set; }

		[Required]
		public UserRole Role { get; set; }

		[Required]
		[StringLength(50)]
		public string Username { get; set; }

		[Required]
		[StringLength(255)]
		public string Password { get; set; }

		[ForeignKey("EmployeeId")]
		public virtual Employee Employee { get; set; }

		public virtual ICollection<Task> CreatedTasks { get; set; } = new List<Task>();
	}
}
