namespace POSPRA.Launcher
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            LauncherSilent.RunAsync().Wait();
        }
    }
}
