namespace BotForMedicalStudent.Services
{
    public class EnvironmentService : IEnvironmentService
    {
        public string GetEnvironmentVariable(string name)
        {
            return Environment.GetEnvironmentVariable(name) ?? string.Empty;
        }
    }
}