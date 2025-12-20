using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BotForMedicalStudent.Services; // これから作成するサービス用の名前空間

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        // Application Insights（ログ機能）の有効化
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        // HTTP通信を行うための部品（HttpClientFactory）を登録 
        services.AddHttpClient();

        // --- 独自サービスの登録 (依存関係の注入) [cite: 339, 340] ---
        // 「このインターフェースを使いたい時は、このクラスを呼び出してね」という予約
        services.AddSingleton<IEnvironmentService, EnvironmentService>();
        services.AddSingleton<ITimeService, TimeService>();
        services.AddScoped<INotionService, NotionService>();
        services.AddScoped<ILineMessagingService, LineMessagingService>();
    })
    .Build();

host.Run();