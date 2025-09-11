using POSPRA.Domain.Entities;
using POSPRA.Domain.ValueObjects;
using POSPRA.Repositories.LogRepository;
using POSPRA.Repositories.UnitOfWork;
using static POSPRA.Application.Utility.GlobalEnums;

namespace POSPRA.Application.Services.LogService
{
    public class LogService : ILogService
    {
        private readonly ILogRepository _logRepository;
        private readonly ISqliteUnitOfWork _sqliteUnitOfWork;

        public LogService(ILogRepository logRepository, ISqliteUnitOfWork sqliteUnitOfWork)
        {
            _logRepository = logRepository;
            _sqliteUnitOfWork = sqliteUnitOfWork;
        }

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
                    var errorLog = new Logs(
                        $"{DateTime.Now}: Try {retry}, DbInsertIssue: {(ex.InnerException?.Message ?? ex.Message)}",
                        (int)AlertType.Exception,
                        false);

                    await _logRepository.AddAsync(errorLog);
                    await _sqliteUnitOfWork.SaveChangesAsync();

                    if (retry <= 3)
                    {
                        if (retry is 2 or 3)
                            CreateDatabaseBackup();

                        await LogAsync(model, retry);
                    }
                }
                catch
                {
                    // Final fallback: write to file or console
                    File.AppendAllText("log_fallback.txt", $"{DateTime.Now}: Failed to log -> {ex.Message}{Environment.NewLine}");
                }
            }
        }

        private void CreateDatabaseBackup()
        {
            try
            {
                string backupDir = Path.Combine(GlobalVariables.FOLDER_PATH, "Backup");

                // Ensure backup directory exists
                if (!Directory.Exists(backupDir))
                    Directory.CreateDirectory(backupDir);

                // Count existing backups
                var existingBackups = Directory.GetFiles(backupDir, "*.bak", SearchOption.TopDirectoryOnly);

                // Keep a maximum of 10 backups
                if (existingBackups.Length >= 10)
                {
                    // delete the oldest backup (by creation time)
                    var oldestBackup = existingBackups
                        .Select(f => new FileInfo(f))
                        .OrderBy(f => f.CreationTimeUtc)
                        .First();

                    File.Delete(oldestBackup.FullName);
                }

                // Create new backup file
                string backupFile = Path.Combine(backupDir, $"{Guid.NewGuid()}.bak");

                if (File.Exists(GlobalVariables.FILE_NAME))
                {
                    File.Copy(GlobalVariables.FILE_NAME, backupFile, overwrite: true);
                    File.Delete(GlobalVariables.FILE_NAME); // optional: only if you need fresh db
                }
            }
            catch (Exception ex)
            {
                // Last-resort fallback: write error to a text file
                try
                {
                    string errorLog = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "backup_error.txt");
                    File.AppendAllText(errorLog, $"{DateTime.Now}: Backup failed - {ex.Message}{Environment.NewLine}");
                }
                catch
                {
                    // swallow exception - we don't want backup errors to crash the app
                }
            }
        }
    }
}