// ============================================================
// ChatDataService.cs
// Chatbot support service — fetches role-filtered project data
// from the database to give the AI assistant context about what
// the current user is allowed to see.
//
// - Admin: sees all projects and all employees
// - ProjectManager / Engineer: sees only their assigned projects
//
// Property names match the main project's DAL models:
//   p.Client_Name  (not ClientName)
//   t.Task_Name    (not TaskName)
//   e.UserAccount  (not e.User)
// ============================================================

using Bonyan.DAL.Context;
using Microsoft.EntityFrameworkCore;

namespace BonyanForEngineeringConsultingFirms.Services
{
    public class ChatDataService
    {
        private readonly BonyanDbContext _context;

        public ChatDataService(BonyanDbContext context)
        {
            _context = context;
        }

        public async Task<string> GetFilteredDataAsync(string role, int? employeeId)
        {
            var sb = new System.Text.StringBuilder();

            if (role == "Admin")
            {
                var projects = await _context.Projects
                    .Include(p => p.EmployeeProjects)
                        .ThenInclude(ep => ep.Employee)
                    .Include(p => p.Tasks)
                    .Include(p => p.SiteVisits)
                        .ThenInclude(sv => sv.Employee)
                    .ToListAsync();

                sb.AppendLine("=== ALL PROJECTS ===");
                foreach (var p in projects)
                {
                    // Client_Name and Task_Name match the main project's model
                    sb.AppendLine($"Project: {p.ProjectName} | Location: {p.Location} | Status: {p.Status} | Client: {p.Client_Name} | Start: {p.StartDate} | End: {p.EndDate}");

                    sb.AppendLine("  Tasks:");
                    foreach (var t in p.Tasks)
                        sb.AppendLine($"    - {t.Task_Name} | Status: {t.Status} | Due: {t.DueDate} | Notes: {t.Notes}");

                    sb.AppendLine("  Site Visits:");
                    foreach (var sv in p.SiteVisits)
                        sb.AppendLine($"    - Visit by {sv.Employee.FirstName} {sv.Employee.LastName} on {sv.VisitDate} | Safety: {sv.SafetyStatus} | Report: {sv.Report}");

                    sb.AppendLine("  Team:");
                    foreach (var ep in p.EmployeeProjects)
                        sb.AppendLine($"    - {ep.Employee.FirstName} {ep.Employee.LastName} | Role: {ep.RoleInProject}");
                }

                // UserAccount is the navigation property in the main project's Employee model
                var allEmployees = await _context.Employees
                    .Include(e => e.UserAccount)
                    .ToListAsync();

                sb.AppendLine("\n=== ALL EMPLOYEES ===");
                foreach (var e in allEmployees)
                    sb.AppendLine($"- {e.FirstName} {e.LastName} | Email: {e.Email} | Specialization: {e.Specialization} | Role: {e.UserAccount?.Role}");
            }
            else if (employeeId != null)
            {
                var projects = await _context.Projects
                    .Include(p => p.EmployeeProjects)
                        .ThenInclude(ep => ep.Employee)
                    .Include(p => p.Tasks)
                    .Include(p => p.SiteVisits)
                        .ThenInclude(sv => sv.Employee)
                    .Where(p => p.EmployeeProjects.Any(ep => ep.EmployeeId == employeeId))
                    .ToListAsync();

                sb.AppendLine("=== YOUR ASSIGNED PROJECTS ===");
                foreach (var p in projects)
                {
                    var myRole = p.EmployeeProjects
                        .FirstOrDefault(ep => ep.EmployeeId == employeeId)?.RoleInProject;

                    sb.AppendLine($"Project: {p.ProjectName} | Location: {p.Location} | Status: {p.Status} | Client: {p.Client_Name} | Start: {p.StartDate} | End: {p.EndDate} | Your Role: {myRole}");
                    sb.AppendLine("  Tasks:");
                    foreach (var t in p.Tasks)
                        sb.AppendLine($"    - {t.Task_Name} | Status: {t.Status} | Due: {t.DueDate} | Notes: {t.Notes}");

                    sb.AppendLine("  Site Visits:");
                    foreach (var sv in p.SiteVisits)
                        sb.AppendLine($"    - Visit by {sv.Employee.FirstName} {sv.Employee.LastName} on {sv.VisitDate} | Safety: {sv.SafetyStatus} | Report: {sv.Report}");

                    sb.AppendLine("  Team:");
                    foreach (var ep in p.EmployeeProjects)
                        sb.AppendLine($"    - {ep.Employee.FirstName} {ep.Employee.LastName} | Role: {ep.RoleInProject}");
                }
            }

            return sb.ToString();
        }
    }
}