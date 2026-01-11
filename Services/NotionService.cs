using System.Net.Http.Json;
using BotForMedicalStudent.Models;

namespace BotForMedicalStudent.Services
{
    public class NotionService : INotionService
    {
        private readonly HttpClient _httpClient;
        private readonly IEnvironmentService _env;
        private readonly ITimeService _time;
        public NotionService(HttpClient httpClient, IEnvironmentService env, ITimeService time)
        {
            _httpClient = httpClient;
            _env = env;
            _time = time;
        }

        public async Task<Dictionary<string, int>> GetStudyCountsByCategoryAsync()
        {
            var apiKey = _env.GetEnvironmentVariable("NOTION_API_KEY");
            var dbId = _env.GetEnvironmentVariable("NOTION_DATA_SOURCE_ID");
            var range = _time.GetJstTodayUtcRange();
            string startStr = DateTime.Parse(range.utcStart).ToString("yyyy-MM-ddTHH:mm:ssZ");
            string endStr = DateTime.Parse(range.utcEnd).ToString("yyyy-MM-ddTHH:mm:ssZ");
            var requestBody = new
            {
                filter = new
                {
                    and = new object[]
                    {
                        new
                        { 
                            property = "最終更新日時", 
                            date = new { on_or_after = startStr } 
                        },
                        new
                        { 
                            property = "最終更新日時", 
                            date = new { on_or_before = endStr } 
                        }
                    }
                }
            };

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            _httpClient.DefaultRequestHeaders.Add("Notion-Version", Constants.NotionVersion);

            var response = await _httpClient.PostAsJsonAsync(
                $"{Constants.NotionApiBaseUrl}{dbId}/query", 
                requestBody
            );
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<NotionQueryResponse>();

            var counts = new Dictionary<string, int>();
            if (result?.Results != null)
            {
                foreach (var page in result.Results)
                {
                    var categoryName = page.Properties?.Category?.Select?.Name ?? "未分類";
            
                    if (counts.ContainsKey(categoryName))
                        counts[categoryName]++;
                    else
                        counts[categoryName] = 1;
                }
            }
            return counts;
        }
    }
}
