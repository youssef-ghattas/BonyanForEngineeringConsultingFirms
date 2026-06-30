using Bonyan.DAL.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Bonyan.DAL.Models
{
	public class Task
	{
		[Key]
		public int TaskId { get; set; }

		[Required]
		[StringLength(150)]
		public string Task_Name { get; set; }

		[Required]
		public int ProjectId { get; set; }
		public int? CreatedBy_UserID { get; set; }

		[Required]
		public DateTime CreatedAt { get; set; } = DateTime.Now;

		[Required]
		public TasksStatus Status { get; set; } = TasksStatus.Pending;

		[DataType(DataType.Date)]
		public DateTime? DueDate { get; set; }

		public string Notes { get; set; }

        public int? AssignedToEmployeeId { get; set; }

        [ForeignKey("AssignedToEmployeeId")]
        public virtual Employee AssignedToEmployee { get; set; }

        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; }

        [ForeignKey("CreatedBy_UserID")]
		public virtual UserAccount Creator { get; set; }

		public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
		public virtual ICollection<Drawing> Drawings { get; set; } = new List<Drawing>();
		public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
		public virtual ICollection<MaterialTask> MaterialTasks { get; set; } = new List<MaterialTask>();
	}
}
