using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Pos.Application.Interfaces;
using Pos.Domain.ValueObjects;

namespace Pos.Infrastructure.Persistence
{
    public class DbContextFactory
    {
        private readonly IConfiguration _configuration;
        private readonly IEnvironmentService _environmentService;

        public DbContextFactory(IConfiguration configuration, IEnvironmentService environmentService)
        {
            _configuration = configuration;
            _environmentService = environmentService;
        }

        /// <summary>
        /// Creates a new SqlServerDbContext based on the current process environment (async).
        /// </summary>
        public async Task<SqlServerDbContext?> CreateSqlServerDbContextAsync(bool forceProduction = false)
        {
            var env = await _environmentService.GetCurrentEnvironmentAsync();

            // If environment is null → stop and return null (or throw if you prefer)
            if (env == null)
                return null;

            if (forceProduction)
                env = EnvironmentType.Production;

            var connectionString = env == EnvironmentType.Sandbox
                ? _configuration.GetConnectionString("SandboxConnection")
                : _configuration.GetConnectionString("ProductionConnection");

            var optionsBuilder = new DbContextOptionsBuilder<SqlServerDbContext>();
            optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            });

            return new SqlServerDbContext(optionsBuilder.Options);
        }

        public async Task<SqlServerDbContext?> CreateSqlServerDbContextAsync(EnvironmentType environment)
        {
            var connectionString = environment == EnvironmentType.Sandbox
                ? _configuration.GetConnectionString("SandboxConnection")
                : _configuration.GetConnectionString("ProductionConnection");

            var optionsBuilder = new DbContextOptionsBuilder<SqlServerDbContext>();
            optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            });

            return new SqlServerDbContext(optionsBuilder.Options);
        }

        /// <summary>
        /// Synchronous wrapper for DI usage.
        /// </summary>
        public SqlServerDbContext CreateSqlServerDbContext(bool forceProduction = false)
        {
            // Use .GetAwaiter().GetResult() to block until async completes
            return CreateSqlServerDbContextAsync(forceProduction).GetAwaiter().GetResult();
        }
    }
}
