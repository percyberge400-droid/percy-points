using AutoMapper;
using POSPRA.Domain.Entities;
using POSPRA.DTOs.UserDtos;
using POSPRA.Repositories.BaseRepository;
using POSPRA.Repositories.UnitOfWork;

namespace POSPRA.Application.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ISqliteUnitOfWork _sqliteUnitOfWork;

        private readonly ISqlServerUnitOfWork _sqlServerUnitOfWork;

        private readonly IMapper _mapper;

        public UserService(
            IUserRepository userRepository,
            ISqliteUnitOfWork sqliteUnitOfWork,
            ISqlServerUnitOfWork sqlServerUnitOfWork,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _sqliteUnitOfWork = sqliteUnitOfWork;

            _sqlServerUnitOfWork = sqlServerUnitOfWork;

            _mapper = mapper;
        }

        public async Task<UserDto> CreateUserAsync(UserDto dto)
        {
            var user = _mapper.Map<User>(dto);
            await _userRepository.AddAsync(user);
            await _sqliteUnitOfWork.SaveChangesAsync();
            return _mapper.Map<UserDto>(user);
        }

        public async Task<List<UserDto>> GetUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return _mapper.Map<List<UserDto>>(users).ToList();
        }
    }
}
