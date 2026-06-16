using System.Runtime.InteropServices;

namespace Pos.WinFormsUI.Helpers
{
    public class NoTodayDatePicker : DateTimePicker
    {
        private const uint DTM_GETMONTHCAL = 0x1008;
        private const uint MCM_SETCURRENTVIEW = 0x100C;
        private const uint MCS_NOTODAY = 0x0010;

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        protected override void OnDropDown(EventArgs eventargs)
        {
            base.OnDropDown(eventargs);

            // Get the handle to the embedded MonthCalendar popup
            IntPtr hMonthCal = SendMessage(Handle, DTM_GETMONTHCAL, IntPtr.Zero, IntPtr.Zero);
            if (hMonthCal != IntPtr.Zero)
            {
                const int GWL_STYLE = -16;
                int style = GetWindowLong(hMonthCal, GWL_STYLE);
                // Apply MCS_NOTODAY to remove the Today footer entirely
                SetWindowLong(hMonthCal, GWL_STYLE, style | (int)MCS_NOTODAY);
            }
        }
    }
}
