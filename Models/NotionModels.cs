using System.Text.Json.Serialization;

namespace BotForMedicalStudent.Models
{
    public class NotionQueryRequest
    {
        [JsonPropertyName("filter")]
        public NotionFilter Filter { get; set; } = null!;
    }

    public class NotionFilter
    {
        [JsonPropertyName("and")]
        public List<object> And { get; set; } = null!;
    }

    public class NotionDateFilterContainer
    {
        [JsonPropertyName("property")]
        //Notionのカラム名
        public string Property { get; set; } = "最終更新日時";

        [JsonPropertyName("date")]
        public NotionDateFilter Date { get; set; } = null!;
    }

    public class NotionDateFilter
    {
        [JsonPropertyName("on_or_after")]
        public string? OnOrAfter { get; set; } // JST 00:00のUTC値

        [JsonPropertyName("on_or_before")]
        public string? OnOrBefore { get; set; } // JST 23:59のUTC値
    }

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