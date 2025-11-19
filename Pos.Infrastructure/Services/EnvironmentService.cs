using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Pos.Application.Interfaces;
using Pos.Domain.ValueObjects;

namespace Pos.Infrastructure.Services
{
    public class EnvironmentService : IEnvironmentService
    {
        private readonly string _jsonFilePath;
        private readonly string _appConfigPath; // path to App.config (WinForms)

        public EnvironmentService(string configFilePath, string appConfigPath = null)
        {
            if (string.IsNullOrWhiteSpace(configFilePath))
                throw new ArgumentNullException(nameof(configFilePath));

            if (!File.Exists(configFilePath))
                throw new FileNotFoundException("Environment config file not found", configFilePath);

            _jsonFilePath = configFilePath;
            _appConfigPath = appConfigPath; // optional
        }

        public async Task<EnvironmentType> GetCurrentEnvironmentAsync()
        {
            try
            {
                if (!File.Exists(_jsonFilePath))
                    return EnvironmentType.Production;

                var json = await File.ReadAllTextAsync(_jsonFilePath);
                using var doc = JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("AppSettings", out var appSettings) &&
                    appSettings.TryGetProperty("Environment", out var envProp))
                {
                    if (Enum.TryParse<EnvironmentType>(envProp.GetString(), out var env))
                        return env;
                }

                return EnvironmentType.Production;
            }
            catch
            {
                return EnvironmentType.Production;
            }
        }

        public async Task SetCurrentEnvironment(EnvironmentType type)
        {
            await SetCurrentEnvironmentAsync(type);
        }

        public async Task SetCurrentEnvironmentAsync(EnvironmentType type)
        {
            await UpdateWorkerJsonAsync(type);

            if (!string.IsNullOrWhiteSpace(_appConfigPath) && File.Exists(_appConfigPath))
            {
                UpdateAppConfig(type);
            }
        }

        // -----------------------------
        // Private helper: update worker JSON
        // -----------------------------
        private async Task UpdateWorkerJsonAsync(EnvironmentType type)
        {
            try
            {
                string json = File.Exists(_jsonFilePath) ? await File.ReadAllTextAsync(_jsonFilePath) : "{}";

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                using var stream = new MemoryStream();
                using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });

                writer.WriteStartObject();

                // AppSettings
                if (root.TryGetProperty("AppSettings", out var appSettings))
                {
                    writer.WritePropertyName("AppSettings");
                    writer.WriteStartObject();

                    bool hasEnvironment = false;

                    foreach (var prop in appSettings.EnumerateObject())
                    {
                        if (prop.NameEquals("Environment"))
                        {
                            writer.WriteString("Environment", type.ToString());
                            hasEnvironment = true;
                        }
                        else
                        {
                            prop.WriteTo(writer);
                        }
                    }

                    if (!hasEnvironment)
                        writer.WriteString("Environment", type.ToString());

                    writer.WriteEndObject();
                }
                else
                {
                    writer.WritePropertyName("AppSettings");
                    writer.WriteStartObject();
                    writer.WriteString("Environment", type.ToString());
                    writer.WriteEndObject();
                }

                // Copy other top-level properties
                foreach (var prop in root.EnumerateObject())
                {
                    if (prop.NameEquals("AppSettings")) continue;
                    prop.WriteTo(writer);
                }

                writer.WriteEndObject();
                await writer.FlushAsync();

                await File.WriteAllTextAsync(_jsonFilePath, Encoding.UTF8.GetString(stream.ToArray()));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating worker JSON: {ex}");
            }
        }

        // -----------------------------
        // Private helper: update App.config
        // -----------------------------
        private void UpdateAppConfig(EnvironmentType type)
        {
            try
            {
                var doc = XDocument.Load(_appConfigPath);
                var appSettings = doc.Root.Element("appSettings");

                if (appSettings != null)
                {
                    var envElement = appSettings.Elements("add")
                                                .FirstOrDefault(x => x.Attribute("key")?.Value == "Environment");

                    if (envElement != null)
                    {
                        envElement.SetAttributeValue("value", type.ToString());
                    }
                    else
                    {
                        appSettings.Add(new XElement("add",
                            new XAttribute("key", "Environment"),
                            new XAttribute("value", type.ToString())));
                    }

                    doc.Save(_appConfigPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating App.config: {ex}");
            }
        }
    }
}
