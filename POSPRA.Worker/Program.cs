using Microsoft.EntityFrameworkCore;
using POSPRA.Application.AutoMapperProfile;
using POSPRA.Application.Services.FiscalService;
using POSPRA.Application.Services.HelperService;
using POSPRA.Application.Services.HttpClientService;
using POSPRA.Application.Services.LiveService;
using POSPRA.Application.Services.LogService;
using POSPRA.Application.Services.NetworkService;
using POSPRA.DTOs;
using POSPRA.Infrastructure.Context;
using POSPRA.Repositories.BaseRepository;
using POSPRA.Repositories.BaseRepository.Repository;
using POSPRA.Repositories.FiscalRepository;
using POSPRA.Repositories.LogRepository;
using POSPRA.Repositories.PosRepository;
using POSPRA.Repositories.UnitOfWork;
using POSPRA.Repositories.UserRepository;
using POSPRA.Worker;


var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        //----------------------------------------------------
        // 🔧 Database configuration
        //----------------------------------------------------
        // SQLite
        var dbPath = SqliteDbContext.GetDbPath();
        services.AddDbContext<SqliteDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        // SQL Server  ➜ must match the API connection string name
        services.AddDbContext<SqlServerDbContext>(options =>
            options.UseSqlServer(context.Configuration.GetConnectionString("SqlServerConnection")),
            ServiceLifetime.Scoped);

        //----------------------------------------------------
        // 🔧 AppSettings / API settings
        //----------------------------------------------------
        services.Configure<AppSettings>(context.Configuration.GetSection("ApiSettings"));

        //----------------------------------------------------
        // 🔧 Repository & Unit of Work
        //----------------------------------------------------
        services.AddScoped<ISqliteUnitOfWork, SqliteUnitOfWork>();
        services.AddScoped<ISqlServerUnitOfWork, SqlServerUnitOfWork>();

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped(typeof(SqlServerRepository<>), typeof(SqlServerRepository<>)); // <-- add
        services.AddScoped(typeof(SqliteRepository<>), typeof(SqliteRepository<>));      // <-- add
        services.AddScoped<ILogRepository, LogRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IFiscalRepository, FiscalRepository>();
        services.AddScoped<IPosClientRepository, PosClientRepository>();
        services.AddScoped<IFiscalRepository, FiscalRepository>();
        services.AddScoped<IRequestHeaderService, RequestHeaderService>();

        // Services
        services.AddScoped<ILiveService, LiveService>();   // <-- add
        services.AddScoped<IFiscalService, FiscalService>();
        services.AddScoped<InvoiceValidatorService>(); // <-- Add this
        services.AddSingleton<INetworkService, NetworkService>();

        services.AddHttpClient<HttpService>();

        // ---------- HttpContextAccessor ----------
        services.AddHttpContextAccessor();   // <--- add this
        //----------------------------------------------------
        // 🔧 Services
        //----------------------------------------------------
        services.AddScoped<ILogService, LogService>();
        // register any other services the Worker might call (FiscalService, etc.)

        //----------------------------------------------------
        // 🔧 AutoMapper
        //----------------------------------------------------
        // Scan the assemblies that contain your profiles
        services.AddAutoMapper(cfg => { }, typeof(Program).Assembly, typeof(PosProfile).Assembly);

        //----------------------------------------------------
        // 🔧 HttpClient
        //----------------------------------------------------
        services.AddHttpClient("SelfHostedApi", client =>
        {
            client.BaseAddress = new Uri("http://localhost:5000/");
        });

        //----------------------------------------------------
        // 🔧 Hosted Worker
        //----------------------------------------------------
        services.AddHostedService<Worker>();
    })
    .UseConsoleLifetime();

var host = builder.Build();

// Start the self-hosted API in the background
var apiHost = POSPRA.API.Program.BuildApiHost();
await apiHost.StartAsync();

var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStopping.Register(() =>
{
    apiHost.StopAsync().GetAwaiter().GetResult();
});

await host.RunAsync();
