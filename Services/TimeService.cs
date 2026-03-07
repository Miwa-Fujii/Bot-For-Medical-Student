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

            // 日付の境界を午前3時とするため、現在時刻が3時未満の場合は前日扱いにする
            var targetDate = jstNow.Hour < 3 ? jstNow.Date.AddDays(-1) : jstNow.Date;

            // 今日の 03:00:00 と 翌日の 02:59:59 を計算
            var jstStart = targetDate.AddHours(3);
            var jstEnd = jstStart.AddDays(1).AddTicks(-1);

            // Notion APIが要求するUTCのISO 8601形式に変換して返却
            return (jstStart.ToUniversalTime().ToString("O"), 
                    jstEnd.ToUniversalTime().ToString("O"));
        }
    }
}