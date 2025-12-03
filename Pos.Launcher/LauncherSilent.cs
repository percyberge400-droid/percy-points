using System.Net.NetworkInformation;
using System.Xml.Linq;

namespace Pos.Launcher
{
    internal static class LauncherSilent
    {
        private static string LocalFolder = "";
        private static string ServerRoot = "";
        private static string LogFile => Path.Combine(LocalFolder, "launcher_log.txt");
        private static readonly string[] ExcludedFiles = { "System.ServiceProcess.ServiceController.dll" };
        private static readonly string RuntimesFolder = "runtimes";

        public static async Task RunAsync()
        {
            try
            {
                // STEP 1️⃣ Detect install path
                DetectInstallPath();

                // STEP 2️⃣ Load server path from Updater-Version.config
                if (!LoadServerPathFromConfig())
                {
                    Log("❌ Failed to load server path from Updater-Version.config.");
                    return;
                }

                string localVersionPath = Path.Combine(LocalFolder, "launcher-version.txt");

                // STEP 3️⃣ Fetch server version via HTTP
                string serverVersionUrl = $"{ServerRoot}Updater/launcher-version.txt";
                string serverVersion;
                using (var client = new HttpClient())
                {
                    try
                    {
                        serverVersion = (await client.GetStringAsync(serverVersionUrl)).Trim();
                    }
                    catch
                    {
                        Log($"❌ Failed to fetch server launcher-version.txt at {serverVersionUrl}");
                        return;
                    }
                }

                string localVersion = File.Exists(localVersionPath)
                    ? File.ReadAllText(localVersionPath).Trim()
                    : "0.0.0";

                // STEP 4️⃣ Compare versions
                bool updateNeeded = localVersion != serverVersion;

                if (updateNeeded)
                {
                    // STEP 5️⃣ Pre-update safety checks
                    if (!CheckInternetConnection())
                    {
                        Log("❌ No internet connection. Skipping update.");
                        return;
                    }

                    // STEP 6️⃣ Fetch filelist.txt from server
                    string filelistUrl = $"{ServerRoot}Updater/{serverVersion}/filelist.txt";
                    string[] filesToDownload;
                    using (var client = new HttpClient())
                    {
                        try
                        {
                            string filelistContent = await client.GetStringAsync(filelistUrl);
                            filesToDownload = filelistContent
                                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        }
                        catch
                        {
                            Log($"❌ Failed to fetch filelist.txt at {filelistUrl}");
                            return;
                        }
                    }

                    // STEP 7️⃣ Download each file
                    using (var client = new HttpClient())
                    {
                        foreach (var file in filesToDownload)
                        {
                            string fileName = Path.GetFileName(file);
                            if (ExcludedFiles.Any(x => x.Equals(fileName, StringComparison.OrdinalIgnoreCase)) ||
                                file.StartsWith(RuntimesFolder + "/", StringComparison.OrdinalIgnoreCase))
                            {
                                Log($"Skipped: {file}");
                                continue;
                            }

                            string destFile = Path.Combine(LocalFolder, file.Replace('/', Path.DirectorySeparatorChar));
                            try
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(destFile)!);
                                byte[] fileBytes = await client.GetByteArrayAsync($"{ServerRoot}Updater/{serverVersion}/{file}");
                                await File.WriteAllBytesAsync(destFile, fileBytes);
                                Log($"Downloaded: {file}");
                            }
                            catch (Exception ex)
                            {
                                Log($"❌ Failed to download {file}: {ex.Message}");
                            }
                        }
                    }

                    // STEP 8️⃣ Update local version
                    try
                    {
                        File.WriteAllText(localVersionPath, serverVersion);
                        Log($"✅ Updated local launcher-version.txt to {serverVersion}");
                    }
                    catch (Exception ex)
                    {
                        Log($"⚠ Failed to update launcher-version.txt: {ex.Message}");
                    }

                    Log($"✅ Launcher updated successfully (local: {localVersion} → server: {serverVersion})");
                }
                else
                {
                    Log($"Launcher already up to date (v{localVersion}).");
                }

                // STEP 9️⃣ Signal completion to parent app
                using (EventWaitHandle launcherEvent = new EventWaitHandle(false, EventResetMode.AutoReset, "POSPRA_LauncherDone"))
                {
                    launcherEvent.Set();
                    Log("✅ Launcher signaled completion to parent process.");
                }

                Log("Exiting silent launcher.");
            }
            catch (Exception ex)
            {
                Log("❌ Unexpected error: " + ex);
            }
        }

        // -------------------- Helper Methods --------------------

        private static void DetectInstallPath()
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

        private static bool LoadServerPathFromConfig()
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

        private static bool CheckInternetConnection()
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

        private static void Log(string msg)
        {
            try
            {
                File.AppendAllText(LogFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {msg}\r\n");
            }
            catch
            {
                // ignore
            }
        }
    }
}
