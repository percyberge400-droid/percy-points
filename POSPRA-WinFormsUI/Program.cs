using Microsoft.Extensions.DependencyInjection;
using POSPRA.Infrastructure.Data;
using POSPRA_WinFormsUI.Forms;
using System.Drawing.Text;
//using PRA_POS.Services;

namespace POSPRA_WinFormsUI
{
    internal static class Program
    {
        private static PrivateFontCollection privateFonts;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Ensure database exists
            DbInitializer.Initialize();
            // Setup DI
            var services = new ServiceCollection();



            // Register your service manager
            // services.AddSingleton<IWindowsServiceManager>(sp => new WindowsServiceManager("IMS_Fiscalization"));

            // Register your forms with DI
            services.AddTransient<LoginForm>();
            //services.AddTransient<DashboardForm>();
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();


            using (var provider = services.BuildServiceProvider())
            {
                Application.EnableVisualStyles();

                var loginForm = provider.GetRequiredService<LoginForm>();
                Application.Run(loginForm);
            }
        }

    }
}
