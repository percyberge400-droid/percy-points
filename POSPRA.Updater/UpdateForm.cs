using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace POSPRA.Updater
{
    public partial class UpdateForm : Form
    {
        private string LocalFolder = "";
        private string ServerRoot = "";
        private readonly string WorkerServiceName = "POSPRAWorker";
        private readonly string AppProcessName = "POSPRA-WinFormsUI";
        private string LogFile => Path.Combine(LocalFolder, "update_log.txt");

        public UpdateForm() => InitializeComponent();

        private async void UpdateForm_Load(object sender, EventArgs e)
        {
            try
            {
                progressBar.Style = ProgressBarStyle.Marquee;
                lblStatus.Text = "Loading configuration...";
                await Task.Delay(200);

                if (!LoadServerPathFromConfig())
                {
                    Log("Updater terminated: failed to load server path.");
                    MessageBox.Show("Failed to load server path. Update aborted.", "Updater Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Close();
                    return;
                }

                DetectInstallPath();

                lblStatus.Text = "Checking for updates...";
                await Task.Delay(300);

                await RunUpdateAsync();
            }
            catch (Exception ex)
            {
                Log($"Updater Load Error: {ex.Message}");
                MessageBox.Show($"Updater failed to start:\n{ex.Message}", "Updater Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }

        private bool LoadServerPathFromConfig()
        {
            try
            {
                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Updater-Version.config");
                if (!File.Exists(configPath))
                {
                    Log($"Config missing: {configPath}");
                    return false;
                }

                var xml = System.Xml.Linq.XDocument.Load(configPath);
                var serverPathElement = xml.Descendants("add")
                                           .FirstOrDefault(x => (string)x.Attribute("key") == "ServerPath");

                if (serverPathElement == null)
                {
                    Log("Missing 'ServerPath' key in Updater-Version.config.");
                    return false;
                }

                ServerRoot = (string)serverPathElement.Attribute("value") ?? "";
                if (string.IsNullOrWhiteSpace(ServerRoot))
                {
                    Log("ServerPath value empty in config.");
                    return false;
                }

                if (!ServerRoot.EndsWith("\\")) ServerRoot += "\\";
                Log($"Loaded server path: {ServerRoot}");
                return true;
            }
            catch (Exception ex)
            {
                Log($"Error reading Updater-Version.config: {ex.Message}");
                return false;
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
                Log($"DetectInstallPath error: {ex.Message}");
                LocalFolder = AppDomain.CurrentDomain.BaseDirectory;
            }
        }

        private async Task RunUpdateAsync()
        {
            try
            {
                string localVersionPath = Path.Combine(LocalFolder, "app-version.txt");
                string serverVersionPath = Path.Combine(ServerRoot, "app-version.txt");

                // ✅ Check server version file exists
                if (!File.Exists(serverVersionPath))
                {
                    string msg = $"Server version file missing at {serverVersionPath}";
                    Log(msg);
                    MessageBox.Show(msg, "Updater Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string serverVersion = File.ReadAllText(serverVersionPath).Trim();
                string localVersion = File.Exists(localVersionPath) ? File.ReadAllText(localVersionPath).Trim() : "0.0.0";
                string serverVersionFolder = Path.Combine(ServerRoot, serverVersion);

                // ✅ Check server version folder exists
                if (!Directory.Exists(serverVersionFolder) || !Directory.EnumerateFileSystemEntries(serverVersionFolder).Any())
                {
                    string msg = $"Server version folder missing or empty: {serverVersionFolder}";
                    Log(msg);
                    MessageBox.Show(msg, "Updater Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                Log($"Local version: {localVersion}, Server version: {serverVersion}");

                if (serverVersion == localVersion)
                {
                    MessageBox.Show($"Already running the latest version ({localVersion}).", "No Update Needed",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Log("No update required.");
                    return;
                }

                // ✅ Ask user confirmation
                var confirm = MessageBox.Show(
                    $"A new update ({serverVersion}) is available.\nCurrent version: {localVersion}\nDo you want to update now?",
                    "Update Available", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (confirm == DialogResult.No)
                {
                    Log("User declined update.");
                    return;
                }

                // ✅ Stop services/processes before update
                lblStatus.Text = "Stopping services...";
                StopService(WorkerServiceName);
                StopProcess(AppProcessName);

                // ✅ Copy files
                lblStatus.Text = "Applying updates...";
                progressBar.Style = ProgressBarStyle.Continuous;
                progressBar.Value = 0;

                await Task.Run(() => CopyFilesRecursive(serverVersionFolder, LocalFolder));

                // ✅ Update version file
                File.WriteAllText(localVersionPath, serverVersion);
                Log($"Version updated: {localVersion} → {serverVersion}");

                // ✅ Restart service
                lblStatus.Text = "Restarting worker service...";
                StartService(WorkerServiceName);

                // ✅ Relaunch UI
                string uiExe = Path.Combine(LocalFolder, "POSPRA-WinFormsUI.exe");
                if (File.Exists(uiExe)) Process.Start(uiExe);

                lblStatus.Text = "Update completed!";
                progressBar.Value = 100;
                MessageBox.Show($"Update completed successfully!\nPrevious version: {localVersion}\nNew version: {serverVersion}",
                    "Update Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

                Log("✅ Update completed successfully.");
            }
            catch (Exception ex)
            {
                Log($"Update failed: {ex.Message}");
                MessageBox.Show($"Update failed: {ex.Message}", "Updater Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StopService(string name)
        {
            try
            {
                using var sc = new ServiceController(name);
                if (sc.Status != ServiceControllerStatus.Stopped)
                {
                    Log($"Stopping service {name}...");
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(20));
                }
            }
            catch (Exception ex) { Log($"StopService: {ex.Message}"); }
        }

        private void StartService(string name)
        {
            try
            {
                using var sc = new ServiceController(name);
                sc.Start();
                sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(20));
            }
            catch (Exception ex) { Log($"StartService: {ex.Message}"); }
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
                catch (Exception ex) { Log($"StopProcess: {ex.Message}"); }
            }
        }

        private void CopyFilesRecursive(string src, string dst)
        {
            var files = Directory.GetFiles(src, "*", SearchOption.AllDirectories);
            int copiedCount = 0;
            int totalFiles = files.Length;

            string[] excluded = new[]
            {
                "POSPRA-WinFormsUI.dll.config",
                "POSPRA.SetupUI.dll.config",
                "appsettings.json",
                "appsettings.worker.json"
            };

            foreach (string file in files)
            {
                string relPath = file.Substring(src.Length).TrimStart('\\');
                string destFile = Path.Combine(dst, relPath);
                string fileName = Path.GetFileName(file);

                if (excluded.Any(x => x.Equals(fileName, StringComparison.OrdinalIgnoreCase)))
                {
                    Log($"Skipped file: {relPath}");
                    continue;
                }

                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(destFile)!);
                    File.Copy(file, destFile, true);
                    Log($"Copied file: {relPath}");
                }
                catch (Exception ex)
                {
                    Log($"Failed to copy {relPath}: {ex.Message}");
                }

                copiedCount++;
                int percent = (int)((copiedCount * 100.0) / totalFiles);
                Invoke((Action)(() => progressBar.Value = Math.Min(percent, 100)));
            }

            Log($"Copy complete: {copiedCount}/{totalFiles} files processed.");
        }

        private void Log(string msg)
        {
            try { File.AppendAllText(LogFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {msg}\r\n"); }
            catch { /* ignore */ }
        }
    }
}
