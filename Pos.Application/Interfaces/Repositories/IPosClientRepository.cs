using Pos.Application.DTOs.ClientDtos;
using Pos.Domain.Entities;

namespace Pos.Application.Interfaces.Repositories
{
    public interface IPosClientRepository
    {
        Task<PosClients> GetByPosId(long posId, string env);
        Task<PosClients> GetByMacAsync(ClientValidationDto dto);
        Task<bool> UpdatePosCLientStatus(ClientValidationDto dto);
        Task<PosClients> UpdatePosClientHeartBeat(long posId, string env);
        Task<bool> UpdatePosCLientStatus(long posId, bool isActive, string envrionment);
        Task<PosClients?> GetByTokenAsync(string token, string env);
        Task<bool> DisablePosCLientLogBit(string env, long posId);
    }
}