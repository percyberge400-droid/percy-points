using POSPRA.Application.Utility;
using POSPRA.Domain.Entities;
using POSPRA.DTOs.PosDTOs;

namespace POSPRA.Application.Services.PosService
{
    public interface IPosService
    {
        Task<ApiResponse<string>> UpdateHeartBeatAsync();
        Task<ApiResponse<List<ResponseConfigurationDto>>> GetConfigurationsAsync();
        Task<string> InsertPosStatusAsync(IList<Logs> logs);
    }
}
