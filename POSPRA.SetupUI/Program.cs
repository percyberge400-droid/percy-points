using System;
using System.IO;
using System.Windows.Forms;
using System.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using POSPRA.Infrastructure.Context;

namespace POSPRA.SetupUI
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
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

            // 🔹 Step 5: Build DI container (if needed later)
            var services = new ServiceCollection();
            services.AddDbContext<SqliteDbContext>(opt => opt.UseSqlite($"Data Source={dbPath}"));

            // 🔹 Step 6: Launch ConfigForm (so it can update BOTH JSON + XML configs)
            ApplicationConfiguration.Initialize();
            Application.Run(new ConfigForm(configPath, jsonWorkerPath, jsonMainPath, setupConfig, configPath));

        }
    }
}
