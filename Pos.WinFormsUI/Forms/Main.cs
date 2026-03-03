using Microsoft.Extensions.DependencyInjection;
using Pos.Application.DTOs.LogDTOs;
using Pos.Application.Services.LogService;
using Pos.Application.Utility;
using Pos.SecurityEncryption;
using Pos.WinFormsUI.AlertClasses;
using Pos.WinFormsUI.Dashboard;
using System.Configuration;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Net.Http.Json;
using System.Net.NetworkInformation;
using System.ServiceProcess;

namespace Pos.WinFormsUI.Forms
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
        private readonly string _jsonworkerpath;

        private string LocalFolder = "";

        // Use your actual API base URL here
        private const string ApiBaseUrl = "http://10.105.200.161/api/Configuration/";
        private const string ApiGetVersion = "get-update-version";

        // 🎨 Animation tracking for status badges
        private int _internetPulseFrame = 0;
        private int _posPulseFrame = 0;
        private int _environmentPulseFrame = 0; // New pulse frame for environment
        private System.Windows.Forms.Timer _animationTimer;
        private long decryptedPosId;
        private static System.Timers.Timer updateTimer;
        private bool updateCheckInProgress = false;

        public Main(IServiceProvider provider, ILogService logService)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
            InitializeComponent();
            StyleContextMenu();
            var cts = new CancellationTokenSource();
            StartUpdateWatcher(cts.Token);  // 👈 Start hidden checker

            productCatalogToolStripMenuItem.Click += productCatalogToolStripMenuItem_Click;
            uploadLogoToolStripMenuItem.Click += uploadLogoToolStripMenuItem_Click;
            btnProfile.Click += btnProfile_Click;
            panel2.Paint += Panel2_Paint;
            internetStatus.Paint += InternetStatus_Paint;
            posStatus.Paint += PosStatus_Paint;
            lblEnvironment.Paint += EnvironmentStatus_Paint; // Add paint handler for environment

            this.IsMdiContainer = true;
            panInvoiceSelection.Visible = false;
            panExportInvoice.Visible = false;
            panCatalogView.Visible = false;
            pnlProfile.Visible = false;
            btnDashboard.ForeColor = ColorTranslator.FromHtml("#48A787");
            Form childForm = _provider.GetRequiredService<DashboardFormNew>();
            childForm.MdiParent = this;
            childForm.Dock = DockStyle.Fill;
            childForm.Show();

            this.Resize += Main_Resize;
            _baseUrl = ConfigurationManager.AppSettings["BaseUrl"];
            string isProduction = ConfigurationManager.AppSettings["Environment"];
            updatelbl(isProduction);
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

        public void StartUpdateWatcher(CancellationToken cancellationToken)
        {
            // Run immediately
            _ = RunCheckForUpdateOnce(cancellationToken);

            // Setup timer for periodic checks
            updateTimer = new System.Timers.Timer(15000); // check every 5 sec
            updateTimer.AutoReset = true;

            updateTimer.Elapsed += async (s, e) =>
            {
                await RunCheckForUpdateOnce(cancellationToken);
            };

            updateTimer.Start();
        }

        // Wrapper to prevent overlapping calls
        private async Task RunCheckForUpdateOnce(CancellationToken cancellationToken)
        {
            if (updateCheckInProgress) return;

            updateCheckInProgress = true;
            try
            {
                await CheckForUpdate(cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Update check error: " + ex.Message);
            }
            finally
            {
                updateCheckInProgress = false;
            }
        }

        // Your existing CheckForUpdate method stays the same
        private async Task CheckForUpdate(CancellationToken cancellationToken)
        {
            string installPath = GetInstallPath();
            if (!installPath.EndsWith("\\")) installPath += "\\";

            string logFile = Path.Combine(installPath, "update-log.txt");
            void Log(string msg)
            {
                try { File.AppendAllText(logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {msg}\r\n"); }
                catch { }
            }

            try
            {
                string localVersionPath = Path.Combine(installPath, "app-version.txt");
                string fullLocalVersion = File.Exists(localVersionPath)
                    ? AesEncryptionHelper.Decrypt(File.ReadAllText(localVersionPath).Trim())
                    : "0.0.0,0,Date";

                Log($"Loaded local version: {fullLocalVersion}");

                var parts = fullLocalVersion.Split(',');
                string localVersion = parts.Length > 0 ? parts[0] : "0.0.0";
                string isUpdate = parts.Length > 1 ? parts[1] : "0";
                string updateDate = parts.Length > 2 ? parts[2] : DateTime.Now.ToString();

                // Fetch server version
                string apiBase = "http://10.105.200.161/api/Configuration/";
                string versionApi = $"{apiBase}get-update-version";

                string serverVersion;
                using (var client = new HttpClient())
                {
                    var response = await client.GetFromJsonAsync<ApiResponse<ConfigurationResponseDto>>(versionApi, cancellationToken);

                    if (response?.Data == null || string.IsNullOrWhiteSpace(response.Data.AppVersion))
                    {
                        Log("Server returned invalid version data.");
                        return;
                    }

                    serverVersion = response.Data.AppVersion.Trim();
                    Log($"Server version: {serverVersion}");
                }

                // Compare versions
                bool updateRequired = false;
                try
                {
                    var vLocal = Version.Parse(localVersion);
                    var vServer = Version.Parse(serverVersion);
                    if (vLocal < vServer) updateRequired = true;
                }
                catch
                {
                    if (localVersion != serverVersion) updateRequired = true;
                }

                if (!updateRequired)
                {
                    Log("No update required.");
                    return;
                }

                Log($"Update available: {localVersion} → {serverVersion}");

                // Mark pending update
                if (isUpdate != "1")
                {
                    string updatedFullVersion = $"{localVersion},1,{DateTime.Now}";
                    File.WriteAllText(localVersionPath, AesEncryptionHelper.Encrypt(updatedFullVersion));
                    Log("Marked update as pending in app-version.txt.");
                }
                else if (DateTime.TryParse(updateDate, out DateTime lastUpdate))
                {
                    if ((DateTime.Now - lastUpdate) > TimeSpan.FromSeconds(10))
                    {
                        string updaterExe = Path.Combine(installPath, "Pos.Updater.exe");
                        if (!File.Exists(updaterExe))
                        {
                            Log("Updater executable not found.");
                            return;
                        }

                        try
                        {
                            var psi = new ProcessStartInfo
                            {
                                FileName = updaterExe,
                                UseShellExecute = true,
                                // Remove: Verb = "runas",
                                WindowStyle = ProcessWindowStyle.Normal
                            };
                            Process.Start(psi);
                            Log("Updater launched successfully.");
                        }
                        catch (Exception ex)
                        {
                            Log($"Failed to launch updater: {ex.Message}");
                        }

                        string updatedFullVersion = $"{serverVersion},0,{DateTime.Now}";
                        File.WriteAllText(localVersionPath, AesEncryptionHelper.Encrypt(updatedFullVersion));
                        Log("Updated local version after launching updater.");
                        System.Windows.Forms.Application.Exit();
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Unexpected error: {ex.Message}");
            }
        }

        private string GetInstallPath()
        {
            string commonInfo = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "PRAL", "install_info.txt");

            string installPath = "";

            if (File.Exists(commonInfo))
            {
                foreach (var line in File.ReadAllLines(commonInfo))
                {
                    if (line.StartsWith("InstallPath=", StringComparison.OrdinalIgnoreCase))
                    {
                        installPath = line.Substring("InstallPath=".Length).Trim();
                        break;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(installPath) || !Directory.Exists(installPath))
            {
                var defaultPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    "PRAL", "POSComponent");

                installPath = Directory.Exists(defaultPath)
                    ? defaultPath
                    : AppDomain.CurrentDomain.BaseDirectory;
            }

            return installPath;
        }

        public class ConfigurationResponseDto
        {
            public string AppVersion { get; set; } = string.Empty;
        }

        public class ApiResponse<T>
        {
            public string StatusCode { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
            public T Data { get; set; } = default!;
            public string Errors { get; set; } = string.Empty;
        }

        private void InitializeStatusSystem()
        {
            panel2.BackColor = Color.White;

            // Initialize badges ONCE
            InitializeStatusBadges();

            // Setup panel styling ONCE
            StyleStatusPanel();

            // Start background tasks
            StartInternetStatusChecker();
            StartWorkerServiceStatusChecker();
            StartStatusAnimations();
        }
        private void updatelbl(string enviroment)
        {
            Color backgroundColor = Color.DarkGoldenrod; // Dark yellow for sandbox

            if (enviroment == "Production")
            {
                backgroundColor = ColorTranslator.FromHtml("#3E577D"); // light blue
            }

            if (lblEnvironment.InvokeRequired)
            {
                lblEnvironment.BeginInvoke(new Action(() =>
                {
                    lblEnvironment.Text = enviroment;
                    lblEnvironment.Tag = backgroundColor;
                    lblEnvironment.Invalidate();
                    RepositionStatusControls(); // Reposition after text update
                }));
            }
            else
            {
                lblEnvironment.Text = enviroment;
                lblEnvironment.Tag = backgroundColor;
                lblEnvironment.Invalidate();
                RepositionStatusControls(); // Reposition after text update
            }
        }
        private void EnvironmentStatus_Paint(object? sender, PaintEventArgs e)
        {
            PaintEnvironmentBadge(e.Graphics, lblEnvironment, _environmentPulseFrame);
        }

        private void InternetStatus_Paint(object? sender, PaintEventArgs e)
        {
            PaintStatusBadge(e.Graphics, internetStatus, _internetPulseFrame);
        }

        private void PosStatus_Paint(object? sender, PaintEventArgs e)
        {
            PaintStatusBadge(e.Graphics, posStatus, _posPulseFrame);
        }

        private void InitializeStatusBadges()
        {
            // Setup fonts
            posStatus.Font = new Font("Segoe UI", 9.8f, FontStyle.Bold);
            internetStatus.Font = new Font("Segoe UI", 9.8f, FontStyle.Bold);
            lblEnvironment.Font = new Font("Segoe UI", 9.8f, FontStyle.Bold);

            // Setup badge appearance for status badges
            internetStatus.AutoSize = false;
            internetStatus.TextAlign = ContentAlignment.MiddleCenter;
            internetStatus.Size = new Size(110, 32);
            internetStatus.Text = "Checking...";

            posStatus.AutoSize = false;
            posStatus.TextAlign = ContentAlignment.MiddleCenter;
            posStatus.Size = new Size(110, 32);
            posStatus.Text = "Checking...";

            // Environment badge setup - increase width for "Production" text
            lblEnvironment.AutoSize = false;
            lblEnvironment.TextAlign = ContentAlignment.MiddleCenter;
            lblEnvironment.Size = new Size(130, 32); // Increased from 110 to 130
                                                     // Remove it from panel1 and add to panel2
            if (panel1.Controls.Contains(lblEnvironment))
            {
                panel1.Controls.Remove(lblEnvironment);
            }
            if (!panel2.Controls.Contains(lblEnvironment))
            {
                panel2.Controls.Add(lblEnvironment);
            }

            // ✅ Attach paint handlers ONCE
            internetStatus.Paint += (s, e) => PaintStatusBadge(e.Graphics, internetStatus, _internetPulseFrame);
            posStatus.Paint += (s, e) => PaintStatusBadge(e.Graphics, posStatus, _posPulseFrame);
            lblEnvironment.Paint += (s, e) => PaintEnvironmentBadge(e.Graphics, lblEnvironment, _environmentPulseFrame);

            // Create rounded regions with updated size
            internetStatus.Region = new Region(CreateRoundRectPath(new Rectangle(0, 0, 110, 32), 12));
            posStatus.Region = new Region(CreateRoundRectPath(new Rectangle(0, 0, 110, 32), 12));
            lblEnvironment.Region = new Region(CreateRoundRectPath(new Rectangle(0, 0, 130, 32), 12)); // Updated size
        }

        private void PaintStatusBadge(Graphics g, Label lbl, int pulseFrame)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(0, 0, lbl.Width - 1, lbl.Height - 1);

            // Determine if active based on text
            bool isActive = lbl.Text.Contains("Online") || lbl.Text.Contains("Active");

            // Colors
            Color bgColor = isActive ? Color.FromArgb(72, 167, 135) : Color.FromArgb(220, 53, 69);
            Color glowColor = isActive ? Color.FromArgb(187, 247, 208) : Color.FromArgb(248, 215, 218);
            Color dotColor = Color.White;

            // Gradient background
            using (var bgBrush = new LinearGradientBrush(
                rect,
                bgColor,
                Color.FromArgb(Math.Max(0, bgColor.R - 20), Math.Max(0, bgColor.G - 20), Math.Max(0, bgColor.B - 20)),
                LinearGradientMode.Vertical))
            {
                g.FillRoundedRectangle(bgBrush, rect, 6);
            }

            // Animated pulse effect
            float pulseAlpha = (float)(Math.Sin(pulseFrame * 0.1) * 0.3 + 0.7);
            using (Pen pulsePen = new Pen(Color.FromArgb((int)(pulseAlpha * 150), glowColor), 2))
            {
                g.DrawRoundedRectangle(pulsePen, rect, 12);
            }

            // Get status text (remove dot if present)
            string statusText = lbl.Text.Replace("●", "").Trim();

            // Measure text
            Size textSize = TextRenderer.MeasureText(statusText, lbl.Font);
            int dotSize = 8;
            int spacing = 6;
            int totalContentWidth = dotSize + spacing + textSize.Width;
            int startX = (lbl.Width - totalContentWidth) / 2;

            // Draw dot
            int dotX = startX;
            int dotY = (lbl.Height - dotSize) / 2;

            // Glow effect
            float glowIntensity = (float)(Math.Sin(pulseFrame * 0.15) * 0.4 + 0.6);
            using (var glowBrush = new SolidBrush(Color.FromArgb((int)(glowIntensity * 80), dotColor)))
            {
                g.FillEllipse(glowBrush, dotX - 3, dotY - 3, dotSize + 6, dotSize + 6);
            }

            // Main dot
            using (var dotBrush = new LinearGradientBrush(
                new Rectangle(dotX, dotY, dotSize, dotSize),
                dotColor,
                Color.White,
                LinearGradientMode.Vertical))
            {
                g.FillEllipse(dotBrush, dotX, dotY, dotSize, dotSize);
            }

            // Draw text
            int textX = dotX + dotSize + spacing;
            int textY = (lbl.Height - textSize.Height) / 2;

            // Shadow
            TextRenderer.DrawText(g, statusText, lbl.Font, new Point(textX + 1, textY + 1),
                Color.FromArgb(40, 0, 0, 0), TextFormatFlags.Left | TextFormatFlags.NoPadding);

            // Main text
            TextRenderer.DrawText(g, statusText, lbl.Font, new Point(textX, textY),
                Color.White, TextFormatFlags.Left | TextFormatFlags.NoPadding);
        }

        private void PaintEnvironmentBadge(Graphics g, Label lbl, int pulseFrame)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(0, 0, lbl.Width - 1, lbl.Height - 1);

            // Get background color from Tag (set in updatelbl method)
            Color backgroundColor = lbl.Tag is Color color ? color : Color.DarkGoldenrod;
            bool isSandbox = lbl.Text == "Sandbox";

            // Adjust colors based on environment
            Color bgColor = backgroundColor;
            Color glowColor = isSandbox ? Color.FromArgb(255, 255, 200) : Color.FromArgb(180, 240, 240);
            Color dotColor = Color.White;

            // Gradient background
            using (var bgBrush = new LinearGradientBrush(
                rect,
                bgColor,
                Color.FromArgb(Math.Max(0, bgColor.R - 20), Math.Max(0, bgColor.G - 20), Math.Max(0, bgColor.B - 20)),
                LinearGradientMode.Vertical))
            {
                g.FillRoundedRectangle(bgBrush, rect, 6);
            }

            // Animated pulse effect
            float pulseAlpha = (float)(Math.Sin(pulseFrame * 0.1) * 0.3 + 0.7);
            using (Pen pulsePen = new Pen(Color.FromArgb((int)(pulseAlpha * 150), glowColor), 2))
            {
                g.DrawRoundedRectangle(pulsePen, rect, 12);
            }

            // Get status text
            string statusText = lbl.Text;

            // Measure text
            Size textSize = TextRenderer.MeasureText(statusText, lbl.Font);
            int dotSize = 8;
            int spacing = 6;
            int totalContentWidth = dotSize + spacing + textSize.Width;
            int startX = (lbl.Width - totalContentWidth) / 2;

            // Ensure the dot doesn't get cut off by adjusting startX if needed
            if (startX < 5) // Add minimum left margin
            {
                startX = 5;
            }

            // Draw dot
            int dotX = startX;
            int dotY = (lbl.Height - dotSize) / 2;

            // Glow effect
            float glowIntensity = (float)(Math.Sin(pulseFrame * 0.15) * 0.4 + 0.6);
            using (var glowBrush = new SolidBrush(Color.FromArgb((int)(glowIntensity * 80), dotColor)))
            {
                g.FillEllipse(glowBrush, dotX - 3, dotY - 3, dotSize + 6, dotSize + 6);
            }

            // Main dot
            using (var dotBrush = new LinearGradientBrush(
                new Rectangle(dotX, dotY, dotSize, dotSize),
                dotColor,
                Color.White,
                LinearGradientMode.Vertical))
            {
                g.FillEllipse(dotBrush, dotX, dotY, dotSize, dotSize);
            }

            // Draw text
            int textX = dotX + dotSize + spacing;
            int textY = (lbl.Height - textSize.Height) / 2;

            // Ensure text doesn't get cut off
            if (textX + textSize.Width > lbl.Width - 5)
            {
                textX = lbl.Width - textSize.Width - 5;
            }

            // Shadow
            TextRenderer.DrawText(g, statusText, lbl.Font, new Point(textX + 1, textY + 1),
                Color.FromArgb(40, 0, 0, 0), TextFormatFlags.Left | TextFormatFlags.NoPadding);

            // Main text
            TextRenderer.DrawText(g, statusText, lbl.Font, new Point(textX, textY),
                Color.White, TextFormatFlags.Left | TextFormatFlags.NoPadding);
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

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);

            try
            {
                _animationTimer?.Stop();
                _animationTimer?.Dispose();

                StopInternetStatusChecker();
                StopWorkerServiceStatusChecker();

                foreach (var form in _independentForms.ToArray())
                {
                    try
                    {
                        if (form != null && !form.IsDisposed)
                        {
                            form.Close();
                            form.Dispose();
                        }
                    }
                    catch { }
                }
            }
            catch { }

            _ = Task.Run(async () =>
            {
                await Task.Delay(400);
                try
                {
                    System.Windows.Forms.Application.Exit();
                }
                finally
                {
                    Environment.Exit(0);
                }
            });
        }


        private void StyleStatusPanel()
        {
            // Setup label properties
            lblNetworkStatus.AutoSize = true;
            lblNetworkStatus.Anchor = AnchorStyles.None;
            lblWorkerService.AutoSize = true;
            lblWorkerService.Anchor = AnchorStyles.None;

            // Calculate and set positions
            RepositionStatusControls();
        }

        private void RepositionStatusControls(string activeView = "Dashboard")
        {
            int screenWidth = Screen.PrimaryScreen.Bounds.Width; // full monitor width
            bool compactMode = screenWidth < 1360;


            // All panels hidden by default
            pnlProfile.Visible = false;
            panDashboard.Visible = false;
            panInvoiceSelection.Visible = false;
            panExportInvoice.Visible = false;
            panCatalogView.Visible = false;

            // Show the active panel
            switch (activeView)
            {
                case "Profile":
                    pnlProfile.Visible = true;
                    pnlProfile.BringToFront();
                    break;
                case "Dashboard":
                    panDashboard.Visible = true;
                    panDashboard.BringToFront();
                    break;
                case "Invoice Entry":
                    panInvoiceSelection.Visible = true;
                    panInvoiceSelection.BringToFront();
                    break;
                case "Export Invoice":
                    panExportInvoice.Visible = true;
                    panExportInvoice.BringToFront();
                    break;
                case "Catalog View":
                    panCatalogView.Visible = true;
                    panCatalogView.BringToFront();
                    break;
            }
            if (compactMode && activeView == "Catalog View")
            {
                panCatalogView.Visible = true;

                // Calculate center alignment
                int panelX = btnCatalogView.Location.X + (btnCatalogView.Width / 2) - (panCatalogView.Width / 2);
                int panelY = btnCatalogView.Location.Y + btnCatalogView.Height; // Right below the button

                panCatalogView.Location = new Point(panelX, panelY - 4);
                panCatalogView.BringToFront();
            }

            // Setup label properties first - use LEFT anchoring for manual positioning
            lblNetworkStatus.AutoSize = true;
            lblNetworkStatus.Anchor = AnchorStyles.Left;
            lblWorkerService.AutoSize = true;
            lblWorkerService.Anchor = AnchorStyles.Left;
            lblEnvironment.Anchor = AnchorStyles.Left;
            internetStatus.Anchor = AnchorStyles.Left;
            posStatus.Anchor = AnchorStyles.Left;

            if (compactMode)
            {
                btnCatalogView.Text = "CONFIGURATION";



                int spacing = 0;          // space between groups
                int badgeSpacing = -2;      // space between label and badge
                int panelPadding = 10;

                // Calculate total required width
                int totalWidth =
                    lblEnvironment.Width +
                    spacing +
                    lblNetworkStatus.Width +
                    badgeSpacing +
                    internetStatus.Width +
                    spacing +
                    lblWorkerService.Width +
                    badgeSpacing +
                    posStatus.Width +
                    panelPadding;

                panel2.Width = totalWidth;
                panel2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                panel2.Location = new Point(panel1.Width - panel2.Width, 0);

                int centerY = (panel2.Height - lblEnvironment.Height) / 2;
                int x = 0;

                // -----------------------------
                // 1. Environment Badge / Label
                // -----------------------------
                lblEnvironment.Location = new Point(x, centerY);
                lblEnvironment.BringToFront();
                x += lblEnvironment.Width + spacing;

                // -----------------------------
                // 2. Internet Status Label
                // -----------------------------
                lblNetworkStatus.Location = new Point(x, centerY);
                lblNetworkStatus.BringToFront();
                x += lblNetworkStatus.Width + badgeSpacing;

                // Internet Status Badge
                internetStatus.Location = new Point(x, centerY);
                internetStatus.BringToFront();
                x += internetStatus.Width + spacing;

                // -----------------------------
                // 3. POS Service Label
                // -----------------------------
                lblWorkerService.Location = new Point(x, centerY);
                lblWorkerService.BringToFront();
                x += lblWorkerService.Width + badgeSpacing;

                // POS Service Badge
                posStatus.Location = new Point(x, centerY);
                posStatus.BringToFront();

                return;
            }

            else
            {

                // Calculate total width needed for all controls with compact spacing
                int totalNeededWidth = lblEnvironment.Width + 15 + lblNetworkStatus.Width + 8 + internetStatus.Width + 15 + lblWorkerService.Width + 8 + posStatus.Width + 35;

                // Make panel2 SMALLER so it doesn't cover menu items
                panel2.Width = totalNeededWidth;
                panel2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                panel2.Location = new Point(panel1.Width - panel2.Width, 0);

                int centerY = (panel2.Height - lblEnvironment.Height) / 2;

                // Position from RIGHT to LEFT with compact spacing
                int rightMargin = 5; // Space from right edge
                int badgeToLabelSpacing = 8; // Tight spacing between badge and its label
                int groupSpacing = 15; // Space between different groups

                // Start from the rightmost element and work backwards
                // Group 1: POS Status (rightmost)
                int currentX = panel2.Width - posStatus.Width - rightMargin;
                posStatus.Location = new Point(currentX, centerY);
                posStatus.BringToFront();

                currentX = posStatus.Left - badgeToLabelSpacing - lblWorkerService.Width;
                lblWorkerService.Location = new Point(currentX, centerY + 10);
                lblWorkerService.BringToFront();

                // Group 2: Internet Status
                currentX = lblWorkerService.Left - groupSpacing - internetStatus.Width;
                internetStatus.Location = new Point(currentX, centerY);
                internetStatus.BringToFront();

                currentX = internetStatus.Left - badgeToLabelSpacing - lblNetworkStatus.Width;
                lblNetworkStatus.Location = new Point(currentX, centerY + 10);
                lblNetworkStatus.BringToFront();

                // Group 3: Environment (leftmost of status indicators)
                currentX = lblNetworkStatus.Left - groupSpacing - lblEnvironment.Width;
                lblEnvironment.Location = new Point(currentX, centerY);
                lblEnvironment.BringToFront();
            }
        }

        private void PositionEnvironmentBadge()
        {
            if (lblEnvironment == null) return;

            // Position environment badge to the right of the logo in panel1
            int logoRight = pictureBox2.Right;
            int spacing = 20;

            lblEnvironment.Location = new Point(logoRight + spacing, (panel1.Height - lblEnvironment.Height) / 2);
            lblEnvironment.BringToFront();
        }

        private void Main_Resize(object sender, EventArgs e)
        {
            HandleFormStateChange();
            RepositionStatusControls(); // Reposition all controls in panel2 on resize
        }

        private void Panel2_Paint(object sender, PaintEventArgs e)
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
        }

        private void StartStatusAnimations()
        {
            _animationTimer = new System.Windows.Forms.Timer { Interval = 50 };
            _animationTimer.Tick += (s, e) =>
            {
                _internetPulseFrame++;
                _posPulseFrame++;
                _environmentPulseFrame++;

                // Only invalidate if controls are visible
                if (internetStatus?.Visible == true && internetStatus.IsHandleCreated)
                    internetStatus.Invalidate();
                if (posStatus?.Visible == true && posStatus.IsHandleCreated)
                    posStatus.Invalidate();
                if (lblEnvironment?.Visible == true && lblEnvironment.IsHandleCreated)
                    lblEnvironment.Invalidate();
            };
            _animationTimer.Start();
        }

        private void UpdateStatusBadge(Label lbl, bool isActive, string text)
        {
            if (lbl == null || !lbl.IsHandleCreated) return;

            if (lbl.InvokeRequired)
            {
                lbl.BeginInvoke(new Action(() =>
                {
                    if (lbl.Text != text) // Only update if changed
                    {
                        lbl.Text = text;
                        lbl.Invalidate(); // Trigger repaint
                    }
                }));
            }
            else
            {
                if (lbl.Text != text)
                {
                    lbl.Text = text;
                    lbl.Invalidate();
                }
            }
        }

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
                            string selectedenvironment = ConfigurationManager.AppSettings["Environment"]!;
                            string token = ConfigurationManager.AppSettings["Token"]!; // CHANGED

                            var fullUrl = $"{_baseUrl}{Endpoints.IsServiceEnabled}?posId={decryptedPosId}&env={selectedenvironment}";
                            bool isEnabled = true;

                            using (var httpClient = new HttpClient())
                            {
                                try
                                {
                                    using var request = new HttpRequestMessage(HttpMethod.Get, fullUrl);

                                    // CHANGED: Add Authorization header for middleware
                                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                                    // CHANGED: Use SendAsync instead of GetAsync
                                    var response = await httpClient.SendAsync(request);

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
                    using var sc = new ServiceController("POSWorker");
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
                    using var sc = new ServiceController("POSWorker");

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
                    using var sc = new ServiceController("POSWorker");

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
            var log = new CreateLogDto { Message = message, Type = type };
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
            btnProfile.ForeColor = Color.Black;
            btnDashboard.ForeColor = Color.Black;
            btnInvoiceSelection.ForeColor = Color.Black;
            btnExportInvoice.ForeColor = Color.Black;
            btnCatalogView.ForeColor = Color.Black;
            pnlProfile.Visible = false;
            panDashboard.Visible = false;
            panInvoiceSelection.Visible = false;
            panExportInvoice.Visible = false;
            panCatalogView.Visible = false;
        }

        private void btnProfile_Click(object sender, EventArgs e)
        {
            ResetNavStyles();
            btnProfile.ForeColor = ColorTranslator.FromHtml("#48A787");
            RepositionStatusControls("Profile");
            LoadView("Profile");
        }

        private void btnDashboard_Click(object sender, EventArgs e)
        {
            ResetNavStyles();
            btnDashboard.ForeColor = ColorTranslator.FromHtml("#48A787");
            RepositionStatusControls("Dashboard");
            LoadView("Dashboard");
        }

        private void btnInvoiceSelection_Click(object sender, EventArgs e)
        {
            ResetNavStyles();
            btnInvoiceSelection.ForeColor = ColorTranslator.FromHtml("#48A787");
            RepositionStatusControls("Invoice Entry");
            LoadView("Invoice Entry");
        }

        private void btnExportInvoice_Click(object sender, EventArgs e)
        {
            ResetNavStyles();
            btnExportInvoice.ForeColor = ColorTranslator.FromHtml("#48A787");
            RepositionStatusControls("Export Invoice");
            LoadView("Export Invoice");
        }

        private void btnCatalogView_Click(object sender, EventArgs e)
        {
            contextMenuCatalog.Show(btnCatalogView, new Point(0, btnCatalogView.Height));
        }

        private void productCatalogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResetNavStyles();
            btnCatalogView.ForeColor = ColorTranslator.FromHtml("#48A787");
            RepositionStatusControls("Catalog View");

            // Call LoadView here
            LoadView("Catalog View");
        }

        private void uploadLogoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var frm = new UploadLogoForm())
            {
                frm.ShowDialog();
            }
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
                "Profile" => _provider.GetRequiredService<Profile>(),
                "Dashboard" => _provider.GetRequiredService<DashboardFormNew>(),
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

        private void StyleContextMenu()
        {
            contextMenuCatalog.Renderer = new ModernContextMenuRenderer();
            contextMenuCatalog.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            contextMenuCatalog.BackColor = Color.White;
            contextMenuCatalog.Padding = new Padding(2, 8, 2, 8);
            contextMenuCatalog.ShowImageMargin = false;
            contextMenuCatalog.AutoSize = true;
            contextMenuCatalog.RenderMode = ToolStripRenderMode.Professional;

            // Style each item with uppercase and better spacing
            foreach (ToolStripItem item in contextMenuCatalog.Items)
            {
                item.Text = item.Text.ToUpper(); // Capitalize text
                item.ForeColor = Color.FromArgb(33, 37, 41);
                item.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold);
            }
        }

        private void Main_Load(object sender, EventArgs e)
        {

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

    // Custom renderer for the context menu
    public class ModernContextMenuRenderer : ToolStripProfessionalRenderer
    {
        private readonly Color _accentColor = ColorTranslator.FromHtml("#48A787");
        private readonly Color _hoverColor = ColorTranslator.FromHtml("#E8F5F1");
        private readonly Color _borderColor = Color.FromArgb(222, 226, 230);
        private readonly Color _textColor = Color.FromArgb(33, 37, 41);

        public ModernContextMenuRenderer() : base(new ModernColorTable()) { }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            var rc = new Rectangle(4, 2, e.Item.Width - 8, e.Item.Height - 4);

            if (e.Item.Selected)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                // Gradient background on hover
                using (var brush = new LinearGradientBrush(
                    rc,
                    _hoverColor,
                    Color.FromArgb(250, 255, 253),
                    LinearGradientMode.Vertical))
                using (var path = Main.CreateRoundRectPath(rc, 8))
                {
                    e.Graphics.FillPath(brush, path);
                }

                // Accent border on hover
                using (var pen = new Pen(_accentColor, 2))
                using (var path = Main.CreateRoundRectPath(rc, 8))
                {
                    e.Graphics.DrawPath(pen, path);
                }

                // Subtle glow effect
                using (var glowPen = new Pen(Color.FromArgb(40, 72, 167, 135), 4))
                {
                    var glowRect = new Rectangle(rc.X - 2, rc.Y - 2, rc.Width + 4, rc.Height + 4);
                    using (var path = Main.CreateRoundRectPath(glowRect, 10))
                    {
                        e.Graphics.DrawPath(glowPen, path);
                    }
                }
            }
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            if (e.Item is ToolStripMenuItem)
            {
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                // Colors + font
                Color textColor = e.Item.Selected ? _accentColor : _textColor;
                Font font = new Font("Segoe UI", 10.2F, FontStyle.Bold);

                // Use FULL ITEM RECTANGLE
                Rectangle rect = new Rectangle(0, 0, e.Item.Width, e.Item.Height);

                // Measure text
                SizeF textSize = e.Graphics.MeasureString(e.Text, font);

                // Center X, Y
                float x = rect.X + (rect.Width - textSize.Width) / 2;
                float y = rect.Y + (rect.Height - textSize.Height) / 2;

                using (var brush = new SolidBrush(textColor))
                {
                    e.Graphics.DrawString(e.Text, font, brush, x, y);
                }
            }
            else
            {
                base.OnRenderItemText(e);
            }
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Rounded border with shadow
            var rect = new Rectangle(0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);
            using (var shadowPen = new Pen(Color.FromArgb(30, 0, 0, 0), 3))
            {
                var shadowRect = new Rectangle(2, 2, rect.Width, rect.Height);
                e.Graphics.DrawRectangle(shadowPen, shadowRect);
            }

            using (var pen = new Pen(_borderColor, 1.5f))
            {
                e.Graphics.DrawRectangle(pen, rect);
            }
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            var rc = new Rectangle(15, e.Item.Height / 2, e.Item.Width - 30, 1);

            // Gradient separator line
            using (var brush = new LinearGradientBrush(
                new Point(rc.Left, rc.Top),
                new Point(rc.Right, rc.Top),
                Color.Transparent,
                _borderColor))
            using (var pen = new Pen(brush, 1))
            {
                e.Graphics.DrawLine(pen, rc.Left, rc.Top, rc.Right, rc.Top);
            }
        }
    }

    // Custom color table
    public class ModernColorTable : ProfessionalColorTable
    {
        public override Color MenuItemSelected => ColorTranslator.FromHtml("#E8F5F1");
        public override Color MenuItemBorder => Color.Transparent;
        public override Color MenuBorder => Color.FromArgb(222, 226, 230);
        public override Color ImageMarginGradientBegin => Color.White;
        public override Color ImageMarginGradientMiddle => Color.White;
        public override Color ImageMarginGradientEnd => Color.White;
        public override Color ToolStripDropDownBackground => Color.White;
    }
}

