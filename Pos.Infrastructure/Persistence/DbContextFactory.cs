using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Pos.Application.Interfaces;

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
        /// Creates a new SqlServerDbContext based on current environment.
        /// Use forceProduction = true to always connect to the Production database.
        /// </summary>
        /// <param name="forceProduction">If true, always use Production DB</param>
        /// <returns>SqlServerDbContext</returns>
        public SqlServerDbContext CreateSqlServerDbContext(bool forceProduction = false)
        {
            // Get current environment (Sandbox or Production)
            var env = _environmentService.GetCurrentEnvironment();

            // Force Production if requested
            if (forceProduction)
                env = EnvironmentType.Production;

            // Pick connection string based on environment
            var connectionString = env == EnvironmentType.Sandbox
                ? _configuration.GetConnectionString("SandboxConnection")
                : _configuration.GetConnectionString("ProductionConnection");

            // Configure DbContextOptions
            var optionsBuilder = new DbContextOptionsBuilder<SqlServerDbContext>();

            optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
            {
                // Enable retry logic for transient SQL issues
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null
                );
            });

            return new SqlServerDbContext(optionsBuilder.Options);
        }
    }
}
