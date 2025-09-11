using AutoMapper;
using POSPRA.Domain.Entities;
using POSPRA.DTOs.UserDTOs;
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

        public async Task<UserDTO> CreateUserAsync(UserDTO dto)
        {
            var user = _mapper.Map<User>(dto);
            await _userRepository.AddAsync(user);
            await _sqliteUnitOfWork.SaveChangesAsync();
            return _mapper.Map<UserDTO>(user);
        }

        public async Task<List<UserDTO>> GetUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return _mapper.Map<List<UserDTO>>(users).ToList();
        }
    }
}
