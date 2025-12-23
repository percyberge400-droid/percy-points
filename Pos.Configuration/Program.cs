using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        string exeDir = AppDomain.CurrentDomain.BaseDirectory;
        string logFile = Path.Combine(exeDir, "Pos.Configuration.log");

        void Log(string msg)
        {
            string line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {msg}";
            Console.WriteLine(line);
            try
            {
                File.AppendAllText(logFile, line + Environment.NewLine);
            }
            catch { /* never crash installer */ }
        }

        Log("===== Pos.Configuration : DLL Replacement START =====");
        Log($"EXE Location: {exeDir}");

        if (args.Length == 0)
        {
            Log("ERROR: Install folder argument missing.");
            Log("Expected usage: Pos.Configuration.exe <InstallFolder>");
            return;
        }

        string installFolder = args[0].Trim('"');
        Log($"Install Folder Received: {installFolder}");

        if (!Directory.Exists(installFolder))
        {
            Log($"ERROR: Install folder does not exist: {installFolder}");
            return;
        }

        // ✅ ALWAYS use Obfuscated folder next to EXE
        string obfuscatedFolder = Path.Combine(exeDir, "Obfuscated");
        Log($"Obfuscated Source Folder: {obfuscatedFolder}");

        if (!Directory.Exists(obfuscatedFolder))
        {
            Log("ERROR: Obfuscated folder NOT FOUND.");
            return;
        }

        string[] dllFiles = Directory.GetFiles(obfuscatedFolder, "*.dll");
        Log($"DLLs Found in Obfuscated Folder: {dllFiles.Length}");

        foreach (string sourcePath in dllFiles)
        {
            string fileName = Path.GetFileName(sourcePath);
            string targetPath = Path.Combine(installFolder, fileName);

            try
            {
                Log($"Replacing: {fileName}");
                File.Copy(sourcePath, targetPath, overwrite: true);
                Log($"SUCCESS: {fileName} replaced");
            }
            catch (Exception ex)
            {
                Log($"FAILED: {fileName} | {ex.Message}");
            }
        }

        Log("===== Pos.Configuration : DLL Replacement END =====");
    }
}
