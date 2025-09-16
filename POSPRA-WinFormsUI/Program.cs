using System.Drawing.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using POSPRA.Application.AutoMapperProfile;
using POSPRA.Application.Services.FiscalService;
using POSPRA.Application.Services.LogService;
using POSPRA.Application.Services.PosService;
using POSPRA.Application.Services.POSService;
using POSPRA.Application.Services.UserService;
using POSPRA.DTOs;
using POSPRA.Infrastructure.Context;
using POSPRA.Infrastructure.Data;
using POSPRA.Repositories.BaseRepository;
using POSPRA.Repositories.BaseRepository.Repository;
using POSPRA.Repositories.FiscalRepository;
using POSPRA.Repositories.LogRepository;
using POSPRA.Repositories.UnitOfWork;
using POSPRA.Repositories.UserRepository;
using POSPRA_WinFormsUI.Forms;

namespace POSPRA_WinFormsUI
{
    internal static class Program
    {
        private static PrivateFontCollection privateFonts;

        [STAThread]
        static void Main()
        {
            DbInitializer.Initialize();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            var services = new ServiceCollection();

            // DbContexts
            var dbPath = SqliteDbContext.GetDbPath();
            services.AddDbContext<SqliteDbContext>(opt => opt.UseSqlite($"Data Source={dbPath}"));
            services.AddDbContext<SqlServerDbContext>(opt =>
                opt.UseSqlServer(configuration.GetConnectionString("SqlServerConnection")));

            // AutoMapper
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<UserProfile>();
                cfg.AddProfile<InvoiceProfile>();
            });

            // Repositories & Services
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



            // ✅ Typed HttpClient with base address
            services.AddHttpClient("SelfHostedApi", (sp, client) =>
            {
                var cfg = sp.GetRequiredService<IConfiguration>();
                client.BaseAddress = new Uri(cfg["ApiSettings:BaseUrl"]);
            });

            services.Configure<AppSettings>(configuration.GetSection("AppSettings"));

            // Forms
            services.AddTransient<LoginForm>();
            services.AddTransient<DashboardForm>();
            services.AddTransient<Main>();
            services.AddTransient<item_entry>();

            using var provider = services.BuildServiceProvider();

            ApplicationConfiguration.Initialize();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var loginForm = provider.GetRequiredService<LoginForm>();
            Application.Run(loginForm);
        }
    }
}
