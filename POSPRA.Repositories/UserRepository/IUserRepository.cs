using POSPRA.Domain.Entities;

namespace POSPRA.Repositories.UserRepository
{
    public interface IUserRepository
    {
        Task<User> AddUserAsync(User user);
        Task<List<User>> GetAllUsersAsync();
    }
}
