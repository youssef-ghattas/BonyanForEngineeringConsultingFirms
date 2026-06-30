// ============================================================
// GroqService.cs
// Chatbot support service — sends the user's question along with
// their role-filtered data context to the Groq LLM API and
// returns the AI's answer as a plain string.
// ============================================================

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BonyanForEngineeringConsultingFirms.Services   // ← changed from BonyanChatbot.Services
{
    public class GroqService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public GroqService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<string> AskAsync(string dataContext, string userQuestion, string role, string userName)
        {
            var apiKey = _config["GroqSettings:ApiKey"];
            var model = _config["GroqSettings:Model"];

            var systemPrompt = $@"
You are Bonyan Assistant, a helpful AI for an engineering consulting firm management system.
The user logged in is: {userName} with role: {role}.
You ONLY answer based on the data provided to you below.
You NEVER reveal sensitive data like salaries, SSN, passwords, or financial amounts.
When showing lists or tables of data, format them clearly.
If the user asks something not in the data, say you don't have that information.

=== DATA YOU CAN USE ===
{dataContext}
=== END OF DATA ===
";

            var messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userQuestion }
            };

            var requestBody = new { model, max_tokens = 1024, messages };
            var json = JsonSerializer.Serialize(requestBody);
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return $"Error from Groq API: {responseBody}";

            using var doc = JsonDocument.Parse(responseBody);
            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "No response";
        }
    }
}