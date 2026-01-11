using System.Threading.Tasks;

namespace BotForMedicalStudent.Services
{
    public interface INotionService
    {
        Task<Dictionary<string, int>> GetStudyCountsByCategoryAsync();
    }
}