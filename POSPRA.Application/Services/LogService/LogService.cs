using POSPRA.Domain.Entities;
using POSPRA.Domain.ValueObjects;
using POSPRA.Repositories.LogRepository;
using POSPRA.Repositories.UnitOfWork;
using static POSPRA.Application.Utility.GlobalEnums;

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

        /// <summary>
        /// Initializes a new instance of <see cref="LogService"/>.
        /// </summary>
        /// <param name="logRepository">The repository to persist Logs entities.</param>
        /// <param name="sqliteUnitOfWork">Unit of Work for SQLite context.</param>
        public LogService(ILogRepository logRepository, ISqliteUnitOfWork sqliteUnitOfWork)
        {
            _logRepository = logRepository;
            _sqliteUnitOfWork = sqliteUnitOfWork;
        }

        /// <inheritdoc/>
        public async Task LogAsync(Logs model, int retry = 0)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

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
                    // Log the exception internally
                    var errorLog = new Logs(
                        $"{DateTime.Now}: Try {retry}, DbInsertIssue: {(ex.InnerException?.Message ?? ex.Message)}",
                        (int)AlertType.Exception,
                        false);

                    await _logRepository.AddAsync(errorLog);
                    await _sqliteUnitOfWork.SaveChangesAsync();

                    // Retry logic: max 3 attempts
                    if (retry <= 3)
                    {
                        if (retry is 2 or 3)
                            CreateDatabaseBackup();

                        await LogAsync(model, retry);
                    }
                }
                catch
                {
                    // Final fallback: write to a file if database logging fails
                    File.AppendAllText(
                        "log_fallback.txt",
                        $"{DateTime.Now}: Failed to log -> {ex.Message}{Environment.NewLine}");
                }
            }
        }

        /// <summary>
        /// Creates a backup of the SQLite database.
        /// Keeps a maximum of 10 backup files.
        /// </summary>
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
