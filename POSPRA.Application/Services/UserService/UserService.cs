using AutoMapper;
using POSPRA.Domain.Entities;
using POSPRA.DTOs.UserDTOs;
using POSPRA.Repositories;
using POSPRA.Repositories.BaseRepository;

namespace POSPRA.Application.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public UserService(IRepository<User> userRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<UserDTO> CreateUserAsync(UserDTO dto)
        {
            var userEntity = _mapper.Map<User>(dto);

            await _userRepository.AddAsync(userEntity);
            await _unitOfWork.SaveChangesAsync();

            // Map back to DTO (including generated Id)
            return _mapper.Map<UserDTO>(userEntity);
        }

        public async Task<List<UserDTO>> GetUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return (List<UserDTO>)_mapper.Map<IEnumerable<UserDTO>>(users);
        }
    }
}
