using Pos.Application.DTOs;
using Pos.Application.DTOs.ConfigurationsDtos;
using Pos.SecurityEncryption;
using System.Diagnostics;
using System.Net.Http.Json;

namespace Pos.Worker.Services
{
    public class UpdaterWorkerSevice : BackgroundService
    {
        private string LocalFolder = "";

        // Use your actual API base URL here
        private const string ApiBaseUrl = "http://10.105.200.161/api/Configuration/";
        private const string ApiGetVersion = "get-update-version";
        public UpdaterWorkerSevice()
        {
            DetectInstallPath();
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            string logFile = Path.Combine(LocalFolder, "workerUpdaterLog.txt");

            void Log(string message)
            {
                try
                {
                    File.AppendAllText(logFile,
                        $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\r\n");
                }
                catch { /* silently ignore logging failures */ }
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    string localVersionPath = Path.Combine(LocalFolder, "app-version.txt");

                    string fullLocalVersion = File.Exists(localVersionPath)
                        ? AesEncryptionHelper.Decrypt(File.ReadAllText(localVersionPath).Trim())
                        : "0.0.0,0,Date";

                    Log($"Loaded local version: {fullLocalVersion}");

                    var parts = fullLocalVersion.Split(',');

                    string localVersion = parts.Length > 0 ? parts[0] : "0.0.0";
                    string isUpdate = parts.Length > 1 ? parts[1] : "0";
                    string updateDate = parts.Length > 2 ? parts[2] : DateTime.Now.ToString();

                    using var client = new HttpClient();
                    ApiResponse<ConfigurationResponseDto>? versionResponse;
                    try
                    {
                        versionResponse = await client.GetFromJsonAsync<ApiResponse<ConfigurationResponseDto>>(ApiBaseUrl + ApiGetVersion);
                        Log("Fetched server version from API.");
                    }
                    catch (Exception ex)
                    {
                        Log($"API request failed: {ex.Message}");
                        await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                        continue;
                    }

                    if (versionResponse?.Data == null || string.IsNullOrWhiteSpace(versionResponse.Data.AppVersion))
                    {
                        Log("Server returned invalid version data.");
                        await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                        continue;
                    }

                    string serverVersion = versionResponse.Data.AppVersion.Trim();
                    Log($"Server version: {serverVersion}");

                    // ---------------- Version Compare ----------------
                    var vLocal = Version.Parse(localVersion);
                    var vServer = Version.Parse(serverVersion);
                    try
                    {
                        if (vLocal >= vServer)
                        {
                            Log("No update required.");
                            await Task.Delay(TimeSpan.FromSeconds(20), cancellationToken);
                            continue;
                        }
                    }
                    catch
                    {
                        if (localVersion == serverVersion)
                        {
                            Log("No update required (string compare).");
                            await Task.Delay(TimeSpan.FromSeconds(20), cancellationToken);
                            continue;
                        }
                    }

                    Log($"Update available: {localVersion} → {serverVersion}");

                    if ((vLocal < vServer) && isUpdate != "1")
                    {
                        string updatedFullVersion = $"{localVersion},1,{DateTime.Now}";
                        File.WriteAllText(localVersionPath, AesEncryptionHelper.Encrypt(updatedFullVersion));
                        Log("Marked update as pending in app-version.txt.");
                    }
                    else
                    {
                        if (DateTime.TryParse(updateDate, out DateTime lastUpdate))
                        {
                            if ((DateTime.Now - lastUpdate) > TimeSpan.FromSeconds(10))
                            {
                                string installPath = GetInstallPath();
                                if (!installPath.EndsWith("\\")) installPath += "\\";

                                string updaterExe = Path.Combine(installPath, "Pos.Updater.exe");
                                if (!File.Exists(updaterExe))
                                {
                                    Log("Updater executable not found.");
                                    await Task.Delay(TimeSpan.FromSeconds(20), cancellationToken);
                                    continue;
                                }

                                try
                                {
                                    //var psi = new ProcessStartInfo
                                    //{
                                    //    FileName = updaterExe,
                                    //    UseShellExecute = true,
                                    //    Verb = "runas",
                                    //    WindowStyle = ProcessWindowStyle.Normal
                                    //};

                                    var psi = new ProcessStartInfo
                                    {
                                        FileName = updaterExe,
                                        UseShellExecute = true,
                                        WindowStyle = ProcessWindowStyle.Normal
                                    };

                                    Process.Start(psi);
                                    Log("Updater launched successfully.");
                                }
                                catch (Exception ex)
                                {
                                    Log($"Failed to launch updater: {ex.Message}");
                                }

                                // Update app-version.txt with server version
                                string updatedFullVersion = $"{serverVersion},0,{DateTime.Now}";
                                File.WriteAllText(localVersionPath, AesEncryptionHelper.Encrypt(updatedFullVersion));
                                Log("Updated local version after launching updater.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log($"Unexpected error in worker update loop: {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromSeconds(20), cancellationToken);
            }
        }
        public void LaunchUpdaterUI()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = @"Pos.Updater.exe", // path to your WinForms updater
                    UseShellExecute = true,                 // important for user session
                    WorkingDirectory = GetInstallPath(),
                    WindowStyle = ProcessWindowStyle.Normal
                };

                Process.Start(psi);
            }
            catch (Exception ex)
            {
                // Log the error to file or EventLog
                File.AppendAllText(@"C:\UpdaterLogs.txt", $"Failed to launch UI: {ex.Message}\n");
            }
        }

        private void DetectInstallPath()
        {
            try
            {
                string commonInfo = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "PRAL", "install_info.txt");

                if (File.Exists(commonInfo))
                {
                    foreach (var line in File.ReadAllLines(commonInfo))
                    {
                        if (line.StartsWith("InstallPath=", StringComparison.OrdinalIgnoreCase))
                        {
                            LocalFolder = line.Substring("InstallPath=".Length).Trim();
                            break;
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(LocalFolder) || !Directory.Exists(LocalFolder))
                {
                    var defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                        "PRAL", "POSComponent");
                    LocalFolder = Directory.Exists(defaultPath) ? defaultPath : AppDomain.CurrentDomain.BaseDirectory;
                }

                if (!LocalFolder.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    LocalFolder += Path.DirectorySeparatorChar;

            }
            catch (Exception ex)
            {
                LocalFolder = AppDomain.CurrentDomain.BaseDirectory;
            }
        }

        private string GetInstallPath()
        {
            string commonInfo = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "PRAL", "install_info.txt");

            string installPath = "";

            if (File.Exists(commonInfo))
            {
                foreach (var line in File.ReadAllLines(commonInfo))
                {
                    if (line.StartsWith("InstallPath=", StringComparison.OrdinalIgnoreCase))
                    {
                        installPath = line.Substring("InstallPath=".Length).Trim();
                        break;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(installPath) || !Directory.Exists(installPath))
            {
                var defaultPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    "PRAL", "POSComponent");

                installPath = Directory.Exists(defaultPath)
                    ? defaultPath
                    : AppDomain.CurrentDomain.BaseDirectory;
            }

            return installPath;
        }

        private void CloseWinFormsApplication()
        {
            try
            {
                var processes = Process.GetProcessesByName("Pos.WinFormsUI");

                foreach (var process in processes)
                {
                    process.Kill();
                    process.WaitForExit();
                }
            }
            catch
            {
                // optional logging
            }
        }
    }
}
