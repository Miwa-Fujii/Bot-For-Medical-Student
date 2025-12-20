using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BotForMedicalStudent.Models;

namespace BotForMedicalStudent.Services
{
    public class NotionService : INotionService
    {
        private readonly HttpClient _httpClient;
        private readonly IEnvironmentService _env;
        private readonly ITimeService _time;

        // コンストラクタ：必要な道具(HttpClient, 設定読み取り, 日付計算)をDIでもらいます
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
            var (start, end) = _time.GetJstTodayUtcRange();

        // --- 修正ポイント：フィルタ条件を「日付のみ」にします ---
            var requestBody = new NotionQueryRequest
            {
                Filter = new NotionFilter
                {
                    And = new System.Collections.Generic.List<object>
                    {
                    // 「最終更新日時」が「今日（JST）」の範囲内であること
                        new NotionDateFilterContainer { 
                            Property = "最終更新日時", // ここがNotion側の実際の項目名と合っているか確認！
                            Date = new NotionDateFilter { OnOrAfter = start } 
                        },
                        new NotionDateFilterContainer { 
                            Property = "最終更新日時", // 同上
                            Date = new NotionDateFilter { OnOrBefore = end } 
                        }
                // 【削除】ステータスのフィルタを消しました
                    }
                }
            };

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            _httpClient.DefaultRequestHeaders.Add("Notion-Version", Constants.NotionVersion);

            var response = await _httpClient.PostAsJsonAsync($"{Constants.NotionApiBaseUrl}{dbId}/query", requestBody);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<NotionQueryResponse>();
    
            var counts = new Dictionary<string, int>();
            if (result?.Results != null)
            {
                foreach (var page in result.Results)    
                {
                // プロパティ「範囲」からカテゴリ名を取得（以前の修正を反映）
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
