using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using BotForMedicalStudent.Services;
using System;
using System.Threading.Tasks;

namespace BotForMedicalStudent
{
    public class NotifyStudyStatusFunction
    {
        private readonly INotionService _notion;
        private readonly ILineMessagingService _line;
        private readonly IEnvironmentService _env;
        private readonly ILogger _logger;

        // コンストラクタ：すべての道具をDIで一括受け取り 
        public NotifyStudyStatusFunction(INotionService notion, ILineMessagingService line, IEnvironmentService env, ILoggerFactory loggerFactory)
        {
            _notion = notion;
            _line = line;
            _env = env;
            _logger = loggerFactory.CreateLogger<NotifyStudyStatusFunction>();
        }

        // 日本時間 20:00 (UTC 11:00) に起動 [cite: 410]
        // [Function("NotifyStudyStatusFunction")]
        // public async Task Run([TimerTrigger("0 0 11 * * *")] TimerInfo myTimer)
        // {
        //     _logger.LogInformation($"実行開始: {DateTime.Now}");
        //     try 
        //     // 正常系フロー [cite: 413]
        //     {
        //         var myId = _env.GetEnvironmentVariable("MY_LINE_USER_ID");
        //         var boyfriendId = _env.GetEnvironmentVariable("BOYFRIEND_LINE_USER_ID");

        //         // Notionに勉強記録を確認 [cite: 415]
        //         bool hasStudied = await _notion.HasStudiedTodayAsync();

        //         string warningMsg = _env.GetEnvironmentVariable("WARNING_MESSAGE");
        //         string praiseMsg = _env.GetEnvironmentVariable("PRAISE_MESSAGE");
        //         // 判定結果に応じたメッセージを決定 [cite: 416]
        //         string message = hasStudied 
        //             ? praiseMsg : warningMsg;

        //         // 彼氏と自分に送信 [cite: 417, 418]
        //         await _line.SendPushMessageAsync(boyfriendId, message);
        //         await _line.SendPushMessageAsync(myId, message);
        //     }
        //     catch (Exception ex) // 異常系（エラー）フロー [cite: 420]
        //     {
        //         _logger.LogError(ex, "予期せぬエラーが発生しました");

        //         try
        //         {
        //     // 開発者（あなた）にのみエラー通知を試みる [cite: 424]
        //             var myId = _env.GetEnvironmentVariable("MY_LINE_USER_ID");
        //             await _line.SendPushMessageAsync(myId, $"システムエラーが発生しました: {ex.Message}");
        //         }
        //         catch (Exception innerEx)
        //         {
        //     // LINE送信すら失敗した場合は、Azureのログに残すのみとする [cite: 426]
        //             _logger.LogCritical(innerEx, "エラー通知の送信に失敗しました");
        //         }
        //     }
        // }
        [Function("NotifyStudyStatusFunction")]
        public async Task Run([TimerTrigger("0 30 13 * * *")] TimerInfo myTimer)
        {
            try
            {
                var myId = _env.GetEnvironmentVariable("MY_LINE_USER_ID");
                var boyfriendId = _env.GetEnvironmentVariable("BOYFRIEND_LINE_USER_ID");

                // カテゴリごとの集計結果を取得
                var studyCounts = await _notion.GetStudyCountsByCategoryAsync();

                string message;
                var todayJst = DateTime.UtcNow.AddHours(9);
                if (studyCounts.Count > 0)
                {
                    // 賞賛メッセージの組み立て
                    var sb = new System.Text.StringBuilder();

                    // 2. 日付を【yyyy-MM-dd】の形式で追加
                    sb.AppendLine($"【{todayJst:yyyy-MM-dd}】");
                    string praiseMsg = _env.GetEnvironmentVariable("PRAISE_MESSAGE");
                    sb.AppendLine(praiseMsg);
                    sb.AppendLine("\n【学習内容】");
                    foreach (var kvp in studyCounts)
                    {
                        sb.AppendLine($"・{kvp.Key}: {kvp.Value}件");
                    }
                    message = sb.ToString().TrimEnd();
                }
                else
                {
                    // 1件もない場合は、環境変数の警告メッセージを使う
                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine($"【{todayJst:yyyy-MM-dd}】");
                    sb.AppendLine(_env.GetEnvironmentVariable("WARNING_MESSAGE"));
                    message = sb.ToString().TrimEnd();
                }

                await _line.SendPushMessageAsync(boyfriendId, message);
                await _line.SendPushMessageAsync(myId, message);
            }
            catch (Exception ex) // 異常系（エラー）フロー [cite: 420]
            {
                _logger.LogError(ex, "予期せぬエラーが発生しました");

                try
                {
            // 開発者（あなた）にのみエラー通知を試みる [cite: 424]
                    var myId = _env.GetEnvironmentVariable("MY_LINE_USER_ID");
                    await _line.SendPushMessageAsync(myId, $"システムエラーが発生しました: {ex.Message}");
                }
                catch (Exception innerEx)
                {
            // LINE送信すら失敗した場合は、Azureのログに残すのみとする [cite: 426]
                    _logger.LogCritical(innerEx, "エラー通知の送信に失敗しました");
                }
            }
        }
    }
}