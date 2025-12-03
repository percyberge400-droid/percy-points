using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace Pos.Updater
{
    public partial class UpdateForm : Form
    {
        private string LocalFolder = "";
        private string ServerRoot = "";
        private readonly string WorkerServiceName = "POSPRAWorker";
        private readonly string AppProcessName = "POSPRA-WinFormsUI";
        private string LogFile => Path.Combine(LocalFolder, "update_log.txt");

        private readonly string[] ExcludedFiles = new[]
        {
            "POSPRA-WinFormsUI.dll.config",
            "POSPRA.SetupUI.dll.config",
            "appsettings.json",
            "appsettings.worker.json"
        };

        public UpdateForm()
        {
            InitializeComponent();
        }

        private async void UpdateForm_Load(object sender, EventArgs e)
        {
            try
            {
                progressBar.Style = ProgressBarStyle.Marquee;
                lblStatus.Text = "Loading configuration...";
                await Task.Delay(200);

                DetectInstallPath();

                if (!LoadServerPathFromConfig())
                {
                    ShowErrorAndClose("Failed to load server path. Update aborted.");
                    return;
                }

                lblStatus.Text = "Checking for update";
                await Task.Delay(300);

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

        private bool LoadServerPathFromConfig()
        {
            try
            {
                string configPath = Path.Combine(LocalFolder, "Updater-Version.config");
                if (!File.Exists(configPath))
                {
                    Log($"Config missing");
                    return false;
                }

                var xml = System.Xml.Linq.XDocument.Load(configPath);
                var serverPathElement = xml.Descendants("add")
                                           .FirstOrDefault(x => (string)x.Attribute("key") == "ServerPath");

                if (serverPathElement == null) return false;

                ServerRoot = (string)serverPathElement.Attribute("value") ?? "";
                if (string.IsNullOrWhiteSpace(ServerRoot)) return false;

                // Ensure proper trailing slash for HTTP/HTTPS
                if (ServerRoot.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    if (!ServerRoot.EndsWith("/"))
                        ServerRoot += "/";
                }
                else
                {
                    if (!ServerRoot.EndsWith("\\"))
                        ServerRoot += "\\";
                }

                Log($"Loaded server path: {ServerRoot}");
                return true;
            }
            catch (Exception ex)
            {
                Log($"Error reading Updater-Version.config: {ex.Message}");
                return false;
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
            string serverVersionUrl = ServerRoot + "app-version.txt";

            string serverVersion;
            try
            {
                using var client = new HttpClient();
                serverVersion = (await client.GetStringAsync(serverVersionUrl)).Trim();
            }
            catch
            {
                ShowErrorAndClose("Failed to fetch server version. Update aborted.");
                return;
            }

            string localVersion = File.Exists(localVersionPath) ? File.ReadAllText(localVersionPath).Trim() : "0.0.0";

            string serverVersionFolderUrl = $"{ServerRoot}{serverVersion}/";

            Log($"Local version: {localVersion}, Server version: {serverVersion}");

            try
            {
                // Stop worker service & running UI processes
                Invoke((Action)(() => lblStatus.Text = "Stopping worker service..."));
                StopService(WorkerServiceName);

                Invoke((Action)(() => lblStatus.Text = "Stopping running applications..."));
                StopProcess(AppProcessName);

                await Task.Delay(1000);

                // Download update files
                Invoke((Action)(() => lblStatus.Text = "Applying updates..."));
                Invoke((Action)(() =>
                {
                    progressBar.Style = ProgressBarStyle.Continuous;
                    progressBar.Value = 0;
                }));

                await DownloadFilesFromHttpAsync(serverVersionFolderUrl, LocalFolder);

                // Update version file
                File.WriteAllText(localVersionPath, serverVersion);
                Log($"Version updated: {localVersion} → {serverVersion}");

                // Restart service
                Invoke((Action)(() => lblStatus.Text = "Restarting worker service..."));
                StartService(WorkerServiceName);

                // Relaunch UI
                string uiExe = Path.Combine(LocalFolder, "POSPRA-WinFormsUI.exe");
                if (File.Exists(uiExe)) Process.Start(uiExe);

                Invoke((Action)(() => lblStatus.Text = "Update completed!"));
                Invoke((Action)(() => progressBar.Value = 100));

                MessageBox.Show(new Form { TopMost = true },
                    $"Update completed successfully!\nPrevious version: {localVersion}\nNew version: {serverVersion}",
                    "Update Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

                Log("✅ Update completed successfully.");
                Close();
            }
            catch (Exception ex)
            {
                ShowErrorAndClose($"Unexpected error during update: {ex.Message}");
            }
        }

        private async Task DownloadFilesFromHttpAsync(string serverFolderUrl, string localFolder)
        {
            using var client = new HttpClient();
            string fileListUrl = serverFolderUrl + "filelist.txt";

            string[] files;
            try
            {
                string fileListContent = await client.GetStringAsync(fileListUrl);
                files = fileListContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            }
            catch
            {
                ShowErrorAndClose("Failed to fetch file list from server.");
                return;
            }

            int copiedCount = 0;
            int totalFiles = files.Length;

            foreach (var file in files)
            {
                string fileName = Path.GetFileName(file);

                if (ExcludedFiles.Any(x => x.Equals(fileName, StringComparison.OrdinalIgnoreCase)))
                {
                    Log($"Skipped file: {fileName}");
                    continue;
                }

                string localPath = Path.Combine(localFolder, file.Replace("/", "\\"));

                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(localPath)!);
                    byte[] data = await client.GetByteArrayAsync(serverFolderUrl + file);
                    await File.WriteAllBytesAsync(localPath, data);
                    Log($"Downloaded file: {file}");
                }
                catch (Exception ex)
                {
                    ShowErrorAndClose($"Failed to download file {file}: {ex.Message}");
                }

                copiedCount++;
                int percent = (int)((copiedCount * 100.0) / totalFiles);
                Invoke((Action)(() => progressBar.Value = Math.Min(percent, 100)));
            }

            Log($"Download complete: {copiedCount}/{totalFiles} files processed.");
        }

        private void StopService(string serviceName)
        {
            try
            {
                using (var mutex = Mutex.OpenExisting("POSPRAWorkerServiceMutex"))
                {
                    mutex.WaitOne(TimeSpan.FromSeconds(60));
                }
            }
            catch (WaitHandleCannotBeOpenedException) { }
            catch (AbandonedMutexException) { }

            using var sc = new ServiceController(serviceName);
            if (sc.Status == ServiceControllerStatus.Running ||
                sc.Status == ServiceControllerStatus.Paused)
            {
                sc.Stop();
                sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(60));
            }
        }

        private void StartService(string serviceName)
        {
            using var sc = new ServiceController(serviceName);
            if (sc.Status != ServiceControllerStatus.Running)
            {
                sc.Start();
                sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(60));
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
                }
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
            try { File.AppendAllText(LogFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {msg}\r\n"); }
            catch { }
        }
    }
}
