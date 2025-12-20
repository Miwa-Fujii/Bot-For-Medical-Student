using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BotForMedicalStudent.Models
{
    // LINEプッシュメッセージリクエスト
    public class LinePushMessageRequest
    {
        [JsonPropertyName("to")]
        public string To { get; set; } = null!; // 送信先LINE User ID [cite: 397]

        [JsonPropertyName("messages")]
        public List<LineMessage> Messages { get; set; } = null!; // メッセージのリスト [cite: 399]
    }

    public class LineMessage
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "text"; // デフォルト値として "text" を設定 [cite: 404]

        [JsonPropertyName("text")]
        public string Text { get; set; } = null!; // 送信する本文 [cite: 406]
    }
}