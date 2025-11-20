using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pos.Application.Services.ConfigurationService;
using Pos.Application.DTOs;
using Pos.Application.Interfaces;
using Pos.Application.Interfaces.Repositories;
using Pos.Application.Services;
using Pos.Application.Services.ClientService;
using Pos.Application.Services.CloudSyncService.CloudSyncInvoiceService;
using Pos.Application.Services.CloudSyncService.CloudSyncLogService;
using Pos.Application.Services.CloudSyncService.WorkerLogService;
using Pos.Application.Services.ConfigurationService;
using Pos.Application.Services.FileRecordService;
using Pos.Application.Services.HelperService;
using Pos.Application.Services.HttpClientService;
using Pos.Application.Services.InvoiceService;
using Pos.Application.Services.LiveService;
using Pos.Application.Services.LogService;
using Pos.Application.Services.NetworkService;
using Pos.Application.Services.PosService;
using Pos.Application.Services.POSService;
using Pos.Application.Services.ProductCatalogService;
using Pos.Application.Services.ScriptService;
using Pos.Domain.ValueObjects;
using Pos.Infrastructure.Persistence;
using Pos.Infrastructure.Persistence.Factory;
using Pos.Infrastructure.Persistence.Repositories;
using Pos.Infrastructure.Persistence.Repositories.ProductCatalogue;
using Pos.Infrastructure.Services;
using POSPRA.Application.Services.FiscalService;
namespace Pos.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, bool isWinForm = false)
        {
            // -------------------------
            // Add HttpContextAccessor first
            // -------------------------
            services.AddHttpContextAccessor();

            // -------------------------
            // Environment Service
            // -------------------------
            // Load Worker config dynamically
            if (!isWinForm)
                services.AddSingleton<IEnvironmentService>(provider =>
                {
                    // Read worker JSON path from configuration
                    var workerConfigPath = configuration["WorkerConfigPath"];
                    if (string.IsNullOrWhiteSpace(workerConfigPath))
                    {
                        workerConfigPath = Path.GetFullPath(
                            Path.Combine(AppContext.BaseDirectory, @"..\..\..\POSPRA.Worker\appsettings.worker.json")
                        );
                    }

                    if (!File.Exists(workerConfigPath))
                        throw new FileNotFoundException("Worker config file not found", workerConfigPath);

                    return new EnvironmentService(workerConfigPath);
                });
            else
            {
                // WinForms mode → Use AppConfigEnvironmentService
                services.AddSingleton<IEnvironmentService>(provider =>
                {
                    // No need for path logic now — AppConfigEnvironmentService reads from ConfigurationManager directly
                    return new AppConfigEnvironmentService();
                });
            }

            // -------------------------
            // Repositories
            // -------------------------
            services.AddScoped<IProductCatalogueByPosIdRepository, ProductCatalogueByPosIdRepository>();
            services.AddScoped<IPosClientRepository, PosClientRepository>();

            // -------------------------
            // Core Services
            // -------------------------
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<ILogService, LogService>();
            services.AddScoped<InvoiceValidatorService>();
            services.AddScoped<IRequestHeaderService, RequestHeaderService>(); // depends on IHttpContextAccessor
            services.AddScoped<ILiveService, LiveService>();
            services.AddScoped<INetworkService, NetworkService>();
            services.AddScoped<IProductCatalogueService, ProductCatalogueService>();
            services.AddScoped<IClientService, ClientService>();
            services.AddScoped<IInvoiceService, InvoiceService>();
            services.AddScoped<IFileRecordService, FileRecordService>();
            services.AddScoped<IConfigurationService, ConfigurationService>();
            services.AddScoped<IWorkerLogService, WorkerLogService>();
            services.AddScoped<ISendInvoiceToCloudService, SendInvoiceToCloudService>();
            services.AddScoped<ISendLogToCloudService, SendLogToCloudService>();
            services.AddScoped<IScriptService, ScriptService>();
            services.AddScoped<IPosService, PosService>();
            services.AddScoped<ICloudLogService, CloudLogService>();

            // -------------------------
            // SQLite & SQL Server factories
            // -------------------------
            services.AddSingleton<ISqliteDynamicFactory, SqliteDynamicFactory>();
            services.AddScoped<DbContextFactory>();

            // -------------------------
            // Configuration
            // -------------------------
            services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
            services.AddHttpClient<HttpService>();

            // -------------------------
            // SQLite DbContext
            // -------------------------
            var dbFilePath = configuration["AppSettings:DefaultDBFilePath"];
            var sqliteConnectionString = $"Data Source={dbFilePath}";
            services.AddDbContext<SqliteDbContext>(options =>
                options.UseSqlite(sqliteConnectionString));

            // -------------------------
            // Generic Repository
            // -------------------------
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // -------------------------
            // SQL Server UnitOfWork
            // -------------------------
            services.AddScoped<ISqlServerUnitOfWork>(provider =>
            {
                var factory = provider.GetRequiredService<DbContextFactory>();
                var sqlCtx = factory.CreateSqlServerDbContext(); // synchronous wrapper
                return new SqlServerUnitOfWork(sqlCtx);
            });

            services.AddScoped<ISqlServerRepositoryFactory>(provider =>
            {
                var uow = provider.GetRequiredService<ISqlServerUnitOfWork>() as SqlServerUnitOfWork;
                return new SqlServerRepositoryFactory(uow.DbContext);
            });

            // -------------------------
            // SQLite UnitOfWork
            // -------------------------
            services.AddScoped<ISqliteUnitOfWork>(provider =>
            {
                var sqliteCtx = provider.GetRequiredService<SqliteDbContext>();
                return new SqliteUnitOfWork(sqliteCtx);
            });

            services.AddScoped<ISqliteRepositoryFactory>(provider =>
            {
                var uow = provider.GetRequiredService<ISqliteUnitOfWork>() as SqliteUnitOfWork;
                return new SqliteRepositoryFactory(uow.DbContext);
            });

            // -------------------------
            // Unified UnitOfWork per environment
            // -------------------------
            services.AddScoped<IUnitOfWork>(provider =>
            {
                var envService = provider.GetRequiredService<IEnvironmentService>();
                var env = envService.GetCurrentEnvironmentAsync().Result; // synchronous

                return env == EnvironmentType.Sandbox
                    ? provider.GetRequiredService<ISqliteUnitOfWork>()
                    : provider.GetRequiredService<ISqlServerUnitOfWork>();
            });

            return services;
        }
    }
}
