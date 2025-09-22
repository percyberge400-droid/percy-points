using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace POSPRA_WinFormsUI.AlertClasses
{
    public enum AlertType
    {
        Success,
        Error,
        Warning,
        Info,
        Critical,
        Update,
        Custom
    }

    public class AlertManager : Form
    {
        // ----- Static stack for stacking multiple alerts -----
        private static readonly List<AlertManager> ActiveAlerts = new List<AlertManager>();

        private static readonly object LockObj = new object();

        // ----- Controls & visual fields -----
        private readonly Label messageLabel;
        private readonly PictureBox iconBox;
        private readonly System.Windows.Forms.Timer fadeInTimer; //Timer -> System.Windows.Forms.Timer
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
        private Label label1;
        private const int BottomOffset = 20;

        // ------------------ Constructor ------------------
        public AlertManager(string message, AlertType type = AlertType.Info, int duration = 3000)
        {

            // base form settings
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.Size = new Size(AlertWidth, AlertHeight);
            this.Opacity = 0; // start invisible
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.DoubleBuffered = true;

            duration = Math.Max(2000, Math.Min(duration, 3000)); // clamp between 2–3 sec
            this.displayTime = duration;
            // timers
            fadeInTimer = new System.Windows.Forms.Timer { Interval = 20 };
            fadeInTimer.Tick += FadeInTimer_Tick;

            lifeTimer = new System.Windows.Forms.Timer { Interval = duration };
            lifeTimer.Tick += LifeTimer_Tick;

            fadeOutTimer = new System.Windows.Forms.Timer { Interval = 20 };
            fadeOutTimer.Tick += FadeOutTimer_Tick;

            slideTimer = new System.Windows.Forms.Timer { Interval = 15 };
            slideTimer.Tick += SlideTimer_Tick;
            // configure visuals for this alert type
            this.displayTime = duration;
            ConfigureStyle(type);

            // controls
            iconBox = new PictureBox
            {
                Size = new Size(40, 40),
                Location = new Point(18, (AlertHeight - 40) / 2),
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
                BackColor = Color.Transparent
            };

            this.Controls.Add(iconBox);
            this.Controls.Add(messageLabel);

            // custom paint for rounded corners + subtle border
            this.Paint += AlertManager_Paint;

            // set region (rounded) so form shape matches drawn area
            this.Region = new Region(GetRoundedPath(new Rectangle(0, 0, this.Width, this.Height), CornerRadius));
        }

        // ------------------ Public static helpers ------------------
        public static void ShowMessage(string msg, AlertType type, int duration = 3000)
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
        public static void ShowCustom(string msg, Image icon, Color bg, Color fg, int duration = 3000)
        {
            var alert = new AlertManager(msg, AlertType.Custom, duration);
            alert.iconBox.Image = icon;
            alert.BackColor = bg;
            alert.messageLabel.ForeColor = fg;
            alert.ShowAlert();
        }

        // ------------------ Lifecycle / animation ------------------
        private const int MaxVisibleAlerts = 3;
        public void ShowAlert()
        {
            lock (LockObj)
            {
                if (ActiveAlerts.Count >= MaxVisibleAlerts)
                {
                    ActiveAlerts[0].Close(); // remove oldest
                }

                ActiveAlerts.Add(this);
                RepositionAlerts(); // 🔹 position before showing
            }

            //var loc = this.Location;
            //targetX = loc.X;

            var workingArea = Screen.PrimaryScreen.WorkingArea;
            targetX = workingArea.Right - this.Width - RightOffset;

            // Start position: off-screen to the right
            this.Location = new Point(Screen.PrimaryScreen.WorkingArea.Right, workingArea.Y);

            this.Opacity = 0;     // start invisible
            this.Show();          // make window handle
            this.BringToFront();  // ensure on top

            // Start animations
            slideTimer.Start();
            fadeInTimer.Start();
            lifeTimer.Start();
        }

        private void SlideTimer_Tick(object sender, EventArgs e)
        {
            if (this.Location.X > targetX)
            {
                this.Location = new Point(this.Location.X - 20, this.Location.Y); // slide speed
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

            // remove from active list and reposition remaining alerts
            if (ActiveAlerts.Contains(this))
                ActiveAlerts.Remove(this);

            RepositionAlerts();
        }
        private const int TopOffset = 20;
        // Repositions all alerts in stack (bottom-right upwards)
        private static void RepositionAlerts()
        {
            if (ActiveAlerts.Count == 0) return;

            var workingArea = Screen.PrimaryScreen.WorkingArea;

            // Work on a copy to avoid collection modified issues
            var alertsCopy = new List<AlertManager>(ActiveAlerts);
            alertsCopy.Reverse();

            for (int i = 0; i < alertsCopy.Count; i++)
            {
                var a = alertsCopy[i];

                // 🔹 Skip if closed/disposed
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
                    catch (ObjectDisposedException) { /* already closed */ }
                    catch (InvalidOperationException) { /* already closed */ }
                }
                else
                {
                    a.Location = new Point(x, y);
                }
            }


        }

        // ------------------ Style configuration ------------------
        private void ConfigureStyle(AlertType type)
        {
            // default safe values
            backColor = Color.White;
            foreColor = Color.Black;
            iconImage = SystemIcons.Application.ToBitmap();

            switch (type)
            {
                case AlertType.Success:
                    backColor = Color.FromArgb(220, 248, 230);
                    foreColor = Color.FromArgb(0, 100, 40);
                    iconImage = SystemIcons.Information.ToBitmap();
                    break;
                case AlertType.Error:
                    backColor = Color.FromArgb(255, 230, 230);
                    foreColor = Color.FromArgb(160, 0, 0);
                    iconImage = SystemIcons.Error.ToBitmap();
                    break;
                case AlertType.Warning:
                    backColor = Color.FromArgb(255, 245, 210);
                    foreColor = Color.FromArgb(140, 90, 0);
                    iconImage = SystemIcons.Warning.ToBitmap();
                    break;
                case AlertType.Info:
                    backColor = Color.FromArgb(225, 240, 255);
                    foreColor = Color.FromArgb(0, 70, 140);
                    iconImage = SystemIcons.Information.ToBitmap();
                    break;
                case AlertType.Critical:
                    backColor = Color.FromArgb(255, 200, 200);
                    foreColor = Color.FromArgb(120, 0, 0);
                    iconImage = SystemIcons.Error.ToBitmap();
                    break;
                case AlertType.Update:
                    backColor = Color.FromArgb(230, 230, 250);
                    foreColor = Color.FromArgb(50, 0, 120);
                    iconImage = SystemIcons.Application.ToBitmap();
                    break;
                case AlertType.Custom:
                default:
                    backColor = Color.White;
                    foreColor = Color.Black;
                    iconImage = SystemIcons.Question.ToBitmap();
                    break;
            }

            // apply to controls / form
            this.BackColor = backColor;
            messageLabel?.BackColor.Equals(Color.Transparent); // keep label transparent
            if (messageLabel != null) messageLabel.ForeColor = foreColor;
            if (iconBox != null && iconImage != null) iconBox.Image = iconImage;
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

                // subtle border
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

            // If radius is zero, return rectangle
            if (radius <= 0)
            {
                path.AddRectangle(rect);
                path.CloseFigure();
                return path;
            }

            // corners
            path.AddArc(rect.Left, rect.Top, diameter, diameter, 180, 90); // top-left
            path.AddArc(rect.Right - diameter, rect.Top, diameter, diameter, 270, 90); // top-right
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90); // bottom-right
            path.AddArc(rect.Left, rect.Bottom - diameter, diameter, diameter, 90, 90); // bottom-left
            path.CloseFigure();

            return path;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // AlertManager
            // 
            this.ClientSize = new System.Drawing.Size(397, 314);
            this.Name = "AlertManager";
            this.Load += new System.EventHandler(this.AlertManager_Load);
            this.ResumeLayout(false);

        }

        private void AlertManager_Load(object sender, EventArgs e)
        {

        }

        //private void InitializeComponent()
        //{
        //    this.label1 = new System.Windows.Forms.Label();
        //    this.SuspendLayout();
        //    // 
        //    // label1
        //    // 
        //    this.label1.AutoSize = true;
        //    this.label1.Location = new System.Drawing.Point(213, 171);
        //    this.label1.Name = "label1";
        //    this.label1.Size = new System.Drawing.Size(44, 16);
        //    this.label1.TabIndex = 0;
        //    this.label1.Text = "label1";
        //    // 
        //    // AlertManager
        //    // 
        //    this.ClientSize = new System.Drawing.Size(616, 544);
        //    this.Controls.Add(this.label1);
        //    this.Name = "AlertManager";
        //    this.Load += new System.EventHandler(this.AlertManager_Load);
        //    this.ResumeLayout(false);
        //    this.PerformLayout();

        //}

        //private void AlertManager_Load(object sender, EventArgs e)
        //{

        //}
    }
}
