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
using System.Configuration; // ✅ Needed for ConfigurationManager
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

            if (string.IsNullOrWhiteSpace(dbPath))
            {
                // fallback path if not found
                dbPath = Path.Combine(AppContext.BaseDirectory, "POSPRA.db");
            }

            // ✅ Ensure the directory exists
            string? dbDirectory = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrWhiteSpace(dbDirectory) && !Directory.Exists(dbDirectory))
            {
                Directory.CreateDirectory(dbDirectory);
            }

            // ✅ Initialize SQLite database if needed
            var sqliteOptions = new DbContextOptionsBuilder<SqliteDbContext>()
                .UseSqlite($"Data Source={dbPath}")
                .Options;

            using (var context = new SqliteDbContext(sqliteOptions))
            {
                context.Database.EnsureCreated();
            }

            // ✅ Load JSON config (for any additional modern config)
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            // ✅ Build DI container
            var services = new ServiceCollection();

            // Register DbContexts
            services.AddDbContext<SqliteDbContext>(opt => opt.UseSqlite($"Data Source={dbPath}"));
            services.AddDbContext<SqlServerDbContext>(opt =>
                opt.UseSqlServer(configuration.GetConnectionString("SqlServerConnection")));

            // AutoMapper
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
            services.AddScoped<ILogSQLiteRepository, LogSQLiteRepository>();
            services.AddScoped<ILogSQLServerRepository, LogSQLServerRepository>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IFiscalService, FiscalService>();
            services.AddScoped<IPosService, PosService>();
            services.AddScoped<ILogService, LogService>();
            services.AddScoped<InvoiceValidatorService>();
            services.AddScoped<IProductCatalogueService, ProductCatalogueService>();
            services.AddScoped<IProductCatalogueSQLServerRepository, ProductCatalogueSQLServerRepository>();
            services.AddScoped<IProductCatalogueSQLiteRepository, ProductCatalogueSQLiteRepository>();
            services.AddScoped<ILiveService, LiveService>();
            services.AddScoped<INetworkService, NetworkService>();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddHttpContextAccessor();
            services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
            services.AddHttpClient();

            // WinForms UI forms
            services.AddTransient<LoginForm2>();
            services.AddTransient<LoginForm>();
            services.AddTransient<DashboardForm>();
            services.AddTransient<Main>();
            services.AddTransient<item_entry>();
            services.AddTransient<ItemEntry>();
            services.AddTransient<ExportInvoiceForm>();
            services.AddTransient<CatalogView>();


            using var provider = services.BuildServiceProvider();

            // ✅ Start API self-hosted inside WinForms
            var apiHost = POSPRA.API.Program.BuildApiHost();
            _ = apiHost.RunAsync(); // fire and forget

            // ✅ Launch WinForms
            ApplicationConfiguration.Initialize();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var loginForm = provider.GetRequiredService<LoginForm2>();
            Application.Run(loginForm);

            // Shutdown API when app closes
            await apiHost.StopAsync();
        }
    }
}
