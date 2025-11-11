//using Microsoft.AspNetCore.Http;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using pos.Application.Services.ConfigurationService;
//using Pos.Application.DTOs;
//using Pos.Application.Interfaces;
//using Pos.Application.Services;
//using Pos.Application.Services.ClientService;
//using Pos.Application.Services.CloudSyncService.CloudSyncInvoiceService;
//using Pos.Application.Services.CloudSyncService.CloudSyncLogService;
//using Pos.Application.Services.CloudSyncService.WorkerLogService;
//using Pos.Application.Services.ConfigurationService;
//using Pos.Application.Services.FileRecordService;
//using Pos.Application.Services.HelperService;
//using Pos.Application.Services.HttpClientService;
//using Pos.Application.Services.InvoiceService;
//using Pos.Application.Services.LiveService;
//using Pos.Application.Services.LogService;
//using Pos.Application.Services.NetworkService;
//using Pos.Application.Services.PosService;
//using Pos.Application.Services.POSService;
//using Pos.Application.Services.ProductCatalogService;
//using Pos.Application.Services.ScriptService;
//using Pos.Infrastructure.Persistence;
//using Pos.Infrastructure.Persistence.Repositories;
//using Pos.Infrastructure.Services;
//using POSPRA.Application.Services.FiscalService;

//namespace Pos.Infrastructure
//{
//    public static class InfrastructureServiceRegistration
//    {
//        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
//        {
//            // Core Services
//            services.AddScoped<ICustomerService, CustomerService>();
//            services.AddScoped<ILogService, LogService>();
//            services.AddScoped<InvoiceValidatorService>();
//            services.AddScoped<IRequestHeaderService, RequestHeaderService>();
//            services.AddScoped<ILiveService, LiveService>();
//            services.AddScoped<INetworkService, NetworkService>();
//            services.AddScoped<IProductCatalogueService, ProductCatalogueService>();
//            services.AddScoped<IClientService, ClientService>();
//            services.AddScoped<IInvoiceService, InvoiceService>();
//            services.AddScoped<IFileRecordService, FileRecordService>();
//            services.AddScoped<IConfigurationService, ConfigurationService>();
//            services.AddScoped<IWorkerLogService, WorkerLogService>();
//            services.AddScoped<ISendInvoiceToCloudService, SendInvoiceToCloudService>();
//            services.AddScoped<ISendLogToCloudService, SendLogToCloudService>();
//            services.AddScoped<IScriptService, ScriptService>();
//            services.AddScoped<IPosService, PosService>();
//            services.AddScoped<ICloudLogService, CloudLogService>();
//            services.AddSingleton<IEnvironmentService, EnvironmentService>();

//            // ✅ Fixed Configuration
//            services.Configure<AppSettings>(configuration.GetSection("AppSettings"));

//            // ✅ Http helpers
//            services.AddHttpClient<HttpService>();
//            services.AddHttpContextAccessor();

//            // ✅ SQLite DbContext
//            // Read the DB file path from configuration
//            var dbFilePath = configuration["AppSettings:DefaultDBFilePath"];
//            var sqliteConnectionString = $"Data Source={dbFilePath}";

//            // Register DbContext
//            services.AddDbContext<SqliteDbContext>(options =>
//                options.UseSqlite(sqliteConnectionString));

//            // ✅ SQL Server Repository Factory
//            services.AddScoped<ISqlServerRepositoryFactory, SqlServerRepositoryFactory>(provider =>
//            {
//                var factory = provider.GetRequiredService<DbContextFactory>();
//                var sqlCtx = factory.CreateSqlServerDbContext();
//                return new SqlServerRepositoryFactory(sqlCtx);
//            });

//            // ✅ SQLite Repository Factory
//            services.AddScoped<ISqliteRepositoryFactory, SqliteRepositoryFactory>(provider =>
//            {
//                var sqliteCtx = provider.GetRequiredService<SqliteDbContext>();
//                return new SqliteRepositoryFactory(sqliteCtx);
//            });

//            // ✅ Generic Repository
//            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

//            // ✅ DbContext Factory (for SQL Server)
//            services.AddScoped<DbContextFactory>();

//            // ✅ SQL Server UnitOfWork
//            services.AddScoped<ISqlServerUnitOfWork>(provider =>
//            {
//                var factory = provider.GetRequiredService<DbContextFactory>();
//                var sqlCtx = factory.CreateSqlServerDbContext();
//                return new SqlServerUnitOfWork(sqlCtx);
//            });

//            // ✅ SQLite UnitOfWork
//            services.AddScoped<ISqliteUnitOfWork>(provider =>
//            {
//                var sqliteCtx = provider.GetRequiredService<SqliteDbContext>();
//                return new SqliteUnitOfWork(sqliteCtx);
//            });

//            // ✅ Environment-based UnitOfWork
//            services.AddScoped<IUnitOfWork>(provider =>
//            {
//                var envService = provider.GetRequiredService<IEnvironmentService>();
//                var env = envService.GetCurrentEnvironment();
//                return env == EnvironmentType.Sandbox
//                    ? provider.GetRequiredService<ISqliteUnitOfWork>()
//                    : provider.GetRequiredService<ISqlServerUnitOfWork>();
//            });

//            return services;
//        }
//    }
//}


using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pos.Application.Services.ConfigurationService;
using Pos.Application.DTOs;
using Pos.Application.Interfaces;
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
using Pos.Infrastructure.Persistence;
using Pos.Infrastructure.Persistence.Repositories;
using Pos.Infrastructure.Services;
using POSPRA.Application.Services.FiscalService;

namespace Pos.Infrastructure
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // -------------------------
            // Core Services
            // -------------------------
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<ILogService, LogService>();
            services.AddScoped<InvoiceValidatorService>();
            services.AddScoped<IRequestHeaderService, RequestHeaderService>();
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
            services.AddSingleton<IEnvironmentService, EnvironmentService>();

            // Configuration
            services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
            services.AddHttpClient<HttpService>();
            services.AddHttpContextAccessor();

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
            // SQL Server DbContext Factory (for dynamic switching)
            // -------------------------
            services.AddScoped<DbContextFactory>();

            // -------------------------
            // Environment-based dynamic DbContext & UnitOfWork
            // -------------------------
            services.AddScoped<ISqlServerUnitOfWork>(provider =>
            {
                var factory = provider.GetRequiredService<DbContextFactory>();
                var sqlCtx = factory.CreateSqlServerDbContext();
                return new SqlServerUnitOfWork(sqlCtx);
            });

            services.AddScoped<ISqlServerRepositoryFactory>(provider =>
            {
                var uow = provider.GetRequiredService<ISqlServerUnitOfWork>() as SqlServerUnitOfWork;
                return new SqlServerRepositoryFactory(uow.DbContext); // Reuse same context
            });

            services.AddScoped<ISqliteUnitOfWork>(provider =>
            {
                var sqliteCtx = provider.GetRequiredService<SqliteDbContext>();
                return new SqliteUnitOfWork(sqliteCtx);
            });

            services.AddScoped<ISqliteRepositoryFactory>(provider =>
            {
                var uow = provider.GetRequiredService<ISqliteUnitOfWork>() as SqliteUnitOfWork;
                return new SqliteRepositoryFactory(uow.DbContext); // Reuse same context
            });

            // -------------------------
            // Unified UnitOfWork for service
            // -------------------------
            services.AddScoped<IUnitOfWork>(provider =>
            {
                var envService = provider.GetRequiredService<IEnvironmentService>();
                var env = envService.GetCurrentEnvironment();

                return env == EnvironmentType.Sandbox
                    ? provider.GetRequiredService<ISqliteUnitOfWork>()
                    : provider.GetRequiredService<ISqlServerUnitOfWork>();
            });

            return services;
        }
    }
}
