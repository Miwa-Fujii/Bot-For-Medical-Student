using System;

namespace BotForMedicalStudent.Services
{
    public class TimeService : ITimeService
    {
        public (string utcStart, string utcEnd) GetJstTodayUtcRange()
        {
            // 日本時間(JST)のタイムゾーンを取得
            var jstZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Tokyo");
            var jstNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, jstZone);

            // 今日の 00:00:00 と 23:59:59 を計算
            var jstStart = jstNow.Date;
            var jstEnd = jstStart.AddDays(1).AddTicks(-1);

            // Notion APIが要求するUTCのISO 8601形式に変換して返却
            return (jstStart.ToUniversalTime().ToString("O"), 
                    jstEnd.ToUniversalTime().ToString("O"));
        }
    }
}