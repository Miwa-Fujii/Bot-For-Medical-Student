using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BotForMedicalStudent.Models
{
    // Notion APIへのクエリリクエスト
    public class NotionQueryRequest
    {
        [JsonPropertyName("filter")]
        public NotionFilter Filter { get; set; } = null!; // 必ず値を入れるので null! を使用
    }

    public class NotionFilter
    {
        // "and": [ {日付フィルタ1}, {日付フィルタ2}, {ステータスフィルタ} ]
        [JsonPropertyName("and")]
        public List<object> And { get; set; } = null!; // 検索条件リスト
    }

    // 日付フィルタ用
    public class NotionDateFilterContainer
    {
        [JsonPropertyName("property")]
        public string Property { get; set; } = "最終更新日時";

        [JsonPropertyName("date")]
        public NotionDateFilter Date { get; set; } = null!;
    }

    public class NotionDateFilter
    {
        [JsonPropertyName("on_or_after")]
        public string? OnOrAfter { get; set; } // JST 00:00のUTC値、null許容[cite: 368]

        [JsonPropertyName("on_or_before")]
        public string? OnOrBefore { get; set; } // JST 23:59のUTC値[cite: 370]
    }

    // ステータスフィルタ用
    public class NotionStatusFilterContainer
    {
        [JsonPropertyName("property")]
        public string Property { get; set; } = "ステータス";//カラム名

        [JsonPropertyName("status")]
        public NotionStatusFilter Status { get; set; } = null!;
    }

    public class NotionStatusFilter
    {
        [JsonPropertyName("does_not_equal")]
        public string DoesNotEqual { get; set; } = "未着手"; // 除外する値を設定 [cite: 383]
    }

    // Notion API レスポンスを受け取るクラス
    // public class NotionQueryResponse
    // {
    //     [JsonPropertyName("results")]
    //     public List<object> Results { get; set; } = null!; // 結果が入るリスト,件数取得用 [cite: 390]
    // }
    public class NotionQueryResponse
    {
        [JsonPropertyName("results")]
        public List<NotionPage> Results { get; set; } = new();
    }

    public class NotionPage
    {
        [JsonPropertyName("properties")]
        public NotionProperties Properties { get; set; } = null!;
    }

    public class NotionProperties
    {
        [JsonPropertyName("範囲")]
        public NotionSelectProperty? Category { get; set; }
    }

    public class NotionSelectProperty
    {
        [JsonPropertyName("select")]
        public NotionSelectOption? Select { get; set; }
    }

    public class NotionSelectOption
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;
    }
}