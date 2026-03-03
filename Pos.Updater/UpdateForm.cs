using Pos.SecurityEncryption;
using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http.Json;
using System.Net.NetworkInformation;
using System.ServiceProcess;

namespace Pos.Updater
{
    public partial class UpdateForm : Form
    {
        private string LocalFolder = "";
        private readonly string WorkerServiceName = "POSWorker";
        private readonly string AppProcessName = "Pos.WinFormsUI";
        private string LogFile => Path.Combine(LocalFolder, "update_log.txt");

        private readonly string[] ExcludedFiles = new[]
        {
            "Pos.WinFormsUI.dll.config",
            "POSPRA.SetupUI.dll.config",
            "appsettings.json",
            "appsettings.worker.json"
        };

        // Use your actual API base URL here
        private const string ApiBaseUrl = "http://10.105.200.161/api/Configuration/";
        private const string ApiGetVersion = "get-update-version";
        private const string ApiGetUpdaterFile = "get-updater-file";

        public UpdateForm()
        {
            InitializeComponent();
        }

        private async void UpdateForm_Load(object sender, EventArgs e)
        {
            try
            {
                progressBar.Style = ProgressBarStyle.Marquee;
                lblStatus.Text = "Detecting install path...";
                await Task.Delay(200);

                DetectInstallPath();

                if (!CheckInternetConnection())
                {
                    ShowErrorAndClose("No internet connection. Update aborted.");
                    return;
                }

                await RunUpdateAsync();
            }
            catch (Exception ex)
            {
                ShowErrorAndClose($"Updater failed: {ex.Message}");
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

                Log($"Detected install path: {LocalFolder}");
            }
            catch (Exception ex)
            {
                LocalFolder = AppDomain.CurrentDomain.BaseDirectory;
                Log($"DetectInstallPath error: {ex.Message}");
            }
        }

        private bool CheckInternetConnection()
        {
            try
            {
                return NetworkInterface.GetIsNetworkAvailable();
            }
            catch
            {
                return false;
            }
        }

        private async Task RunUpdateAsync()
        {
            string localVersionPath = Path.Combine(LocalFolder, "app-version.txt");

            string fullLocalVersion = File.Exists(localVersionPath)
                ? AesEncryptionHelper.Decrypt(File.ReadAllText(localVersionPath).Trim())
                : "0.0.0,0,Date";

            var parts = fullLocalVersion.Split(',');

            string localVersion = parts.Length > 0 ? parts[0] : "0.0.0";
            //string isUpdate = parts.Length > 1 ? parts[1] : "0";
            string isUpdate = "0";
            string updateDate = DateTime.Now.ToString();

            using var client = new HttpClient();

            // 1️⃣ Get server version
            Invoke(() => lblStatus.Text = "Checking latest version...");
            ApiResponse<ConfigurationResponseDto>? versionResponse;
            try
            {
                versionResponse = await client.GetFromJsonAsync<ApiResponse<ConfigurationResponseDto>>(ApiBaseUrl + ApiGetVersion);
            }
            catch (Exception ex)
            {
                ShowErrorAndClose($"Failed to fetch version from API: {ex.Message}");
                return;
            }

            if (versionResponse?.Data == null || string.IsNullOrWhiteSpace(versionResponse.Data.AppVersion))
            {
                ShowErrorAndClose("Invalid version response from API.");
                return;
            }

            string serverVersion = versionResponse.Data.AppVersion.Trim();
            Log($"Local version: {localVersion}, Server version: {serverVersion}");

            if (TryCompareVersions(localVersion, serverVersion, out var cmp) && cmp >= 0)
            {
                MessageBox.Show("Already up-to-date.");
                Close();
                return;
            }

            // 2️⃣ Stop service & UI
            Invoke(() => lblStatus.Text = "Stopping worker service...");
            StopService(WorkerServiceName);

            Invoke(() => lblStatus.Text = "Stopping running applications...");
            StopProcess(AppProcessName);

            await Task.Delay(500);

            // 3️⃣ Download updater zip from API
            Invoke(() => lblStatus.Text = "Downloading update package...");
            UpdaterFileResponse? zipResponse;
            try
            {
                zipResponse = await client.GetFromJsonAsync<UpdaterFileResponse>(ApiBaseUrl + ApiGetUpdaterFile);
            }
            catch (Exception ex)
            {
                ShowErrorAndClose($"Failed to download updater package: {ex.Message}");
                return;
            }

            if (zipResponse?.Data == null || string.IsNullOrWhiteSpace(zipResponse.Data.Base64File))
            {
                ShowErrorAndClose("Updater file missing or empty.");
                return;
            }


            string tempZip = Path.Combine(Path.GetTempPath(),
    zipResponse.Data.FileName ?? "updater.zip");
            try
            {
                byte[] zipBytes = Convert.FromBase64String(zipResponse.Data.Base64File);
                await File.WriteAllBytesAsync(tempZip, zipBytes);
                Log($"Saved updater zip to {tempZip}");
            }
            catch (Exception ex)
            {
                ShowErrorAndClose($"Failed to save updater zip: {ex.Message}");
                return;
            }

            // 4️⃣ Extract & apply
            Invoke(() => lblStatus.Text = "Applying updates...");
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.Value = 0;

            bool ok = await ExtractAndCopySafeAsync(tempZip, LocalFolder, ExcludedFiles);
            if (!ok)
            {
                ShowErrorAndClose("Failed to apply update package.");
                return;
            }

            progressBar.Value = 70;

            // 5️⃣ Update local version file
            try
            {
                string updatedFullVersion = $"{serverVersion},{isUpdate},{updateDate}";

                Log($"Full updated Version: {updatedFullVersion}");

                File.WriteAllText(localVersionPath, AesEncryptionHelper.Encrypt(updatedFullVersion));
                Log($"Version updated: {localVersion} → {serverVersion}");
            }
            catch (Exception ex)
            {
                Log($"Failed to update local version file: {ex.Message}");
            }

            // 6️⃣ Restart service & relaunch UI
            Invoke(() => lblStatus.Text = "Restarting worker service...");
            StartService(WorkerServiceName);

            string uiExe = Path.Combine(LocalFolder, "Pos.WinFormsUI.exe");
            if (File.Exists(uiExe))
            {
                try { Process.Start(uiExe); }
                catch (Exception ex) { Log($"Failed to launch UI: {ex.Message}"); }
            }

            Invoke(() =>
            {
                lblStatus.Text = "Update completed!";
                progressBar.Value = 100;
            });

            MessageBox.Show(new Form { TopMost = true },
                $"Update completed successfully!\nPrevious version: {localVersion}\nNew version: {serverVersion}",
                "Update Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

            Log("✅ Update completed successfully.");
            Close();
        }

        private async Task<bool> ExtractAndCopySafeAsync(string zipPath, string targetFolder, string[] excludedFiles)
        {
            string tempExtract = Path.Combine(Path.GetTempPath(), "pos_updater_tmp_" + Guid.NewGuid());
            string backupFolder = Path.Combine(Path.GetTempPath(), "pos_updater_bak_" + Guid.NewGuid());
            Directory.CreateDirectory(tempExtract);
            Directory.CreateDirectory(backupFolder);

            try
            {
                ZipFile.ExtractToDirectory(zipPath, tempExtract);

                var allFiles = Directory.GetFiles(tempExtract, "*", SearchOption.AllDirectories);
                int processed = 0, total = allFiles.Length;

                foreach (var src in allFiles)
                {
                    string relative = Path.GetRelativePath(tempExtract, src);
                    string fileName = Path.GetFileName(relative);

                    if (excludedFiles.Any(x => x.Equals(fileName, StringComparison.OrdinalIgnoreCase)))
                    {
                        Log($"Skipping excluded file: {relative}");
                        processed++;
                        Invoke(() => progressBar.Value = Math.Min(100, (int)((processed * 100.0) / total)));
                        continue;
                    }

                    string dest = Path.Combine(targetFolder, relative);
                    string destDir = Path.GetDirectoryName(dest)!;
                    Directory.CreateDirectory(destDir);

                    if (File.Exists(dest))
                    {
                        string bakPath = Path.Combine(backupFolder, relative);
                        Directory.CreateDirectory(Path.GetDirectoryName(bakPath)!);
                        File.Copy(dest, bakPath, overwrite: true);
                    }

                    File.Copy(src, dest, overwrite: true);
                    Log($"Replaced: {relative}");

                    processed++;
                    Invoke(() => progressBar.Value = Math.Min(100, (int)((processed * 100.0) / total)));
                    await Task.Yield();
                }

                // cleanup backups
                try { Directory.Delete(backupFolder, true); } catch { }
                try { Directory.Delete(tempExtract, true); } catch { }

                return true;
            }
            catch (Exception ex)
            {
                Log($"ExtractOrCopy failed: {ex.Message}. Attempting rollback.");
                try
                {
                    if (Directory.Exists(backupFolder))
                    {
                        var bakFiles = Directory.GetFiles(backupFolder, "*", SearchOption.AllDirectories);
                        foreach (var bak in bakFiles)
                        {
                            string rel = Path.GetRelativePath(backupFolder, bak);
                            string dest = Path.Combine(targetFolder, rel);
                            Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
                            File.Copy(bak, dest, overwrite: true);
                        }
                    }
                }
                catch (Exception rbEx)
                {
                    Log($"Rollback failed: {rbEx.Message}");
                }

                return false;
            }
            finally
            {
                try { if (Directory.Exists(tempExtract)) Directory.Delete(tempExtract, true); } catch { }
                try { if (Directory.Exists(backupFolder)) Directory.Delete(backupFolder, true); } catch { }
            }
        }

        #region Service & Process Helpers

        private void StopService(string serviceName)
        {
            try
            {
                using var sc = new ServiceController(serviceName);
                if (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.Paused)
                {
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(60));
                }
            }
            catch (Exception ex)
            {
                Log($"Service stop error: {ex.Message}");
            }
        }

        private void StartService(string serviceName)
        {
            try
            {
                using var sc = new ServiceController(serviceName);
                if (sc.Status != ServiceControllerStatus.Running)
                {
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(60));
                }
            }
            catch (Exception ex)
            {
                Log($"Service start error: {ex.Message}");
            }
        }

        private void StopProcess(string name)
        {
            foreach (var p in Process.GetProcessesByName(name))
            {
                try
                {
                    Log($"Killing {name} (PID {p.Id})...");
                    p.Kill();
                    p.WaitForExit(3000);
                }
                catch (Exception ex)
                {
                    ShowErrorAndClose($"Failed to stop process {name}: {ex.Message}");
                    return;
                }
            }
        }

        #endregion

        #region Utilities

        private bool TryCompareVersions(string vLocal, string vServer, out int cmp)
        {
            cmp = 0;
            try
            {
                var lv = Version.Parse(vLocal);
                var sv = Version.Parse(vServer);
                cmp = lv.CompareTo(sv);
                return true;
            }
            catch
            {
                cmp = string.Compare(vLocal, vServer, StringComparison.OrdinalIgnoreCase);
                return false;
            }
        }

        private void ShowErrorAndClose(string msg)
        {
            Log(msg);
            MessageBox.Show(new Form { TopMost = true }, msg, "Updater Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Close();
        }

        private void Log(string msg)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(LocalFolder))
                {
                    File.AppendAllText(Path.Combine(Path.GetTempPath(), "updater_log.txt"), $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {msg}\r\n");
                    return;
                }
                File.AppendAllText(LogFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {msg}\r\n");
            }
            catch { }
        }

        #endregion
    }

    #region DTOs

    public class UpdaterFileResponse
    {
        public string StatusCode { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public UpdaterFileData Data { get; set; } = new();
        public string Errors { get; set; } = string.Empty;
    }

    public class UpdaterFileData
    {
        public string FileName { get; set; } = string.Empty;
        public string Base64File { get; set; } = string.Empty;
    }

    public class ApiResponse<T>
    {
        public string StatusCode { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public T Data { get; set; } = default!;
        public string Errors { get; set; } = string.Empty;
    }

    public class ConfigurationResponseDto
    {
        public string AppVersion { get; set; } = string.Empty;
    }


    #endregion
}
