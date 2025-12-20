using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
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
            // 設定からトークンを取得 [cite: 456]
            var token = _env.GetEnvironmentVariable("LINE_CHANNEL_ACCESS_TOKEN");

            // 送信内容（手紙）を作成 [cite: 457]
            var requestBody = new LinePushMessageRequest
            {
                To = userId,
                Messages = new System.Collections.Generic.List<LineMessage>
                {
                    new LineMessage { Text = message }
                }
            };

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}"); // [cite: 462]

            // LINE APIへPOST送信！ [cite: 460]
            var response = await _httpClient.PostAsJsonAsync(Constants.LineApiPushUrl, requestBody);
            response.EnsureSuccessStatusCode(); // [cite: 464]
        }
    }
}