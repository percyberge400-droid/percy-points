using Pos.Application.DTOs.ClientDtos;
using Pos.Domain.Entities;

namespace Pos.Application.Interfaces.Repositories
{
    public interface IPosClientRepository
    {
        Task<PosClients> GetByMacAsync(ClientValidationDto dto);
    }
}
