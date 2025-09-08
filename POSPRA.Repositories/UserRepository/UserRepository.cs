using Microsoft.EntityFrameworkCore;
using POSPRA.Domain.Entities;
using POSPRA.Infrastructure.Context;

namespace POSPRA.Repositories.UserRepository
{
    public class UserRepository : IUserRepository
    {
        private readonly SqliteDbContext _context;

        public UserRepository(SqliteDbContext context) => _context = context;

        public async Task<User> AddUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }
    }
}