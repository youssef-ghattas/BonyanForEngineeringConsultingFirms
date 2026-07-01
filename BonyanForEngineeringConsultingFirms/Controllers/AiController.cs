using BonyanForEngineeringConsultingFirms.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace BonyanForEngineeringConsultingFirms.Controllers
{
    public class AiController : Controller
    {
        private readonly ChatDataService _chatDataService;
        private readonly GroqService _groqService;

        public AiController(ChatDataService chatDataService, GroqService groqService)
        {
            _chatDataService = chatDataService;
            _groqService = groqService;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            var role = HttpContext.Session.GetString("Role");
            var fullName = HttpContext.Session.GetString("FullName");
            var employeeId = HttpContext.Session.GetInt32("EmployeeId");

            if (string.IsNullOrEmpty(role))
                return Json(new { success = false, message = "Session expired. Please log in again." });

            var filteredData = await _chatDataService.GetFilteredDataAsync(role, employeeId);
            var answer = await _groqService.AskAsync(filteredData, request.Message, role, fullName ?? "User");

            return Json(new { success = true, message = answer });
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}