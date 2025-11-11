using Pos.Domain.Entities;
using Pos.Application.DTOs;
using Pos.Application.DTOs.ClientDtos;
using Pos.Application.DTOs.PosDtos;

namespace Pos.Application.Services.PosService
{
    /// <summary>
    /// Service contract for POS-related operations.
    /// </summary>
    public interface IPosService
    {
        Task<ApiResponse<HeartBeatDto>> UpdateHeartBeatAsync(int posId);
        Task<ApiResponse<List<PosConfigurationDto>>> GetConfigurationsAsync();
        Task<ApiResponse<List<PosStatus>>> InsertPosStatusAsync();
    }
}
