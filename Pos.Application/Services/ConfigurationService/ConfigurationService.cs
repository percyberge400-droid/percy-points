using pos.Application.Services.ConfigurationService;
using Pos.Application.DTOs.CommanDtos;
using Pos.Application.Interfaces.Repositories;

namespace Pos.Application.Services.ConfigurationService
{
    public class ConfigurationService(IConfigurationRepository configurationRepository) : IConfigurationService
    {

        private readonly IConfigurationRepository _configurationRepository = configurationRepository;

        public async Task<bool> IsCloudSyncEnabledAsync(GetByPosIdDto dto)
        {
            try
            {
                var isEnabled = await _configurationRepository.IsCloudSyncEnabledAsync(dto.PosId, dto.Environment!);

                return isEnabled;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}