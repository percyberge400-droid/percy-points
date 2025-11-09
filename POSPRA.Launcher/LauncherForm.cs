using System;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace POSPRA.Launcher
{
    public partial class LauncherForm : Form
    {
        private string LocalFolder = "";
        private string ServerRoot = "";
        private string LogFile => Path.Combine(LocalFolder, "launcher_log.txt");
        private readonly string[] ExcludedFiles = { "System.ServiceProcess.ServiceController.dll" };
        private readonly string RuntimesFolder = "runtimes";

        public LauncherForm() => InitializeComponent();

        private async void LauncherForm_Load(object sender, EventArgs e)
        {
            try
            {
                // STEP 1️⃣ Detect install path
                DetectInstallPath();

                // STEP 2️⃣ Load server path from Updater-Version.config
                if (!LoadServerPathFromConfig())
                {
                    ShowErrorAndExit("❌ Failed to load server path from Updater-Version.config.");
                    return;
                }

                string localVersionPath = Path.Combine(LocalFolder, "launcher-version.txt");
                string serverUpdaterPath = Path.Combine(ServerRoot, "Updater");
                string serverVersionPath = Path.Combine(serverUpdaterPath, "launcher-version.txt");

                // STEP 3️⃣ Check required version files exist
                if (!File.Exists(serverVersionPath))
                {
                    ShowErrorAndExit($"❌ Server version file missing at:\n{serverVersionPath}");
                    return;
                }

                if (!File.Exists(localVersionPath))
                {
                    Log("⚠ No local launcher-version.txt found, treating as fresh install.");
                }

                string localVersion = File.Exists(localVersionPath)
                    ? File.ReadAllText(localVersionPath).Trim()
                    : "0.0.0";
                string serverVersion = File.ReadAllText(serverVersionPath).Trim();

                // STEP 4️⃣ Compare versions — if same, exit silently
                if (localVersion == serverVersion)
                {
                    Log($"Launcher already up to date (v{localVersion}). Exiting silently.");
                    Application.Exit();
                    return;
                }

                // STEP 5️⃣ Pre-update safety checks
                if (!CheckInternetConnection())
                {
                    ShowErrorAndExit("❌ No internet connection. Please check your network and try again.");
                    return;
                }

                string versionFolderPath = Path.Combine(serverUpdaterPath, serverVersion);
                if (!Directory.Exists(versionFolderPath))
                {
                    ShowErrorAndExit($"❌ Update folder not found on server:\n{versionFolderPath}");
                    return;
                }

                if (!Directory.EnumerateFileSystemEntries(versionFolderPath).Any())
                {
                    ShowErrorAndExit($"❌ The update folder on the server is empty:\n{versionFolderPath}");
                    return;
                }

                // STEP 6️⃣ Apply update
                Log($"Starting update from: {versionFolderPath}");
                await Task.Run(() => CopyFilesRecursive(versionFolderPath, LocalFolder));

                Log($"✅ Update completed successfully (local: {localVersion} → server: {serverVersion})");

                // STEP 7️⃣ Signal login form and exit
                string flagFile = Path.Combine(LocalFolder, "launcher_updated.flag");
                try
                {
                    if (File.Exists(flagFile)) File.Delete(flagFile);
                    File.WriteAllText(flagFile, "true");
                }
                catch (Exception ex)
                {
                    Log($"⚠ Could not create flag file: {ex.Message}");
                }

                Log("Exiting after successful update.");
                Application.Exit();
            }
            catch (Exception ex)
            {
                Log("❌ Unexpected error: " + ex);
                ShowErrorAndExit($"❌ Launcher failed:\n\n{ex.Message}");
            }
        }

        // -------------------- Helper Methods --------------------

        private void DetectInstallPath()
        {
            try
            {
                string infoPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "PRAL", "install_info.txt");

                if (File.Exists(infoPath))
                {
                    foreach (var line in File.ReadAllLines(infoPath))
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
                    LocalFolder = Directory.Exists(defaultPath)
                        ? defaultPath
                        : AppDomain.CurrentDomain.BaseDirectory;
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
                    Log($"Missing config file: {configPath}");
                    return false;
                }

                var xml = System.Xml.Linq.XDocument.Load(configPath);
                var serverPathElement = xml.Descendants("add")
                    .FirstOrDefault(x => (string)x.Attribute("key") == "ServerPath");

                if (serverPathElement == null)
                    return false;

                ServerRoot = (string)serverPathElement.Attribute("value") ?? "";
                if (string.IsNullOrWhiteSpace(ServerRoot))
                    return false;

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

        private void CopyFilesRecursive(string src, string dst)
        {
            var files = Directory.GetFiles(src, "*", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                string relPath = file.Substring(src.Length).TrimStart('\\');
                string destFile = Path.Combine(dst, relPath);
                string fileName = Path.GetFileName(file);

                // Skip excluded files
                if (ExcludedFiles.Any(x => x.Equals(fileName, StringComparison.OrdinalIgnoreCase)))
                {
                    Log($"Skipped excluded file: {relPath}");
                    continue;
                }

                // Skip runtimes folder
                if (relPath.StartsWith(RuntimesFolder + "\\", StringComparison.OrdinalIgnoreCase))
                {
                    Log($"Skipped folder: {relPath}");
                    continue;
                }

                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(destFile)!);
                    File.Copy(file, destFile, true);
                    Log($"Copied: {relPath}");
                }
                catch (Exception ex)
                {
                    ShowErrorAndExit($"❌ Failed to copy file:\n{relPath}\n\n{ex.Message}");
                }
            }
        }

        private void ShowErrorAndExit(string msg)
        {
            try
            {
                Log(msg);

                // Ensure message box is visible above all windows
                this.Invoke((MethodInvoker)(() =>
                {
                    this.TopMost = true;
                    this.BringToFront();
                    MessageBox.Show(this, msg, "Launcher Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }));

                Application.DoEvents();
                System.Threading.Thread.Sleep(500);
                Application.Exit();
            }
            catch
            {
                Environment.Exit(1);
            }
        }

        private void Log(string msg)
        {
            try
            {
                File.AppendAllText(LogFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {msg}\r\n");
            }
            catch
            {
                // Ignore logging errors
            }
        }
    }
}
