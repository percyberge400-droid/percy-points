using POSPRA.Domain.Entities;
using POSPRA.DTOs;
using POSPRA.DTOs.ClientDtos;
using POSPRA.DTOs.PosDtos;

namespace POSPRA.Application.Services.PosService
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
