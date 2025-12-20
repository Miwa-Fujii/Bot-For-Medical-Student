using System.Threading.Tasks;

namespace BotForMedicalStudent.Services
{
    public interface INotionService
    {
        // 今日、勉強記録（「未着手」以外）があるかどうかを判定します
        //Task<bool> HasStudiedTodayAsync();
        // 戻り値を Dictionary（カテゴリ名と件数のセット）に変更します
        Task<Dictionary<string, int>> GetStudyCountsByCategoryAsync();
    }
}