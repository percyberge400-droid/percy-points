using System.Data;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using POSPRA.Application.Utility;
using POSPRA.Domain.Entities;
using POSPRA.Domain.ValueObjects;
using POSPRA.DTOs;
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
        private readonly ILogRepository _logRepository;
        private readonly ISqliteUnitOfWork _sqliteUnitOfWork;
        private readonly SqlServerRepository<object> _sqlServerRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AutoMapper.IMapper _mapper;


        /// <summary>
        /// Initializes a new instance of <see cref="LogService"/>.
        /// </summary>
        /// <param name="logRepository">The repository to persist Logs entities.</param>
        /// <param name="sqliteUnitOfWork">Unit of Work for SQLite context.</param>
        public LogService(ILogRepository logRepository,
            ISqliteUnitOfWork sqliteUnitOfWork,
            SqlServerRepository<object> sqlServerRepository,
            IHttpContextAccessor httpContextAccessor,
            AutoMapper.IMapper mapper)
        {
            _logRepository = logRepository;
            _sqliteUnitOfWork = sqliteUnitOfWork;
            _sqlServerRepository = sqlServerRepository;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
        }


        public async Task<ApiResponse<List<LogDto>>> GetAllAsync()
        {
            var output = await _logRepository.GetAllAsync();
            var logDTO = _mapper.Map<List<LogDto>>(output);

            return new ApiResponse<List<LogDto>>(null, null, logDTO, null);
        }

        /// <summary>
        /// Logs to local SQLite with retry and fallback-to-file.
        /// Automatically fills CreatedAtUtc/CreatedAtPk in the entity.
        /// </summary>
        public async Task LogAsync(Logs model, int retry = 0)
        {
            var user = _httpContextAccessor?.HttpContext?.User?.Identity?.Name
           ?? "WinFormsUser";
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            // ensure timestamps always set
            model.CreatedAtUtc = DateTime.UtcNow;
            model.CreatedAtPk = TimeZoneInfo.ConvertTimeFromUtc(
                                        DateTime.UtcNow,
                                        TimeZoneInfo.FindSystemTimeZoneById("Asia/Karachi"));

            try
            {
                retry++;
                await _logRepository.AddAsync(model);
                await _sqliteUnitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                try
                {
                    // create an internal log for the failure itself
                    var errorLog = new Logs
                    {
                        Message = $"{DateTime.UtcNow}: Retry {retry}, DbInsertIssue: {ex.InnerException?.Message ?? ex.Message}",
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

                    await _logRepository.AddAsync(errorLog);
                    await _sqliteUnitOfWork.SaveChangesAsync();

                    // Retry logic: up to 3 attempts
                    if (retry <= 3)
                    {
                        if (retry is 2 or 3)
                            CreateDatabaseBackup();

                        await LogAsync(model, retry);
                    }
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


        // ✅ 2. Central SQL Server error log
        public async Task SaveErrorLogAsync(ErrorLogDTO dto)
        {
            try
            {
                var parameters = new[]
                {
                    new Microsoft.Data.SqlClient.SqlParameter("@POSID",          SqlDbType.BigInt) { Value = dto.POSID },
                    new Microsoft.Data.SqlClient.SqlParameter("@ActualData",     SqlDbType.VarChar, 8000) { Value =dto.ActualData},
                    new Microsoft.Data.SqlClient.SqlParameter("@IsValidSignature",SqlDbType.Bit)   { Value =dto.IsValidSignature},
                    new Microsoft.Data.SqlClient.SqlParameter("@Message",        SqlDbType.VarChar, 8000) { Value =dto.Message},
                    new Microsoft.Data.SqlClient.SqlParameter("@TotalFiles",     SqlDbType.Int)    { Value =dto.TotalFiles}
                };

                // We don’t need row results, so use object as T and no mapper.
                await _sqlServerRepository.ExecuteProcedureAsync<object>(
                    "sp_SaveErroLog",
                    map: null,
                    parameters: parameters
                );
            }
            catch (Exception ex)
            {
                throw; // or swallow if you prefer
            }
        }
    }
}
