using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using POSPRA.Application.AutoMapperProfile;
using POSPRA_WinFormsUI.Forms;
using System.Drawing.Text;
using Pos.Infrastructure;
using Microsoft.Extensions.Configuration;
using ConfigurationManager = System.Configuration.ConfigurationManager;

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
            // ------------------------------
            // 1️⃣ Read appsettings.json
            // ------------------------------
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            // ------------------------------
            // 2️⃣ Read value from app.config
            // ------------------------------
            string dbPathFromAppConfig = ConfigurationManager.AppSettings["DefaultDBFilePath"]!;
            if (string.IsNullOrWhiteSpace(dbPathFromAppConfig))
            {
                dbPathFromAppConfig = Path.Combine(AppContext.BaseDirectory, "POSPRA.db");
            }


            // ------------------------------
            // 3️⃣ Inject app.config value into IConfiguration
            // ------------------------------
            builder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AppSettings:DefaultDBFilePath"] = dbPathFromAppConfig,
                ["BaseUrl"] = ConfigurationManager.AppSettings["BaseUrl"]
            });

            var configuration = builder.Build();

            // ------------------------------
            // 4️⃣ Ensure DB directory exists
            // ------------------------------
            string dbPath = configuration["AppSettings:DefaultDBFilePath"]!;
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

            using (var scope = provider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<SqliteDbContext>();
                dbContext.Database.EnsureCreated();
            }

            // ✅ Launch WinForms
            ApplicationConfiguration.Initialize();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var loginForm = provider.GetRequiredService<LoginForm2>();
            Application.Run(loginForm);
        }
    }
}