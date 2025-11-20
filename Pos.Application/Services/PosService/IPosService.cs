using Pos.Application.DTOs;
using Pos.Application.DTOs.ClientDtos;
using Pos.Application.DTOs.PosDtos;
using Pos.Domain.Entities;

namespace Pos.Application.Services.PosService
{
    /// <summary>
    /// Service contract for POS-related operations.
    /// </summary>
    public interface IPosService
    {
        Task<ApiResponse<HeartBeatDto>> UpdateHeartBeatAsync(int posId, string env);
        Task<ApiResponse<List<PosConfigurationDto>>> GetConfigurationsAsync(string env);
        Task<ApiResponse<List<PosStatus>>> InsertPosStatusAsync();
    }
}
