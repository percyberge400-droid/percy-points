using AutoMapper;
using POSPRA.Domain.Entities;
using POSPRA.DTOs.UserDTOs;

namespace POSPRA.Application.AutoMapperProfile
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            // DTO -> Entity
            CreateMap<UserDTO, User>();

            // Entity -> DTO (useful if you return DTOs later)
            CreateMap<User, UserDTO>();
        }
    }
}
