using POSPRA.Domain.Entities;
using POSPRA.Infrastructure.Context;
using POSPRA.Repositories.BaseRepository;

namespace POSPRA.Repositories.UserRepository
{
    // SQLite User repository
    public class UserRepository : SqliteRepository<User>, IUserRepository
    {
        public UserRepository(SqliteDbContext context) : base(context) { }
    }
}