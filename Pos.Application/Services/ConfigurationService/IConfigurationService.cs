using Pos.Application.DTOs.CommanDtos;

namespace pos.Application.Services.ConfigurationService
{
    public interface IConfigurationService
    {
        Task<bool> IsCloudSyncEnabledAsync(GetByPosIdDto dto);
    }
}