using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using POSPRA.Application.AutoMapperProfile;
using POSPRA.Application.Services.FiscalService;
using POSPRA.Application.Services.LogService;
using POSPRA.Application.Services.PosService;
using POSPRA.Application.Services.POSService;
using POSPRA.Application.Services.UserService;
using POSPRA.Application.Utility;
using POSPRA.Infrastructure.Context;
using POSPRA.Infrastructure.Data;
using POSPRA.Repositories.BaseRepository;
using POSPRA.Repositories.BaseRepository.Repository;
using POSPRA.Repositories.FiscalRepository;
using POSPRA.Repositories.LogRepository;
using POSPRA.Repositories.UnitOfWork;
using POSPRA.Repositories.UserRepository;
using POSPRA_WinFormsUI.Forms;
using System.Drawing.Text;

namespace POSPRA_WinFormsUI
{
    internal static class Program
    {
        private static PrivateFontCollection privateFonts;

        [STAThread]
        static void Main()
        {
            // Ensure database exists
            DbInitializer.Initialize();

            // Load appsettings.json
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            IConfiguration configuration = builder.Build();

            // Setup DI
            var services = new ServiceCollection();        

            // DbContexts
            var dbPath = SqliteDbContext.GetDbPath();
            services.AddDbContext<SqliteDbContext>(opt => opt.UseSqlite($"Data Source={dbPath}"));
            services.AddDbContext<SqlServerDbContext>(opt =>
                opt.UseSqlServer(configuration.GetConnectionString("SqlServerConnection")));

            // AutoMapper (register all profiles in assembly)
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<UserProfile>();
                cfg.AddProfile<InvoiceProfile>();
            });

            // Repositories & UnitOfWork
            services.AddScoped<ISqliteUnitOfWork, SqliteUnitOfWork>();
            services.AddScoped<ISqlServerUnitOfWork, SqlServerUnitOfWork>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IFiscalRepository, FiscalRepository>();
            services.AddScoped<ILogRepository, LogRepository>();

            // Services
            services.AddScoped<InvoiceValidatorService>();
            services.AddScoped<ILogService, LogService>();
            services.AddScoped<SendModelToServer>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IPosService, PosService>();
            services.AddScoped<IFiscalService, FiscalService>();
            // Register HttpClient + IHttpService
            services.AddHttpClient<IHttpService, HttpService>(); // <-- FIX (needed by SendModelToServer)
            services.AddScoped<SendModelToServer>();

            // AppSettings (Options pattern)
            services.Configure<AppSettings>(configuration.GetSection("AppSettings"));

            // Forms (use DI)
            services.AddTransient<LoginForm>();
            services.AddTransient<DashboardForm>();
            services.AddTransient<Main>();
            services.AddTransient<item_entry>();

            // Build provider
            using var provider = services.BuildServiceProvider();

            // Start WinForms
            ApplicationConfiguration.Initialize();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Resolve login form from DI
            var loginForm = provider.GetRequiredService<LoginForm>();
            Application.Run(loginForm);
        }
    }
}