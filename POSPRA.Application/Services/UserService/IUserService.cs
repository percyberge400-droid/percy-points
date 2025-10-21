using POSPRA.DTOs.UserDtos;

namespace POSPRA.Application.Services.UserService
{
    public interface IUserService
    {
        Task<UserDto> CreateUserAsync(UserDto dto);
        Task<List<UserDto>> GetUsersAsync();
    }
}
