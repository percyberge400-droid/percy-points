using Microsoft.EntityFrameworkCore;
using POSPRA.Application.AutoMapperProfile;
using POSPRA.Application.Services.CloudSyncService.CloudSyncInvoiceService;
using POSPRA.Application.Services.CloudSyncService.WorkerLogService;
using POSPRA.Application.Services.FileRecordService;
using POSPRA.Application.Services.HttpClientService;
using POSPRA.Application.Services.LogService;
using POSPRA.Application.Services.NetworkService;
using POSPRA.DTOs;
using POSPRA.Infrastructure.Context;
using POSPRA.Repositories.FileRecordRepository;
using POSPRA.Repositories.LogRepository;
using POSPRA.Repositories.UnitOfWork;
using POSPRA.Worker;

var builder = Host.CreateDefaultBuilder(args)
    .UseWindowsService() // ✅ Allow running as Windows Service
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        // Only load worker-specific configs
        config.Sources.Clear();

        config.AddJsonFile("appsettings.worker.json", optional: false, reloadOnChange: true)
              .AddJsonFile($"appsettings.worker.{hostingContext.HostingEnvironment.EnvironmentName}.json",
                            optional: true, reloadOnChange: true)
              .AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        //----------------------------------------------------
        // 🔧 Load AppSettings section
        //----------------------------------------------------
        services.Configure<AppSettings>(context.Configuration.GetSection("AppSettings"));
        var appSettings = context.Configuration.GetSection("AppSettings").Get<AppSettings>();

        //----------------------------------------------------
        // 🔧 SQLite Database
        //----------------------------------------------------
        var dbPath = appSettings!.DefaultDBFilePath;
        services.AddDbContext<SqliteDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        //----------------------------------------------------
        // 🔧 Core Utility Services
        //----------------------------------------------------
        services.AddSingleton<INetworkService, NetworkService>();
        services.AddHttpClient<HttpService>(); // For calling external APIs
        services.AddHttpContextAccessor();
        //----------------------------------------------------
        // 🔧 Repositories
        //----------------------------------------------------
        services.AddScoped<IFileRecordRepository, FileRecordRepository>();
        services.AddScoped<ILogSQLiteRepository, LogSQLiteRepository>();
        services.AddScoped<ISqliteUnitOfWork, SqliteUnitOfWork>();

        //----------------------------------------------------
        // 🔧 Cloud Sync & Logging Services
        //----------------------------------------------------
        services.AddScoped<IWorkerLogService, WorkerLogService>();
        services.AddScoped<ISendInvoiceToCloudService, SendInvoiceToCloudService>();
        services.AddScoped<IWorkerLogService, WorkerLogService>();
        services.AddScoped<ILogService, LogService>();
        services.AddScoped<IFileRecordService, FileRecordService>();
        //----------------------------------------------------
        // 🔧 AutoMapper
        //----------------------------------------------------
        services.AddAutoMapper(cfg => { }, typeof(Program).Assembly, typeof(PosProfile).Assembly);

        //----------------------------------------------------
        // 🔧 Hosted Workers
        //----------------------------------------------------
        services.AddHostedService<Worker>();
        services.AddHostedService<SqliteBackupService>();
    });

var host = builder.Build();


// ✅ 1. Start the API self-host (runs alongside worker)
_ = Task.Run(async () =>
{
    try
    {
        var apiHost = POSPRA.API.Program.BuildApiHost(args);
        await apiHost.StartAsync();
        Console.WriteLine("API self-hosted successfully at http://localhost:5010");
    }
    catch (Exception ex)
    {
        var logPath = Path.Combine(AppContext.BaseDirectory, "api-start-error.log");
        File.AppendAllText(logPath, $"[{DateTime.Now}] {ex}\n");
    }
});


// ✅ 2. Log database path (for diagnostics)
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SqliteDbContext>();
    var dbPath = db.Database.GetDbConnection().DataSource;

    File.AppendAllText(Path.Combine(AppContext.BaseDirectory, "service-log.txt"),
        $"[{DateTime.Now}] Using existing SQLite DB: {dbPath}{Environment.NewLine}");
}


// ✅ 3. Run the worker service
await host.RunAsync();
