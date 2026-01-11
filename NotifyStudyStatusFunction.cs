using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using BotForMedicalStudent.Services;
using Microsoft.Azure.Functions.Worker.Http;

namespace BotForMedicalStudent
{
    public class NotifyStudyStatusFunction
    {
        private readonly INotionService _notion;
        private readonly ILineMessagingService _line;
        private readonly IEnvironmentService _env;
        private readonly ILogger _logger;
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
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            // 関数実行のトリガーとなる、LINEからのテキストメッセージ
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
                var studyCounts = await _notion.GetStudyCountsByCategoryAsync();
                var todayJst = DateTime.UtcNow.AddHours(9);
                string message;
                if (studyCounts.Count > 0)
                //studyCountsが1:勉強した、0:勉強していない
                {
                    message = CreatePraiseMessage(todayJst, studyCounts, _env);
                }
                else
                {
                    message = CreateWarningMessage(todayJst, _env);
                }

                await _line.SendPushMessageAsync(_env.GetEnvironmentVariable("BOYFRIEND_LINE_USER_ID"), message);
                await _line.SendPushMessageAsync(_env.GetEnvironmentVariable("MY_LINE_USER_ID"), message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "予期せぬエラーが発生しました");
                try
                {
                    // 開発者にのみエラー通知
                    var myId = _env.GetEnvironmentVariable("MY_LINE_USER_ID");
                    await _line.SendPushMessageAsync(myId, $"システムエラーが発生しました: {ex.Message}");
                }
                catch (Exception innerEx)
                {
                    // LINE送信すら失敗した場合は、Azureのログに残すのみとする
                    _logger.LogCritical(innerEx, "エラー通知の送信に失敗しました");
                }
            }
        }

        private string CreatePraiseMessage(DateTime todayJst, Dictionary<string, int> studyCounts, IEnvironmentService env)
        {
            int sum = 0;
            foreach (var kvp in studyCounts)
            {
                sum += kvp.Value;
            }
            string praiseMsg = SelectPraiseMessageByCount(sum, env);
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"【{todayJst:yyyy-MM-dd}】");
            sb.AppendLine(praiseMsg);
            sb.AppendLine("\n【学習内容】");
            foreach (var kvp in studyCounts)
            {
                sb.AppendLine($"・{kvp.Key}: {kvp.Value}件");
            }
            return sb.ToString().TrimEnd();
        }

        private string SelectPraiseMessageByCount(int count, IEnvironmentService env)
        {
            if (count >= 41)
            {
                return env.GetEnvironmentVariable("MSG_ABOVE_41");
            }
            else if (count >= 31)
            {
                return env.GetEnvironmentVariable("MSG_FROM_31_TO_40");
            }
            else if (count >= 21)
            {
                return env.GetEnvironmentVariable("MSG_FROM_21_TO_30");
            }
            else if (count >= 11)
            {
                return env.GetEnvironmentVariable("MSG_FROM_11_TO_20");
            }
            else
            {
                return env.GetEnvironmentVariable("MSG_FROM_1_TO_10");
            }
        }

        private string CreateWarningMessage(DateTime todayJst, IEnvironmentService env)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"【{todayJst:yyyy-MM-dd}】 {todayJst:HH:mm}");
            sb.AppendLine(env.GetEnvironmentVariable("WARNING_MESSAGE"));
            return sb.ToString().TrimEnd();
        }
    }
}