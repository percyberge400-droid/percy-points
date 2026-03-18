using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using pos.Application.Services.ConfigurationService;
using Pos.Application.Interfaces;
using Pos.Application.Interfaces.Repositories;
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
using Pos.Application.Utility;
using Pos.Domain.ValueObjects;
using Pos.Infrastructure.Persistence;
using Pos.Infrastructure.Persistence.Factory;
using Pos.Infrastructure.Persistence.Repositories;
using Pos.Infrastructure.Persistence.Repositories.ProductCatalogue;
using POSPRA.Application.Services.FiscalService;

namespace Pos.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, bool isWinForm = false, bool isWorker = false)
        {
            // -------------------------
            // Logging (Debug only for WinForms)
            // -------------------------
            services.AddLogging(builder =>
            {
                builder.AddDebug(); // Logs to Visual Studio Output window
            });

            // -------------------------
            // HttpContextAccessor (optional for WinForms)
            // -------------------------
            services.AddHttpContextAccessor();

            // -------------------------
            // Register HttpService for CloudSync
            // -------------------------
            services.AddHttpClient<HttpService>(); // Recommended for HttpClient usage

            // -------------------------
            // Repositories
            // -------------------------
            services.AddScoped<IProductCatalogueByPosIdRepository, ProductCatalogueByPosIdRepository>();
            services.AddScoped<IPosClientRepository, PosClientRepository>();
            services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
            services.AddScoped<ICloudLogsRepository, CloudLogsRepository>();
            services.AddScoped<IInvoiceRepository, InvoiceRepository>();
            services.AddScoped<IInvoiceTypeRepository, InvoiceTypeRepository>();

            // -------------------------
            // Core Services
            // -------------------------
            services.AddScoped<IEnvironmentService, Services.EnvironmentService>();
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
            services.AddScoped<AESEncryption>();

            // -------------------------
            // SQLite Dynamic Factory
            // -------------------------
            services.AddSingleton<ISqliteDynamicFactory, SqliteDynamicFactory>();

            // -------------------------
            // SQLite DbContext with encryption
            // -------------------------
            string dbFilePath;

            if (!string.IsNullOrWhiteSpace(configuration["AppSettings:DefaultDBFilePath"]))
            {
                dbFilePath = configuration["AppSettings:DefaultDBFilePath"]!;
            }
            else if (!string.IsNullOrWhiteSpace(configuration["DefaultDBFilePath"]))
            {
                dbFilePath = configuration["DefaultDBFilePath"]!;
            }
            else
            {
                dbFilePath = Path.Combine(AppContext.BaseDirectory, "POSPRA.db");
            }


            var folder = Path.GetDirectoryName(dbFilePath);
            if (!string.IsNullOrEmpty(folder))
                Directory.CreateDirectory(folder);

            string sqlitePassword;

            if (!string.IsNullOrWhiteSpace(configuration["AppSettings:DefaultDBPassword"]))
            {
                sqlitePassword = configuration["AppSettings:DefaultDBPassword"]!;
            }
            else if (!string.IsNullOrWhiteSpace(configuration["DefaultDBPassword"]))
            {
                sqlitePassword = configuration["DefaultDBPassword"]!;
            }
            else
            {
                sqlitePassword = Path.Combine(AppContext.BaseDirectory, "POSPRA.db");
            }



            var connectionStringBuilder = new SqliteConnectionStringBuilder
            {
                DataSource = dbFilePath,
                Mode = SqliteOpenMode.ReadWriteCreate,
                Password = sqlitePassword
            };

            var sqliteConnection = new SqliteConnection(connectionStringBuilder.ToString());
            sqliteConnection.Open();

            services.AddDbContext<SqliteDbContext>(options =>
            {
                options.UseSqlite(sqliteConnection);
            });

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

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
            // SQL Server Factory & DbContext
            // -------------------------
            services.AddScoped<DbContextFactory>(); // Custom factory

            services.AddDbContextFactory<SqlServerDbContext>(options =>
            {
                var sqlConnString = configuration.GetConnectionString("SqlServerConnection");
                options.UseSqlServer(sqlConnString);
            });

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
