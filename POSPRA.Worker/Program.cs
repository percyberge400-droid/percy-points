using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pos.Application.DTOs;
using Pos.Infrastructure; // <-- for AddInfrastructure()
using POSPRA.Application.AutoMapperProfile;
using POSPRA.Worker;
using System.Configuration;
using System.IO;

var builder = Host.CreateDefaultBuilder(args)
    .UseWindowsService() // ✅ Allow running as Windows Service
    .ConfigureAppConfiguration((context, config) =>
    {
        // Clear default sources
        config.Sources.Clear();

        // Load worker-specific config
        config.AddJsonFile("appsettings.worker.json", optional: false, reloadOnChange: true)
              .AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;
        //----------------------------------------------------
        // 🔧 Load AppSettings
        //----------------------------------------------------
        // Bind AppSettings manually (optional, if you want immediate access)
        var appSettings = new AppSettings();
        configuration.GetSection("AppSettings").Bind(appSettings);

        // Register AppSettings for IOptions<T>
        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));

        //----------------------------------------------------
        // 🔧 Build SQLite connection path
        //----------------------------------------------------
        var dbPath = appSettings?.DefaultDBFilePath
                     ?? Path.Combine(AppContext.BaseDirectory, "POSPRA.db");

        // Optional: override DB name if needed
        dbPath = Path.Combine(Path.GetDirectoryName(dbPath) ?? AppContext.BaseDirectory, "POSPRA.db");

        //----------------------------------------------------
        // ✅ Register centralized infrastructure
        // Pass the DB path directly to AddInfrastructure
        //----------------------------------------------------
        services.AddInfrastructure(new DirectSqliteConfiguration(dbPath));

        //----------------------------------------------------
        // ✅ AutoMapper
        //----------------------------------------------------
        services.AddAutoMapper(cfg => cfg.AddProfile<PosProfile>());

        //----------------------------------------------------
        // ✅ Worker-specific Hosted Services
        //----------------------------------------------------
        services.AddHostedService<Worker>();
        services.AddHostedService<SqliteBackupService>();
    });

var host = builder.Build();

// ✅ Log SQLite DB path for diagnostics
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SqliteDbContext>();
    var dbPathUsed = db.Database.GetDbConnection().DataSource;

    var logFile = Path.Combine(AppContext.BaseDirectory, "service-log.txt");
    File.AppendAllText(logFile,
        $"[{DateTime.Now}] Using SQLite DB: {dbPathUsed}{System.Environment.NewLine}");
}

// ✅ Run Worker Service
await host.RunAsync();
