using System;
using System.IO;

namespace Pos.WriteInstallPath
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                // 1. Get the path passed from the [TARGETDIR] argument
                string installDir = args.Length > 0 ? args[0] : string.Empty;

                // Validation: Ensure we actually received a path
                if (string.IsNullOrWhiteSpace(installDir))
                {
                    return 1; // Exit with error if no path provided
                }

                // 2. Remove trailing backslash (Common with [TARGETDIR])
                installDir = installDir.TrimEnd('\\', '/');

                // 3. Define target folder
                string targetFolder = @"C:\ProgramData\PRAL\PRAPOS_Component";

                // 4. Create folder if it doesn't exist
                if (!Directory.Exists(targetFolder))
                {
                    Directory.CreateDirectory(targetFolder);
                }

                // 5. Write the file
                string filePath = Path.Combine(targetFolder, "install_info.txt");
                File.WriteAllText(filePath, $"InstallPath={installDir}");

                return 0; // Success
            }
            catch (Exception)
            {
                // In a Custom Action, throwing an exception or returning 1 
                // can roll back the installation depending on your settings.
                return 1;
            }
        }
    }
}