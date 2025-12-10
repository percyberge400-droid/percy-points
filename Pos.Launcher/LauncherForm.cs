using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace POSPRA.Launcher
{
    public partial class LauncherForm : Form
    {
        private string LocalFolder = "";
        private string ServerRoot = "";
        private string LogFile => Path.Combine(LocalFolder, "launcher_log.txt");
        private readonly string[] ExcludedFiles = { "System.ServiceProcess.ServiceController.dll" };
        private readonly string RuntimesFolder = "runtimes";
        private readonly HttpClient http = new HttpClient();

        public LauncherForm() => InitializeComponent();

        private async void LauncherForm_Shown(object sender, EventArgs e)
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
                string serverUpdaterUrl = ServerRoot.EndsWith("/") ? $"{ServerRoot}Updater/" : $"{ServerRoot}/Updater/";
                string serverVersionUrl = $"{serverUpdaterUrl}launcher-version.txt";

                // STEP 3️⃣ Fetch server version over HTTP
                string serverVersion;
                try
                {
                    serverVersion = (await http.GetStringAsync(serverVersionUrl)).Trim();
                }
                catch
                {
                    ShowErrorAndExit($"❌ Cannot fetch server launcher-version.txt at:\n{serverVersionUrl}");
                    return;
                }

                string localVersion = File.Exists(localVersionPath) ? File.ReadAllText(localVersionPath).Trim() : "0.0.0";
                bool updateNeeded = localVersion != serverVersion;

                if (updateNeeded)
                {
                    // STEP 4️⃣ Pre-update safety checks
                    if (!CheckInternetConnection())
                    {
                        ShowErrorAndExit("❌ No internet connection. Please check your network and try again.");
                        return;
                    }

                    string versionFolderUrl = $"{serverUpdaterUrl}{serverVersion}/";

                    Log($"Starting update from: {versionFolderUrl}");
                    await DownloadFilesRecursive(versionFolderUrl, LocalFolder);
                    Log($"✅ Update completed successfully (local: {localVersion} → server: {serverVersion})");

                    try
                    {
                        File.WriteAllText(localVersionPath, serverVersion);
                        Log($"✅ Updated local launcher-version.txt to {serverVersion}");
                    }
                    catch (Exception ex)
                    {
                        Log($"⚠ Failed to update launcher-version.txt: {ex.Message}");
                    }
                }
                else
                {
                    Log($"Launcher already up to date (v{localVersion}).");
                }

                // STEP 5️⃣ Signal LoginForm using EventWaitHandle
                using (EventWaitHandle launcherEvent = new EventWaitHandle(false, EventResetMode.AutoReset, "POSPRA_LauncherDone"))
                {
                    launcherEvent.Set();
                    Log("✅ Launcher signaled completion to LoginForm.");
                }

                Log("Exiting Launcher.");
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

                var xml = XDocument.Load(configPath);
                var serverPathElement = xml.Descendants("add")
                    .FirstOrDefault(x => (string)x.Attribute("key") == "ServerPath");

                if (serverPathElement == null)
                    return false;

                ServerRoot = (string)serverPathElement.Attribute("value") ?? "";
                if (string.IsNullOrWhiteSpace(ServerRoot))
                    return false;

                if (!ServerRoot.EndsWith("/")) ServerRoot += "/";

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

        private async Task DownloadFilesRecursive(string baseUrl, string localDst)
        {
            // Assume server has a filelist.txt describing files in the update folder
            string fileListUrl = $"{baseUrl}filelist.txt";

            string[] files;
            try
            {
                var content = await http.GetStringAsync(fileListUrl);
                files = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            }
            catch
            {
                Log($"❌ Cannot fetch filelist.txt at {fileListUrl}, skipping update.");
                return;
            }

            foreach (var file in files)
            {
                string remoteUrl = $"{baseUrl}{file}";
                string localFile = Path.Combine(localDst, file.Replace('/', Path.DirectorySeparatorChar));

                string fileName = Path.GetFileName(file);
                if (ExcludedFiles.Any(x => x.Equals(fileName, StringComparison.OrdinalIgnoreCase)) ||
                    file.StartsWith($"{RuntimesFolder}/", StringComparison.OrdinalIgnoreCase))
                {
                    Log($"Skipped: {file}");
                    continue;
                }

                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(localFile)!);
                    using var fs = new FileStream(localFile, FileMode.Create, FileAccess.Write, FileShare.None);
                    var stream = await http.GetStreamAsync(remoteUrl);
                    await stream.CopyToAsync(fs);
                    Log($"Downloaded: {file}");
                }
                catch (Exception ex)
                {
                    Log($"❌ Failed to download {file}: {ex.Message}");
                }
            }
        }

        private void ShowErrorAndExit(string msg)
        {
            try
            {
                Log(msg);
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
            catch { }
        }
    }
}
