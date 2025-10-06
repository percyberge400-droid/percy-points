using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using POSPRA.Infrastructure.Context;

namespace POSPRA.SetupUI
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
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
            //var configuration = new ConfigurationBuilder()
            //    .SetBasePath(Directory.GetCurrentDirectory())
            //    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            //    .Build();

            // ✅ Build DI container
            var services = new ServiceCollection();

            // Register DbContexts
            services.AddDbContext<SqliteDbContext>(opt => opt.UseSqlite($"Data Source={dbPath}"));
            string configPath = string.Empty;

            // 1?? Try to read path passed from MSI custom action arguments
            if (args != null && args.Length > 0)
            {
                string candidate = args[0].Trim('"').TrimEnd('\\'); // ?? strip trailing slash

                if (Directory.Exists(candidate))
                {
                    configPath = Path.Combine(candidate, "POSPRA-WinFormsUI.dll.config");
                }
                else if (File.Exists(candidate))
                {
                    configPath = candidate;
                }
            }

            // 2?? Fallback: use app base directory if nothing passed
            if (string.IsNullOrEmpty(configPath))
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                configPath = Path.Combine(baseDir, "POSPRA-WinFormsUI.dll.config");
            }

            // 3?? Check existence before launching form
            //if (!File.Exists(configPath))
            //{
            //    MessageBox.Show($"Config file not found:\n{configPath}",
            //        "Configuration", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}

            ApplicationConfiguration.Initialize();
            Application.Run(new ConfigForm(configPath)); // ? pass FULL PATH directly
        }
    }
}
