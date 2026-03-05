using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pos.Application.AutoMapperProfile;
using Pos.Application.DTOs;
using Pos.Infrastructure;
using Pos.SecurityEncryption;
using Pos.WinFormsUI.Dashboard;
using Pos.WinFormsUI.Forms;
using System.Drawing.Text;
using System.Reflection;
using System.Windows.Forms;

namespace Pos.WinFormsUI
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
            // 1️⃣ Decrypt appsettings.json
            var decryptedSettings = EncryptedSettingsHelper.DecryptSettingsFile("appsettings.json");

            // 2️⃣ Decrypt app.config settings
            var decryptedAppConfigValues = EncryptedSettingsHelper.DecryptAppConfigFile();

            // 3️⃣ Inject decrypted values into ConfigurationManager
            InjectIntoConfigurationManager(decryptedAppConfigValues);

            // 4️⃣ Merge app.config values (override appsettings.json)
            foreach (var kvp in decryptedAppConfigValues)
                decryptedSettings[kvp.Key] = kvp.Value;

            // 5️⃣ Resolve DB path with fallback
            string dbPath = decryptedSettings.TryGetValue("AppSettings:DefaultDBFilePath", out var dbPathValue) &&
                            !string.IsNullOrWhiteSpace(dbPathValue)
                ? dbPathValue
                : Path.Combine(AppContext.BaseDirectory, "POSPRA.db");
            decryptedSettings["AppSettings:DefaultDBFilePath"] = dbPath;

            // 6️⃣ Resolve DB password with fallback
            string dbPassword = decryptedSettings.TryGetValue("AppSettings:DefaultDBPassword", out var passValue) &&
                                !string.IsNullOrWhiteSpace(passValue)
                ? passValue
                : "DefaultPassword123";
            decryptedSettings["AppSettings:DefaultDBPassword"] = dbPassword;

            // 7️⃣ Build merged configuration
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(decryptedSettings)
                .Build();

            // 8️⃣ Ensure DB directory exists
            var dbDirectory = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrWhiteSpace(dbDirectory) && !Directory.Exists(dbDirectory))
               // Directory.CreateDirectory(dbDirectory);

            // 9️⃣ Check DB file validity
            if (File.Exists(dbPath))
            {
                var length = new FileInfo(dbPath).Length;
                if (length == 0)
                {
                    // Empty file cannot be opened by SQLite
                    File.Delete(dbPath);
                }
            }

            // 1️⃣0️⃣ SQLCipher encrypted connection
            var connectionStringBuilder = new SqliteConnectionStringBuilder
            {
                DataSource = dbPath,
                Mode = SqliteOpenMode.ReadWriteCreate,
                Password = dbPassword
            };

            using var sqliteConnection = new SqliteConnection(connectionStringBuilder.ToString());

            try
            {
                sqliteConnection.Open();
            }
            catch (SqliteException ex)
            {
                Console.WriteLine($"SQLite Exception: {ex.Message}");
                // If corrupted file exists, delete and recreate
                if (File.Exists(dbPath))
                {
                    File.Delete(dbPath);
                    sqliteConnection.Open();
                    Console.WriteLine("Database recreated successfully.");
                }
                else
                {
                    throw; // Rethrow for unexpected exceptions
                }
            }

            // 1️⃣1️⃣ Dependency Injection Setup
            var services = new ServiceCollection();

            // Register configuration
            services.AddSingleton<IConfiguration>(configuration);
            services.Configure<AppSettings>(configuration);

            // Logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
            });

            // Infrastructure & EF Core
            services.AddInfrastructure(configuration, true);
            services.AddDbContext<SqliteDbContext>(options =>
                options.UseSqlite(sqliteConnection));

            // AutoMapper
            services.AddAutoMapper(cfg => cfg.AddProfile<PosProfile>(), Assembly.GetExecutingAssembly());

            // WinForms DI registration
            services.AddTransient<Profile>();
            services.AddTransient<LoginForm2>();
            //services.AddTransient<DashboardFormNew>();
            services.AddTransient<DashboardForm>();

            services.AddTransient<Main>();
            services.AddTransient<ItemEntry>();
            services.AddTransient<ExportInvoiceForm>();
            services.AddTransient<CatalogView>();

            using var provider = services.BuildServiceProvider();

            // 1️⃣2️⃣ Ensure DB + tables exist
            using (var scope = provider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<SqliteDbContext>();
              //  dbContext.Database.EnsureCreated();
            }

            // 1️⃣3️⃣ Start WinForms App
            ApplicationConfiguration.Initialize();
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            var loginForm = provider.GetRequiredService<LoginForm2>();
            System.Windows.Forms.Application.Run(loginForm);
        }

        // Inject decrypted values into ConfigurationManager.AppSettings
        private static void InjectIntoConfigurationManager(Dictionary<string, string> decryptedValues)
        {
            try
            {
                var settings = System.Configuration.ConfigurationManager.AppSettings;

                foreach (var kvp in decryptedValues)
                {
                    string key = kvp.Key.Replace("AppSettings:", "");

                    var existing = settings[key];
                    if (existing != null)
                    {
                        settings[key] = kvp.Value;
                    }
                    else
                    {
                        var readonlyField = typeof(System.Collections.Specialized.NameValueCollection)
                            .GetField("_readOnly", BindingFlags.Instance | BindingFlags.NonPublic);

                        readonlyField?.SetValue(settings, false);
                        settings[key] = kvp.Value;
                        readonlyField?.SetValue(settings, true);
                    }
                }
            }
            catch
            {
                // ignore; fallback is stored in decryptedSettings anyway
            }
        }
    }
}
