using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using BotForMedicalStudent.Services;
using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;

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

        [Function("NotifyStudyStatusTimer")]
        public async Task RunTimer([TimerTrigger("0 30 13 * * *")] TimerInfo myTimer)
        {
            await ExecuteNotificationAsync();
        }

        [Function("NotifyStudyStatusHttp")]
        public async Task<HttpResponseData> RunHttp([HttpTrigger(AuthorizationLevel.Function, "post", Route = "notify-status")] HttpRequestData req)
        {
            // リクエストのボディ（中身）を読み取る
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            if (requestBody.Contains("集計"))
            {
                await ExecuteNotificationAsync();
            }
            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            return response;
        }

        /// <summary>
        /// 通知処理実行本体（共通ロジック）
        /// </summary>
        private async Task ExecuteNotificationAsync()
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
                //studyCountsが1:勉強した、0:勉強していない
                {
                    // 賞賛メッセージの組み立て
                    var sb = new System.Text.StringBuilder();

                    // 2. 日付を【yyyy-MM-dd】の形式で追加
                    sb.AppendLine($"【{todayJst:yyyy-MM-dd}】");

                    //TODO: 賞賛メッセージ作成を関数化する
                    //TODO: 賞賛メッセージを環境変数からの取得でいいのか考える
                    int sum = 0;
                    foreach (var kvp in studyCounts)
                    {
                        sum += kvp.Value;
                    }
                    string praiseMsg = _env.GetEnvironmentVariable("PRAISE_MESSAGE");
                    if(sum<=10){
                        praiseMsg = _env.GetEnvironmentVariable("MSG_FROM_1_TO_10");
                    }
                    else if(sum<=20){
                        praiseMsg = _env.GetEnvironmentVariable("MSG_FROM_11_TO_20");
                    }
                    else if(sum<=30){
                        praiseMsg = _env.GetEnvironmentVariable("MSG_FROM_21_TO_30");
                    }
                    else if(sum<=40){
                        praiseMsg = _env.GetEnvironmentVariable("MSG_FROM_31_TO_40");
                    }
                    else{
                        //41問以上解いた場合
                        praiseMsg = _env.GetEnvironmentVariable("MSG_ABOVE_41");
                    }
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