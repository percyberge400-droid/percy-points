using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pos.Application.Interfaces;
using Pos.Domain.ValueObjects;

namespace Pos.Infrastructure.Persistence
{
    public class DbContextFactory
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;

        public DbContextFactory(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Creates a new SqlServerDbContext based on the current environment (async).
        /// </summary>
        public async Task<SqlServerDbContext> CreateSqlServerDbContextAsync(bool forceProduction = false)
        {
            using var scope = _serviceProvider.CreateScope();
            var envService = scope.ServiceProvider.GetRequiredService<IEnvironmentService>();

            var env = await envService.GetCurrentEnvironmentAsync();

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

        /// <summary>
        /// Creates a SqlServerDbContext for a specific environment (async).
        /// </summary>
        public async Task<SqlServerDbContext> CreateSqlServerDbContextAsync(EnvironmentType environment)
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
        /// Synchronous wrapper (if needed, but prefer async usage).
        /// </summary>
        public SqlServerDbContext CreateSqlServerDbContext(bool forceProduction = false)
        {
            return CreateSqlServerDbContextAsync(forceProduction).GetAwaiter().GetResult();
        }
    }
}
