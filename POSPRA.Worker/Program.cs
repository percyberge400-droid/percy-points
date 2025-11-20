using Microsoft.EntityFrameworkCore;
using Pos.Infrastructure;
using Pos.Worker;
using Pos.Worker.Logging;
using POSPRA.Application.AutoMapperProfile;
using POSPRA.Worker;
using System.Reflection;

var builder = Host.CreateDefaultBuilder(args)
    .UseWindowsService()
    .ConfigureAppConfiguration((context, config) =>
    {
        // Clear default sources if needed
        config.Sources.Clear();

        // Load worker-specific config
        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
              .AddJsonFile("appsettings.worker.json", optional: true, reloadOnChange: true)
              .AddEnvironmentVariables();
    })


    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        // ----------------------------------------------------
        // Infrastructure & AutoMapper
        // ----------------------------------------------------
        services.AddInfrastructure(configuration,false , true);
        services.AddAutoMapper(cfg => cfg.AddProfile<PosProfile>());

        // ----------------------------------------------------
        // Worker-specific Hosted Services
        // ----------------------------------------------------
        services.AddHostedService<Worker>();
        services.AddHostedService<SqliteBackupService>();

    });
// Global exception handlers
AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
{
    FileLogger.LogException(e.ExceptionObject as Exception, "UnhandledException");
};

TaskScheduler.UnobservedTaskException += (sender, e) =>
{
    FileLogger.LogException(e.Exception, "UnobservedTaskException");
};
var host = builder.Build();

// ----------------------------------------------------
// Self-host API inside Worker
// ----------------------------------------------------

// Build API host
var apiHost = Pos.Api.Program.BuildApiHost(args);

// Ensure API reads its own appsettings.json from DLL location
var apiBasePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
Console.WriteLine($"API Base Path: {apiBasePath}");

// Start API in background without blocking Worker
_ = Task.Run(async () =>
{
    await apiHost.StartAsync();
});

// ----------------------------------------------------
// Log SQLite DB for diagnostics
// ----------------------------------------------------
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SqliteDbContext>();
    var dbPathUsed = db.Database.GetDbConnection().DataSource;

    //var logFile = Path.Combine(AppContext.BaseDirectory, "worker-service-log.txt");
    //File.AppendAllText(logFile,
    //    $"[{DateTime.Now}] Using SQLite DB: {dbPathUsed}{Environment.NewLine}");
}

// ----------------------------------------------------
// Run Worker services
// ----------------------------------------------------
await host.RunAsync();
