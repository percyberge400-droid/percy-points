using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pos.Application.Services.ConfigurationService;
using Pos.Application.DTOs;
using Pos.Application.Interfaces;
using Pos.Application.Services.ClientService;
using Pos.Application.Services.CloudSyncService.CloudSyncLogService;
using Pos.Application.Services.ConfigurationService;
using Pos.Application.Services.FileRecordService;
using Pos.Application.Services.HelperService;
using Pos.Application.Services.HttpClientService;
using Pos.Application.Services.InvoiceService;
using Pos.Application.Services.LiveService;
using Pos.Application.Services.LogService;
using Pos.Application.Services.NetworkService;
using Pos.Application.Services.ProductCatalogService;
using Pos.Application.Services.ScriptService;
using Pos.Infrastructure.Persistence.Repositories;
using POSPRA.Application.AutoMapperProfile;
using POSPRA.Application.Services.FiscalService;
using POSPRA_WinFormsUI.Forms;
using System.Drawing.Text;

namespace POSPRA_WinFormsUI
{
    public static class Program
    {
        private static PrivateFontCollection privateFonts;

        [STAThread]
        static void Main()
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            // ✅ Read from App.config
            string? dbPath = System.Configuration.ConfigurationManager.AppSettings["DefaultDBFilePath"];

            // Fallback path if config value is missing or empty
            if (string.IsNullOrWhiteSpace(dbPath))
            {
                dbPath = Path.Combine(AppContext.BaseDirectory, "POSPRA.db");
            }

            // ✅ Ensure the directory exists
            string? dbDirectory = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrWhiteSpace(dbDirectory) && !Directory.Exists(dbDirectory))
            {
                //Directory.CreateDirectory(dbDirectory);
            }

            // ✅ Initialize SQLite database if needed
            var sqliteOptions = new DbContextOptionsBuilder<SqliteDbContext>()
                .UseSqlite($"Data Source={dbPath}")
                .Options;

            using (var context = new SqliteDbContext(sqliteOptions))
            {
                //context.Database.EnsureCreated();
            }

            // ✅ Load JSON config (for modern settings)
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            // ✅ Build DI container
            var services = new ServiceCollection();

            // Register DbContexts
            services.AddDbContext<SqliteDbContext>(opt => opt.UseSqlite($"Data Source={dbPath}"));

            var appSettings = configuration.GetSection("AppSettings").Get<AppSettings>();
            bool isProduction = appSettings?.IsProduction ?? false;

            string sqlServerConnString = configuration.GetConnectionString(
                isProduction ? "SqlServerConnectionProduction" : "SqlServerConnectionSandbox"
            ) ?? throw new InvalidOperationException("No valid SQL Server connection string found.");

            services.AddDbContext<SqlServerDbContext>(opt => opt.UseSqlServer(sqlServerConnString));

            // AutoMapper
            services.AddAutoMapper(cfg => cfg.AddProfile<PosProfile>());

            // Repositories & Services
            services.AddScoped<ISqliteUnitOfWork, SqliteUnitOfWork>();
            services.AddScoped<ISqlServerUnitOfWork, SqlServerUnitOfWork>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IFileRecordService, FileRecordService>();
            services.AddScoped<ILogService, LogService>();
            services.AddScoped<InvoiceValidatorService>();
            services.AddScoped<IProductCatalogueService, ProductCatalogueService>();
            services.AddScoped<IScriptService, ScriptService>();
            services.AddScoped<ISendLogToCloudService, SendLogToCloudService>();
            services.AddScoped<IRequestHeaderService, RequestHeaderService>();
            services.AddScoped<IConfigurationService, ConfigurationService>();
            services.AddScoped<ILiveService, LiveService>();
            services.AddScoped<INetworkService, NetworkService>();
            services.AddScoped<IInvoiceService, InvoiceService>();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddHttpClient<HttpService>();
            services.AddScoped<IClientService, ClientService>();

            services.AddHttpContextAccessor();
            services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
            services.AddHttpClient();

            // WinForms UI forms
            services.AddTransient<LoginForm2>();
            services.AddTransient<DashboardForm>();
            services.AddTransient<Main>();
            services.AddTransient<ItemEntry>();
            services.AddTransient<ExportInvoiceForm>();
            services.AddTransient<CatalogView>();

            using var provider = services.BuildServiceProvider();

            // ✅ Launch WinForms
            ApplicationConfiguration.Initialize();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var loginForm = provider.GetRequiredService<LoginForm2>();
            Application.Run(loginForm);
        }
    }
}
