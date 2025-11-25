using System;
using System.Drawing;
using System.Windows.Forms;

namespace POSPRA_WinFormsUI.AlertClasses
{
    internal class WindowsLocalAppNotification
    {
        private static readonly NotifyIcon notifyIcon;

        static WindowsLocalAppNotification()
        {
            notifyIcon = new NotifyIcon
            {
                Visible = true,
                Icon = SystemIcons.Information,
                Text = "POSPRA Application"
            };
        }

        /// <summary>
        /// Show a notification using system tray balloon tip.
        /// </summary>
        public static void Show(string title, string message, int duration = 30000)
        {
            ShowNotification(title, message, ToolTipIcon.Info, duration);
        }

        /// <summary>
        /// Show error notification.
        /// </summary>
        public static void ShowError(string title, string message, int duration = 5000)
        {
            ShowNotification(title, message, ToolTipIcon.Error, duration);
        }

        /// <summary>
        /// Show warning notification.
        /// </summary>
        public static void ShowWarning(string title, string message, int duration = 5000)
        {
            ShowNotification(title, message, ToolTipIcon.Warning, duration);
        }

        /// <summary>
        /// Show success notification.
        /// </summary>
        public static void ShowSuccess(string title, string message, int duration = 5000)
        {
            ShowNotification(title, message, ToolTipIcon.Info, duration);
        }

        private static void ShowNotification(string title, string message, ToolTipIcon icon, int duration)
        {
            notifyIcon.BalloonTipTitle = title;
            notifyIcon.BalloonTipText = message;
            notifyIcon.BalloonTipIcon = icon;
            notifyIcon.ShowBalloonTip(duration);
        }

        public static void Dispose()
        {
            notifyIcon?.Dispose();
        }
    }
}