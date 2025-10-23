using POSPRA.Application.Utility;
using System.Drawing.Drawing2D;

namespace POSPRA_WinFormsUI.AlertClasses
{
    public class AlertManager : Form
    {
        // ----- Static queue for stacking multiple alerts (FIXED: List -> Queue) -----
        private static readonly Queue<AlertManager> ActiveAlerts = new Queue<AlertManager>();
        private static readonly object LockObj = new object();

        // ----- Cached icon bitmaps (FIXED: Static cache) -----
        private static Image _successIcon;
        private static Image _errorIcon;
        private static Image _warningIcon;
        private static Image _infoIcon;
        private static Image _criticalIcon;
        private static Image _updateIcon;

        // ----- Controls & visual fields -----
        private readonly Label messageLabel;
        private readonly PictureBox iconBox;
        private readonly System.Windows.Forms.Timer fadeInTimer;
        private readonly System.Windows.Forms.Timer lifeTimer;
        private readonly System.Windows.Forms.Timer fadeOutTimer;
        private readonly System.Windows.Forms.Timer slideTimer;
        private int targetX;
        private int displayTime;
        private Color backColor;
        private Color foreColor;
        private Image iconImage;

        // layout constants
        private const int AlertWidth = 300;
        private const int AlertHeight = 75;
        private const int CornerRadius = 14;
        private const int VerticalMargin = 10;
        private const int RightOffset = 20;
        private const int TopOffset = 20;
        private const int MaxVisibleAlerts = 3;

        // ------------------ Constructor ------------------
        public AlertManager(string message, string type = AlertType.Info, int duration = 3000)
        {
            // base form settings
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.Size = new Size(AlertWidth, AlertHeight);
            this.Opacity = 0;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.DoubleBuffered = true;

            duration = Math.Max(2000, Math.Min(duration, 3000));
            this.displayTime = duration;

            // FIXED: Optimized timer intervals (30ms = ~33 FPS)
            fadeInTimer = new System.Windows.Forms.Timer { Interval = 30 };
            fadeInTimer.Tick += FadeInTimer_Tick;

            lifeTimer = new System.Windows.Forms.Timer { Interval = duration };
            lifeTimer.Tick += LifeTimer_Tick;

            fadeOutTimer = new System.Windows.Forms.Timer { Interval = 30 };
            fadeOutTimer.Tick += FadeOutTimer_Tick;

            slideTimer = new System.Windows.Forms.Timer { Interval = 20 };
            slideTimer.Tick += SlideTimer_Tick;

            ConfigureStyle(type);

            // controls
            iconBox = new PictureBox
            {
                Size = new Size(40, 40),
                Location = new Point(18, ((AlertHeight - 40) / 2) + 5),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = iconImage,
                BackColor = Color.Transparent
            };

            messageLabel = new Label
            {
                AutoSize = false,
                Location = new Point(70, 12),
                Size = new Size(AlertWidth - 90, AlertHeight - 24),
                ForeColor = foreColor,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                Text = message,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent // FIXED: Proper assignment
            };

            this.Controls.Add(iconBox);
            this.Controls.Add(messageLabel);

            this.Paint += AlertManager_Paint;
            this.Region = new Region(GetRoundedPath(new Rectangle(0, 0, this.Width, this.Height), CornerRadius));
        }

        // ------------------ Public static helpers ------------------
        public static void ShowMessage(string msg, string type, int duration = 3000)
        {
            var alert = new AlertManager(msg, type, duration);
            alert.ShowAlert();
        }

        public static void ShowSuccess(string msg, int duration = 3000) => ShowMessage(msg, AlertType.Success, duration);
        public static void ShowError(string msg, int duration = 3000) => ShowMessage(msg, AlertType.Error, duration);
        public static void ShowWarning(string msg, int duration = 3000) => ShowMessage(msg, AlertType.Warning, duration);
        public static void ShowInfo(string msg, int duration = 3000) => ShowMessage(msg, AlertType.Info, duration);
        public static void ShowCritical(string msg, int duration = 3000) => ShowMessage(msg, AlertType.Critical, duration);
        public static void ShowUpdate(string msg, int duration = 3000) => ShowMessage(msg, AlertType.Update, duration);

        // ------------------ Lifecycle / animation ------------------
        public void ShowAlert()
        {
            AlertManager oldestAlert = null;

            // FIXED: Minimize lock scope to avoid deadlock with Invoke
            lock (LockObj)
            {
                if (ActiveAlerts.Count >= MaxVisibleAlerts)
                {
                    oldestAlert = ActiveAlerts.Dequeue(); // FIXED: Queue O(1) operation
                }
                ActiveAlerts.Enqueue(this);
            }

            // Close outside of lock to prevent deadlock
            if (oldestAlert != null)
            {
                try
                {
                    if (!oldestAlert.IsDisposed && oldestAlert.IsHandleCreated)
                    {
                        if (oldestAlert.InvokeRequired)
                            oldestAlert.Invoke(new Action(() => oldestAlert.Close()));
                        else
                            oldestAlert.Close();
                    }
                }
                catch (ObjectDisposedException) { }
                catch (InvalidOperationException) { }
            }

            // Reposition after lock
            RepositionAlerts();

            var workingArea = Screen.PrimaryScreen.WorkingArea;
            targetX = workingArea.Right - this.Width - RightOffset;

            this.Location = new Point(Screen.PrimaryScreen.WorkingArea.Right, workingArea.Top);
            this.Opacity = 0;
            this.Show();
            this.BringToFront();

            slideTimer.Start();
            fadeInTimer.Start();
            lifeTimer.Start();
        }

        private void SlideTimer_Tick(object sender, EventArgs e)
        {
            if (this.Location.X > targetX)
            {
                this.Location = new Point(this.Location.X - 20, this.Location.Y);
            }
            else
            {
                this.Location = new Point(targetX, this.Location.Y);
                slideTimer.Stop();
            }
        }

        private void FadeInTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (this.IsDisposed) return;

                if (this.Opacity < 1.0)
                    this.Opacity = Math.Min(1.0, this.Opacity + 0.08);
                else
                    fadeInTimer.Stop();
            }
            catch (ObjectDisposedException)
            {
                fadeInTimer.Stop();
            }
        }

        private void LifeTimer_Tick(object sender, EventArgs e)
        {
            lifeTimer.Stop();
            fadeOutTimer.Start();
        }

        private void FadeOutTimer_Tick(object sender, EventArgs e)
        {
            if (this.Opacity > 0)
                this.Opacity -= 0.08;
            else
            {
                fadeOutTimer.Stop();
                this.Close();
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);

            // FIXED: Remove from queue (convert to list temporarily)
            lock (LockObj)
            {
                var tempList = ActiveAlerts.ToList();
                tempList.Remove(this);
                ActiveAlerts.Clear();
                foreach (var alert in tempList)
                {
                    ActiveAlerts.Enqueue(alert);
                }
            }

            RepositionAlerts();
        }

        // FIXED: Reposition without holding lock
        private static void RepositionAlerts()
        {
            List<AlertManager> alertsCopy;

            lock (LockObj)
            {
                alertsCopy = ActiveAlerts.ToList();
            }

            if (alertsCopy.Count == 0) return;

            var workingArea = Screen.PrimaryScreen.WorkingArea;
            alertsCopy.Reverse();

            for (int i = 0; i < alertsCopy.Count; i++)
            {
                var a = alertsCopy[i];

                if (a.IsDisposed || !a.IsHandleCreated)
                    continue;

                int x = workingArea.Right - a.Width - RightOffset;
                int y = workingArea.Top + (i * (a.Height + VerticalMargin)) + TopOffset;

                if (a.InvokeRequired)
                {
                    try
                    {
                        a.Invoke((Action)(() => a.Location = new Point(x, y)));
                    }
                    catch (ObjectDisposedException) { }
                    catch (InvalidOperationException) { }
                }
                else
                {
                    a.Location = new Point(x, y);
                }
            }
        }

        // ------------------ Style configuration ------------------
        private void ConfigureStyle(string type)
        {
            // FIXED: Use cached icons
            InitializeIconCache();

            backColor = Color.White;
            foreColor = Color.Black;
            iconImage = _infoIcon;

            switch (type)
            {
                case AlertType.Success:
                    backColor = Color.FromArgb(220, 248, 230);
                    foreColor = Color.FromArgb(0, 100, 40);
                    iconImage = _successIcon;
                    break;
                case AlertType.Error:
                    backColor = Color.FromArgb(255, 230, 230);
                    foreColor = Color.FromArgb(160, 0, 0);
                    iconImage = _errorIcon;
                    break;
                case AlertType.Warning:
                    backColor = Color.FromArgb(255, 245, 210);
                    foreColor = Color.FromArgb(140, 90, 0);
                    iconImage = _warningIcon;
                    break;
                case AlertType.Info:
                    backColor = Color.FromArgb(225, 240, 255);
                    foreColor = Color.FromArgb(0, 70, 140);
                    iconImage = _infoIcon;
                    break;
                case AlertType.Critical:
                    backColor = Color.FromArgb(255, 200, 200);
                    foreColor = Color.FromArgb(120, 0, 0);
                    iconImage = _criticalIcon;
                    break;
                case AlertType.Update:
                    backColor = Color.FromArgb(230, 230, 250);
                    foreColor = Color.FromArgb(50, 0, 120);
                    iconImage = _updateIcon;
                    break;
                default:
                    iconImage = _infoIcon;
                    break;
            }

            this.BackColor = backColor;
            if (messageLabel != null) messageLabel.ForeColor = foreColor;
            if (iconBox != null && iconImage != null) iconBox.Image = iconImage;
        }

        // FIXED: Icon cache initialization
        private static void InitializeIconCache()
        {
            if (_successIcon == null)
            {
                _successIcon = SystemIcons.Information.ToBitmap();
                _errorIcon = SystemIcons.Error.ToBitmap();
                _warningIcon = SystemIcons.Warning.ToBitmap();
                _infoIcon = SystemIcons.Information.ToBitmap();
                _criticalIcon = SystemIcons.Error.ToBitmap();
                _updateIcon = SystemIcons.Application.ToBitmap();
            }
        }

        // ------------------ Painting helpers ------------------
        private void AlertManager_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            var rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            using (var path = GetRoundedPath(rect, CornerRadius))
            {
                using (var brush = new SolidBrush(backColor))
                {
                    e.Graphics.FillPath(brush, path);
                }

                using (var pen = new Pen(Color.FromArgb(200, foreColor)))
                {
                    pen.Width = 1;
                    e.Graphics.DrawPath(pen, path);
                }
            }
        }

        private static GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            int diameter = radius * 2;
            var path = new GraphicsPath();

            if (radius <= 0)
            {
                path.AddRectangle(rect);
                path.CloseFigure();
                return path;
            }

            path.AddArc(rect.Left, rect.Top, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Top, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.Left, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }

        // FIXED: Proper disposal of resources
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                fadeInTimer?.Stop();
                fadeInTimer?.Dispose();

                lifeTimer?.Stop();
                lifeTimer?.Dispose();

                fadeOutTimer?.Stop();
                fadeOutTimer?.Dispose();

                slideTimer?.Stop();
                slideTimer?.Dispose();

                // Don't dispose cached static icons
            }
            base.Dispose(disposing);
        }
    }
}