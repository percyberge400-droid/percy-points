using POSPRA.Application.AutoMapperProfile;
using POSPRA.Application.Services.ClientService;
using POSPRA.Application.Services.CloudSyncService.CloudSyncInvoiceService;
using POSPRA.Application.Services.CloudSyncService.WorkerLogService;
using POSPRA.Application.Services.HttpClientService;
using POSPRA.Application.Services.NetworkService;
using POSPRA.DTOs;
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
        bool isProduction = appSettings.IsProductionWorker;

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
        // 🔧 Core & Utility Services
        //----------------------------------------------------
        services.AddSingleton<INetworkService, NetworkService>();
        services.AddHttpContextAccessor();
        services.AddHttpClient<HttpService>();

        //----------------------------------------------------
        // 🔧 Logging & Cloud Sync Services
        //----------------------------------------------------
        services.AddScoped<IWorkerLogService, WorkerLogService>();
        services.AddScoped<ISendInvoiceToCloudService, SendInvoiceToCloudService>();

        //----------------------------------------------------
        // 🔧 AutoMapper
        //----------------------------------------------------
        services.AddAutoMapper(cfg => { }, typeof(Program).Assembly, typeof(PosProfile).Assembly);

        //----------------------------------------------------
        // 🔧 Hosted Worker
        //----------------------------------------------------
        services.AddHostedService<Worker>();
        services.AddHostedService<SqliteBackupService>();
    });

    var host = builder.Build();

await host.RunAsync();
