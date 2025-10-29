using Microsoft.EntityFrameworkCore;
using POSPRA.API.Controllers;
using POSPRA.API.Middlewares;
using POSPRA.Application.AutoMapperProfile;
using POSPRA.Application.Services.ClientService;
using POSPRA.Application.Services.CloudSyncService.CloudSyncInvoiceService;
using POSPRA.Application.Services.CloudSyncService.CloudSyncLogService;
using POSPRA.Application.Services.CloudSyncService.WorkerLogService;
using POSPRA.Application.Services.ConfigurationService;
using POSPRA.Application.Services.EnvironmentConfigService;
using POSPRA.Application.Services.FileRecordService;
using POSPRA.Application.Services.FiscalService;
using POSPRA.Application.Services.HelperService;
using POSPRA.Application.Services.HttpClientService;
using POSPRA.Application.Services.InvoiceService;
using POSPRA.Application.Services.LiveService;
using POSPRA.Application.Services.LogService;
using POSPRA.Application.Services.NetworkService;
using POSPRA.Application.Services.ProductCatalogService;
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
            builder.WebHost.UseUrls("http://localhost:5010");

            //----------------------------------------------------
            // 🔧 Controllers
            //----------------------------------------------------
            builder.Services.AddControllers()
                .PartManager.ApplicationParts.Add(
                    new Microsoft.AspNetCore.Mvc.ApplicationParts.AssemblyPart(typeof(FileRecordController).Assembly));

            //----------------------------------------------------
            // 🔧 Database configuration
            //----------------------------------------------------
            string? dbPath = builder.Configuration.GetSection("AppSettings:DefaultDBFilePath")?.Value;

            // fallback if not found
            if (string.IsNullOrWhiteSpace(dbPath))
            {
                dbPath = Path.Combine(AppContext.BaseDirectory, "POSPRA.db");
            }

            // ✅ Ensure SqliteDbContext uses this path
            SqliteDbContext.SetDatabasePath(dbPath);

            builder.Services.AddDbContext<SqliteDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            // Read the isProduction flag from AppSettings
            //var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>();
            //bool isProduction = appSettings!.IsProduction;

            // Choose the SQL Server connection string based on the flag
            //string sqlConnectionString = builder.Configuration.GetConnectionString(
            //    isProduction ? "SqlServerConnectionProduction" : "SqlServerConnectionSandbox"
            //) ?? throw new InvalidOperationException("Missing SQL Server connection string for the selected environment for api.");

            builder.Services.AddSingleton<IEnvironmentConfigService, EnvironmentConfigService>();
            // Register SQL Server DbContext with the selected connection string
            builder.Services.AddDbContext<SqlServerDbContext>((sp, options) =>
            {
                var configService = sp.GetRequiredService<IEnvironmentConfigService>();
                var connectionString = configService.GetConnectionString();
                options.UseSqlServer(connectionString);
            });


            //----------------------------------------------------
            // 🔧 AutoMapper
            //----------------------------------------------------
            builder.Services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<PosProfile>();
            });

            //----------------------------------------------------
            // 🔧 Repositories & UoW
            //----------------------------------------------------
            builder.Services.AddScoped<ISqliteUnitOfWork, SqliteUnitOfWork>();
            builder.Services.AddScoped<ISqlServerUnitOfWork, SqlServerUnitOfWork>();
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            builder.Services.AddScoped(typeof(SqlServerRepository<>));
            builder.Services.AddScoped<IFileRecordRepository, FileRecordRepository>();
            builder.Services.AddScoped<ILogSQLiteRepository, LogSQLiteRepository>();
            builder.Services.AddScoped<ILogSQLServerRepository, LogSQLServerRepository>();
            builder.Services.AddScoped<IClientRepository, ClientRepository>();
            builder.Services.AddScoped<IProductCatalogueSQLServerRepository, ProductCatalogueSQLServerRepository>();
            builder.Services.AddScoped<IProductCatalogueSQLiteRepository, ProductCatalogueSQLiteRepository>();
            builder.Services.AddScoped<IConfigurationRepository, ConfigurationRepository>();

            //----------------------------------------------------
            // 🔧 Application Services
            //----------------------------------------------------
            builder.Services.AddScoped<ILogService, LogService>();
            builder.Services.AddScoped<InvoiceValidatorService>();
            builder.Services.AddScoped<IRequestHeaderService, RequestHeaderService>();
            builder.Services.AddScoped<ILiveService, LiveService>();
            builder.Services.AddScoped<INetworkService, NetworkService>();
            builder.Services.AddScoped<IProductCatalogueService, ProductCatalogueService>();
            builder.Services.AddScoped<IClientService, ClientService>();
            builder.Services.AddScoped<IInvoiceService, InvoiceService>();
            builder.Services.AddScoped<IFileRecordService, FileRecordService>();
            builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
            builder.Services.AddScoped<IWorkerLogService, WorkerLogService>();
            builder.Services.AddScoped<ISendInvoiceToCloudService, SendInvoiceToCloudService>();
            builder.Services.AddScoped<ISendLogToCloudService, SendLogToCloudService>();
            builder.Services.AddScoped<IScriptService, ScriptService>();

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

            // global exception handler
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.MapControllers();
            app.MapHealthChecks("/health");

            return app;
        }
    }
}
