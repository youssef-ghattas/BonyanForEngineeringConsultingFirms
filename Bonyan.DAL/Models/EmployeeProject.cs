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
	[PrimaryKey(nameof(EmployeeId), nameof(ProjectId))]
	public class EmployeeProject
	{
		public int EmployeeId { get; set; }
		public int ProjectId { get; set; }

		[Required]
		[DataType(DataType.Date)]
		public DateTime AssignmentDate { get; set; } = DateTime.Now;

		[StringLength(100)]
		public string RoleInProject { get; set; }

		[ForeignKey("EmployeeId")]
		public virtual Employee Employee { get; set; }

		[ForeignKey("ProjectId")]
		public virtual Project Project { get; set; }
	}
}
