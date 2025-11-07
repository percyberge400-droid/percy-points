using POSPRA.Domain.Entities;
using POSPRA.DTOs;
using POSPRA.DTOs.LogDtos;
using POSPRA.DTOs.LogDTOs;

namespace POSPRA.Application.Services.LogService
{
    public interface ILogService
    {
        CreateLogDto BuildLog(string message, string type, string? module = null, string? action = null, string? userId = null, string? userName = null, string? clientIp = null, string? userAgent = null);
        Task<ApiResponse<CreateLogDto>> CreateLogAsync(CreateLogDto dto);
        Task<ApiResponse<List<LogDto>>> GetAllAsync();
        Task<ApiResponse<List<SyncLogDto>>> GetAllUnsyncLogs();
        Task<ApiResponse<bool>> UpdateLog(List<Logs> dtos);
    }
}