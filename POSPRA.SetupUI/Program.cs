namespace POSPRA.SetupUI
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            string configPath = string.Empty;

            // 1?? Try to read path passed from MSI custom action arguments
            if (args != null && args.Length > 0)
            {
                string candidate = args[0].Trim('"').TrimEnd('\\'); // ?? strip trailing slash

                if (Directory.Exists(candidate))
                {
                    configPath = Path.Combine(candidate, "POSPRA-WinFormsUI.dll.config");
                }
                else if (File.Exists(candidate))
                {
                    configPath = candidate;
                }
            }

            // 2?? Fallback: use app base directory if nothing passed
            if (string.IsNullOrEmpty(configPath))
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                configPath = Path.Combine(baseDir, "POSPRA-WinFormsUI.dll.config");
            }

            // 3?? Check existence before launching form
            //if (!File.Exists(configPath))
            //{
            //    MessageBox.Show($"Config file not found:\n{configPath}",
            //        "Configuration", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}

            ApplicationConfiguration.Initialize();
            Application.Run(new ConfigForm(configPath)); // ? pass FULL PATH directly
        }
    }
}
