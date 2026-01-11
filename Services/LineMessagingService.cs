using System.Net.Http.Json;
using BotForMedicalStudent.Models;

namespace BotForMedicalStudent.Services
{
    public class LineMessagingService : ILineMessagingService
    {
        private readonly HttpClient _httpClient;
        private readonly IEnvironmentService _env;

        public LineMessagingService(HttpClient httpClient, IEnvironmentService env)
        {
            _httpClient = httpClient;
            _env = env;
        }

        public async Task SendPushMessageAsync(string userId, string message)
        {
            var token = _env.GetEnvironmentVariable("LINE_CHANNEL_ACCESS_TOKEN");

            var requestBody = new LinePushMessageRequest
            {
                To = userId,
                Messages = new List<LineMessage>
                {
                    new LineMessage { Text = message }
                }
            };

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            var response = await _httpClient.PostAsJsonAsync(Constants.LineApiPushUrl, requestBody);
            response.EnsureSuccessStatusCode();
        }
    }
}