using Pos.Domain.Entities;

namespace Pos.Application.Interfaces.Repositories
{
    public interface IConfigurationRepository
    {
        Task<bool> IsCloudSyncEnabledAsync(long posId, string env);
        Task<POSConfigurations> GetByPosId(long posId, string env);

        Task<POSConfigurations> UpdateConfigurationStatus(long posId, string env);
    }
}
