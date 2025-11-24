using System.Drawing.Text;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pos.Infrastructure;
using POSPRA.Application.AutoMapperProfile;
using POSPRA_WinFormsUI.Forms;
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
            string dbPassword = ConfigurationManager.AppSettings["DefaultDBPassword"]!;

            if (string.IsNullOrWhiteSpace(dbPathFromAppConfig))
            {
                dbPathFromAppConfig = Path.Combine(AppContext.BaseDirectory, "POSPRA.db");
            }

            if (string.IsNullOrWhiteSpace(dbPassword))
            {
                dbPassword = "DefaultPassword123"; // fallback password
            }

            // ------------------------------
            // 3️⃣ Inject app.config value into IConfiguration
            // ------------------------------
            var appConfigValues = ConfigurationManager.AppSettings.AllKeys
                .ToDictionary(
                    key => key.StartsWith("AppSettings:") ? key : $"AppSettings:{key}",
                    key => ConfigurationManager.AppSettings[key]
                );

            appConfigValues["AppSettings:DefaultDBFilePath"] = dbPathFromAppConfig;
            appConfigValues["AppSettings:DefaultDBPassword"] = dbPassword;

            builder.AddInMemoryCollection(appConfigValues);
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

            // ------------------------------
            // 5️⃣ Create encrypted SQLCipher connection
            // ------------------------------
            var connectionStringBuilder = new SqliteConnectionStringBuilder
            {
                DataSource = dbPath,
                Mode = SqliteOpenMode.ReadWriteCreate,
                Password = dbPassword
            };

            var sqliteConnection = new SqliteConnection(connectionStringBuilder.ToString());
            sqliteConnection.Open(); // encryption key applied

            // ------------------------------
            // 6️⃣ Setup DI
            // ------------------------------
            var services = new ServiceCollection();

            services.AddSingleton<IConfiguration>(configuration);
            services.AddInfrastructure(configuration, true);

            // Register SQLite DbContext with encrypted connection
            services.AddDbContext<SqliteDbContext>(options =>
                options.UseSqlite(sqliteConnection));

            // AutoMapper
            services.AddAutoMapper(cfg => cfg.AddProfile<PosProfile>());

            // Register WinForms forms
            services.AddTransient<LoginForm2>();
            services.AddTransient<DashboardForm>();
            services.AddTransient<Main>();
            services.AddTransient<ItemEntry>();
            services.AddTransient<ExportInvoiceForm>();
            services.AddTransient<CatalogView>();

            using var provider = services.BuildServiceProvider();

            // ------------------------------
            // 7️⃣ Ensure DB + tables exist
            // ------------------------------
            using (var scope = provider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<SqliteDbContext>();
                dbContext.Database.EnsureCreated();
            }

            // ------------------------------
            // 8️⃣ Launch WinForms
            // ------------------------------
            ApplicationConfiguration.Initialize();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var loginForm = provider.GetRequiredService<LoginForm2>();
            Application.Run(loginForm);
        }
    }
}