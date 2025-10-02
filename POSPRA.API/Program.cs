//using Hangfire;
//using Hangfire.MemoryStorage;
//using Microsoft.EntityFrameworkCore;
//using POSPRA.API.Middlewares;
//using POSPRA.Application.AutoMapperProfile;
//using POSPRA.Application.Services.ClientService;
//using POSPRA.Application.Services.FiscalService;
//using POSPRA.Application.Services.HelperService;
//using POSPRA.Application.Services.HttpClientService;
//using POSPRA.Application.Services.LiveService;
//using POSPRA.Application.Services.LogService;
//using POSPRA.Application.Services.NetworkService;
//using POSPRA.Application.Services.ProductCatalogService;
//using POSPRA.Application.Services.UserService;
//using POSPRA.DTOs;
//using POSPRA.Infrastructure.Context;
//using POSPRA.Repositories.BaseRepository;
//using POSPRA.Repositories.BaseRepository.Repository;
//using POSPRA.Repositories.ClientRepository;
//using POSPRA.Repositories.FiscalRepository;
//using POSPRA.Repositories.LogRepository;
//using POSPRA.Repositories.ProductCatalogueRepository;
//using POSPRA.Repositories.UnitOfWork;
//using POSPRA.Repositories.UserRepository;

//namespace POSPRA.API   // ✅ Added namespace so other projects can reference it
//{
//    public static class Program
//    {
//        /// <summary>
//        /// Entry point when running as a normal Web API (Kestrel, IIS, etc.).
//        /// </summary>
//        public static async Task Main(string[] args)
//        {
//            var apiHost = BuildApiHost();
//            apiHost.Urls.Add("http://localhost:5003/");
//            await apiHost.StartAsync();

//            // Wait here until shutdown is triggered (Ctrl+C, SIGTERM)
//            await apiHost.WaitForShutdownAsync();
//        }

//        /// <summary>
//        /// Creates the fully configured WebApplication.
//        /// WinForms or Worker can call this to start the API in-process.
//        /// </summary>
//        public static WebApplication BuildApiHost(string[]? args = null)
//        {
//            var builder = WebApplication.CreateBuilder(args ?? Array.Empty<string>());

//            builder.Services.AddControllers()
//                .PartManager.ApplicationParts.Add(new Microsoft.AspNetCore.Mvc.ApplicationParts.AssemblyPart(typeof(FiscalController).Assembly));
//            // Force Kestrel to bind to the URL externally
//            // Change this line to:
//            builder.WebHost.UseUrls("http://0.0.0.0:5003");
//            //----------------------------------------------------
//            // 🔧 Database configuration
//            //----------------------------------------------------
//            var dbPath = SqliteDbContext.GetDbPath();
//            builder.Services.AddDbContext<SqliteDbContext>(options =>
//                options.UseSqlite($"Data Source={dbPath}"));

//            builder.Services.AddDbContext<SqlServerDbContext>(options =>
//                options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection")),
//                ServiceLifetime.Scoped);

//            //----------------------------------------------------
//            // 🔧 Dependency Injection
//            //----------------------------------------------------
//            builder.Services.AddAutoMapper(cfg =>
//            {
//                cfg.AddProfile<UserProfile>();
//                cfg.AddProfile<PosProfile>();
//            });

//            builder.Services.AddScoped<ISqliteUnitOfWork, SqliteUnitOfWork>();
//            builder.Services.AddScoped<ISqlServerUnitOfWork, SqlServerUnitOfWork>();

//            //----------------------------------------------------
//            // 🔧 Repositories
//            //----------------------------------------------------
//            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
//            builder.Services.AddScoped(typeof(SqlServerRepository<>));
//            builder.Services.AddScoped<IUserRepository, UserRepository>();
//            builder.Services.AddScoped<IFiscalRepository, FiscalRepository>();
//            builder.Services.AddScoped<ILogRepository, LogRepository>();
//            builder.Services.AddScoped<IClientRepository, ClientRepository>();
//            builder.Services.AddScoped<IProductCatalogueSQLServerRepository, ProductCatalogueSQLServerRepository>();
//            builder.Services.AddScoped<IProductCatalogueSQLiteRepository, ProductCatalogueSQLiteRepository>();

//            //----------------------------------------------------
//            // 🔧 Application Services
//            //----------------------------------------------------
//            builder.Services.AddScoped<IUserService, UserService>();
//            builder.Services.AddScoped<IFiscalService, FiscalService>();
//            builder.Services.AddScoped<ILogService, LogService>();
//            builder.Services.AddScoped<InvoiceValidatorService>();
//            builder.Services.AddScoped<IRequestHeaderService, RequestHeaderService>();
//            builder.Services.AddScoped<ILiveService, LiveService>();
//            builder.Services.AddScoped<INetworkService, NetworkService>();
//            builder.Services.AddScoped<IProductCatalogueService, ProductCatalogueService>();
//            builder.Services.AddScoped<INetworkService, NetworkService>();
//            builder.Services.AddScoped<IClientService, ClientService>();

//            // Http client
//            builder.Services.AddHttpClient<HttpService>();

//            builder.Services.AddHttpContextAccessor();
//            builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

//            //----------------------------------------------------
//            // 🔧 Web / API configuration
//            //----------------------------------------------------
//            builder.Services.AddControllers();
//            builder.Services.AddEndpointsApiExplorer();
//            builder.Services.AddSwaggerGen();
//            builder.Services.AddHealthChecks();


//            //----------------------------------------------------
//            // 🔧 Build and middleware
//            //----------------------------------------------------
//            var app = builder.Build();

//            if (app.Environment.IsDevelopment())
//            {
//                app.UseSwagger();
//                app.UseSwaggerUI();

//                // Redirect root to Swagger UI
//                app.Use(async (context, next) =>
//                {
//                    if (context.Request.Path == "/")
//                    {
//                        context.Response.Redirect("/swagger");
//                        return;
//                    }
//                    await next();
//                });
//            }

//            app.UseHttpsRedirection();
//            app.UseAuthorization();

//            // ✅ Read the flag
//            bool isValidationEnabled = builder.Configuration.GetValue<bool>("Validation:Enabled");
//            // ✅ Only add middleware if the flag is true
//            if (isValidationEnabled)
//                app.UseMiddleware<ValidationMiddleware>();

//            app.MapControllers();
//            app.MapHealthChecks("/health");

//            return app;
//        }
//    }
//}


using Microsoft.EntityFrameworkCore;
using POSPRA.API.Middlewares;
using POSPRA.Application.AutoMapperProfile;
using POSPRA.Application.Services.ClientService;
using POSPRA.Application.Services.FiscalService;
using POSPRA.Application.Services.HelperService;
using POSPRA.Application.Services.HttpClientService;
using POSPRA.Application.Services.LiveService;
using POSPRA.Application.Services.LogService;
using POSPRA.Application.Services.NetworkService;
using POSPRA.Application.Services.ProductCatalogService;
using POSPRA.Application.Services.UserService;
using POSPRA.DTOs;
using POSPRA.Infrastructure.Context;
using POSPRA.Repositories.BaseRepository;
using POSPRA.Repositories.BaseRepository.Repository;
using POSPRA.Repositories.ClientRepository;
using POSPRA.Repositories.FiscalRepository;
using POSPRA.Repositories.LogRepository;
using POSPRA.Repositories.ProductCatalogueRepository;
using POSPRA.Repositories.UnitOfWork;
using POSPRA.Repositories.UserRepository;

namespace POSPRA.API
{
    public static class Program
    {
        /// <summary>
        /// Entry point when running as a normal Web API (Kestrel, IIS, etc.).
        /// Also supports self-hosting inside Worker/WinForms.
        /// </summary>
        public static async Task Main(string[] args)
        {
            var apiHost = BuildApiHost(args);

            // Start the API
            await apiHost.StartAsync();

            // Wait until shutdown (Ctrl+C, SIGTERM, IIS recycle, etc.)
            await apiHost.WaitForShutdownAsync();
        }

        /// <summary>
        /// Creates the fully configured WebApplication.
        /// </summary>
        public static WebApplication BuildApiHost(string[]? args = null)
        {
            var builder = WebApplication.CreateBuilder(args ?? Array.Empty<string>());

            //----------------------------------------------------
            // 🔧 Kestrel URL binding
            // IIS hosting will ignore this and use web.config instead,
            // self-hosting will respect it.
            //----------------------------------------------------
            builder.WebHost.UseUrls("http://localhost:5003");

            //----------------------------------------------------
            // 🔧 Controllers
            //----------------------------------------------------
            builder.Services.AddControllers()
                .PartManager.ApplicationParts.Add(
                    new Microsoft.AspNetCore.Mvc.ApplicationParts.AssemblyPart(typeof(FiscalController).Assembly));

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
            // 🔧 AutoMapper
            //----------------------------------------------------
            builder.Services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<UserProfile>();
                cfg.AddProfile<PosProfile>();
            });

            //----------------------------------------------------
            // 🔧 Repositories & UoW
            //----------------------------------------------------
            builder.Services.AddScoped<ISqliteUnitOfWork, SqliteUnitOfWork>();
            builder.Services.AddScoped<ISqlServerUnitOfWork, SqlServerUnitOfWork>();
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            builder.Services.AddScoped(typeof(SqlServerRepository<>));
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IFiscalRepository, FiscalRepository>();
            builder.Services.AddScoped<ILogRepository, LogRepository>();
            builder.Services.AddScoped<IClientRepository, ClientRepository>();
            builder.Services.AddScoped<IProductCatalogueSQLServerRepository, ProductCatalogueSQLServerRepository>();
            builder.Services.AddScoped<IProductCatalogueSQLiteRepository, ProductCatalogueSQLiteRepository>();

            //----------------------------------------------------
            // 🔧 Application Services
            //----------------------------------------------------
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IFiscalService, FiscalService>();
            builder.Services.AddScoped<ILogService, LogService>();
            builder.Services.AddScoped<InvoiceValidatorService>();
            builder.Services.AddScoped<IRequestHeaderService, RequestHeaderService>();
            builder.Services.AddScoped<ILiveService, LiveService>();
            builder.Services.AddScoped<INetworkService, NetworkService>();
            builder.Services.AddScoped<IProductCatalogueService, ProductCatalogueService>();
            builder.Services.AddScoped<IClientService, ClientService>();

            builder.Services.AddHttpClient<HttpService>();
            builder.Services.AddHttpContextAccessor();
            builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

            //----------------------------------------------------
            // 🔧 Web / API configuration
            //----------------------------------------------------
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddHealthChecks();

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

            // ✅ Only add validation middleware if enabled in config
            bool isValidationEnabled = builder.Configuration.GetValue<bool>("Validation:Enabled");
            if (isValidationEnabled)
                app.UseMiddleware<ValidationMiddleware>();

            app.MapControllers();
            app.MapHealthChecks("/health");

            return app;
        }
    }
}
