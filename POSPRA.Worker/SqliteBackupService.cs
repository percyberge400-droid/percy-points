namespace POSPRA.Worker
{
    public class SqliteBackupService : BackgroundService
    {
        private readonly ILogger<SqliteBackupService> _logger;
        private readonly string _dbPath;
        private readonly string _backupDirectory;
        private readonly TimeSpan _backupTime; // e.g., midnight

        public SqliteBackupService(ILogger<SqliteBackupService> logger)
        {
            _logger = logger;

            // 🔧 CONFIGURE these paths for your environment
            _dbPath = Path.Combine(AppContext.BaseDirectory, "Data", "app.db");
            _backupDirectory = Path.Combine(AppContext.BaseDirectory, "Backups");

            // Set backup time (e.g., midnight)
            _backupTime = new TimeSpan(0, 0, 0);

            if (!Directory.Exists(_backupDirectory))
                Directory.CreateDirectory(_backupDirectory);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SQLite Backup Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.Now;
                    var nextRun = DateTime.Today.Add(_backupTime);

                    if (now > nextRun)
                        nextRun = nextRun.AddDays(1); // schedule for next day

                    var delay = nextRun - now;

                    _logger.LogInformation("Next backup scheduled at {NextRun}", nextRun);

                    await Task.Delay(delay, stoppingToken);

                    await CreateBackupAsync(stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // graceful shutdown
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during backup execution.");
                }
            }
        }

        private async Task CreateBackupAsync(CancellationToken token)
        {
            try
            {
                if (!File.Exists(_dbPath))
                {
                    _logger.LogWarning("Database file not found at {DbPath}", _dbPath);
                    return;
                }

                var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                var backupFileName = $"backup_{timestamp}.db";
                var backupPath = Path.Combine(_backupDirectory, backupFileName);

                // Copy file safely
                await using (var sourceStream = File.Open(_dbPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                await using (var destinationStream = File.Create(backupPath))
                {
                    await sourceStream.CopyToAsync(destinationStream, token);
                }

                _logger.LogInformation("Backup created successfully: {BackupPath}", backupPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create SQLite backup.");
            }
        }
    }
}
