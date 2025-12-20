namespace BotForMedicalStudent.Services
{
    public interface ITimeService
    {
        // 今日のJST 00:00 〜 23:59 を、Notion APIが読める形式(UTC)で返します
        (string utcStart, string utcEnd) GetJstTodayUtcRange();
    }
}