using System;

namespace BotForMedicalStudent.Services
{
    public class EnvironmentService : IEnvironmentService
    {
        public string GetEnvironmentVariable(string name)
        {
            // アプリケーション設定から値を取得する処理 [cite: 340, 466]
            return Environment.GetEnvironmentVariable(name) ?? string.Empty;
        }
    }
}