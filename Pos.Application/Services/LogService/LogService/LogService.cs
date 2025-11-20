using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Pos.Application.DTOs;
using Pos.Application.DTOs.LogDtos;
using Pos.Application.DTOs.LogDTOs;
using Pos.Application.DTOs.PageResponseDTOs;
using Pos.Application.Interfaces;
using Pos.Application.Interfaces.Repositories;
using Pos.Application.Utility;
using Pos.Domain.Entities;
using POSPRA.DTOs.LogDTOs;
using System.Reflection;

namespace Pos.Application.Services.LogService
{
    public class LogService : ILogService
    {
        private readonly IRepository<Logs> _sqlLiteLogRepository;
        private readonly ISqliteUnitOfWork _sqliteUnitOfWork;
        private readonly IMapper _mapper;
        private readonly AppSettings _settings;

        public LogService(

            ISqliteRepositoryFactory sqliteRepositoryFactory,

            ISqliteUnitOfWork sqliteUnitOfWork,
            IMapper mapper,
            IOptions<AppSettings> options)
        {
            _sqlLiteLogRepository = sqliteRepositoryFactory.CreateRepository<Logs>();
            _sqliteUnitOfWork = sqliteUnitOfWork;
            _mapper = mapper;
            _settings = options.Value;
        }

        public async Task<ApiResponse<List<SyncLogDto>>> GetAllUnsyncLogs()
        {
            var allRecords = await _sqlLiteLogRepository.GetAllAsync();
            var unsynced = allRecords.Where(x => !x.IsSynced).Take(1000).ToList();

            var logDtos = _mapper.Map<List<SyncLogDto>>(unsynced);
            if (logDtos.Any())
                return new ApiResponse<List<SyncLogDto>>(ApiStatusCode.Success, ResponseMessages.RecordFound, logDtos, string.Empty);

            return new ApiResponse<List<SyncLogDto>>(ApiStatusCode.NotFound, ResponseMessages.DataNotFound, null!, string.Empty);
        }

        public async Task<ApiResponse<PageResponseDto<LogDto>>> GetAllAsync(GetAllLogsDto dto)
        {
            int pageNumber = dto.PageNumber <= 0 ? 1 : dto.PageNumber;
            int numberOfRecords = dto.NumberOfRecords <= 0 ? 10 : dto.NumberOfRecords;

            var query = _sqlLiteLogRepository.Query();

            if (dto.StartDate.HasValue && dto.EndDate.HasValue)
            {
                DateTime start = dto.StartDate.Value.Date;
                DateTime end = dto.EndDate.Value.Date;
                query = query.Where(m => m.CreatedAtPk.Date >= start && m.CreatedAtPk.Date <= end);
            }

            // Total Records before Pagination
            int totalRecords = await query.CountAsync();

            // Check if no records exist
            if (totalRecords == 0)
            {
                return new ApiResponse<PageResponseDto<LogDto>>(
                    ApiStatusCode.NotFound,
                    ResponseMessages.RecordNotFound,
                    new PageResponseDto<LogDto>
                    {
                        Items = new List<LogDto>(),
                        TotalRecords = 0,
                        TotalPages = 0
                    },
                    null!
                );
            }

            // Calculate Total Pages
            int totalPages = (int)Math.Ceiling((double)totalRecords / numberOfRecords);

            // Validate page number
            if (pageNumber > totalPages)
            {
                pageNumber = totalPages;
            }

            // Sort by CreatedAtPk DESC (most recent first)
            query = query.OrderByDescending(m => m.CreatedAtPk);

            // Apply Pagination
            var output = await query
                .Skip((pageNumber - 1) * numberOfRecords)
                .Take(numberOfRecords)
                .ToListAsync();

            // Map Entity → DTO
            var logDto = _mapper.Map<List<LogDto>>(output);

            // Final result in clean format
            var pagedResult = new PageResponseDto<LogDto>
            {
                Items = logDto ?? new List<LogDto>(),
                TotalRecords = totalRecords,
                TotalPages = totalPages
            };

            return new ApiResponse<PageResponseDto<LogDto>>(
                ApiStatusCode.Success,
                ResponseMessages.RecordFound,
                pagedResult,
                string.Empty
            );
        }

        public async Task<ApiResponse<bool>> UpdateLog(List<Logs> dtos)
        {
            if (dtos == null || !dtos.Any())
                return new ApiResponse<bool>(ApiStatusCode.Error, ResponseMessages.DataNotFound, false, string.Empty);

            _sqlLiteLogRepository.UpdateRange(dtos);
            await _sqliteUnitOfWork.SaveChangesAsync();

            return new ApiResponse<bool>(ApiStatusCode.Success, ResponseMessages.RecordUpdated, true, string.Empty);
        }

        public async Task<ApiResponse<CreateLogDto>> CreateLogAsync(CreateLogDto dto)
        {
            if (dto == null)
                return new ApiResponse<CreateLogDto>(ApiStatusCode.NotFound, ResponseMessages.UnknownError, null!, "Log DTO cannot be null.");

            dto.CreatedAtPk = DateTime.Now;
            dto.POSID = _settings.POS;

            try
            {
                var model = _mapper.Map<Logs>(dto);
                await _sqlLiteLogRepository.AddAsync(model);
                await _sqliteUnitOfWork.SaveChangesAsync();

                return new ApiResponse<CreateLogDto>(ApiStatusCode.Success, ResponseMessages.RecordSaved, dto, string.Empty);
            }
            catch (Exception ex)
            {
                try
                {
                    var errorLog = new Logs
                    {
                        Message = $"{DateTime.UtcNow}, DbInsertIssue: {ex.InnerException?.Message ?? ex.Message}",
                        Type = AlertType.Exception,
                        IsSynced = false,
                        Module = "Logging",
                        ActionName = "LogAsync",
                        ExceptionType = ex.GetType().FullName,
                        ExceptionMessage = ex.Message,
                        StackTrace = ex.StackTrace,
                        MachineName = Environment.MachineName,
                        ApplicationName = "POSPRA"
                    };

                    await _sqlLiteLogRepository.AddAsync(errorLog);
                    await _sqliteUnitOfWork.SaveChangesAsync();
                }
                catch
                {
                    File.AppendAllText("log_fallback.txt", $"{DateTime.UtcNow:o}: Failed to log -> {ex.Message}{Environment.NewLine}");
                }

                return new ApiResponse<CreateLogDto>(ApiStatusCode.ServiceUnavailable, ResponseMessages.UnknownError, null!, ex.Message);
            }
        }

        public CreateLogDto BuildLog(
        string message,
        string type,
        string? module = null,
        string? action = null,
        string? userId = null,
        string? userName = null,
        string? clientIp = null,
        string? userAgent = null)
        {
            return new CreateLogDto
            {
                Message = message,
                Type = type,
                IsSynced = false,
                Module = module,
                ActionName = action,
                UserId = userId,
                UserName = userName,
                HttpMethod = null,
                RequestPath = null,
                QueryString = null,
                RequestHeaders = null,
                ClientIp = clientIp,
                UserAgent = userAgent,
                MachineName = Environment.MachineName,
                ApplicationName = "POSPRA",
                EnvironmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT"),
                AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString()
            };
        }

    }
}
