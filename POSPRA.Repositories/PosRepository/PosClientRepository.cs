using POSPRA.Domain.Entities;
using POSPRA.Infrastructure.Context;
using POSPRA.Repositories.BaseRepository;

namespace POSPRA.Repositories.PosRepository
{
    public class PosClientRepository : SqlServerRepository<PosClients>, IPosClientRepository
    {
        public PosClientRepository(SqlServerDbContext context) : base(context)
        {
        }
    }
}
