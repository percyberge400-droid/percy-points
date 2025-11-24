using System.Configuration;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Pos.Application.Interfaces;
using Pos.Application.Services.ScriptService;
using Pos.Infrastructure.Persistence.Factory;
using Pos.Infrastructure.Persistence.Repositories;

namespace POSPRA.SetupUI
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            // 🔹 Step 1: Resolve config paths
            string configPath = string.Empty;

            if (args != null && args.Length > 0)
            {
                string candidate = args[0].Trim('"').TrimEnd('\\');

                if (Directory.Exists(candidate))
                {
                    configPath = Path.Combine(candidate, "POSPRA-WinFormsUI.dll.config");
                }
                else if (File.Exists(candidate))
                {
                    configPath = candidate;
                }
            }

            if (string.IsNullOrEmpty(configPath))
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                configPath = Path.Combine(baseDir, "POSPRA-WinFormsUI.dll.config");
            }

            string baseFolder = Path.GetDirectoryName(configPath) ?? AppDomain.CurrentDomain.BaseDirectory;
            string jsonWorkerPath = Path.Combine(baseFolder, "appsettings.worker.json");
            string jsonMainPath = Path.Combine(baseFolder, "appsettings.json");
            string setupConfig = Path.Combine(baseFolder, "POSPRA.SetupUI.dll.config");
            string installationInfo = @"C:\ProgramData\PRAL";

            // 🔹 Step 2: Read DB path and password from JSON → SetupUI.config → fallback
            string? dbPath = null;
            string? dbPassword = null;

            // 2a: Try worker JSON
            if (File.Exists(jsonWorkerPath))
            {
                try
                {
                    var json = JObject.Parse(File.ReadAllText(jsonWorkerPath));
                    dbPath = json["AppSettings"]?["DefaultDBFilePath"]?.ToString();
                    dbPassword = json["AppSettings"]?["DefaultDBPassword"]?.ToString();
                }
                catch { /* ignore */ }
            }

            // 2b: If not found → try AppConfig
            if (string.IsNullOrWhiteSpace(dbPath))
                dbPath = ConfigurationManager.AppSettings["DefaultDBFilePath"];

            if (string.IsNullOrWhiteSpace(dbPassword))
                dbPassword = ConfigurationManager.AppSettings["DefaultDBPassword"];

            // 2c: Fallback defaults
            if (string.IsNullOrWhiteSpace(dbPath))
                dbPath = Path.Combine(AppContext.BaseDirectory, "POSPRA.db");

            if (string.IsNullOrWhiteSpace(dbPassword))
                dbPassword = "DefaultPassword123"; // fallback password if missing

            // 🔹 Step 3: Ensure directory exists
            string? dbDirectory = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrWhiteSpace(dbDirectory) && !Directory.Exists(dbDirectory))
            {
                Directory.CreateDirectory(dbDirectory);
            }

            // 🔹 Step 4: Create encrypted SQLite connection
            var connectionStringBuilder = new SqliteConnectionStringBuilder
            {
                DataSource = dbPath,
                Mode = SqliteOpenMode.ReadWriteCreate,
                Password = dbPassword
            };

            var sqliteConnection = new SqliteConnection(connectionStringBuilder.ToString());
            sqliteConnection.Open(); // SQLCipher key applied here

            // 🔹 Step 5: Build DI container
            var services = new ServiceCollection();

            // Register DbContext with open encrypted connection
            services.AddDbContext<SqliteDbContext>(options =>
            {
                options.UseSqlite(sqliteConnection);
            });

            // Register services, repositories, UoW, factory
            services.AddScoped<IScriptService, ScriptService>();
            services.AddScoped<ISqliteRepositoryFactory, SqliteRepositoryFactory>();
            services.AddScoped<ISqliteUnitOfWork, SqliteUnitOfWork>();
            services.AddSingleton<ISqliteDynamicFactory, SqliteDynamicFactory>();

            // ✅ Build the provider to resolve services
            using (var serviceProvider = services.BuildServiceProvider())
            {
                // Ensure database and tables exist
                using (var context = serviceProvider.GetRequiredService<SqliteDbContext>())
                {
                    context.Database.EnsureCreated();
                }

                // ✅ Get an instance of IScriptService from DI
                var scriptService = serviceProvider.GetRequiredService<IScriptService>();

                // 🔹 Step 6: Configure and launch the WinForms application
                ApplicationConfiguration.Initialize();
                System.Windows.Forms.Application.Run(new ConfigForm2(
                    configPath,
                    jsonWorkerPath,
                    jsonMainPath,
                    setupConfig,
                    configPath,
                    scriptService,
                    installationInfo
                ));
            }
        }
    }
}
