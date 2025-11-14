using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace POSPRA.Updater
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
                ShowErrorAndClose($"Updater failed: ");
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

                Log($"Detected install path");
            }
            catch (Exception ex)
            {
                LocalFolder = AppDomain.CurrentDomain.BaseDirectory;
                Log($"DetectInstallPath error: ");
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
                if (!ServerRoot.EndsWith("\\")) ServerRoot += "\\";

                Log($"Loaded server path");
                return true;
            }
            catch (Exception ex)
            {
                Log($"Error reading Updater-Version.config");
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
            string serverVersionPath = Path.Combine(ServerRoot, "app-version.txt");

            if (!File.Exists(serverVersionPath))
            {
                ShowErrorAndClose("Server version file missing. Update aborted.");
                return;
            }

            string serverVersion = File.ReadAllText(serverVersionPath).Trim();
            string localVersion = File.Exists(localVersionPath) ? File.ReadAllText(localVersionPath).Trim() : "0.0.0";

            string serverVersionFolder = Path.Combine(ServerRoot, serverVersion);
            if (!Directory.Exists(serverVersionFolder) || !Directory.EnumerateFileSystemEntries(serverVersionFolder).Any())
            {
                ShowErrorAndClose("Server version folder missing or empty. Update aborted.");
                return;
            }

            Log($"Local version: {localVersion}, Server version: {serverVersion}");

            try
            {
                // Stop worker service & running UI processes
                Invoke((Action)(() => lblStatus.Text = "Stopping worker service..."));
                StopService(WorkerServiceName);

                Invoke((Action)(() => lblStatus.Text = "Stopping running applications..."));
                StopProcess(AppProcessName);

                // Wait briefly to ensure all file handles are released
                await Task.Delay(1000);

                // Copy update files
                Invoke((Action)(() => lblStatus.Text = "Applying updates..."));
                Invoke((Action)(() =>
                {
                    progressBar.Style = ProgressBarStyle.Continuous;
                    progressBar.Value = 0;
                }));
                await Task.Run(() => CopyFilesRecursive(serverVersionFolder, LocalFolder));

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
                ShowErrorAndClose($"Unexpected error during update ");
            }

        }
        private void StopService(string serviceName)
        {
            try
            {
                // Wait for LoginForm2 to finish if it is restarting the service
                using (var mutex = Mutex.OpenExisting("POSPRAWorkerServiceMutex"))
                {
                    mutex.WaitOne(TimeSpan.FromSeconds(60));
                }
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                // Mutex doesn't exist, proceed normally
            }
            catch (AbandonedMutexException)
            {
                // Mutex was abandoned, safe to continue
            }

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
                    ShowErrorAndClose($"Failed to stop process {name} ");
                }
            }
        }

        private void CopyFilesRecursive(string src, string dst)
        {
            var files = Directory.GetFiles(src, "*", SearchOption.AllDirectories);
            int copiedCount = 0;
            int totalFiles = files.Length;

            foreach (string file in files)
            {
                string relPath = file.Substring(src.Length).TrimStart('\\');
                string destFile = Path.Combine(dst, relPath);
                string fileName = Path.GetFileName(file);

                // Skip excluded files
                if (ExcludedFiles.Any(x => x.Equals(fileName, StringComparison.OrdinalIgnoreCase)))
                {
                    Log($"Skipped file");
                    continue;
                }

                // Skip entire runtimes folder to avoid locking DLLs
                if (relPath.StartsWith("runtimes\\", StringComparison.OrdinalIgnoreCase))
                {
                    Log($"Skipped folder");
                    continue;
                }

                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(destFile)!);
                    File.Copy(file, destFile, true);
                    Log($"Copied file");
                }
                catch (Exception ex)
                {
                    ShowErrorAndClose($"Failed to copy {relPath} ");
                }

                copiedCount++;
                int percent = (int)((copiedCount * 100.0) / totalFiles);
                Invoke((Action)(() => progressBar.Value = Math.Min(percent, 100)));
            }

            Log($"Copy complete: {copiedCount}/{totalFiles} files processed.");
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
            catch { /* ignore */ }
        }
    }
}
