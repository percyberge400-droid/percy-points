using Pos.Application.DTOs;
using Pos.Application.DTOs.CommanDtos;
using Pos.Application.DTOs.ConfigurationsDtos;

namespace pos.Application.Services.ConfigurationService
{
    public interface IConfigurationService
    {
        Task<bool> IsCloudSyncEnabledAsync(GetByPosIdDto dto);
        Task<ApiResponse<ConfigurationResponseDto>> GetUpdateVersion();
        Task<(string base64, string fileName)> GetZipFileAsync();
    }
}