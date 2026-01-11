using System.Text.Json.Serialization;

namespace BotForMedicalStudent.Models
{
    public class LinePushMessageRequest
    {
        [JsonPropertyName("to")]
        public string To { get; set; } = null!;

        [JsonPropertyName("messages")]
        public List<LineMessage> Messages { get; set; } = null!;
    }

    public class LineMessage
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "text";

        [JsonPropertyName("text")]
        public string Text { get; set; } = null!;
    }
}