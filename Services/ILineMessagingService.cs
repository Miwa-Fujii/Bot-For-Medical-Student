using System.Threading.Tasks;

namespace BotForMedicalStudent.Services
{
    public interface ILineMessagingService
    {
        Task SendPushMessageAsync(string userId, string message);
    }
}