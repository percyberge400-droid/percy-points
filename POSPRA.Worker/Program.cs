using Microsoft.EntityFrameworkCore;
using Pos.Application.DTOs;
using Pos.Infrastructure; // <-- for AddInfrastructure()
using Pos.Worker;
using POSPRA.Application.AutoMapperProfile;
using POSPRA.Worker;

var builder = Host.CreateDefaultBuilder(args)
    .UseWindowsService()
    .ConfigureAppConfiguration((context, config) =>
    {
        // Clear default sources if needed
        config.Sources.Clear();

        // Load worker-specific config, fallback to default
        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
              .AddJsonFile("appsettings.worker.json", optional: true, reloadOnChange: true)
              .AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        //----------------------------------------------------
        // 🔧 Register Infrastructure using builder.Configuration
        //----------------------------------------------------
        services.AddInfrastructure(configuration); // ✅ Same as API program

        //----------------------------------------------------
        // 🔧 Load AppSettings
        //----------------------------------------------------
        var appSettings = configuration.GetSection("AppSettings").Get<AppSettings>()
                          ?? throw new InvalidOperationException("AppSettings section missing.");

        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));

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

    var logFile = Path.Combine(AppContext.BaseDirectory, "worker-service-log.txt");
    File.AppendAllText(logFile,
        $"[{DateTime.Now}] Using SQLite DB: {dbPathUsed}{System.Environment.NewLine}");
}

// ✅ Run Worker Service
await host.RunAsync();
