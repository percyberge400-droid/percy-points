using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pos.Application.DTOs;
using Pos.Application.Interfaces;
using Pos.Application.Services.ScriptService;
using Pos.Infrastructure.Persistence.Factory;
using Pos.Infrastructure.Persistence.Repositories;
using Pos.SecurityEncryption;
using System.Configuration;
using System.Reflection;
using System.Text.Json;

namespace Pos.SetupUI
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            // ------------------------------
            // 1. DECRYPT APP.CONFIG FIRST
            // ------------------------------
            var decryptedAppConfigValues = EncryptedSettingsHelper.DecryptAppConfigFile();
            InjectIntoConfigurationManager(decryptedAppConfigValues);

            // ------------------------------
            // 2. Resolve config paths
            // ------------------------------
            string configPath = ResolveConfigPath(args);

            string baseFolder = Path.GetDirectoryName(configPath)
                ?? AppDomain.CurrentDomain.BaseDirectory;

            string jsonWorkerPath = Path.Combine(baseFolder, "appsettings.worker.json");
            string jsonMainPath = Path.Combine(baseFolder, "appsettings.json");
            string setupConfig = Path.Combine(baseFolder, "Pos.SetupUI.dll.config");
            string installationInfo = @"C:\ProgramData\PRAL";


            // ------------------------------
            // 3. Read DB path and password
            // ------------------------------
            string? dbPath = null;
            string? dbPassword = null;

            // Try worker JSON (decrypt)
            if (File.Exists(jsonWorkerPath))
            {
                try
                {
                    var decryptedWorkerSettings = DecryptJsonFile(jsonWorkerPath);

                    if (decryptedWorkerSettings.TryGetValue("AppSettings:DefaultDBFilePath", out var p1) ||
                        decryptedWorkerSettings.TryGetValue("DefaultDBFilePath", out p1))
                        dbPath = p1;

                    if (decryptedAppConfigValues.TryGetValue("AppSettings:DefaultDBPassword", out var p2) ||
                        decryptedAppConfigValues.TryGetValue("DefaultDBPassword", out p2))
                        dbPassword = p2;
                }
                catch { }
            }

            // Try decrypted app.config
            if (string.IsNullOrWhiteSpace(dbPath))
                dbPath = ConfigurationManager.AppSettings["DefaultDBFilePath"];

            if (string.IsNullOrWhiteSpace(dbPassword))
                dbPassword = ConfigurationManager.AppSettings["DefaultDBPassword"];   // FIXED BUG


            // Fallback defaults
            if (string.IsNullOrWhiteSpace(dbPath))
                dbPath = Path.Combine(AppContext.BaseDirectory, "POSPRA.db");

            if (string.IsNullOrWhiteSpace(dbPassword))
                dbPassword = "DefaultPassword123";


            // ------------------------------
            // 4. Ensure DB directory exists
            // ------------------------------
            string? dbDirectory = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrWhiteSpace(dbDirectory) && !Directory.Exists(dbDirectory))
                Directory.CreateDirectory(dbDirectory);


            // ------------------------------
            // 5. Create SQLCipher encrypted connection
            // ------------------------------
            var connectionStringBuilder = new SqliteConnectionStringBuilder
            {
                DataSource = dbPath,
                Mode = SqliteOpenMode.ReadWriteCreate,
                Password = dbPassword
            };

            var sqliteConnection = new SqliteConnection(connectionStringBuilder.ToString());
            sqliteConnection.Open();


            // ------------------------------
            // 6. Build DI Container
            // ------------------------------
            var services = new ServiceCollection();

            // Register DbContext
            services.AddDbContext<SqliteDbContext>(options =>
            {
                options.UseSqlite(sqliteConnection);
            });

            // REQUIRED FIX: Register AppSettings for IOptions<AppSettings>
            services.Configure<AppSettings>(opts =>
            {
                opts.Password = dbPassword;
                opts.DefaultDBFilePath = dbPath;
            });

            // Application Services
            services.AddScoped<IScriptService, ScriptService>();
            services.AddScoped<ISqliteRepositoryFactory, SqliteRepositoryFactory>();
            services.AddScoped<ISqliteUnitOfWork, SqliteUnitOfWork>();
            services.AddSingleton<ISqliteDynamicFactory, SqliteDynamicFactory>();


            // ------------------------------
            // 7. Run App
            // ------------------------------
            using (var serviceProvider = services.BuildServiceProvider())
            {
                using (var context = serviceProvider.GetRequiredService<SqliteDbContext>())
                {
                    context.Database.EnsureCreated();
                }

                ApplicationConfiguration.Initialize();

                System.Windows.Forms.Application.Run(
                    new ConfigForm2(
                        configPath,
                        jsonWorkerPath,
                        jsonMainPath,
                        setupConfig,
                        configPath,
                        serviceProvider.GetService<IScriptService>(),
                        installationInfo
                    )
                );
            }
        }

        // --------------------------------------------
        // Resolve EXE or Folder Passed in Arguments
        // --------------------------------------------
        private static string ResolveConfigPath(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                string candidate = args[0].Trim('"').TrimEnd('\\');

                if (Directory.Exists(candidate))
                    return Path.Combine(candidate, "Pos.WinFormsUI.dll.config");

                if (File.Exists(candidate))
                    return candidate;
            }

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(baseDir, "Pos.WinFormsUI.dll.config");
        }


        // --------------------------------------------
        // JSON Decryption
        // --------------------------------------------
        private static Dictionary<string, string> DecryptJsonFile(string jsonFilePath)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                if (!File.Exists(jsonFilePath))
                    return result;

                string jsonContent = File.ReadAllText(jsonFilePath);
                using JsonDocument doc = JsonDocument.Parse(jsonContent);

                string encryptedBlob = FindEncryptedBlobInJson(doc.RootElement);

                if (!string.IsNullOrEmpty(encryptedBlob))
                {
                    string decryptedJson = AesEncryptionHelper.Decrypt(encryptedBlob);
                    var decryptedDict = JsonSerializer.Deserialize<Dictionary<string, object>>(decryptedJson);

                    if (decryptedDict != null)
                    {
                        foreach (var kvp in decryptedDict)
                        {
                            string configKey = kvp.Key.StartsWith("AppSettings:")
                                ? kvp.Key
                                : $"AppSettings:{kvp.Key}";

                            result[configKey] = kvp.Value?.ToString();
                        }
                    }
                }
                else
                {
                    if (doc.RootElement.TryGetProperty("AppSettings", out JsonElement appSettings))
                    {
                        foreach (var prop in appSettings.EnumerateObject())
                        {
                            result[$"AppSettings:{prop.Name}"] = prop.Value.ToString();
                        }
                    }
                }
            }
            catch { }

            return result;
        }

        private static string FindEncryptedBlobInJson(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                if (element.TryGetProperty("EncryptedSettings", out JsonElement enc) &&
                    enc.ValueKind == JsonValueKind.String)
                {
                    return enc.GetString();
                }

                foreach (var prop in element.EnumerateObject())
                {
                    var result = FindEncryptedBlobInJson(prop.Value);
                    if (result != null)
                        return result;
                }
            }

            return null;
        }


        // --------------------------------------------
        // Inject decrypted values into ConfigurationManager
        // --------------------------------------------
        private static void InjectIntoConfigurationManager(Dictionary<string, string> decryptedValues)
        {
            try
            {
                var settings = ConfigurationManager.AppSettings;

                var readOnlyField = typeof(System.Collections.Specialized.NameValueCollection)
                    .GetField("_readOnly", BindingFlags.Instance | BindingFlags.NonPublic);

                readOnlyField?.SetValue(settings, false);

                foreach (var kvp in decryptedValues)
                {
                    string key = kvp.Key.Replace("AppSettings:", "");
                    settings[key] = kvp.Value;
                }

                readOnlyField?.SetValue(settings, true);
            }
            catch { }
        }
    }
}
