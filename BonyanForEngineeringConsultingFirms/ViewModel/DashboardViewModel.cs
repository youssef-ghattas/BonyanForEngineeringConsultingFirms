// ViewModels/DashboardViewModel.cs
using Bonyan.DAL.Enums;

namespace BonyanForEngineeringConsultingFirms.ViewModels
{
    public class DashboardViewModel
    {
        // ── Top cards ──────────────────────────────────
        public int TotalProjects { get; set; }
        public int ActiveTasks { get; set; }
        public int TotalClients { get; set; }
        public int TotalDocuments { get; set; }
        public decimal TotalBudget { get; set; }

        // ── Bar chart (projects by status) ─────────────
        public int ProjPlanning { get; set; }
        public int ProjInProgress { get; set; }
        public int ProjOnHold { get; set; }
        public int ProjCompleted { get; set; }
        public int ProjCancelled { get; set; }

        // ── Doughnut chart (tasks by status) ───────────
        public int TaskPending { get; set; }
        public int TaskInProgress { get; set; }
        public int TaskUnderReview { get; set; }
        public int TaskCompleted { get; set; }
        public int TaskCancelled { get; set; }

        // ── Tables ─────────────────────────────────────
        public List<RecentProjectRow> RecentProjects { get; set; } = new();
        public List<OverdueTaskRow> OverdueTasks { get; set; } = new();

        public bool IsAdmin { get; set; }
    }

    public class RecentProjectRow
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = "";
        public string ClientName { get; set; } = "";
        public ProjectStatus Status { get; set; }
        public int TaskCount { get; set; }
    }

    public class OverdueTaskRow
    {
        public int TaskId { get; set; }
        public string TaskName { get; set; } = "";
        public string ProjectName { get; set; } = "";
        public DateTime DueDate { get; set; }
    }
}