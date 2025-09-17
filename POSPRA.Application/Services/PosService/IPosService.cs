using POSPRA.Domain.Entities;
using POSPRA.DTOs;
using POSPRA.DTOs.PosDTOs;

namespace POSPRA.Application.Services.PosService
{
    /// <summary>
    /// Service contract for POS-related operations.
    /// </summary>
    public interface IPosService
    {
        /// <summary>
        /// Updates the heartbeat of the current POS by executing the
        /// <c>sp_UpdatePOSHeartbeat</c> stored procedure.
        /// </summary>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> containing the status or result of the operation.
        /// </returns>
        Task<ApiResponse<string>> UpdateHeartBeatAsync();
        Task<ApiResponse<List<ResponseConfigurationDto>>> GetConfigurationsAsync();
        Task<ApiResponse<List<PosStatus>>> InsertPosStatusAsync();
    }
}
