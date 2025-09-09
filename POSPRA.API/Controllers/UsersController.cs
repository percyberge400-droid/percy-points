using Microsoft.AspNetCore.Mvc;
using POSPRA.Application.Services.UserService;
using POSPRA.Domain.Entities;
using POSPRA.DTOs.UserDTOs;

namespace POSPRA.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<ActionResult<UserDTO>> CreateUser([FromBody] UserDTO dto)
        {
            var created = await _userService.CreateUserAsync(dto);
            return Ok(created); // returns DTO instead of entity
        }

        [HttpGet]
        public async Task<ActionResult<List<User>>> GetAllUsers()
        {
            var users = await _userService.GetUsersAsync();
            return Ok(users);
        }
    }
}
