namespace BotForMedicalStudent.Services
{
    public interface IEnvironmentService
    {
        // 環境変数を取得するための定義 
        string GetEnvironmentVariable(string name);
    }
}