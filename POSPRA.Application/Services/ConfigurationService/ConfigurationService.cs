using POSPRA.DTOs.CommanDtos;
using POSPRA.Repositories.ConfigurationRepository;

namespace POSPRA.Application.Services.ConfigurationService
{
    public class ConfigurationService(IConfigurationRepository configurationRepository) : IConfigurationService
    {
        private readonly IConfigurationRepository _configurationRepository = configurationRepository;

        public async Task<bool> IsCloudSyncEnabledAsync(GetByPosIdDto dto)
        {
            var config = await _configurationRepository
                .FirstOrDefaultAsync(x => x.POSID == dto.PosId);

            return config?.IsCloudSyncEnabled ?? true;
        }
    }
}