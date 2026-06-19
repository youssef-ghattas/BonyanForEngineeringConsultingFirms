using Bonyan.DAL.Enums;

namespace Bonyan.PL.ViewModels
{
	public class ProfileViewModel
	{
		// ── Common ─────────────────────────────────────────
		public string FullName { get; set; }
		public string Email { get; set; }
		public string PhoneNum { get; set; }
		public string Role { get; set; }

		// ── Employee only ──────────────────────────────────
		public bool IsEmployee { get; set; }
		public string SSN { get; set; }
		public Specialization? Specialization { get; set; }
		public Gender? Gender { get; set; }
		public DateTime? HireDate { get; set; }
		public decimal? Salary { get; set; }

		// ── Project summary ────────────────────────────────
		public List<ProfileProjectSummary> Projects { get; set; } = new();

		// ── Task stats ─────────────────────────────────────
		public int TotalTasks { get; set; }
		public int CompletedTasks { get; set; }
		public int InProgressTasks { get; set; }
		public int PendingTasks { get; set; }
	}

	public class ProfileProjectSummary
	{
		public string ProjectName { get; set; }
		public string Location { get; set; }
		public string Status { get; set; }
		public string StatusClass { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public string ClientName { get; set; }
	}
}