using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Pos.Application.DTOs;
using Pos.Infrastructure;
using POSPRA.Application.AutoMapperProfile;
using POSPRA.SecurityEncryption;
using POSPRA.Worker.Configurations;
using POSPRA.Worker.Services;


var builder = Host.CreateDefaultBuilder(args)
    .UseWindowsService()
    .ConfigureAppConfiguration((context, config) =>
    {
        config.Sources.Clear();

        // Use the helper to decrypt settings - much cleaner!
        var decryptedSettings = EncryptedSettingsHelper.DecryptSettingsFiles(
            "appsettings.json",
            "appsettings.worker.json"
        );

        // Add decrypted settings as in-memory collection
        config.AddInMemoryCollection(decryptedSettings);
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;
        services.Configure<AppSettings>(configuration);
        services.AddInfrastructure(configuration, false, true);
        services.AddAutoMapper(cfg => cfg.AddProfile<PosProfile>());
        services.AddHostedService<InvoiceWorkerSevice>();
        services.AddHostedService<LogWorkerService>();
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
var apiHost = Pos.Local.Api.Program.BuildApiHost(args);

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