using POSPRA.Domain.Entities;
using POSPRA.DTOs;
using POSPRA.DTOs.ClientDtos;

namespace POSPRA.Application.Services.ClientService
{
    public interface IClientService
    {
        Task<ApiResponse<PosClients>> GetByMacAsync(ClientValidationDto dto);
    }
}
