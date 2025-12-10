using Pos.Application.DTOs;
using Pos.Application.DTOs.LogDtos;
using Pos.Application.DTOs.LogDTOs;
using Pos.Application.DTOs.PageResponseDTOs;
using Pos.Domain.Entities;
using Pos.DTOs.LogDTOs;

namespace Pos.Application.Services.LogService
{
    public interface ILogService
    {
        CreateLogDto BuildLog(string message, string type, string? module = null, string? action = null, string? userId = null, string? userName = null, string? clientIp = null, string? userAgent = null);
        Task<ApiResponse<CreateLogDto>> CreateLogAsync(CreateLogDto dto);
        Task<ApiResponse<PageResponseDto<LogDto>>> GetAllAsync(GetAllLogsDto dto);
        Task<ApiResponse<List<SyncLogDto>>> GetAllUnsyncLogs();
        Task<ApiResponse<bool>> UpdateLog(List<Logs> dtos);
    }
}