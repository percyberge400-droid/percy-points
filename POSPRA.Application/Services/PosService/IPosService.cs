using POSPRA.Application.Utility;

namespace POSPRA.Application.Services.PosService
{
    public interface IPosService
    {
        Task<ApiResponse<string>> UpdateHeartBeatAsync();
    }
}
