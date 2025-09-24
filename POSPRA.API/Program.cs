using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.EntityFrameworkCore;
using POSPRA.Application.AutoMapperProfile;
using POSPRA.Application.Services.FiscalService;
using POSPRA.Application.Services.HelperService;
using POSPRA.Application.Services.HttpClientService;
using POSPRA.Application.Services.LiveService;
using POSPRA.Application.Services.LogService;
using POSPRA.Application.Services.NetworkService;
using POSPRA.Application.Services.PosService;
using POSPRA.Application.Services.POSService;
using POSPRA.Application.Services.ProductCatalogService;
using POSPRA.Application.Services.UserService;
using POSPRA.DTOs;
using POSPRA.Infrastructure.Context;
using POSPRA.Repositories.BaseRepository;
using POSPRA.Repositories.BaseRepository.Repository;
using POSPRA.Repositories.FiscalRepository;
using POSPRA.Repositories.LogRepository;
using POSPRA.Repositories.PosRepository;
using POSPRA.Repositories.ProductCatalogueRepository;
using POSPRA.Repositories.UnitOfWork;
using POSPRA.Repositories.UserRepository;

namespace POSPRA.API   // ✅ Added namespace so other projects can reference it
{
    public static class Program
    {
        /// <summary>
        /// Entry point when running as a normal Web API (Kestrel, IIS, etc.).
        /// </summary>
        public static async Task Main(string[] args)
        {
            var apiHost = BuildApiHost();
            apiHost.Urls.Add("http://localhost:5000/");
            await apiHost.StartAsync();

            // Wait here until shutdown is triggered (Ctrl+C, SIGTERM)
            await apiHost.WaitForShutdownAsync();


        }

        /// <summary>
        /// Creates the fully configured WebApplication.
        /// WinForms or Worker can call this to start the API in-process.
        /// </summary>
        public static WebApplication BuildApiHost(string[]? args = null)
        {
            var builder = WebApplication.CreateBuilder(args ?? Array.Empty<string>());

            builder.Services.AddControllers()
                .PartManager.ApplicationParts.Add(new Microsoft.AspNetCore.Mvc.ApplicationParts.AssemblyPart(typeof(FiscalController).Assembly));
            // Force Kestrel to bind to the URL externally
            // Change this line to:
            builder.WebHost.UseUrls("http://0.0.0.0:5000");
            //----------------------------------------------------
            // 🔧 Database configuration
            //----------------------------------------------------
            var dbPath = SqliteDbContext.GetDbPath();
            builder.Services.AddDbContext<SqliteDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            builder.Services.AddDbContext<SqlServerDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection")),
                ServiceLifetime.Scoped);

            //----------------------------------------------------
            // 🔧 Dependency Injection
            //----------------------------------------------------
            builder.Services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<UserProfile>();
                cfg.AddProfile<PosProfile>();
            });

            builder.Services.AddScoped<ISqliteUnitOfWork, SqliteUnitOfWork>();
            builder.Services.AddScoped<ISqlServerUnitOfWork, SqlServerUnitOfWork>();

            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            builder.Services.AddScoped(typeof(SqlServerRepository<>));
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IFiscalRepository, FiscalRepository>();
            builder.Services.AddScoped<ILogRepository, LogRepository>();
            builder.Services.AddScoped<IPosClientRepository, PosClientRepository>();
            builder.Services.AddScoped<IProductCatalogueRepository, ProductCatalogueRepository>();

            // Application services
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IFiscalService, FiscalService>();
            builder.Services.AddScoped<ILogService, LogService>();
            builder.Services.AddScoped<IPosService, PosService>();
            builder.Services.AddScoped<InvoiceValidatorService>();
            builder.Services.AddScoped<IRequestHeaderService, RequestHeaderService>();
            builder.Services.AddScoped<ILiveService, LiveService>();
            builder.Services.AddScoped<INetworkService, NetworkService>();
            builder.Services.AddScoped<IProductCatalogueService, ProductCatalogueService>();

            // Http client
            builder.Services.AddHttpClient<HttpService>();

            builder.Services.AddHttpContextAccessor();
            builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

            //----------------------------------------------------
            // 🔧 Web / API configuration
            //----------------------------------------------------
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddHealthChecks();

            //----------------------------------------------------
            // ✅ Hangfire configuration  (ADD THESE LINES)
            //----------------------------------------------------
            builder.Services.AddHangfire(cfg =>
            {
                cfg.UseMemoryStorage();          // or UseSqlServerStorage for production
            });
            builder.Services.AddHangfireServer();
            //----------------------------------------------------
            // 🔧 Build and middleware
            //----------------------------------------------------
            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();

                // Redirect root to Swagger UI
                app.Use(async (context, next) =>
                {
                    if (context.Request.Path == "/")
                    {
                        context.Response.Redirect("/swagger");
                        return;
                    }
                    await next();
                });
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            // ✅ Now this works because services were registered above
            app.UseHangfireDashboard();
            app.MapControllers();
            app.MapHealthChecks("/health");

            return app;
        }
    }
}