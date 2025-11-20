using Pos.Domain.Entities;
using Pos.Domain.ValueObjects;
using Pos.Infrastructure.Persistence;

namespace Pos.Application.Interfaces.Repositories
{
    public class CloudLogsRepository(DbContextFactory dbContextFactory) : ICloudLogsRepository
    {
        private readonly DbContextFactory _dbContextFactory = dbContextFactory;

        private async Task<SqlServerDbContext> SetEnvironmentAsync(string env)
        {
            EnvironmentType evnironment = env == "Production" ? EnvironmentType.Production : EnvironmentType.Sandbox;
            var dbContext = await _dbContextFactory.CreateSqlServerDbContextAsync(evnironment);
            return dbContext!;
        }


        public async Task AddRangeAsync(List<Logs> logs, string env)
        {
            var context = await SetEnvironmentAsync(env);

            await context.Logs.AddRangeAsync(logs);

            await context.SaveChangesAsync();

        }

        public async Task<IEnumerable<Logs>> GetAllLogsAsync(long posId, string env)
        {
            var context = await SetEnvironmentAsync(env);

            var logs = context.Logs.Where(X => X.POSID == posId);

            return logs.ToList();
        }
    }
}
