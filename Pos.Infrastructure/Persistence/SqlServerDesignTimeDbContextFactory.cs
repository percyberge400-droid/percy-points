using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Pos.Infrastructure.Persistence.Factories
{
    public class SqlServerProductionDesignTimeDbContextFactory : IDesignTimeDbContextFactory<SqlServerDbContext>
    {
        public SqlServerDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile(Path.Combine("..", "Pos.Api", "appsettings.json"), optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString("SqlServerProduction");

            var optionsBuilder = new DbContextOptionsBuilder<SqlServerDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new SqlServerDbContext(optionsBuilder.Options);
        }
    }
}
