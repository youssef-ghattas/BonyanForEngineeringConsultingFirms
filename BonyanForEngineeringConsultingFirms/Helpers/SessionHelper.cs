using Bonyan.DAL.Models;
using Microsoft.AspNetCore.Http;

namespace Bonyan.PL.Helpers
{
    public static class SessionHelper
    {
        public static void SetUserSession(ISession session, UserAccount user)
        {
            session.SetInt32("UserId", user.UserId);
            session.SetInt32("EmployeeId", user.EmployeeId);
            session.SetString("Username", user.Username);
            session.SetString("Role", user.Role.ToString());
        }

        public static bool IsLoggedIn(ISession session) => session.GetInt32("UserId") != null;

        public static void ClearSession(ISession session) => session.Clear();
    }
}