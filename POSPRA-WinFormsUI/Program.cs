using System.Drawing.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using POSPRA.Application.AutoMapperProfile;
using POSPRA.Application.Services.FiscalService;
using POSPRA.Application.Services.LiveService;
using POSPRA.Application.Services.LogService;
using POSPRA.Application.Services.NetworkService;
using POSPRA.Application.Services.PosService;
using POSPRA.Application.Services.POSService;
using POSPRA.Application.Services.ProductCatalogService;
using POSPRA.Application.Services.UserService;
using POSPRA.DTOs;
using POSPRA.Infrastructure.Context;
using POSPRA.Infrastructure.Data;
using POSPRA.Repositories.BaseRepository;
using POSPRA.Repositories.BaseRepository.Repository;
using POSPRA.Repositories.FiscalRepository;
using POSPRA.Repositories.LogRepository;
using POSPRA.Repositories.ProductCatalogueRepository;
using POSPRA.Repositories.UnitOfWork;
using POSPRA.Repositories.UserRepository;
using POSPRA_WinFormsUI.Forms;

namespace POSPRA_WinFormsUI
{
    public static class Program
    {
        private static PrivateFontCollection privateFonts;

        [STAThread]
        static void Main()
        {
            // Run async code synchronously to maintain STA context
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            // Initialize SQLite database if needed
            DbInitializer.Initialize();

            // Load configuration from appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            // Build DI container for WinForms
            var services = new ServiceCollection();

            // Database contexts
            var dbPath = SqliteDbContext.GetDbPath();
            services.AddDbContext<SqliteDbContext>(opt => opt.UseSqlite($"Data Source={dbPath}"));
            services.AddDbContext<SqlServerDbContext>(opt =>
                opt.UseSqlServer(configuration.GetConnectionString("SqlServerConnection")));

            // AutoMapper profiles
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<UserProfile>();
                cfg.AddProfile<PosProfile>();
            });

            // Repositories & Services
            services.AddScoped(typeof(SqlServerRepository<>));
            services.AddScoped<ISqliteUnitOfWork, SqliteUnitOfWork>();
            services.AddScoped<ISqlServerUnitOfWork, SqlServerUnitOfWork>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IFiscalRepository, FiscalRepository>();
            services.AddScoped<ILogRepository, LogRepository>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IFiscalService, FiscalService>();
            services.AddScoped<IPosService, PosService>();
            services.AddScoped<ILogService, LogService>();
            services.AddScoped<InvoiceValidatorService>();
            services.AddScoped<IProductCatalogueService, ProductCatalogueService>();
            services.AddScoped<IProductCatalogueSQLServerRepository, ProductCatalogueSQLServerRepository>();
            services.AddScoped<IProductCatalogueSQLiteRepository, ProductCatalogueSQLiteRepository>();
            // services.AddScoped<IRequestHeaderService, RequestHeaderService>();
            services.AddScoped<ILiveService, LiveService>();
            services.AddScoped<INetworkService, NetworkService>();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddHttpContextAccessor();
            services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
            services.AddHttpClient();

            // WinForms UI forms
            services.AddTransient<LoginForm>();
            services.AddTransient<DashboardForm>();
            services.AddTransient<Main>();
            services.AddTransient<item_entry>();
            services.AddTransient<ExportInvoiceForm>();
            services.AddTransient<CatalogView>();

            using var provider = services.BuildServiceProvider();

            // ✅ Launch WinForms UI (API is NOT started here)
            ApplicationConfiguration.Initialize();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var loginForm = provider.GetRequiredService<LoginForm>();
            Application.Run(loginForm);
        }
    }
}
