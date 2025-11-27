using System;
using System.IO;

namespace POSPRA.Worker.Configurations
{
    public static class FileLogger
    {
        private static readonly string LogFilePath =
            Path.Combine(AppContext.BaseDirectory, "WorkerServiceLog.txt");

        public static void Log(string message)
        {
            try
            {
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";
                File.AppendAllText(LogFilePath, logEntry);
            }
            catch
            {
                // ignored (avoid crashes due to logging failure)
            }
        }

        public static void LogException(Exception ex, string? source = null)
        {
            string error = $"EXCEPTION {(source != null ? $"[{source}]" : "")}: {ex.Message}\n{ex.StackTrace}";
            Log(error);
        }
    }
}
