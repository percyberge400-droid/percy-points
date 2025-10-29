using POSPRA.DTOs;

namespace POSPRA.Application.Services.EnvironmentConfigService
{
    public interface IEnvironmentConfigService
    {
        bool IsProduction { get; }
        ApiResponse<string> SetEnvironment(bool isProduction);
        string GetConnectionString();
    }
}