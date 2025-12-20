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
    
    // 1. タプルを受け取ります (utcStart, utcEnd という名前になっています)
    var range = _time.GetJstTodayUtcRange();

    // 2. コンパイルエラー(CS1061)の修正：
    // 文字列として受け取り、Notionの100件問題を避けるために秒単位で整形し直します
    string startStr = DateTime.Parse(range.utcStart).ToString("yyyy-MM-ddTHH:mm:ssZ");
    string endStr = DateTime.Parse(range.utcEnd).ToString("yyyy-MM-ddTHH:mm:ssZ");

    // 3. Postmanで成功した構造を再現
    // ステータス条件は完全に削除し、日付だけで絞り込むようにしました
    var requestBody = new
    {
        filter = new
        {
            and = new object[]
            {
                new { 
                    property = "最終更新日時", 
                    date = new { on_or_after = startStr } 
                },
                new { 
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
    
    // 4. 集計ロジック
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
