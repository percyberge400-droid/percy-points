using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Client;
using POSPRA.Application.Utility;
using POSPRA.Domain.Entities;
using POSPRA.Domain.ValueObjects;
using POSPRA.DTOs;
using POSPRA.DTOs.LogDtos;
using POSPRA.DTOs.LogDTOs;
using POSPRA.Repositories.BaseRepository;
using POSPRA.Repositories.LogRepository;
using POSPRA.Repositories.UnitOfWork;

namespace POSPRA.Application.Services.LogService
{
    /// <summary>
    /// Service implementation for logging operations.
    /// Provides robust async logging with retry logic and backup creation.
    /// </summary>
    public class LogService : ILogService
    {
        private readonly ILogSQLiteRepository _logSQLiteRepository;
        private readonly ISqliteUnitOfWork _sqliteUnitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AutoMapper.IMapper _mapper;
        private readonly ILogSQLServerRepository _logSQLServerRepository;
        private readonly ISqlServerUnitOfWork _sqlServerUnitOfWork;

        /// <summary>
        /// Initializes a new instance of <see cref="LogService"/>.
        /// </summary>
        /// <param name="logRepository">The repository to persist Logs entities.</param>
        /// <param name="sqliteUnitOfWork">Unit of Work for SQLite context.</param>
        public LogService(ILogSQLiteRepository logSQLiteRepository,
            ISqliteUnitOfWork sqliteUnitOfWork,
            IHttpContextAccessor httpContextAccessor,
            AutoMapper.IMapper mapper,
            ILogSQLServerRepository logSQLServerRepository,
            ISqlServerUnitOfWork sqlServerUnitOfWork)
        {
            _logSQLiteRepository = logSQLiteRepository;
            _sqliteUnitOfWork = sqliteUnitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
            _logSQLServerRepository = logSQLServerRepository;
            _sqlServerUnitOfWork = sqlServerUnitOfWork;
        }

        public async Task<ApiResponse<List<LogDto>>> GetAllCloudAsync()
        {
            var allRecords = await _logSQLServerRepository.GetAllAsync();
            var logDtos = _mapper.Map<List<LogDto>>(allRecords);

            if (logDtos.Any())
                return new ApiResponse<List<LogDto>>(ApiStatusCode.Success, ResponseMessages.RecordFound, logDtos, string.Empty);

            return new ApiResponse<List<LogDto>>(ApiStatusCode.NotFound, ResponseMessages.DataNotFound, null!, string.Empty);
        }

        public async Task<ApiResponse<List<SyncLogDto>>> GetAllUnsyncLogs()
        {
            // Await the repository call directly (don't use .Result)
            var allRecords = await _logSQLiteRepository.GetAllAsync();

            // Filter in memory for unsynced records
            var unsynced = allRecords
                .Where(x => !x.IsSynced)
                .Take(1000)
                .ToList();

            // Map to DTOs
            var logDtos = _mapper.Map<List<SyncLogDto>>(unsynced);
            if (logDtos.Any())
                return new ApiResponse<List<SyncLogDto>>(ApiStatusCode.Success, ResponseMessages.RecordFound, logDtos, string.Empty);

            return new ApiResponse<List<SyncLogDto>>(ApiStatusCode.NotFound, ResponseMessages.DataNotFound, null!, string.Empty);
        }

        public async Task<ApiResponse<List<LogDto>>> GetAllCloudAsync()
        {
            var allRecords = await _logSQLServerRepository.GetAllAsync();
            var logDtos = _mapper.Map<List<LogDto>>(allRecords);

            if (logDtos.Any())
                return new ApiResponse<List<LogDto>>(ApiStatusCode.Success, ResponseMessages.RecordFound, logDtos, string.Empty);

            return new ApiResponse<List<LogDto>>(ApiStatusCode.NotFound, ResponseMessages.DataNotFound, null, string.Empty);
        }

        public async Task<ApiResponse<List<LogDto>>> GetAllAsync()
        {
            var allRecords = await _logSQLiteRepository.GetAllAsync();
            var logDtos = _mapper.Map<List<LogDto>>(allRecords);

            if (logDtos.Any())
                return new ApiResponse<List<LogDto>>(ApiStatusCode.Success, ResponseMessages.RecordFound, logDtos, string.Empty);

            return new ApiResponse<List<LogDto>>(ApiStatusCode.NotFound, ResponseMessages.DataNotFound, null, string.Empty);
        }

        public async Task<ApiResponse<List<LogDto>>> CreateCloudLog(List<LogDto> dto)
        {
            if (dto is null || !dto.Any())
                return new ApiResponse<List<LogDto>>(ApiStatusCode.Error, ResponseMessages.InvalidInput, dto, string.Empty);

            var logs = _mapper.Map<List<Logs>>(dto);
            await _logSQLServerRepository.AddRangeAsync(logs);
            await _sqlServerUnitOfWork.SaveChangesAsync();

            return new ApiResponse<List<LogDto>>(ApiStatusCode.Success, ResponseMessages.RecordSaved, null, string.Empty);
        }

        public async Task<ApiResponse<List<LogDto>>> UpdateLogAsync(List<LogDto> logDtos)
        {
            if (logDtos == null || !logDtos.Any())
            {
                return new ApiResponse<List<LogDto>>(
                    ApiStatusCode.Error,
                    ResponseMessages.DataNotFound,
                    null!,
                    string.Empty);
            }

            // Map to entities
            var entities = _mapper.Map<List<Logs>>(logDtos);

            // Mark all as synced
            entities.ForEach(log => log.IsSynced = true);

            // Update in batch
            _logSQLServerRepository.UpdateRange(entities);
            await _sqlServerUnitOfWork.SaveChangesAsync();

            return new ApiResponse<List<LogDto>>(null!, null!, logDTO, null!);
        }

        public async Task<ApiResponse<bool>> UpdateLog(List<Logs> dtos)
        {
            if (dtos == null || !dtos.Any())
                return new ApiResponse<bool>(
                    ApiStatusCode.Error,
                    ResponseMessages.DataNotFound,
                    false,
                    string.Empty);

            _logSQLiteRepository.UpdateRange(dtos);
           await _sqliteUnitOfWork.SaveChangesAsync();

            return new ApiResponse<bool>(
            ApiStatusCode.Success,
            ResponseMessages.RecordUpdated,
            true,
            string.Empty);
        }

        public async Task<ApiResponse<List<LogDto>>> UpdateLogAsync(List<LogDto> logDtos)
        {
            if (logDtos == null || !logDtos.Any())
            {
                return new ApiResponse<List<LogDto>>(
                    ApiStatusCode.Error,
                    ResponseMessages.DataNotFound,
                    null!,
                    string.Empty);
            }

            // Map to entities
            var entities = _mapper.Map<List<Logs>>(logDtos);

            // Mark all as synced
            entities.ForEach(log => log.IsSynced = true);

            // Update in batch
            _logSQLServerRepository.UpdateRange(entities);
            await _sqlServerUnitOfWork.SaveChangesAsync();

            // Map back to DTOs
            var updatedDtos = _mapper.Map<List<LogDto>>(entities);

            return new ApiResponse<List<LogDto>>(
                ApiStatusCode.Success,
                ResponseMessages.RecordUpdated,
                updatedDtos,
                string.Empty);
        }

        /// <summary>
        /// Logs to local SQLite with retry and fallback-to-file.
        /// Automatically fills CreatedAtUtc/CreatedAtPk in the entity.
        /// </summary>
        public async Task CreateLogAsync(Logs model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            // ensure timestamps always set
            model.CreatedAtPk = DateTime.Now;

            try
            {
                await _logSQLiteRepository.AddAsync(model);
                await _sqliteUnitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                try
                {
                    // create an internal log for the failure itself
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
                    // Final fallback: write to a local text file
                    File.AppendAllText(
                        "log_fallback.txt",
                        $"{DateTime.UtcNow:o}: Failed to log -> {ex.Message}{Environment.NewLine}");
                }
            }
        }

        public async Task<ApiResponse<List<Logs>>> CreateCloudLog(List<SyncLogDto> dto)
        {
            if (dto is null || !dto.Any())
                return new ApiResponse<List<Logs>>(ApiStatusCode.Error, ResponseMessages.InvalidInput, null!, string.Empty);

            var logs = _mapper.Map<List<Logs>>(dto);
            
            await _logSQLServerRepository.AddRangeAsync(logs);
            await _sqlServerUnitOfWork.SaveChangesAsync();

            // Simulate response evaluation (you can replace this with your actual logic)
            List<Logs> syncedRecords = new List<Logs>();
            bool anySaved = true;

            foreach (var item in logs)
            {
                item.IsSynced = true;
                syncedRecords.Add(item);
            }

            if (anySaved)
            {
                return new ApiResponse<List<Logs>>(ApiStatusCode.Success, ResponseMessages.RecordSaved, syncedRecords, string.Empty);
            }
            else
            {
                return new ApiResponse<List<Logs>>(ApiStatusCode.Error, "No records were synced.", null!, string.Empty);
            }
        }

        /// <summary>
        /// Build a fully populated Logs entity from the current HTTP context
        /// and any extra data you supply.
        /// </summary>
        public Logs BuildLog(
         string message,
         string type,
         string? module = null,
         string? action = null,
         string? userId = null,
         string? userName = null,
         string? clientIp = null,
         string? userAgent = null)
        {
            var ctx = _httpContextAccessor.HttpContext;   // will be null in WinForms

            return new Logs
            {
                Message = message,
                Type = type,
                IsSynced = false,
                Module = module,
                ActionName = action,
                UserId = userId ?? ctx?.User?.FindFirst("sub")?.Value,
                UserName = userName ?? ctx?.User?.Identity?.Name,
                HttpMethod = ctx?.Request?.Method,
                RequestPath = ctx?.Request?.Path,
                QueryString = ctx?.Request?.QueryString.ToString(),
                RequestHeaders = ctx == null ? null :
                                  System.Text.Json.JsonSerializer.Serialize(
                                       ctx.Request.Headers.ToDictionary(k => k.Key, v => v.Value.ToString())),
                ClientIp = clientIp ?? ctx?.Connection?.RemoteIpAddress?.ToString(),
                UserAgent = userAgent ?? ctx?.Request?.Headers["User-Agent"].ToString(),
                MachineName = Environment.MachineName,
                ApplicationName = "POSPRA",
                EnvironmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT"),
                AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString()
            };
        }

        /// <summary>
        /// Creates a backup of the SQLite database.
        /// Keeps a maximum of 10 backup files.
        /// </summary>
        /// 
        private void CreateDatabaseBackup()
        {
            try
            {
                string backupDir = Path.Combine(GlobalVariables.FOLDER_PATH, "Backup");

                if (!Directory.Exists(backupDir))
                    Directory.CreateDirectory(backupDir);

                var existingBackups = Directory.GetFiles(backupDir, "*.bak", SearchOption.TopDirectoryOnly);

                if (existingBackups.Length >= 10)
                {
                    var oldestBackup = existingBackups
                        .Select(f => new FileInfo(f))
                        .OrderBy(f => f.CreationTimeUtc)
                        .First();

                    File.Delete(oldestBackup.FullName);
                }

                string backupFile = Path.Combine(backupDir, $"{Guid.NewGuid()}.bak");

                if (File.Exists(GlobalVariables.FILE_NAME))
                {
                    File.Copy(GlobalVariables.FILE_NAME, backupFile, overwrite: true);
                    File.Delete(GlobalVariables.FILE_NAME); // optional: to start fresh
                }
            }
            catch (Exception ex)
            {
                // Last-resort fallback: log backup errors to text file
                try
                {
                    string errorLog = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "backup_error.txt");
                    File.AppendAllText(errorLog, $"{DateTime.Now}: Backup failed - {ex.Message}{Environment.NewLine}");
                }
                catch
                {
                    // swallow exception to prevent app crash
                }
            }
        }
    }
}