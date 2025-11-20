using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Pos.Application.Interfaces;
using Pos.Application.Services.ScriptService;
using Pos.Infrastructure.Persistence.Factory;
using Pos.Infrastructure.Persistence.Repositories;
using System.Configuration;

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

            // 🔹 Step 2: Read DB path from JSON → SetupUI.config → fallback
            string? dbPath = null;

            // 2a: Try worker JSON
            if (File.Exists(jsonWorkerPath))
            {
                try
                {
                    var json = JObject.Parse(File.ReadAllText(jsonWorkerPath));
                    dbPath = json["AppSettings"]?["DefaultDBFilePath"]?.ToString();
                }
                catch { /* ignore */ }
            }

            // 2b: If not found → try SetupUI.config
            if (string.IsNullOrWhiteSpace(dbPath))
            {
                try
                {
                    dbPath = ConfigurationManager.AppSettings["DefaultDBFilePath"];
                }
                catch { /* ignore */ }
            }

            // 2c: Fallback → default db
            if (string.IsNullOrWhiteSpace(dbPath))
            {
                dbPath = Path.Combine(AppContext.BaseDirectory, "POSPRA.db");
            }

            // 🔹 Step 3: Ensure directory exists
            string? dbDirectory = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrWhiteSpace(dbDirectory) && !Directory.Exists(dbDirectory))
            {
                //Directory.CreateDirectory(dbDirectory);
            }

            // 🔹 Step 4: Initialize SQLite database
            var sqliteOptions = new DbContextOptionsBuilder<SqliteDbContext>()
                .UseSqlite($"Data Source={dbPath}")
                .Options;

            using (var context = new SqliteDbContext(sqliteOptions))
            {
                //context.Database.EnsureCreated();
            }

            // 🔹 Step 5: Build DI container
            var services = new ServiceCollection();
            services.AddDbContext<SqliteDbContext>(opt => opt.UseSqlite($"Data Source={dbPath}"));
            services.AddScoped<IScriptService, ScriptService>();
            services.AddScoped<ISqliteRepositoryFactory, SqliteRepositoryFactory>();
            services.AddScoped<ISqliteUnitOfWork, SqliteUnitOfWork>();
            services.AddSingleton<ISqliteDynamicFactory, SqliteDynamicFactory>();

            // ✅ Build the provider to resolve services
            using (var serviceProvider = services.BuildServiceProvider())
            {
                // ✅ Get an actual instance of IScriptService from DI
                var scriptService = serviceProvider.GetRequiredService<IScriptService>();


                // 🔹 Step 6: Configure and launch the application
                ApplicationConfiguration.Initialize();

                // Use Application.Run() instead of app.Run()
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