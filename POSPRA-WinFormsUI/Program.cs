using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using POSPRA.Application.AutoMapperProfile;
using POSPRA_WinFormsUI.Forms;
using System.Drawing.Text;
using Pos.Infrastructure;

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


            // ✅ Load configuration from appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            // ✅ Get DB path from config, fallback to default
            string dbPath = configuration.GetValue<string>("DefaultDBFilePath")
                            ?? Path.Combine(AppContext.BaseDirectory, "POSPRA.db");

            var dbDirectory = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrWhiteSpace(dbDirectory) && !Directory.Exists(dbDirectory))
            {
                Directory.CreateDirectory(dbDirectory);
            }

            // ✅ Setup DI
            var services = new ServiceCollection();


            // ✅ Register IConfiguration first
            services.AddSingleton<IConfiguration>(configuration);

            // ✅ Add all infrastructure services (repositories, unit of work, core services)
            services.AddInfrastructure(configuration);

            // ✅ Register SQLite DbContext manually for file path override
            services.AddDbContext<SqliteDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            // ✅ AutoMapper
            services.AddAutoMapper(cfg => cfg.AddProfile<PosProfile>());

            // ✅ Register WinForms forms
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