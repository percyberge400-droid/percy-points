using Microsoft.EntityFrameworkCore;
using POSPRA.Application.AutoMapperProfile;
using POSPRA.Application.Services.CloudSyncService;
using POSPRA.Application.Services.ConfigurationService;
using POSPRA.Application.Services.FileRecordService;
using POSPRA.Application.Services.FiscalService;
using POSPRA.Application.Services.HelperService;
using POSPRA.Application.Services.HttpClientService;
using POSPRA.Application.Services.InvoiceService;
using POSPRA.Application.Services.LiveService;
using POSPRA.Application.Services.LogService;
using POSPRA.Application.Services.NetworkService;
using POSPRA.DTOs;
using POSPRA.Infrastructure.Context;
using POSPRA.Repositories.BaseRepository;
using POSPRA.Repositories.BaseRepository.Repository;
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
        // 🔧 Database configuration
        //----------------------------------------------------
        // SQLite
        var dbPath = SqliteDbContext.GetDbPath();
        services.AddDbContext<SqliteDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        // SQL Server
        services.AddDbContext<SqlServerDbContext>(options =>
            options.UseSqlServer(context.Configuration.GetConnectionString("SqlServerConnection")),
            ServiceLifetime.Scoped);

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
        services.AddScoped<InvoiceValidatorService>();
        services.AddSingleton<INetworkService, NetworkService>();
        services.AddScoped<ILogService, LogService>();
        services.AddScoped<IRequestHeaderService, RequestHeaderService>();
        services.AddScoped<IConfigurationService, ConfigurationService>();
        services.AddScoped<IFileRecordService, FileRecordService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<ISendInvoiceToCloudService, SendInvoiceToCloudService>();
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
    });

var host = builder.Build();

// ✅ Ensure SQLite DB is created with all tables before the worker starts
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SqliteDbContext>();
    db.Database.EnsureCreated();   // Creates DB + tables if missing
                                   // If you use migrations instead of EnsureCreated, call:
                                   // db.Database.Migrate();

    // Optional: log the path
    var dbPath = db.Database.GetDbConnection().DataSource;
    File.AppendAllText(Path.Combine(AppContext.BaseDirectory, "service-log.txt"),
        $"[{DateTime.Now}] Database ensured at: {dbPath}{Environment.NewLine}");
}

await host.RunAsync();
