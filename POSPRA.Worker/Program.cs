using Microsoft.EntityFrameworkCore;
using POSPRA.Application.AutoMapperProfile;
using POSPRA.Application.Services.ClientService;
using POSPRA.Application.Services.CloudSyncService.CloudSyncInvoiceService;
using POSPRA.Application.Services.CloudSyncService.CloudSyncLogService;
using POSPRA.Application.Services.CloudSyncService.WorkerLogService;
using POSPRA.Application.Services.ConfigurationService;
using POSPRA.Application.Services.FileRecordService;
using POSPRA.Application.Services.FiscalService;
using POSPRA.Application.Services.HelperService;
using POSPRA.Application.Services.HttpClientService;
using POSPRA.Application.Services.InvoiceService;
using POSPRA.Application.Services.LiveService;
using POSPRA.Application.Services.LogService;
using POSPRA.Application.Services.NetworkService;
using POSPRA.Application.Services.ScriptService;
using POSPRA.DTOs;
using POSPRA.Infrastructure.Context;
using POSPRA.Repositories.BaseRepository;
using POSPRA.Repositories.BaseRepository.Repository;
using POSPRA.Repositories.ClientRepository;
using POSPRA.Repositories.ConfigurationRepository;
using POSPRA.Repositories.FileRecordRepository;
using POSPRA.Repositories.LogRepository;
using POSPRA.Repositories.ProductCatalogueRepository;
using POSPRA.Repositories.UnitOfWork;
using POSPRA.Repositories.UserRepository;
using POSPRA.Worker;

var builder = Host.CreateDefaultBuilder(args)
    .UseWindowsService() // ✅ Run as Windows Service
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        // Clear default configs to avoid conflicts with API project files
        config.Sources.Clear();

        // Load only the Worker’s configuration files
        config.AddJsonFile("appsettings.worker.json", optional: false, reloadOnChange: true)
              .AddJsonFile($"appsettings.worker.{hostingContext.HostingEnvironment.EnvironmentName}.json",
                            optional: true, reloadOnChange: true)
              .AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        //----------------------------------------------------
        // 🔧 Read SQLite DB file path from AppSettings
        //----------------------------------------------------
        var appSettings = context.Configuration.GetSection("AppSettings").Get<AppSettings>();
        var dbPath = appSettings!.DefaultDBFilePath;
        bool isProduction = appSettings.IsProduction;

        //----------------------------------------------------
        // 🔧 Database configuration
        //----------------------------------------------------

        // Choose the SQL Server connection string based on the flag
        string sqlConnectionString = context.Configuration.GetConnectionString(
            isProduction ? "SqlServerConnectionProduction" : "SqlServerConnectionSandbox"
        ) ?? throw new InvalidOperationException("Missing SQL Server connection string for the selected environment for worker.");

        // SQLite (use existing DB)
        services.AddDbContext<SqliteDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        // SQL Server
        services.AddDbContext<SqlServerDbContext>(opt =>
            opt.UseSqlServer(sqlConnectionString));

        //----------------------------------------------------
        // 🔧 AppSettings / API settings
        //----------------------------------------------------
        services.Configure<AppSettings>(context.Configuration.GetSection("AppSettings"));

        //----------------------------------------------------
        // 🔧 Repository & Unit of Work
        //----------------------------------------------------
        services.AddScoped<ISqliteUnitOfWork, SqliteUnitOfWork>();
        services.AddScoped<ISqlServerUnitOfWork, SqlServerUnitOfWork>();

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped(typeof(SqlServerRepository<>), typeof(SqlServerRepository<>));
        services.AddScoped(typeof(SqliteRepository<>), typeof(SqliteRepository<>));
        services.AddScoped<ILogSQLiteRepository, LogSQLiteRepository>();
        services.AddScoped<ILogSQLServerRepository, LogSQLServerRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IFileRecordRepository, FileRecordRepository>();
        services.AddScoped<IProductCatalogueSQLiteRepository, ProductCatalogueSQLiteRepository>();
        services.AddScoped<IConfigurationRepository, ConfigurationRepository>();

        //----------------------------------------------------
        // 🔧 Services
        //----------------------------------------------------
        services.AddScoped<ILiveService, LiveService>();
        services.AddScoped<IFileRecordService, FileRecordService>();
        services.AddScoped<InvoiceValidatorService>();
        services.AddSingleton<INetworkService, NetworkService>();
        services.AddScoped<ILogService, LogService>();
        services.AddScoped<IRequestHeaderService, RequestHeaderService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IConfigurationService, ConfigurationService>();
        services.AddScoped<IWorkerLogService, WorkerLogService>();
        services.AddScoped<ISendInvoiceToCloudService, SendInvoiceToCloudService>();
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<ISendLogToCloudService, SendLogToCloudService>();
        services.AddScoped<IScriptService, ScriptService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddHttpContextAccessor();

        //----------------------------------------------------
        // 🔧 AutoMapper
        //----------------------------------------------------
        services.AddAutoMapper(cfg => { }, typeof(Program).Assembly, typeof(PosProfile).Assembly);

        //----------------------------------------------------
        // 🔧 HttpClient
        //----------------------------------------------------
        services.AddHttpClient<HttpService>();

        //----------------------------------------------------
        // 🔧 Hosted Worker
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

        //File.AppendAllText(Path.Combine(AppContext.BaseDirectory, "service-log.txt"),
        //    $"[{DateTime.Now}] Using existing SQLite DB: {dbPath}{Environment.NewLine}");
    }

    // ✅ 3. Run the worker service
    await host.RunAsync();