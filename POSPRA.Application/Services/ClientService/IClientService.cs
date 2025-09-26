using POSPRA.DTOs;
using POSPRA.DTOs.ClientDtos;

namespace POSPRA.Application.Services.ClientService
{
    public interface IClientService
    {
        Task<ApiResponse<bool>> GetByMacAsync(ClientValidationDto dto);
    }
}
