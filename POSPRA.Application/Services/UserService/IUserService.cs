using POSPRA.DTOs.UserDTOs;

namespace POSPRA.Application.Services.UserService
{
    public interface IUserService
    {
        Task<UserDTO> CreateUserAsync(UserDTO dto);
        Task<List<UserDTO>> GetUsersAsync();
    }
}
