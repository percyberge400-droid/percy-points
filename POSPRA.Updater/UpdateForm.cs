using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;

namespace POSPRA.Updater
{
    public partial class UpdateForm : Form
    {
        private string LocalFolder = "";
        private readonly string ServerRoot = @"\\10.16.68.231\Shared\Talha Arif\Update_Installer\";
        private readonly string WorkerServiceName = "POSPRAWorker"; // ✅ Corrected service name
        private readonly string AppProcessName = "POSPRA-WinFormsUI";
        private string LogFile => Path.Combine(LocalFolder, "update_log.txt");

        public UpdateForm()
        {
            InitializeComponent();
        }

        private async void UpdateForm_Load(object sender, EventArgs e)
        {
            progressBar.Style = ProgressBarStyle.Marquee;
            lblStatus.Text = "Checking for updates...";
            await Task.Delay(300);
            await CheckForUpdatesAsync();
        }

        private async Task CheckForUpdatesAsync()
        {
            try
            {
                // Detect install folder dynamically (from install_info.txt)
                string commonInfo = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "PRAL",
                    "install_info.txt"
                );

                if (File.Exists(commonInfo))
                {
                    foreach (var l in File.ReadAllLines(commonInfo))
                    {
                        if (l.StartsWith("InstallPath=", StringComparison.OrdinalIgnoreCase))
                        {
                            LocalFolder = l.Substring("InstallPath=".Length).Trim();
                            break;
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(LocalFolder))
                {
                    var defaultPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                        "PRAL",
                        "POSComponent"
                    );

                    LocalFolder = Directory.Exists(defaultPath)
                        ? defaultPath
                        : AppDomain.CurrentDomain.BaseDirectory;
                }

                string localVerFile = Path.Combine(LocalFolder, "version.txt");
                string remoteVerFile = Path.Combine(ServerRoot, "latest.txt");

                if (!File.Exists(remoteVerFile))
                {
                    lblStatus.Text = "Could not reach update server.";
                    await Task.Delay(2000);
                    Close();
                    return;
                }

                if (!File.Exists(localVerFile))
                {
                    lblStatus.Text = "Local version info missing.";
                    await Task.Delay(2000);
                    Close();
                    return;
                }

                string localVer = File.ReadAllText(localVerFile).Trim();
                string remoteVer = File.ReadAllText(remoteVerFile).Trim();

                if (localVer == remoteVer)
                {
                    lblStatus.Text = "You already have the latest version.";
                    await Task.Delay(1500);
                    Close();
                    return;
                }

                // 🧠 Ask user for confirmation
                var result = MessageBox.Show(
                    $"A new update ({remoteVer}) is available.\n\nYour version: {localVer}\n\nDo you want to update now?",
                    "Update Available",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    await RunUpdateAsync(localVer, remoteVer);
                }
                else
                {
                    lblStatus.Text = "Update cancelled by user.";
                    await Task.Delay(1500);
                    Close();
                }
            }
            catch (Exception ex)
            {
                Log("ERROR (CheckForUpdates): " + ex.Message);
                lblStatus.Text = "Error while checking for updates.";
                await Task.Delay(3000);
                Close();
            }
        }

        private async Task RunUpdateAsync(string localVer, string remoteVer)
        {
            try
            {
                lblStatus.Text = "Stopping services...";
                StopService(WorkerServiceName);
                StopProcess(AppProcessName);

                string remoteFolder = Path.Combine(ServerRoot, remoteVer);
                if (!Directory.Exists(remoteFolder))
                {
                    lblStatus.Text = "Remote version folder missing.";
                    Log("Remote folder not found: " + remoteFolder);
                    await Task.Delay(2000);
                    Close();
                    return;
                }

                lblStatus.Text = "Applying updates...";
                progressBar.Style = ProgressBarStyle.Continuous;
                progressBar.Value = 0;

                await Task.Run(() => CopyFilesRecursive(remoteFolder, LocalFolder));

                File.WriteAllText(Path.Combine(LocalFolder, "version.txt"), remoteVer);
                Log("Version updated to " + remoteVer);

                lblStatus.Text = "Restarting worker service...";
                StartService(WorkerServiceName);

                lblStatus.Text = "Launching application...";
                string appPath = Path.Combine(LocalFolder, $"{AppProcessName}.exe");
                if (File.Exists(appPath))
                    Process.Start(appPath, "/updated");

                lblStatus.Text = "Update completed successfully!";
                progressBar.Value = 100;

                await Task.Delay(2000);
                Close();
            }
            catch (Exception ex)
            {
                Log("ERROR (RunUpdate): " + ex.Message);
                lblStatus.Text = "Update failed. See log file.";
                await Task.Delay(3000);
                Close();
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
                    sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                }
            }
            catch (Exception ex) { Log("StopService: " + ex.Message); }
        }

        private void StartService(string name)
        {
            try
            {
                using var sc = new ServiceController(name);
                sc.Start();
                sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
            }
            catch (Exception ex) { Log("StartService: " + ex.Message); }
        }

        private void StopProcess(string name)
        {
            foreach (var p in Process.GetProcessesByName(name))
            {
                try
                {
                    Log($"Killing {name} (PID {p.Id})...");
                    p.Kill();
                    p.WaitForExit(5000);
                }
                catch (Exception ex) { Log("StopProcess: " + ex.Message); }
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
                string ext = Path.GetExtension(file).ToLowerInvariant();

                // 🧠 Skip user configuration files
                if (ext == ".config" || ext == ".json" || ext == ".settings")
                {
                    Log($"Skipped config file: {relPath}");
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

                // Update progress
                copiedCount++;
                int percent = (int)((copiedCount * 100.0) / totalFiles);
                Invoke((Action)(() => progressBar.Value = Math.Min(percent, 100)));
            }

            Log($"✅ Copy complete. {copiedCount}/{totalFiles} files processed (configs skipped).");
        }


        private void Log(string msg)
        {
            File.AppendAllText(LogFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {msg}\r\n");
        }
    }
}
