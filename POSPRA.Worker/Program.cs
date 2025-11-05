using POSPRA.Application.AutoMapperProfile;
using POSPRA.Application.Services.CloudSyncService.CloudSyncInvoiceService;
using POSPRA.Application.Services.CloudSyncService.WorkerLogService;
using POSPRA.Application.Services.HttpClientService;
using POSPRA.Application.Services.NetworkService;
using POSPRA.DTOs;
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
        // 🔧 Core Utility Services
        //----------------------------------------------------
        services.AddSingleton<INetworkService, NetworkService>();
        services.AddHttpClient<HttpService>(); // For calling external APIs
        services.AddHttpContextAccessor();

        //----------------------------------------------------
        // 🔧 Cloud Sync & Logging Services
        //----------------------------------------------------
        services.AddScoped<IWorkerLogService, WorkerLogService>();
        services.AddScoped<ISendInvoiceToCloudService, SendInvoiceToCloudService>();

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
await host.RunAsync();
