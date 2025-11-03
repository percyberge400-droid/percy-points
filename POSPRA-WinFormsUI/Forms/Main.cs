using Microsoft.Extensions.DependencyInjection;
using POSPRA.Application.Services.LogService;
using POSPRA.Application.Utility;
using POSPRA.Domain.Entities;
using POSPRA.SecurityEncryption;
using POSPRA_WinFormsUI.AlertClasses;
using System.Configuration;
using System.Drawing.Drawing2D;
using System.Net.NetworkInformation;
using System.ServiceProcess;
using AlertType = POSPRA.Application.Utility.AlertType;

namespace POSPRA_WinFormsUI.Forms
{
    public partial class Main : Form
    {
        private readonly IServiceProvider _provider;
        private readonly ILogService _logService;
        private CancellationTokenSource _internetCheckCts;
        private CancellationTokenSource _workerServiceCts;
        private readonly List<Form> _independentForms = new();
        private DateTime? offlineSince = null;
        private bool? wasOnline = null;
        private DateTime lastOfflineAlertTime = DateTime.MinValue;
        private bool _workerServiceAlertShown = false;
        private readonly string _baseUrl;

        // 🎨 Animation tracking for status badges
        private int _internetPulseFrame = 0;
        private int _posPulseFrame = 0;
        private System.Windows.Forms.Timer _animationTimer;
        private long decryptedPosId;

        public Main(IServiceProvider provider, ILogService logService)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
            InitializeComponent();

            this.IsMdiContainer = true;
            panInvoiceSelection.Visible = false;
            panExportInvoice.Visible = false;
            panCatalogView.Visible = false;
            btnDashboard.ForeColor = ColorTranslator.FromHtml("#48A787");
            Form childForm = _provider.GetRequiredService<DashboardForm>();
            childForm.MdiParent = this;
            childForm.Dock = DockStyle.Fill;
            childForm.Show();
            this.Resize += Main_Resize;
            _baseUrl = ConfigurationManager.AppSettings["BaseUrl"];

            // 🚀 Initialize catchy status system
            InitializeStatusSystem();

            string posCOMP = ConfigurationManager.AppSettings["posCOMP"];
            if (!string.IsNullOrEmpty(posCOMP))
            {
                var res = Resources.ResourceManager.GetObject(posCOMP);
                if (res is Image img)
                {
                    pictureBox2.Image = img;
                }
            }
            var encryptedPosId = ConfigurationManager.AppSettings["Username"] ?? "0";
            decryptedPosId = Convert.ToInt64(AesEncryptionHelper.Decrypt(encryptedPosId));
        }

        private void InitializeStatusSystem()
        {
            // First set the background color before styling
            panel2.BackColor = Color.White;

            StyleStatusPanel();
            StartInternetStatusChecker();
            StartWorkerServiceStatusChecker();
            StartStatusAnimations();

            posStatus.Font = new Font("Segoe UI", 9.8f, FontStyle.Bold);
            internetStatus.Font = new Font("Segoe UI", 9.8f, FontStyle.Bold);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            _animationTimer?.Stop();
            _animationTimer?.Dispose();
            StopInternetStatusChecker();
            StopWorkerServiceStatusChecker();
            foreach (var form in _independentForms.ToArray())
            {
                if (form != null && !form.IsDisposed)
                {
                    form.Close();
                }
            }
        }

        private void StyleStatusPanel()
        {
            panel2.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                // Glossy gradient background
                using (var brush = new LinearGradientBrush(
                    panel2.ClientRectangle,
                    Color.White,
                    Color.White,
                    LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(brush, panel2.ClientRectangle);
                }

                // Subtle top highlight
                using (var highlight = new LinearGradientBrush(
                    new Rectangle(0, 0, panel2.Width, 20),
                    Color.FromArgb(60, 255, 255, 255),
                    Color.Transparent,
                    LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(highlight, 0, 0, panel2.Width, 20);
                }

                // Bottom shadow line
                using (Pen shadow = new Pen(Color.FromArgb(40, 0, 0, 0), 1))
                    e.Graphics.DrawLine(shadow, 0, panel2.Height - 1, panel2.Width, panel2.Height - 1);
            };

            // Disable AutoSize and Anchor for manual positioning
            lblNetworkStatus.AutoSize = true;
            lblNetworkStatus.Anchor = AnchorStyles.None;
            internetStatus.AutoSize = false;
            internetStatus.Anchor = AnchorStyles.None;
            lblWorkerService.AutoSize = true;
            lblWorkerService.Anchor = AnchorStyles.None;
            posStatus.AutoSize = false;
            posStatus.Anchor = AnchorStyles.None;

            // Style the status badges first (this sets their size)
            StyleStatusBadge(internetStatus, false);
            StyleStatusBadge(posStatus, false);

            // Calculate total width needed (with padding)
            int totalWidth = lblNetworkStatus.Width + 10 + internetStatus.Width + 30 +
                           lblWorkerService.Width + 10 + posStatus.Width + 60; // Extra padding

            // Resize panel2 to fit content with margins
            panel2.Width = Math.Max(totalWidth, 550); // Minimum width of 550

            // Reposition panel2 to stay anchored to the right
            panel2.Location = new Point(panel1.Width - panel2.Width, 0);

            // Center the entire group in panel2
            int startX = (panel2.Width - (totalWidth - 60)) / 2; // Subtract extra padding for centering
            int centerY = (panel2.Height - lblNetworkStatus.Height) / 2;

            // Position all elements
            lblNetworkStatus.Location = new Point(startX, centerY);
            internetStatus.Location = new Point(lblNetworkStatus.Right + 10,
                                                (panel2.Height - internetStatus.Height) / 2);
            lblWorkerService.Location = new Point(internetStatus.Right + 30, centerY);
            posStatus.Location = new Point(lblWorkerService.Right + 10,
                                          (panel2.Height - posStatus.Height) / 2);
        }

        private void StyleStatusBadge(Label lbl, bool isActive)
        {
            lbl.AutoSize = false;
            lbl.TextAlign = ContentAlignment.MiddleCenter;
            lbl.Font = new Font("Segoe UI", 9.8f, FontStyle.Bold);
            lbl.Size = new Size(110, 32);
            lbl.Region = new Region(CreateRoundRectPath(new Rectangle(0, 0, lbl.Width, lbl.Height), 12));

            // Store which pulse frame to use based on label
            lbl.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle rect = new Rectangle(0, 0, lbl.Width - 1, lbl.Height - 1);

                // Get the appropriate pulse frame
                int currentPulseFrame = (lbl == internetStatus) ? _internetPulseFrame : _posPulseFrame;

                // Determine colors based on status
                Color bgColor, glowColor, dotColor;
                if (isActive)
                {
                    bgColor = Color.FromArgb(72, 167, 135);
                    glowColor = Color.FromArgb(187, 247, 208);
                    dotColor = Color.White;
                }
                else
                {
                    bgColor = Color.FromArgb(220, 53, 69); // Red for offline/inactive
                    glowColor = Color.FromArgb(248, 215, 218);
                    dotColor = Color.White;
                }

                // Gradient background
                using (var bgBrush = new LinearGradientBrush(
                    rect,
                    bgColor,
                    Color.FromArgb(Math.Max(0, bgColor.R - 20), Math.Max(0, bgColor.G - 20), Math.Max(0, bgColor.B - 20)),
                    LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRoundedRectangle(bgBrush, rect, 6);
                }

                // Animated pulse effect for both states
                float pulseAlpha = (float)(Math.Sin(currentPulseFrame * 0.1) * 0.3 + 0.7);
                using (Pen pulsePen = new Pen(Color.FromArgb((int)(pulseAlpha * 150), glowColor), 2))
                {
                    e.Graphics.DrawRoundedRectangle(pulsePen, rect, 12);
                }

                // Animated status dot with glow (both states get glow)
                int dotSize = isActive ? 8 : 8;
                int dotX = 8;
                int dotY = lbl.Height / 2 - dotSize / 2;

                // Glow effect for both active and inactive states
                float glowIntensity = (float)(Math.Sin(currentPulseFrame * 0.15) * 0.4 + 0.6);
                using (var glowBrush = new SolidBrush(Color.FromArgb((int)(glowIntensity * 80), dotColor)))
                {
                    e.Graphics.FillEllipse(glowBrush, dotX - 3, dotY - 3, dotSize + 6, dotSize + 6);
                }

                // Main dot
                using (var dotBrush = new LinearGradientBrush(
                    new Rectangle(dotX, dotY, dotSize, dotSize),
                    dotColor,
                    Color.White,
                    LinearGradientMode.Vertical))
                {
                    e.Graphics.FillEllipse(dotBrush, dotX, dotY, dotSize, dotSize);
                }

                // Text with subtle shadow
                string statusText = lbl.Text.Replace("●", "").Trim();
                using (var shadowBrush = new SolidBrush(Color.FromArgb(40, 0, 0, 0)))
                {
                    TextRenderer.DrawText(
                        e.Graphics,
                        statusText,
                        lbl.Font,
                        new Rectangle(21, 1, lbl.Width - 25, lbl.Height),
                        Color.FromArgb(40, 0, 0, 0),
                        TextFormatFlags.VerticalCenter | TextFormatFlags.Left
                    );
                }

                TextRenderer.DrawText(
                    e.Graphics,
                    statusText,
                    lbl.Font,
                    new Rectangle(20, 0, lbl.Width - 24, lbl.Height),
                    Color.White,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left
                );
            };
        }
        private void StartStatusAnimations()
        {
            _animationTimer = new System.Windows.Forms.Timer { Interval = 50 };
            _animationTimer.Tick += (s, e) =>
            {
                _internetPulseFrame++;
                _posPulseFrame++;

                if (internetStatus?.IsHandleCreated == true)
                    internetStatus.Invalidate();
                if (posStatus?.IsHandleCreated == true)
                    posStatus.Invalidate();
            };
            _animationTimer.Start();
        }

        private void UpdateStatusBadge(Label lbl, bool isActive, string text)
        {
            if (lbl == null || !lbl.IsHandleCreated) return;

            lbl.BeginInvoke(new Action(() =>
            {
                lbl.Text = text;
                StyleStatusBadge(lbl, isActive);
                lbl.Invalidate();
            }));
        }

        private void Main_Resize(object sender, EventArgs e) => HandleFormStateChange();

        private void HandleFormStateChange()
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                foreach (var form in _independentForms)
                {
                    if (form != null && !form.IsDisposed && form.Visible)
                    {
                        form.Hide();
                        form.Tag = "was_visible";
                    }
                }
            }
            else if (this.WindowState == FormWindowState.Normal || this.WindowState == FormWindowState.Maximized)
            {
                foreach (var form in _independentForms)
                {
                    if (form != null && !form.IsDisposed && form.Tag?.ToString() == "was_visible")
                    {
                        form.Show();
                        form.BringToFront();
                        form.Tag = null;
                    }
                }
            }
        }

        // CATCHY INTERNET STATUS CHECKER
        private void StartInternetStatusChecker()
        {
            _internetCheckCts = new();
            CancellationToken ct = _internetCheckCts.Token;

            _ = Task.Run(async () =>
            {
                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        bool online = await CheckInternetConnectivityAsync();

                        UpdateStatusBadge(internetStatus, online, online ? "Online" : "Offline");

                        if (wasOnline != null && wasOnline != online)
                        {
                            if (!online)
                            {
                                offlineSince = DateTime.Now;
                                ShowAlert("Connection Lost! Going offline...", nameof(AlertType.Error), true, "Internet Alert");
                                _ = CreateLog("Internet connection lost", AlertType.Error);
                                lastOfflineAlertTime = DateTime.Now;
                            }
                            else
                            {
                                string downtimeMsg = "";
                                if (offlineSince.HasValue)
                                {
                                    TimeSpan downTime = DateTime.Now - offlineSince.Value;
                                    downtimeMsg = $" Reconnected after {downTime.TotalSeconds:F0}s";
                                }
                                ShowAlert($"Back Online!{downtimeMsg}", nameof(AlertType.Success), true, "Internet Restored");
                                _ = CreateLog("Internet connection restored" + downtimeMsg, AlertType.Success);
                                offlineSince = null;
                            }
                        }
                        else if (!online)
                        {
                            if ((DateTime.Now - lastOfflineAlertTime).TotalSeconds >= 30)
                            {
                                TimeSpan downTime = offlineSince.HasValue ? DateTime.Now - offlineSince.Value : TimeSpan.Zero;
                                ShowAlert($"Still Offline ({downTime.TotalSeconds:F0}s)", nameof(AlertType.Error), false, "Connection Status");
                                lastOfflineAlertTime = DateTime.Now;
                            }
                        }

                        wasOnline = online;
                        await Task.Delay(5000, ct);
                    }
                    catch (TaskCanceledException) { break; }
                }
            }, ct);
        }

        private async Task<bool> CheckInternetConnectivityAsync()
        {
            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync("8.8.8.8", 2000);
                return reply.Status == IPStatus.Success;
            }
            catch { return false; }
        }

        private void StopInternetStatusChecker()
        {
            _internetCheckCts?.Cancel();
            _internetCheckCts?.Dispose();
        }

        // CATCHY POS SERVICE STATUS CHECKER
        private void StartWorkerServiceStatusChecker()
        {
            _workerServiceCts = new();
            CancellationToken ct = _workerServiceCts.Token;

            _ = Task.Run(async () =>
            {
                bool wasRunning = true;
                bool wasEnabled = true; // initially assume enabled
                int consecutiveChecks = 0;
                bool internetStatus = false;

                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        internetStatus = await CheckInternetConnectivityAsync();
                        if (internetStatus)
                        {
                            var fullUrl = $"{_baseUrl}{Endpoints.IsServiceEnabled}?posId={decryptedPosId}";
                            bool isEnabled = true;

                            using (var httpClient = new HttpClient())
                            {
                                try
                                {
                                    var response = await httpClient.GetAsync(fullUrl);
                                    response.EnsureSuccessStatusCode();

                                    string result = await response.Content.ReadAsStringAsync();
                                    isEnabled = bool.TryParse(result, out bool parsedValue) && parsedValue;

                                    // Trigger only when status changes from enabled → disabled
                                    if (wasEnabled && !isEnabled)
                                    {

                                        ShowAlert("POS Service Disabled, Contact FBR!", AlertType.Warning, true);
                                        _ = CreateLog("POS Service Disabled, Contact FBR!", AlertType.Warning);

                                        if (await IsWorkerServiceRunningAsync())
                                        {
                                            bool stopped = await StopWorkerServiceAsync();
                                        }

                                    }
                                    // Trigger when service comes back online
                                    else if (!wasEnabled && isEnabled)
                                    {
                                        _ = CreateLog("POS Service Enabled.", AlertType.Info);
                                        ShowAlert("POS Service Enabled.", AlertType.Info, true);

                                        if (!await IsWorkerServiceRunningAsync())
                                        {
                                            bool started = await StartWorkerServiceAsync();
                                        }
                                    }

                                    wasEnabled = isEnabled;
                                }
                                catch (Exception ex)
                                {
                                    _ = CreateLog($"Error calling API", AlertType.Error);
                                }
                            }
                        }

                        // --- Worker Service state handling ---
                        bool isRunning = await IsWorkerServiceRunningAsync();
                        UpdateStatusBadge(posStatus, isRunning, isRunning ? "Active" : "Inactive");

                        if (!isRunning && wasRunning)
                        {
                            _ = CreateLog("POS service is inactive!", AlertType.Warning);
                            _workerServiceAlertShown = true;
                            consecutiveChecks = 0;
                        }
                        else if (isRunning && !wasRunning)
                        {
                            _ = CreateLog("POS service restored!", AlertType.Success);
                            _workerServiceAlertShown = false;
                            consecutiveChecks = 0;
                        }
                        else if (!isRunning)
                        {
                            consecutiveChecks++;

                            // Remind every 5 checks (25 seconds)
                            if (consecutiveChecks % 5 == 0)
                            {
                                ShowAlert($"POS Service still inactive ({consecutiveChecks * 5}s)",
                                          nameof(AlertType.Warning), false, "Service Monitor");
                            }
                        }

                        wasRunning = isRunning;
                        await Task.Delay(5000, ct);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _ = CreateLog($"Error checking POS Service status: {ex.Message}", AlertType.Error);
                        await Task.Delay(5000, ct);
                    }
                }
            }, ct);
        }

        private Task<bool> IsWorkerServiceRunningAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    using var sc = new ServiceController("POSPRAWorker");
                    return sc.Status == ServiceControllerStatus.Running;
                }
                catch { return false; }
            });
        }

        private async Task<bool> StopWorkerServiceAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var sc = new ServiceController("POSPRAWorker");

                    if (sc.Status == ServiceControllerStatus.Running)
                    {
                        sc.Stop(); // Request stop
                        sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30)); // Wait up to 30s
                    }

                    return sc.Status == ServiceControllerStatus.Stopped;
                }
                catch (Exception ex)
                {
                    // Optionally log the exception
                    Console.WriteLine($"Error stopping service: {ex.Message}");
                    return false;
                }
            });
        }

        private async Task<bool> StartWorkerServiceAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var sc = new ServiceController("POSPRAWorker");

                    // Start only if not already running
                    if (sc.Status != ServiceControllerStatus.Running)
                    {
                        sc.Start();
                        sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30)); // wait up to 30 seconds
                    }

                    return sc.Status == ServiceControllerStatus.Running;
                }
                catch (Exception ex)
                {
                    // Optionally log or show error
                    Console.WriteLine($"Error starting service: {ex.Message}");
                    return false;
                }
            });
        }



        private void StopWorkerServiceStatusChecker()
        {
            _workerServiceCts?.Cancel();
            _workerServiceCts?.Dispose();
        }

        private async Task CreateLog(string message, string type)
        {
            var log = new Logs { Message = message, Type = type };
            await _logService.CreateLogAsync(log);
        }

        private void ShowAlert(string message, string alertType, bool showWindowsNotification, string title = "Alert")
        {
            if (!this.IsHandleCreated) return;
            this.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (showWindowsNotification)
                        WindowsLocalAppNotification.Show(title, message);
                    if (alertType == nameof(AlertType.Error))
                        AlertManager.ShowError(message);
                    else if (alertType == nameof(AlertType.Success))
                        AlertManager.ShowSuccess(message);
                }
                catch
                {
                    MessageBox.Show(this, message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }));
        }

        private void ResetNavStyles()
        {
            btnDashboard.ForeColor = Color.Black;
            btnInvoiceSelection.ForeColor = Color.Black;
            btnExportInvoice.ForeColor = Color.Black;
            btnCatalogView.ForeColor = Color.Black;
            panDashboard.Visible = false;
            panInvoiceSelection.Visible = false;
            panExportInvoice.Visible = false;
            panCatalogView.Visible = false;
        }

        private void btnDashboard_Click(object sender, EventArgs e)
        {
            ResetNavStyles();
            btnDashboard.ForeColor = ColorTranslator.FromHtml("#48A787");
            panDashboard.Visible = true;
            LoadView("Dashboard");
        }

        private void btnInvoiceSelection_Click(object sender, EventArgs e)
        {
            ResetNavStyles();
            btnInvoiceSelection.ForeColor = ColorTranslator.FromHtml("#48A787");
            panInvoiceSelection.Visible = true;
            LoadView("Invoice Entry");
        }

        private void btnExportInvoice_Click(object sender, EventArgs e)
        {
            ResetNavStyles();
            btnExportInvoice.ForeColor = ColorTranslator.FromHtml("#48A787");
            panExportInvoice.Visible = true;
            LoadView("Export Invoice");
        }

        private void btnCatalogView_Click(object sender, EventArgs e)
        {
            ResetNavStyles();
            btnCatalogView.ForeColor = ColorTranslator.FromHtml("#48A787");
            panCatalogView.Visible = true;
            LoadView("Catalog View");
        }

        public void LoadView(string viewName)
        {
            if (this.ActiveMdiChild != null)
            {
                this.ActiveMdiChild.Close();
                // this.ActiveMdiChild.Dispose();
            }
            Form childForm = viewName switch
            {
                "Dashboard" => _provider.GetRequiredService<DashboardForm>(),
                "Invoice Entry" => _provider.GetRequiredService<ItemEntry>(),
                "Export Invoice" => _provider.GetRequiredService<ExportInvoiceForm>(),
                "Catalog View" => _provider.GetRequiredService<CatalogView>(),
                _ => throw new NotImplementedException()
            };
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.MdiParent = this;
            childForm.Dock = DockStyle.Fill;
            childForm.Show();
        }

        // 🔹 CHANGED TO PUBLIC for extension methods
        public static GraphicsPath CreateRoundRectPath(Rectangle rect, int radius)
        {
            int d = radius * 2;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    public static class GraphicsExtensions
    {
        public static void DrawRoundedRectangle(this Graphics g, Pen pen, Rectangle rect, int radius)
        {
            using var path = Main.CreateRoundRectPath(rect, radius);
            g.DrawPath(pen, path);
        }

        public static void FillRoundedRectangle(this Graphics g, Brush brush, Rectangle rect, int radius)
        {
            using var path = Main.CreateRoundRectPath(rect, radius);
            g.FillPath(brush, path);
        }
    }
}