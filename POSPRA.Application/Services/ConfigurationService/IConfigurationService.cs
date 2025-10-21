using POSPRA.DTOs.CommanDtos;

namespace POSPRA.Application.Services.ConfigurationService
{
    public interface IConfigurationService
    {
        Task<bool> IsCloudSyncEnabledAsync(GetByPosIdDto dto);
    }
}