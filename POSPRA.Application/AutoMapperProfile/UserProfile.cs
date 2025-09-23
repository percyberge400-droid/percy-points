using AutoMapper;
using POSPRA.Domain.Entities;
using POSPRA.DTOs.UserDtos;

namespace POSPRA.Application.AutoMapperProfile
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            // DTO -> Entity
            CreateMap<UserDto, User>();

            // Entity -> DTO (useful if you return DTOs later)
            CreateMap<User, UserDto>();
        }
    }
}
