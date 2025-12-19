using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("===== Pos.Configuration: DLL Replacer =====");

        if (args.Length == 0)
        {
            Console.WriteLine("Usage: Pos.Configuration.exe <InstallFolder>");
            return;
        }

        string installFolder = args[0].Trim('"');

        if (!Directory.Exists(installFolder))
        {
            Console.WriteLine($"Error: Install folder does not exist: {installFolder}");
            return;
        }

        // Folder where Pos.Configuration.exe is running from
        string exeFolder = Path.GetDirectoryName(Environment.ProcessPath!)!;

        // Get all DLLs next to the EXE
        string[] dlls = Directory.GetFiles(exeFolder, "*.dll");

        foreach (var sourcePath in dlls)
        {
            string fileName = Path.GetFileName(sourcePath);

            // Safety: don't copy itself or runtime DLLs if any
            if (fileName.Equals("Pos.Configuration.dll", StringComparison.OrdinalIgnoreCase))
                continue;

            string targetPath = Path.Combine(installFolder, fileName);

            try
            {
                File.Copy(sourcePath, targetPath, overwrite: true);
                Console.WriteLine($"Replaced: {fileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to replace {fileName}: {ex.Message}");
            }
        }

        Console.WriteLine("DLL replacement completed.");
    }
}
