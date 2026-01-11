using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BotForMedicalStudent.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddHttpClient();
        services.AddSingleton<IEnvironmentService, EnvironmentService>();
        services.AddSingleton<ITimeService, TimeService>();
        services.AddScoped<INotionService, NotionService>();
        services.AddScoped<ILineMessagingService, LineMessagingService>();
    })
    .Build();

host.Run();