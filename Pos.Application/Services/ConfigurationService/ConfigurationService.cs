using pos.Application.Services.ConfigurationService;
using Pos.Application.DTOs.CommanDtos;
using Pos.Application.Interfaces;
using Pos.Domain.Entities;

namespace Pos.Application.Services.ConfigurationService
{
    public class ConfigurationService(ISqlServerRepositoryFactory sqlRepositoryFactory) : IConfigurationService
    {

        private readonly IRepository<POSConfigurations> _sqlConfigurationRepository = sqlRepositoryFactory.CreateRepository<POSConfigurations>();

        public async Task<bool> IsCloudSyncEnabledAsync(GetByPosIdDto dto)
        {
            try
            {
                var config = await _sqlConfigurationRepository
                .FirstOrDefaultAsync(x => x.POSID == dto.PosId);

                return config?.IsCloudSyncEnabled ?? false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}