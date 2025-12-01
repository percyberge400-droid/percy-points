namespace Pos.Application.Utility
{
    public static class FileHelper
    {
        /// <summary>
        /// Get absolute wwwroot path for the project
        /// </summary>
        public static string GetWwwRootPath(string projectName = "Pos.Api")
        {
            // Step 1: Base runtime path (bin\Debug\net9.0 or published folder)
            string basePath = AppContext.BaseDirectory;

            // Step 2: Go up to project root (adjust number of ".." depending on your structure)
            string projectRoot = Path.GetFullPath(Path.Combine(basePath, "..", "..", "..", ".."));

            // Step 3: Combine with project name and wwwroot
            string wwwrootPath = Path.Combine(projectRoot, projectName, "wwwroot");

            if (!Directory.Exists(wwwrootPath))
                throw new DirectoryNotFoundException($"wwwroot folder not found at path: {wwwrootPath}");

            return wwwrootPath;
        }

        /// <summary>
        /// Read text file from wwwroot safely
        /// </summary>
        public static async Task<string> ReadFileFromWwwRootAsync(string fileName, string projectName = "Pos.Api")
        {
            string wwwroot = GetWwwRootPath(projectName);
            string filePath = Path.Combine(wwwroot, fileName);

            if (!File.Exists(filePath))
                return "File Not Found";

            return (await File.ReadAllTextAsync(filePath)).Trim();
        }
    }

}
