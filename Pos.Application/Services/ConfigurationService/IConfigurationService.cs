using Pos.Application.DTOs;
using Pos.Application.DTOs.CommanDtos;
using Pos.Application.DTOs.ConfigurationsDtos;

namespace pos.Application.Services.ConfigurationService
{
    public interface IConfigurationService
    {
        Task<bool> IsCloudSyncEnabledAsync(GetByPosIdDto dto);
        Task<ApiResponse<ConfigurationResponseDto>> GetUpdateVersion(string wwwrootPath);
        Task<ApiResponse<ConfigurationZipFileResponseDto>> GetZipFileAsync(string wwwrootPath);
    }
}