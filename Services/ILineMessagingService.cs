using System.Threading.Tasks;

namespace BotForMedicalStudent.Services
{
    public interface ILineMessagingService
    {
        // 指定されたユーザーIDへ、メッセージを送信します
        Task SendPushMessageAsync(string userId, string message);
    }
}