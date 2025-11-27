using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using Pos.Application.DTOs;
using System.IO.Compression;

namespace POSPRA.Worker.Services
{
    public class SqliteBackupService : BackgroundService
    {
        private readonly ILogger<SqliteBackupService> _logger;
        private readonly AppSettings _settings;
        private readonly string _dbPath;
        private readonly string _dbPassword;

        private readonly string _backupDirectory;
        private readonly TimeSpan _backupTime;
        private readonly int _maxBackupDays = 30;
        private readonly string _logFilePath;

        public SqliteBackupService(ILogger<SqliteBackupService> logger, IOptions<AppSettings> options)
        {
            _logger = logger;
            _settings = options.Value;

            _dbPath = _settings.DefaultDBFilePath ?? Path.Combine(AppContext.BaseDirectory, "POSPRA.db");
            _dbPassword = _settings.DefaultDBPassword;
            _backupDirectory = _settings.BackupDirectoryPath ?? Path.Combine(AppContext.BaseDirectory, "Backups");
            _backupTime = new TimeSpan(0, 0, 0); // midnight

            _logFilePath = Path.Combine(_backupDirectory, "BackupLog.txt");
            EnsureDirectoriesExist();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            EnsureDirectoriesExist();
            LogToFile("SQLite Backup Service started.");

            await CheckAndRestoreDatabaseAsync();

            bool isTestMode = _settings.IsBackupTestMode;
            LogToFile(isTestMode
                ? "⚙️ TEST MODE ENABLED — running backup every 30 seconds and simulating corruption."
                : "📅 NORMAL MODE — daily backups at midnight.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    EnsureDirectoriesExist();

                    if (isTestMode)
                    {
                        await CreateBackupAsync(stoppingToken);
                        //SimulateCorruption();
                        await CheckAndRestoreDatabaseAsync();
                        CleanupOldBackups();
                        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    }
                    else
                    {
                        var now = DateTime.Now;
                        var nextRun = DateTime.Today.Add(_backupTime);

                        if (now > nextRun)
                            nextRun = nextRun.AddDays(1);

                        var delay = nextRun - now;
                        LogToFile($"Next backup scheduled at {nextRun}");
                        await Task.Delay(delay, stoppingToken);

                        EnsureDirectoriesExist();
                        await CreateBackupAsync(stoppingToken);
                        CleanupOldBackups();
                    }
                }
                catch (TaskCanceledException)
                {
                    LogToFile("SQLite Backup Service stopping...");
                }
                catch (Exception ex)
                {
                    LogToFile($"❌ Error during backup execution: {ex}");
                }
            }
        }

        private void EnsureDirectoriesExist()
        {
            try
            {
                if (!Directory.Exists(_backupDirectory))
                {
                    Directory.CreateDirectory(_backupDirectory);
                    LogToFile($"📁 Backup directory recreated at: {_backupDirectory}");
                }

                var logDir = Path.GetDirectoryName(_logFilePath);
                if (!string.IsNullOrEmpty(logDir) && !Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                    LogToFile($"📁 Log directory recreated at: {logDir}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Failed to ensure directories exist: {ex.Message}");
            }
        }

        private async Task CreateBackupAsync(CancellationToken token)
        {
            try
            {
                EnsureDirectoriesExist();

                if (!File.Exists(_dbPath))
                {
                    LogToFile($"⚠️ Database not found at {_dbPath}. Attempting restore...");
                    await RestoreLatestBackupAsync();
                    return;
                }

                if (!await ValidateDatabaseAsync())
                {
                    LogToFile("⚠️ Database appears corrupted. Restoring...");
                    await RestoreLatestBackupAsync();
                    return;
                }
                var connectionstring = GetDbConnectionStringForConfig();
                // 🔸 Force SQLite to flush all pending writes to disk before backup
                await using (var conn = new SqliteConnection($"Data Source={connectionstring}"))
                {
                    await conn.OpenAsync();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "PRAGMA wal_checkpoint(FULL);"; // ensures write-ahead logs are flushed
                    await cmd.ExecuteNonQueryAsync();
                    await conn.CloseAsync(); // close connection to unlock DB
                }

                // 🔸 Wait briefly to ensure OS releases file handle
                await Task.Delay(500, token);

                var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                var backupName = $"backup_{timestamp}.db";
                var backupPath = Path.Combine(_backupDirectory, backupName);

                // 🔸 Now reopen fresh DB file and copy its latest data
                await using (var src = new FileStream(_dbPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                await using (var dst = new FileStream(backupPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await src.CopyToAsync(dst, token);
                }

                // 🔸 Close and reopen before compression to avoid file-in-use
                await Task.Delay(200, token);

                var zipName = $"backup_{timestamp}.zip";
                var zipPath = Path.Combine(_backupDirectory, zipName);

                using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                {
                    zip.CreateEntryFromFile(backupPath, backupName);
                }

                File.Delete(backupPath);
                LogToFile($"✅ Backup created with fresh data: {zipPath}");
            }
            catch (Exception ex)
            {
                LogToFile($"❌ Backup failed: {ex}");
            }
        }


        private async Task<bool> ValidateDatabaseAsync()
        {
            try
            {
                if (!File.Exists(_dbPath))
                    return false;

                var connectionstring = GetDbConnectionStringForConfig();
                await using var conn = new SqliteConnection($"Data Source={connectionstring}");
                await conn.OpenAsync();

                var cmd = conn.CreateCommand();
                cmd.CommandText = "PRAGMA integrity_check;";
                var result = (string?)await cmd.ExecuteScalarAsync();

                return result != null && result.Contains("ok", StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                LogToFile($"❌ Database validation failed: {ex.Message}");
                return false;
            }
        }

        private async Task CheckAndRestoreDatabaseAsync()
        {
            try
            {
                EnsureDirectoriesExist();

                if (!File.Exists(_dbPath))
                {
                    LogToFile("⚠️ DB missing. Attempting restore...");
                    await RestoreLatestBackupAsync();
                }
                else if (!await ValidateDatabaseAsync())
                {
                    LogToFile("⚠️ DB corrupted. Restoring from latest backup...");
                    await RestoreLatestBackupAsync();
                }
            }
            catch (Exception ex)
            {
                LogToFile($"❌ DB check/restore failed: {ex}");
            }
        }

        private async Task RestoreLatestBackupAsync()
        {
            try
            {
                EnsureDirectoriesExist();

                var latestZip = Directory.GetFiles(_backupDirectory, "backup_*.zip")
                                         .OrderByDescending(f => f)
                                         .FirstOrDefault();

                if (latestZip == null)
                {
                    LogToFile("⚠️ No backups available to restore!");
                    return;
                }

                LogToFile($"Restoring from backup: {latestZip}");

                var tempDir = Path.Combine(_backupDirectory, "temp_restore");
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
                Directory.CreateDirectory(tempDir);

                ZipFile.ExtractToDirectory(latestZip, tempDir);

                var dbFile = Directory.GetFiles(tempDir, "*.db").FirstOrDefault();
                if (dbFile == null)
                {
                    LogToFile("❌ No .db file found inside ZIP!");
                    return;
                }

                File.Copy(dbFile, _dbPath, true);
                Directory.Delete(tempDir, true);
                LogToFile("✅ Database restored successfully.");
            }
            catch (Exception ex)
            {
                LogToFile($"❌ Restore failed: {ex}");
            }
        }

        private void CleanupOldBackups()
        {
            try
            {
                EnsureDirectoriesExist();

                var files = Directory.GetFiles(_backupDirectory, "backup_*.zip")
                                     .Select(f => new FileInfo(f))
                                     .OrderByDescending(f => f.CreationTime)
                                     .ToList();

                if (files.Count <= _maxBackupDays)
                    return;

                foreach (var oldFile in files.Skip(_maxBackupDays))
                {
                    oldFile.Delete();
                    LogToFile($"🧹 Deleted old backup: {oldFile.FullName}");
                }
            }
            catch (Exception ex)
            {
                LogToFile($"❌ Cleanup failed: {ex}");
            }
        }

        private void SimulateCorruption()
        {
            try
            {
                if (!_settings.IsBackupTestMode) return;

                var random = new Random();
                if (random.Next(1, 4) != 2) return;

                if (File.Exists(_dbPath))
                {
                    LogToFile("🧪 Simulating corruption for testing...");
                    using (var fs = new FileStream(_dbPath, FileMode.Open, FileAccess.Write))
                    {
                        fs.Seek(0, SeekOrigin.Begin);
                        byte[] corruptData = { 0x00, 0xFF, 0xAB, 0xCD };
                        fs.Write(corruptData, 0, corruptData.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                LogToFile($"❌ Failed to simulate corruption: {ex}");
            }
        }
        private string GetDbConnectionStringForConfig()
        {
            if (string.IsNullOrWhiteSpace(_dbPath))
                throw new ArgumentException("dbPath cannot be null or empty.", nameof(_dbPath));

            // Replace single backslashes with double for storing in config
            string escapedPath = _dbPath.Replace("\\", "\\\\");

            // Return only path + mode + password, no "Data Source="
            string connectionString = $"{escapedPath};Mode=ReadWriteCreate;Password={_dbPassword}";

            return connectionString;
        }
        private void LogToFile(string message)
        {
            try
            {
                var log = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {message}";
                _logger.LogInformation(log);
                EnsureDirectoriesExist();
                File.AppendAllText(_logFilePath, log + Environment.NewLine);
            }
            catch
            {
                // prevent recursive errors
            }
        }
    }
}
