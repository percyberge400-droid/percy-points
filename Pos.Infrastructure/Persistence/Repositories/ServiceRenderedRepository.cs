using Microsoft.EntityFrameworkCore;
using Pos.Application.Interfaces.Repositories;
using Pos.Domain.Entities;
using Pos.Domain.ValueObjects;

namespace Pos.Infrastructure.Persistence.Repositories
{
    public class ServiceRenderedRepository(DbContextFactory dbContextFactory) : IServiceRenderedRepository
    {
        private readonly DbContextFactory _dbContextFactory = dbContextFactory;

        public async Task<IEnumerable<ServiceRendered>> GetServiceRendered(string env)
        {
            var context = await SetEnvironmentAsync(env);
            IQueryable<ServiceRendered> query = context.ServiceRendered.AsQueryable();
            return await query.ToListAsync();
        }

        private async Task<SqlServerDbContext> SetEnvironmentAsync(string env)
        {
            EnvironmentType evnironment = env == "Production" ? EnvironmentType.Production : EnvironmentType.Sandbox;
            var dbContext = await _dbContextFactory.CreateSqlServerDbContextAsync(evnironment);
            return dbContext!;
        }
    }
}
