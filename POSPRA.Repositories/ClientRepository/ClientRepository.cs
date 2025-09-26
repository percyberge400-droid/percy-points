using POSPRA.Domain.Entities;
using POSPRA.Infrastructure.Context;
using POSPRA.Repositories.BaseRepository;

namespace POSPRA.Repositories.ClientRepository
{
    public class ClientRepository : SqlServerRepository<PosClients>, IClientRepository
    {
        public ClientRepository(SqlServerDbContext context) : base(context)
        {
        }
    }
}