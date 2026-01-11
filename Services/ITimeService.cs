namespace BotForMedicalStudent.Services
{
    public interface ITimeService
    {
        // 今日のJST 00:00 〜 23:59 を、UTCで返します
        (string utcStart, string utcEnd) GetJstTodayUtcRange();
    }
}