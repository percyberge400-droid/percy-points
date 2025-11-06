using Microsoft.Extensions.Options;
using POSPRA.Application.Utility;
using POSPRA.DTOs;
using System.Text.RegularExpressions;

namespace POSPRA.Application.Services.EnvironmentConfigService
{
    public class EnvironmentConfigService : IEnvironmentConfigService
    {
        private readonly IOptionsMonitor<AppSettings> _options;
        private readonly string _configFilePath;

        public EnvironmentConfigService(IOptionsMonitor<AppSettings> options)
        {
            _options = options;

            // Gets the appsettings.json file from current directory
            _configFilePath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
        }

        public bool IsProduction => _options.CurrentValue.IsProduction;

        public ApiResponse<string> SetEnvironment(bool isProduction)
        {
            try
            {
                if (!File.Exists(_configFilePath))
                {
                    return new ApiResponse<string>(
                        ApiStatusCode.NotFound,
                        "Configuration file not found.",
                        null!,
                        $"Missing file: {_configFilePath}");
                }

                var json = File.ReadAllText(_configFilePath);

                // ✅ Safely replace the IsProduction flag
                var newJson = Regex.Replace(
                    json,
                    "\"IsProduction\"\\s*:\\s*(true|false)",
                    $"\"IsProduction\": {isProduction.ToString().ToLower()}");

                File.WriteAllText(_configFilePath, newJson);

                // ✅ Return success response
                string envName = isProduction ? "Production" : "Sandbox";
                return new ApiResponse<string>(
                    ApiStatusCode.Success,
                    $"Environment switched to {envName} successfully.",
                    envName,
                    string.Empty);
            }
            catch (Exception ex)
            {
                // ✅ Return error response
                return new ApiResponse<string>(
                    ApiStatusCode.Unauthorized,
                    "Failed to update environment configuration.",
                    null!,
                    ex.Message);
            }
        }
    }
}
