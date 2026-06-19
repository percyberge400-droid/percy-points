using Pos.SecurityEncryption;
using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http.Json;
using System.Net.NetworkInformation;
using System.ServiceProcess;
using System.Text.Json;
using Pos.Application.DTOs;
using Pos.Application.DTOs.ConfigurationsDtos;
using Pos.Application.Utility;

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
            "Pos.SetupUI.dll.config",
            "appsettings.json",
            "Pos.SecurityEncryption.dll",
            "appsettings.worker.json"
        };

        private readonly string[] ExcludedFolders = new[]
        {
            "runtimes"
        };

        // API endpoints
        private string ApiBaseUrl = string.Empty;
        private const string ApiGetVersion = "get-update-version";
        private const string ApiGetUpdaterFile = "get-updater-file";
        private const string ModuleName = "PRAPOS_2.0";

     

        // ✅ FIX: ASP.NET Core serializes responses in camelCase ("statusCode", "data", ...)
        // by default, but System.Text.Json's plain ReadFromJsonAsync<T>() (no options)
        // is case-sensitive. Without this, Data always deserialized as null even on a
        // successful 200 response.
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public UpdateForm()
        {
            InitializeComponent();
        }

        private async void UpdateForm_Load(object sender, EventArgs e)
        {
            // ✅ Read API base URL from argument passed by loginForm2
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length >= 2 && !string.IsNullOrWhiteSpace(args[1]))
            {
                ApiBaseUrl = args[1].TrimEnd('/') + "/Configuration/";
            }
            else
            {
                // fallback — shouldn't happen but prevents crash
                MessageBox.Show("API base URL not provided. Update aborted.",
                    "Config Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
                return;
            }
            // ✅ Admin check
            if (!new System.Security.Principal.WindowsPrincipal(
                    System.Security.Principal.WindowsIdentity.GetCurrent())
                .IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator))
            {
                MessageBox.Show("Updater must be run as Administrator.",
                    "Permission Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Environment.Exit(1);
                return;
            }

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
                string commonInfo = Path.Combine(
    @"C:\ProgramData\PRAL\PRAPOS_Component",
    "install_info.txt");
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
                    var defaultPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
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

            using var client = new HttpClient();
            string moduleBody = $"{{\"moduleName\": \"{ModuleName}\"}}";

            // 1️⃣ Get server version
            Invoke(() => lblStatus.Text = "Checking latest version...");
            ApiResponse<ConfigurationResponseDto>? versionResponse;
            try
            {
                var versionRequest = new HttpRequestMessage(HttpMethod.Post, ApiBaseUrl + ApiGetVersion);
                versionRequest.Headers.Add("accept", "*/*");
                versionRequest.Content = new StringContent(moduleBody, System.Text.Encoding.UTF8, "application/json");

                var versionHttpResponse = await client.SendAsync(versionRequest);
                versionHttpResponse.EnsureSuccessStatusCode();
                versionResponse = await versionHttpResponse.Content
                    .ReadFromJsonAsync<ApiResponse<ConfigurationResponseDto>>(JsonOptions);
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
            ApiResponse<ConfigurationZipFileResponseDto>? zipResponse;
            try
            {
                var fileRequest = new HttpRequestMessage(HttpMethod.Post, ApiBaseUrl + ApiGetUpdaterFile);
                fileRequest.Headers.Add("accept", "*/*");
                fileRequest.Content = new StringContent(moduleBody, System.Text.Encoding.UTF8, "application/json");

                var fileHttpResponse = await client.SendAsync(fileRequest);
                fileHttpResponse.EnsureSuccessStatusCode();
                zipResponse = await fileHttpResponse.Content
                    .ReadFromJsonAsync<ApiResponse<ConfigurationZipFileResponseDto>>(JsonOptions);
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

            try { File.Delete(tempZip); } catch { }

            progressBar.Value = 70;

            // 5️⃣ Update local version file — capture date HERE, not at top
            string updateDate = DateTime.Now.ToString();
            try
            {
                string updatedFullVersion = $"{serverVersion},0,{updateDate}";
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

                // ✅ Step into root folder if zip contains a single subfolder
                var dirs = Directory.GetDirectories(tempExtract);
                var files = Directory.GetFiles(tempExtract);

                if (dirs.Length == 1 && files.Length == 0)
                {
                    Log($"Zip contains root folder '{Path.GetFileName(dirs[0])}', stepping into it...");
                    tempExtract = dirs[0];
                }

                var allFiles = Directory.GetFiles(tempExtract, "*", SearchOption.AllDirectories);
                int processed = 0, total = allFiles.Length;

                foreach (var src in allFiles)
                {
                    string relative = Path.GetRelativePath(tempExtract, src);
                    string fileName = Path.GetFileName(relative);

                    // Skip excluded files
                    if (excludedFiles.Any(x => x.Equals(fileName, StringComparison.OrdinalIgnoreCase)))
                    {
                        Log($"Skipping excluded file: {relative}");
                        processed++;
                        Invoke(() => progressBar.Value = Math.Min(100, (int)((processed * 100.0) / total)));
                        continue;
                    }

                    // Skip excluded folders
                    if (relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                        .Any(part => ExcludedFolders.Any(folder => folder.Equals(part, StringComparison.OrdinalIgnoreCase))))
                    {
                        Log($"Skipping file in excluded folder: {relative}");
                        processed++;
                        Invoke(() => progressBar.Value = Math.Min(100, (int)((processed * 100.0) / total)));
                        continue;
                    }

                    string dest = Path.Combine(targetFolder, relative);
                    string destDir = Path.GetDirectoryName(dest)!;
                    Directory.CreateDirectory(destDir);

                    // Backup existing file before replacing
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

                // Cleanup
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
                    File.AppendAllText(Path.Combine(Path.GetTempPath(), "updater_log.txt"),
                        $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {msg}\r\n");
                    return;
                }
                File.AppendAllText(LogFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {msg}\r\n");
            }
            catch { }
        }

        #endregion
    }

 
}