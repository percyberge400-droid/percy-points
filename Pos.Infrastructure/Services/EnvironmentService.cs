using Pos.Application.Interfaces;
using Pos.Domain.ValueObjects;
using System.Text.Json;

namespace Pos.Infrastructure.Services
{
    public class EnvironmentService : IEnvironmentService
    {
        private readonly string _jsonFilePath;

        public EnvironmentService()
        {
          
        }

        public EnvironmentService(string configFilePath)
        {
            if (string.IsNullOrWhiteSpace(configFilePath))
                throw new ArgumentNullException(nameof(configFilePath));

            if (!File.Exists(configFilePath))
                throw new FileNotFoundException("Environment config file not found", configFilePath);

            _jsonFilePath = configFilePath;
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

        public Task SetCurrentEnvironment(EnvironmentType type)
        {
            throw new NotImplementedException();
        }
    }
}
