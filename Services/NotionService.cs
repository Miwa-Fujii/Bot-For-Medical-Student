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

        // public async Task<bool> HasStudiedTodayAsync()
        // {
        //     var apiKey = _env.GetEnvironmentVariable("NOTION_API_KEY");
        //     var dbId = _env.GetEnvironmentVariable("NOTION_DATA_SOURCE_ID");
        //     var (start, end) = _time.GetJstTodayUtcRange();

        //     // Notion APIへのリクエスト(手紙)を作成
        //     var requestBody = new NotionQueryRequest
        //     {
        //         Filter = new NotionFilter
        //         {
        //             And = new System.Collections.Generic.List<object>
        //             {
        //                 new NotionDateFilterContainer { Date = new NotionDateFilter { OnOrAfter = start } },
        //                 new NotionDateFilterContainer { Date = new NotionDateFilter { OnOrBefore = end } },
        //                 new NotionStatusFilterContainer { Status = new NotionStatusFilter { DoesNotEqual = "未着手" } }
        //             }
        //         }
        //     };

        //     // APIを呼び出すためのヘッダー情報を設定
        //     _httpClient.DefaultRequestHeaders.Clear();
        //     _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        //     _httpClient.DefaultRequestHeaders.Add("Notion-Version", Constants.NotionVersion);

        //     // POST送信！
        //     var response = await _httpClient.PostAsJsonAsync($"{Constants.NotionApiBaseUrl}{dbId}/query", requestBody);
        //     response.EnsureSuccessStatusCode();

        //     // 結果を受け取る
        //     var result = await response.Content.ReadFromJsonAsync<NotionQueryResponse>();
            
        //     // 検索結果が1件以上あれば「勉強した」とみなす
        //     return result?.Results?.Count > 0;
        // }
        public async Task<Dictionary<string, int>> GetStudyCountsByCategoryAsync()
        {
            var apiKey = _env.GetEnvironmentVariable("NOTION_API_KEY");
            var dbId = _env.GetEnvironmentVariable("NOTION_DATA_SOURCE_ID");
            var (start, end) = _time.GetJstTodayUtcRange();

            var requestBody = new NotionQueryRequest { Filter = new NotionFilter
                {
                    And = new System.Collections.Generic.List<object>
                    {
                        new NotionDateFilterContainer { Date = new NotionDateFilter { OnOrAfter = start } },
                        new NotionDateFilterContainer { Date = new NotionDateFilter { OnOrBefore = end } },
                        new NotionStatusFilterContainer { Status = new NotionStatusFilter { DoesNotEqual = "未着手" } }
                    }
                } 
            };

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            _httpClient.DefaultRequestHeaders.Add("Notion-Version", Constants.NotionVersion);

            var response = await _httpClient.PostAsJsonAsync($"{Constants.NotionApiBaseUrl}{dbId}/query", requestBody);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<NotionQueryResponse>();
            
            // --- ここでグループ集計（Group By）を行います ---
            var counts = new Dictionary<string, int>();
            if (result?.Results != null)
            {
                foreach (var page in result.Results)
                {
                    // 「カテゴリ」が未設定の場合は「未分類」とする
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