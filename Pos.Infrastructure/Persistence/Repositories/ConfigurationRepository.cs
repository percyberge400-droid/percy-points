using Microsoft.EntityFrameworkCore;
using Pos.Application.Interfaces.Repositories;
using Pos.Domain.Entities;
using Pos.Domain.ValueObjects;

namespace Pos.Infrastructure.Persistence.Repositories
{
    public class ConfigurationRepository : IConfigurationRepository
    {
        private readonly DbContextFactory _dbContextFactory;
        public ConfigurationRepository(DbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<bool> IsCloudSyncEnabledAsync(long posId, string env)
        {
            var context = await SetEnvironmentAsync(env);
            var config = await context.POSConfigurations.FirstOrDefaultAsync(x => x.POSID == posId);

            return config?.IsCloudSyncEnabled ?? false;
        }

        public async Task<POSConfigurations> GetByPosId(long posId, string env)
        {
            var context = await SetEnvironmentAsync(env);
            var entity = await context!.POSConfigurations.FirstOrDefaultAsync(x => x.POSID == posId);
            return entity!;
        }

        public async Task<POSConfigurations> UpdateConfigurationStatus(long posId, string env)
        {
            var context = await SetEnvironmentAsync(env);
            var posConfigurations = await context!.POSConfigurations.FirstOrDefaultAsync(x => x.POSID == posId);
            if (posConfigurations != null)
            {
                posConfigurations.IsActive = false;
                await context.SaveChangesAsync();
            }

            return posConfigurations;
        }

        private async Task<SqlServerDbContext> SetEnvironmentAsync(string env)
        {
            EnvironmentType evnironment = env == "Production" ? EnvironmentType.Production : EnvironmentType.Sandbox;
            var dbContext = await _dbContextFactory.CreateSqlServerDbContextAsync(evnironment);
            return dbContext!;
        }

    }
}
