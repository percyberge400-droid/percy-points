using AutoMapper;
using Microsoft.Extensions.Options;
using POSPRA.Application.Utility;
using POSPRA.Domain.Entities;
using POSPRA.DTOs;
using POSPRA.DTOs.LogDtos;
using POSPRA.DTOs.LogDTOs;
using POSPRA.Repositories.LogRepository;
using POSPRA.Repositories.UnitOfWork;
using System.Reflection;

namespace POSPRA.Application.Services.LogService
{
    public class LogService : ILogService
    {
        private readonly ILogSQLiteRepository _logSQLiteRepository;
        private readonly ISqliteUnitOfWork _sqliteUnitOfWork;
        private readonly IMapper _mapper;
        private readonly AppSettings _settings;

        public LogService(
            ILogSQLiteRepository logSQLiteRepository,
            ISqliteUnitOfWork sqliteUnitOfWork,
            IMapper mapper,
            IOptions<AppSettings> options)
        {
            _logSQLiteRepository = logSQLiteRepository;
            _sqliteUnitOfWork = sqliteUnitOfWork;
            _mapper = mapper;
            _settings = options.Value;
        }

        public async Task<ApiResponse<List<SyncLogDto>>> GetAllUnsyncLogs()
        {
            var allRecords = await _logSQLiteRepository.GetAllAsync();
            var unsynced = allRecords.Where(x => !x.IsSynced).Take(1000).ToList();

            var logDtos = _mapper.Map<List<SyncLogDto>>(unsynced);
            if (logDtos.Any())
                return new ApiResponse<List<SyncLogDto>>(ApiStatusCode.Success, ResponseMessages.RecordFound, logDtos, string.Empty);

            return new ApiResponse<List<SyncLogDto>>(ApiStatusCode.NotFound, ResponseMessages.DataNotFound, null!, string.Empty);
        }

        public async Task<ApiResponse<List<LogDto>>> GetAllAsync()
        {
            var output = await _logSQLiteRepository.GetAllAsync();
            var logDTO = _mapper.Map<List<LogDto>>(output);
            return new ApiResponse<List<LogDto>>(null!, null!, logDTO, null!);
        }

        public async Task<ApiResponse<bool>> UpdateLog(List<Logs> dtos)
        {
            if (dtos == null || !dtos.Any())
                return new ApiResponse<bool>(ApiStatusCode.Error, ResponseMessages.DataNotFound, false, string.Empty);

            _logSQLiteRepository.UpdateRange(dtos);
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
                await _logSQLiteRepository.AddAsync(model);
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

                    await _logSQLiteRepository.AddAsync(errorLog);
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
