using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pos.Application.Services.ConfigurationService;
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
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, bool isWinForm = false, bool isWorker = false)
        {
            // -------------------------
            // Add HttpContextAccessor first
            // -------------------------
            services.AddHttpContextAccessor();

            // -------------------------
            // Repositories
            // -------------------------
            services.AddScoped<IProductCatalogueByPosIdRepository, ProductCatalogueByPosIdRepository>();
            services.AddScoped<IPosClientRepository, PosClientRepository>();
            services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
            services.AddScoped<ICloudLogsRepository, CloudLogsRepository>();
            services.AddScoped<IInvoiceRepository, InvoiceRepository>();

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
            services.AddScoped<IEnvironmentService, EnvironmentService>();

            // -------------------------
            // SQLite Dynamic Factory
            // -------------------------
            services.AddSingleton<ISqliteDynamicFactory, SqliteDynamicFactory>();

            // -------------------------
            // SQLite DbContext with encryption
            // -------------------------
            var dbFilePath = configuration["AppSettings:DefaultDBFilePath"];
            if (string.IsNullOrEmpty(dbFilePath))
                throw new FileNotFoundException("SQLite DB path is not configured in AppSettings");

            // Ensure folder exists
            var folder = Path.GetDirectoryName(dbFilePath);
            if (!string.IsNullOrEmpty(folder))
                Directory.CreateDirectory(folder);

            // Password for SQLCipher
            var sqlitePassword = configuration["AppSettings:DefaultDBPassword"];
            if (string.IsNullOrEmpty(sqlitePassword))
                throw new InvalidOperationException("SQLite password is not set in AppSettings");

            // Build encrypted connection
            var connectionStringBuilder = new SqliteConnectionStringBuilder
            {
                DataSource = dbFilePath,
                Mode = SqliteOpenMode.ReadWriteCreate,
                Password = sqlitePassword
            };

            var sqliteConnection = new SqliteConnection(connectionStringBuilder.ToString());
            sqliteConnection.Open(); // Key is applied here

            // Register DbContext with open connection
            services.AddDbContext<SqliteDbContext>(options =>
            {
                options.UseSqlite(sqliteConnection);
            });

            // -------------------------
            // Generic Repository
            // -------------------------
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

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
            // SQL Server setup (unchanged)
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
                return new SqlServerRepositoryFactory(uow.DbContext);
            });

            // -------------------------
            // Unified UnitOfWork per environment
            // -------------------------
            services.AddScoped<IUnitOfWork>(provider =>
            {
                var envService = provider.GetRequiredService<IEnvironmentService>();
                var env = envService.GetCurrentEnvironmentAsync().Result;

                return env == EnvironmentType.Sandbox
                    ? provider.GetRequiredService<ISqliteUnitOfWork>()
                    : provider.GetRequiredService<ISqlServerUnitOfWork>();
            });

            return services;
        }
    }
}
