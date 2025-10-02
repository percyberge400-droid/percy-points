using POSPRA.Domain.Entities;
using POSPRA.Infrastructure.Context;
using POSPRA.Repositories.BaseRepository;

namespace POSPRA.Repositories.ConfigurationRepository
{
    public class ConfigurationRepository : SqlServerRepository<Configurations>, IConfigurationRepository
    {
        public ConfigurationRepository(SqlServerDbContext context) : base(context)
        {
        }
    }
}
