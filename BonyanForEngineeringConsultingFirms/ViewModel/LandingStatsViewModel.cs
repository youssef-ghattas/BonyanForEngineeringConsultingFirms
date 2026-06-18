namespace BonyanForEngineeringConsultingFirms.ViewModels
{
    public class LandingStatsViewModel
    {
        // Card 1 (Blue)
        public int TotalSubscribedOffices { get; set; }

        // Card 2 (Green)
        public int TotalCompletedProjects { get; set; }

        // Card 3 (Gold/Yellow)
        public int TotalProjects { get; set; }

        // Card 4 (Red)
        public int TotalDocuments { get; set; }

        // Progress Bar percentages (0-100)
        public double PlanningPercent { get; set; }
        public double InProgressPercent { get; set; }
        public double CompletedPercent { get; set; }
        public double OnHoldPercent { get; set; }

        // Raw counts for tooltips/labels
        public int ProjectsPlanning { get; set; }
        public int ProjectsInProgress { get; set; }
        public int ProjectsOnHold { get; set; }
    }
}