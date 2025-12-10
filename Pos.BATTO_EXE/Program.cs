using System;
using System.Diagnostics;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            // Default action = install
            string action = args.Length > 0 ? args[0].TrimStart('/', '-').ToLower() : "install";

            // Get exe directory
            string exeDir = AppDomain.CurrentDomain.BaseDirectory;

            // Choose script based on action
            string batFile = action == "uninstall"
                ? Path.Combine(exeDir, "UninstallPOSPRAService.bat")
                : Path.Combine(exeDir, "InstallPOSPRAService.bat");

            if (!File.Exists(batFile))
            {
                Console.WriteLine($"Batch file not found: {batFile}");
                return;
            }

            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"{batFile}\"",
                UseShellExecute = true,
                Verb = "runas" // admin rights
            };

            using var process = Process.Start(psi);
            process?.WaitForExit();

            if (process?.ExitCode == 0)
            {
                Console.WriteLine($"Service {action} completed successfully.");
            }
            else
            {
                Console.WriteLine($"Service {action} failed. Exit code: {process?.ExitCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
