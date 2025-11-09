using Microsoft.Extensions.DependencyInjection;
using POSPRA.Application.Services.CloudSyncService.CloudSyncLogService;
using POSPRA.Application.Services.FileRecordService;
using POSPRA.Application.Services.InvoiceService;
using POSPRA.Application.Services.LogService;
using POSPRA.Application.Utility;
using POSPRA.DTOs;
using POSPRA.DTOs.ClientDtos;
using POSPRA.DTOs.LogDtos;
using POSPRA.SecurityEncryption;
using POSPRA_WinFormsUI.AlertClasses;
using System.Configuration;
using System.Data;
using System.Drawing.Drawing2D;
using System.Net.Http.Json;
using System.Text;

namespace POSPRA_WinFormsUI.Forms
{
    public partial class DashboardForm : Form
    {
        private readonly IServiceProvider _provider;
        private readonly IFileRecordService _fileRecordService;
        private readonly ILogService _logService;
        private readonly IInvoiceService _invoiceService;
        private readonly ISendLogToCloudService _sendLogToCloudService;
        private readonly string _baseUrl;

        private readonly HttpClient _httpClient;
        private System.Timers.Timer _heartbeatTimer;

        private bool _isInitialLoad = true;
        private bool _filterSyncedOnly = false;
        private bool _isSyncedSortDescending = true;
        private int _isLoadingFlag = 0;

        private DateTime _startDate = DateTime.Today.AddDays(-7);
        private DateTime _endDate = DateTime.Today;

        private int _pendingCount;
        private int _syncedCount;

        private int _totalLogsCount = 0;
        private int _errorLogsCount = 0;
        private int _warningLogsCount = 0;
        private int _infoLogsCount = 0;
        private Rectangle _printLinkBounds = Rectangle.Empty;
        private DataGridViewCell _hoveredCell = null;

        private System.Windows.Forms.Timer _autoRefreshTimer;
        private bool _autoRefreshEnabled = false;
        private int _lastInvoiceCount = 0;
        private int _lastLogCount = 0;
        private DateTime _lastRefreshTime = DateTime.Now;

        private int _printingRowIndex = -1;
        private string _posId;

        private int _lastSyncedInvoiceCount = 0;

        private readonly SemaphoreSlim _checkSemaphore = new SemaphoreSlim(1, 1);

        // NEW: Virtual Mode Cache
        private List<InvoiceDisplayModel> _invoiceCache;
        private List<LogDisplayModel> _logCache;

        // NEW: Cached images for performance
        private static Image _greenTickCached;
        private static Image _redCrossCached;

        private Bitmap _cachedPieChart = null;
        private int _lastPieChartSynced = -1;
        private int _lastPieChartPending = -1;

        public DashboardForm(IServiceProvider provider,
            ILogService logService,
            IInvoiceService invoiceService,
            IFileRecordService fileRecordService,
            ISendLogToCloudService sendLogToCloudService
            )
        {

            InitializeComponent();

            this.Load += (s, e) => CenterProgressBar();
            this.Resize += (s, e) => CenterProgressBar();
            this.Load += DashboardForm_Load;
            _provider = provider;
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
            _sendLogToCloudService = sendLogToCloudService;
            _baseUrl = ConfigurationManager.AppSettings["BaseUrl"];
            _httpClient = new HttpClient(); // local instance for manual URL handling
            //RoundAllButtons(this, 4);

            FormBorderStyle = FormBorderStyle.None;
            ControlBox = false;
            ShowIcon = false;
            Text = string.Empty;

            // Initialize caches
            _invoiceCache = new List<InvoiceDisplayModel>();
            _logCache = new List<LogDisplayModel>();

            // Cache images
            CacheImages();

            StyleDateRangeLabel();

            lblDateRange.Click += LblDateRange_Click;
            dtpStartDate.ValueChanged += DtpStartDate_ValueChanged;
            dtpEndDate.ValueChanged += DtpEndDate_ValueChanged;
            dtpStartDate.CloseUp += DtpStartDate_CloseUp;
            dtpEndDate.CloseUp += DtpEndDate_CloseUp;

            btnToday.Click += btnToday_Click;
            btnClearFilter.Click += btnClearFilter_Click;
            btnRefresh.Click += btnRefresh_Click;
            btnFilterSynced.Click += btnFilterSynced_Click;
            btnFilter.Click += btnFilterInvoices_Click;
            btnExportInvoice.Click += btnExportInvoice_Click;
            btnSyncLogs.Click += btnSyncLogs_Click;

            btnExportLogs.Click += btnExportLogs_Click;
            panelinvoicechart.Resize -= Panel_Resize;
            panelinvoicechart.Resize += Panel_Resize;

            panellogchart.Resize -= LogChartPanel_Resize;
            panellogchart.Resize += LogChartPanel_Resize;

            lblTotalLogsCount.Click += panelTotalLogs_Click;
            lblTotalLogsTitle.Click += panelTotalLogs_Click;
            panelTotalLogs.Click += panelTotalLogs_Click;

            lblErrorLogsTitle.Click += panelErrorLogs_Click;
            lblErrorLogsCount.Click += panelErrorLogs_Click;
            panelErrorLogs.Click += panelErrorLogs_Click;

            lblInfoLogsCount.Click += panelInfoLogs_Click;
            lblInfoLogsTitle.Click += panelInfoLogs_Click;
            panelInfoLogs.Click += panelInfoLogs_Click;

            lblWarningLogsCount.Click += panelWarningLogs_Click;
            lblWarningLogsTitle.Click += panelWarningLogs_Click;
            panelWarningLogs.Click += panelWarningLogs_Click;

            if (progressBar != null) progressBar.Visible = false;
            ApplyGradientBackground(
                panelAll,
                ColorTranslator.FromHtml("#8860C1"),
                ColorTranslator.FromHtml("#584ABC")
            );
            ApplyGradientBackground(
                panelPending,
                ColorTranslator.FromHtml("#AC0101"),
                ColorTranslator.FromHtml("#D50000")
            );
            ApplyGradientBackground(
                panelPaid,
                ColorTranslator.FromHtml("#48A787"),
                ColorTranslator.FromHtml("#0D7351")
            );
            AddImageToPanelRight(panelAll, Resources.InvoiceAll);
            AddImageToPanelRight(panelPending, Resources.NotSynced);
            AddImageToPanelRight(panelPaid, Resources.Synced);
            SetButtonImage(btnFilter, Resources.Filter, ColorTranslator.FromHtml("#0D7351"));
            SetButtonImage(btnRefresh, Resources.refresh, ColorTranslator.FromHtml("#5BBBB3"));
            SetButtonImage(btnClearFilter, Resources.clearFilter, ColorTranslator.FromHtml("#DC2626"));

            InitializeLogStatistics();
            InitializeAutoRefreshTimer();
            _invoiceService = invoiceService;
            _fileRecordService = fileRecordService;

            // Enable Virtual Mode
            EnableVirtualMode();

        }

        public void MakeRoundedControl(Control control, int radius)
        {
            if (control == null || control.Width <= 0 || control.Height <= 0)
                return;

            using (GraphicsPath path = new GraphicsPath())
            {
                int diameter = radius * 2;
                path.AddArc(0, 0, diameter, diameter, 180, 90);
                path.AddArc(control.Width - diameter, 0, diameter, diameter, 270, 90);
                path.AddArc(control.Width - diameter, control.Height - diameter, diameter, diameter, 0, 90);
                path.AddArc(0, control.Height - diameter, diameter, diameter, 90, 90);
                path.CloseFigure();

                // set the region for click & focus area
                control.Region = new Region(path);
            }
        }

        public void StyleButton(Button btn, int radius = 10)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = ControlPaint.Light(btn.BackColor, 0.15f);
            btn.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(btn.BackColor, 0.15f);

            // Reapply rounded corners on resize
            btn.Resize += (s, e) => MakeRoundedControl(btn, radius);

            btn.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                e.Graphics.CompositingQuality = CompositingQuality.HighQuality;

                Rectangle rect = new Rectangle(0, 0, btn.Width - 1, btn.Height - 1);
                int d = radius * 2;

                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddArc(rect.X, rect.Y, d, d, 180, 90);
                    path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
                    path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
                    path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
                    path.CloseFigure();

                    // --- smoother blended outline ---
                    using (Pen smoothPen = new Pen(Color.FromArgb(60, 0, 0, 0), 3))
                    {
                        smoothPen.Alignment = PenAlignment.Outset;
                        e.Graphics.DrawPath(smoothPen, path);
                    }

                    using (Pen borderPen = new Pen(ControlPaint.Dark(btn.BackColor, 0.3f), 1.5f))
                    {
                        borderPen.Alignment = PenAlignment.Center;
                        e.Graphics.DrawPath(borderPen, path);
                    }
                }

                // Let Windows draw the image & text (don't override)
                // This prevents image disappearance and keeps text sharp
                btn.TextImageRelation = TextImageRelation.ImageBeforeText;
            };
        }

        public void RoundAllButtons(Control parent, int radius = 10)
        {
            foreach (Control ctrl in parent.Controls)
            {
                if (ctrl is Button btn)
                {
                    MakeRoundedControl(btn, radius);
                    StyleButton(btn, radius);
                }

                if (ctrl.HasChildren)
                    RoundAllButtons(ctrl, radius);
            }
        }


        private void CacheImages()
        {
            _greenTickCached = Resources.GreenTick;
            _redCrossCached = Resources.RedCross;
        }

        private void EnableVirtualMode()
        {
            InvoicesDataGridView.VirtualMode = true;
            InvoicesDataGridView.CellValueNeeded += InvoicesDataGridView_CellValueNeeded;

            LogsDataGridView.VirtualMode = true;
            LogsDataGridView.CellValueNeeded += LogsDataGridView_CellValueNeeded;
        }

        private void InvoicesDataGridView_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            if (_invoiceCache == null || e.RowIndex >= _invoiceCache.Count) return;

            var item = _invoiceCache[e.RowIndex];

            switch (InvoicesDataGridView.Columns[e.ColumnIndex].Name)
            {
                case "colId":
                    e.Value = item.SerialNo;
                    break;
                case "colPosId":
                    e.Value = item.PosId;
                    break;
                case "colInvoiceNumber":
                    e.Value = item.InvoiceNumber;
                    break;
                case "colIsSynced":
                    e.Value = item.IsSynced;
                    break;
                case "colPrint":
                    e.Value = "View";
                    break;
                case "colDateCreated":
                    e.Value = item.DateCreated;
                    break;
            }
        }

        private void LogsDataGridView_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            if (_logCache == null || e.RowIndex >= _logCache.Count) return;

            var item = _logCache[e.RowIndex];

            switch (LogsDataGridView.Columns[e.ColumnIndex].Name)
            {
                case "colLogID":
                    e.Value = item.SerialNo;
                    break;
                case "colMessage":
                    e.Value = item.Message;
                    break;
                case "colException":
                    e.Value = item.Type;
                    break;
                case "logdatetime":
                    e.Value = item.DateCreated;
                    break;
            }
        }

        private void StartHeartbeatTimer(string decryptedPosId)
        {
            if (string.IsNullOrEmpty(decryptedPosId))
            {
                UpdateHeartbeatLabel(isError: true);
                return;
            }

            _heartbeatTimer = new System.Timers.Timer(100000);
            _heartbeatTimer.Elapsed += async (s, e) =>
            {
                try
                {
                    await UpdateHeartbeatAsync(decryptedPosId);
                }
                catch (ObjectDisposedException)
                {
                    // Form was disposed, stop timer
                    _heartbeatTimer?.Stop();
                }
            };
            _heartbeatTimer.AutoReset = true;
            _heartbeatTimer.Start();

            _ = Task.Run(() => UpdateHeartbeatAsync(decryptedPosId));
        }

        private async Task UpdateHeartbeatAsync(string decryptedPosId)
        {
            try
            {
                if (!int.TryParse(decryptedPosId, out int posId))
                {
                    System.Diagnostics.Debug.WriteLine($"Invalid POSID: {decryptedPosId}");
                    UpdateHeartbeatLabel(isError: true);
                    return;
                }

                if (string.IsNullOrEmpty(_baseUrl))
                {
                    System.Diagnostics.Debug.WriteLine("BaseUrl not found in configuration.");
                    UpdateHeartbeatLabel(isError: true);
                    return;
                }

                // Combine BaseUrl + Endpoint
                var fullUrl = $"{_baseUrl}{Endpoints.HeartBeat}";
                var requestBody = new { PosId = posId };
                var json = JsonContent.Create(requestBody);

                var response = await _httpClient.PostAsync(fullUrl, json);
                if (response.IsSuccessStatusCode)
                {
                    var heartbeatResponse = await response.Content.ReadFromJsonAsync<ApiResponse<HeartBeatDto>>();

                    if (heartbeatResponse?.StatusCode == ApiStatusCode.Success && heartbeatResponse.Data != null)
                    {
                        var serverTime = heartbeatResponse.Data.HeartbeatUpdatedOn;
                        UpdateHeartbeatLabel(serverTime, isError: false);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Heartbeat API returned no data: {heartbeatResponse?.Message}");
                        UpdateHeartbeatLabel(isError: true);
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Heartbeat failed: {response.StatusCode}");
                    UpdateHeartbeatLabel(isError: true);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating heartbeat: {ex.Message}");
                UpdateHeartbeatLabel(isError: true);
            }
        }

        private void UpdateHeartbeatLabel(DateTime? heartbeatTime = null, bool isError = false)
        {
            if (lblHeartbeat.InvokeRequired)
            {
                lblHeartbeat.Invoke(new Action(() => UpdateHeartbeatLabel(heartbeatTime, isError)));
                return;
            }

            if (isError)
            {
                lblHeartbeat.Text = "Last Heartbeat: Not Received";
                lblHeartbeat.ForeColor = Color.FromArgb(220, 38, 38);
            }
            else
            {
                lblHeartbeat.Text = $"Last Heartbeat: {heartbeatTime:dd-MM-yyyy HH:mm:ss}";
                lblHeartbeat.ForeColor = Color.FromArgb(34, 197, 94);
            }
        }

        private async Task<T> ExecuteWithNewScope<T>(Func<Task<T>> serviceCall)
        {
            using var scope = _provider.CreateScope();
            var fileService = scope.ServiceProvider.GetRequiredService<IFileRecordService>();
            var logService = scope.ServiceProvider.GetRequiredService<ILogService>();
            return await serviceCall();
        }

        private async Task<bool> QuickCheckForChangesAsync()
        {
            if (!await _checkSemaphore.WaitAsync(0))
                return false;

            try
            {
                using var scope = _provider.CreateScope();
                var fileService = scope.ServiceProvider.GetRequiredService<IFileRecordService>();
                var logService = scope.ServiceProvider.GetRequiredService<ILogService>();

                var invoiceResponse = await fileService.GetAllAsync();
                var logResponse = await logService.GetAllAsync();

                if (invoiceResponse?.Data == null || logResponse?.Data == null)
                    return false;

                var allInvoices = invoiceResponse.Data.ToList();
                var allLogs = logResponse.Data.ToList();

                bool shouldFilterByDate = !(_startDate == DateTime.Today.AddDays(-7) && _endDate == DateTime.Today);

                IEnumerable<dynamic> filteredInvoices = allInvoices;
                IEnumerable<LogDto> filteredLogs = allLogs;

                if (shouldFilterByDate)
                {
                    filteredInvoices = allInvoices.Where(i =>
                        i.DateCreated >= _startDate &&
                        i.DateCreated <= _endDate.AddDays(1).AddTicks(-1));

                    filteredLogs = allLogs.Where(l =>
                        l.CreatedAtPk >= _startDate &&
                        l.CreatedAtPk <= _endDate.AddDays(1).AddTicks(-1));
                }

                int currentInvoiceCount = filteredInvoices.Count();
                int currentSyncedCount = filteredInvoices.Count(i => i.IsSynced == 1);
                int currentLogCount = filteredLogs.Count();

                bool hasChanges =
                    (currentInvoiceCount != _lastInvoiceCount) ||
                    (currentSyncedCount != _lastSyncedInvoiceCount) ||
                    (currentLogCount != _lastLogCount);

                if (hasChanges)
                {
                    _lastInvoiceCount = currentInvoiceCount;
                    _lastSyncedInvoiceCount = currentSyncedCount;
                    _lastLogCount = currentLogCount;
                }

                return hasChanges;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"QuickCheck error: {ex.Message}");
                return false;
            }
            finally
            {
                _checkSemaphore.Release();
            }
        }

        #region AutoRefresh
        private void InitializeAutoRefreshTimer()
        {
            _autoRefreshTimer = new System.Windows.Forms.Timer();
            _autoRefreshTimer.Interval = 30000;
            _autoRefreshTimer.Tick += AutoRefreshTimer_Tick;
            _autoRefreshEnabled = true;
            _autoRefreshTimer.Start();
        }

        private async void AutoRefreshTimer_Tick(object sender, EventArgs e)
        {
            // Stop timer during refresh
            _autoRefreshTimer.Stop();

            try
            {
                if (_isLoadingFlag == 1) return;
                if (IsUserInteracting()) return;

                bool hasChanges = await QuickCheckForChangesAsync();
                if (hasChanges)
                {
                    // Use Invoke to marshal back to UI thread
                    if (InvokeRequired)
                    {
                        Invoke(new Action(async () => await BackgroundRefreshAsync()));
                    }
                    else
                    {
                        await BackgroundRefreshAsync();
                    }
                }
            }
            finally
            {
                // Restart timer
                if (_autoRefreshEnabled && !IsDisposed)
                    _autoRefreshTimer.Start();
            }
        }

        private bool IsUserInteracting()
        {
            if (InvoicesDataGridView.SelectedCells.Count > 0) return true;
            if (LogsDataGridView.SelectedCells.Count > 0) return true;
            if (InvoicesDataGridView.IsCurrentCellInEditMode) return true;
            if (LogsDataGridView.IsCurrentCellInEditMode) return true;
            return false;
        }

        private async Task BackgroundRefreshAsync()
        {
            await RunSingleLoad(async () =>
            {
                int invoiceScroll = InvoicesDataGridView.FirstDisplayedScrollingRowIndex;
                int logScroll = LogsDataGridView.FirstDisplayedScrollingRowIndex;

                await LoadAndShowInvoicesAsync();
                await LoadAndShowLogsAsync();

                try
                {
                    if (invoiceScroll >= 0 && invoiceScroll < InvoicesDataGridView.Rows.Count)
                        InvoicesDataGridView.FirstDisplayedScrollingRowIndex = invoiceScroll;

                    if (logScroll >= 0 && logScroll < LogsDataGridView.Rows.Count)
                        LogsDataGridView.FirstDisplayedScrollingRowIndex = logScroll;
                }
                catch { }
            });
        }

        public void ToggleAutoRefresh()
        {
            _autoRefreshEnabled = !_autoRefreshEnabled;

            if (_autoRefreshEnabled)
            {
                _autoRefreshTimer.Start();
                WindowsLocalAppNotification.Show("Auto-Refresh", "Auto-refresh enabled (30 seconds)");
            }
            else
            {
                _autoRefreshTimer.Stop();
                WindowsLocalAppNotification.Show("Auto-Refresh", "Auto-refresh disabled");
            }
        }

        public void SetAutoRefreshInterval(int seconds)
        {
            if (seconds < 10) seconds = 10;

            _autoRefreshTimer.Stop();
            _autoRefreshTimer.Interval = seconds * 1000;

            if (_autoRefreshEnabled)
            {
                _autoRefreshTimer.Start();
            }
        }
        #endregion

        #region log filtering
        private async void panelTotalLogs_Click(object sender, EventArgs e)
        {
            await LoadAndShowLogsAsync(logTypeFilter: "success");
        }
        private async void panelErrorLogs_Click(object sender, EventArgs e)
        {
            await LoadAndShowLogsAsync(logTypeFilter: "error");
        }
        private async void panelWarningLogs_Click(object sender, EventArgs e)
        {
            await LoadAndShowLogsAsync(logTypeFilter: "warning");
        }
        private async void panelInfoLogs_Click(object sender, EventArgs e)
        {
            await LoadAndShowLogsAsync(logTypeFilter: "info");
        }
        #endregion

        private void InitializeLogStatistics()
        {
            UpdateLogStatisticsDisplay();
        }

        private void UpdateLogStatisticsDisplay()
        {
            if (lblTotalLogsCount != null)
                lblTotalLogsCount.Text = _totalLogsCount.ToString();

            if (lblErrorLogsCount != null)
                lblErrorLogsCount.Text = _errorLogsCount.ToString();

            if (lblWarningLogsCount != null)
                lblWarningLogsCount.Text = _warningLogsCount.ToString();

            if (lblInfoLogsCount != null)
                lblInfoLogsCount.Text = _infoLogsCount.ToString();
        }

        private void CalculateLogStatistics(IEnumerable<LogDto> logs)
        {
            if (logs == null)
            {
                _totalLogsCount = 0;
                _errorLogsCount = 0;
                _warningLogsCount = 0;
                _infoLogsCount = 0;
                return;
            }

            _totalLogsCount = logs.Count(l =>
                l.Type?.Equals("Success", StringComparison.OrdinalIgnoreCase) == true);
            _errorLogsCount = logs.Count(l =>
                l.Type?.Equals("Error", StringComparison.OrdinalIgnoreCase) == true ||
                l.Type?.Equals("Exception", StringComparison.OrdinalIgnoreCase) == true);
            _warningLogsCount = logs.Count(l =>
                l.Type?.Equals("Warning", StringComparison.OrdinalIgnoreCase) == true);
            _infoLogsCount = logs.Count(l =>
                l.Type?.Equals("Information", StringComparison.OrdinalIgnoreCase) == true ||
                l.Type?.Equals("Info", StringComparison.OrdinalIgnoreCase) == true);
        }

        private void LogChartPanel_Resize(object sender, EventArgs e)
        {
            UpdateLogStatisticsLayout();
        }

        private void UpdateLogStatisticsLayout()
        {
            if (panellogchart == null || tableLayoutPanelLogStats == null) return;

            int panelHeight = panellogchart.Height;
            int titleFontSize = Math.Max(8, Math.Min(12, panelHeight / 25));
            int countFontSize = Math.Max(12, Math.Min(24, panelHeight / 12));

            UpdateStatPanelFonts(panelTotalLogs, lblTotalLogsCount, lblTotalLogsTitle, countFontSize, titleFontSize);
            UpdateStatPanelFonts(panelErrorLogs, lblErrorLogsCount, lblErrorLogsTitle, countFontSize, titleFontSize);
            UpdateStatPanelFonts(panelWarningLogs, lblWarningLogsCount, lblWarningLogsTitle, countFontSize, titleFontSize);
            UpdateStatPanelFonts(panelInfoLogs, lblInfoLogsCount, lblInfoLogsTitle, countFontSize, titleFontSize);
        }

        private void UpdateStatPanelFonts(Panel panel, Label countLabel, Label titleLabel, int countSize, int titleSize)
        {
            if (countLabel != null)
            {
                countLabel.Font = new Font("Segoe UI", countSize, FontStyle.Bold);
                countLabel.TextAlign = ContentAlignment.MiddleCenter;
                countLabel.AutoSize = false;
                countLabel.Dock = DockStyle.None;
            }

            if (titleLabel != null)
            {
                titleLabel.Font = new Font("Segoe UI", titleSize, FontStyle.Regular);
                titleLabel.TextAlign = ContentAlignment.MiddleCenter;
                titleLabel.AutoSize = false;
                titleLabel.Dock = DockStyle.None;
            }

            if (panel != null && countLabel != null && titleLabel != null)
            {
                int padding = 8;

                countLabel.Width = panel.Width - (padding * 2);
                countLabel.Height = (int)(panel.Height * 0.6);
                countLabel.Left = padding;
                countLabel.Top = padding;
                countLabel.TextAlign = ContentAlignment.MiddleCenter;

                titleLabel.Width = panel.Width - (padding * 2);
                titleLabel.Height = (int)(panel.Height * 0.3);
                titleLabel.Left = padding;
                titleLabel.Top = panel.Height - titleLabel.Height - padding;
                titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            }
        }

        private void CenterProgressBar()
        {
            if (progressBar != null && InvoicesDataGridView != null)
            {
                var gridBounds = InvoicesDataGridView.Bounds;
                progressBar.Left = gridBounds.Left + (gridBounds.Width - progressBar.Width) / 2;
                progressBar.Top = gridBounds.Top + (gridBounds.Height - progressBar.Height) / 2;
                progressBar.BringToFront();
            }
        }
        private void ShowProgressBar()
        {
            if (progressBar.InvokeRequired)
            {
                progressBar.Invoke(new Action(ShowProgressBar));
                return;
            }

            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.MarqueeAnimationSpeed = 30;
            progressBar.Visible = true;
            progressBar.BringToFront();
        }
        private void HideProgressBar()
        {
            if (progressBar.InvokeRequired)
            {
                progressBar.Invoke(new Action(HideProgressBar));
                return;
            }

            progressBar.Visible = false;
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.MarqueeAnimationSpeed = 0; // Stop animation
        }
        private async Task RunSingleLoad(Func<Task> work)
        {
            if (Interlocked.Exchange(ref _isLoadingFlag, 1) == 1) return;

            try
            {
                ShowProgressBar();
                await work();
            }
            finally
            {
                HideProgressBar();
                Interlocked.Exchange(ref _isLoadingFlag, 0);
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _autoRefreshTimer?.Stop();
                _autoRefreshTimer?.Dispose();
                _heartbeatTimer?.Stop();
                _heartbeatTimer?.Dispose();
                _checkSemaphore?.Dispose();
                _httpClient?.Dispose();

                // Dispose cached images
                _cachedPieChart?.Dispose();
                _cachedPieChart = null;

                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void LblDateRange_Click(object sender, EventArgs e)
        {
            dtpStartDate.Visible = true;
            dtpStartDate.Focus();
            SendKeys.Send("%{DOWN}");
        }

        private void DtpStartDate_ValueChanged(object sender, EventArgs e)
        {
            // Prevent selecting a start date after the end date
            if (dtpStartDate.Value > dtpEndDate.Value)
            {
                MessageBox.Show(
                    "Start date cannot be after the end date.",
                    "Invalid Date Range",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                // Reset to the previous valid value
                dtpStartDate.Value = _startDate;
                return;
            }

            _startDate = dtpStartDate.Value.Date;
            UpdateDateRangeLabel();
        }

        private void DtpEndDate_ValueChanged(object sender, EventArgs e)
        {
            // Prevent selecting an end date before the start date
            if (dtpEndDate.Value < dtpStartDate.Value)
            {
                MessageBox.Show(
                    "End date cannot be before the start date.",
                    "Invalid Date Range",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                // Reset to the previous valid value
                dtpEndDate.Value = _endDate;
                return;
            }

            // Optional: Prevent selecting future dates
            if (dtpEndDate.Value > DateTime.Today)
            {
                MessageBox.Show(
                    "End date cannot be in the future.",
                    "Invalid Date",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                dtpEndDate.Value = DateTime.Today;
                return;
            }

            _endDate = dtpEndDate.Value.Date;
            UpdateDateRangeLabel();
        }


        private void DtpStartDate_CloseUp(object sender, EventArgs e)
        {
            dtpStartDate.Visible = false;
            dtpEndDate.Visible = true;
            dtpEndDate.Focus();
            SendKeys.Send("%{DOWN}");
        }

        private void DtpEndDate_CloseUp(object sender, EventArgs e)
        {
            dtpEndDate.Visible = false;
            UpdateDateRangeLabel();
        }

        private void UpdateDateRangeLabel()
        {
            if (_startDate == _endDate)
            {
                lblDateRange.Text = $"  {_startDate:MMM d, yyyy}";
            }
            else
            {
                lblDateRange.Text = $"  {_startDate:MMM d, yyyy} - {_endDate:MMM d, yyyy}";
            }
        }

        private void StyleDateRangeLabel()
        {
            lblDateRange.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(209, 213, 219), 1))
                {
                    var rect = new Rectangle(0, 0, lblDateRange.Width - 1, lblDateRange.Height - 1);
                    var radius = 6;
                    using (var path = GetRoundedRect(rect, radius))
                    {
                        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        e.Graphics.DrawPath(pen, path);
                    }
                }

                using (var iconBrush = new SolidBrush(Color.FromArgb(107, 114, 128)))
                {
                    var iconFont = new Font("Segoe UI Symbol", 10F);
                    var iconText = "📅";
                    var iconSize = e.Graphics.MeasureString(iconText, iconFont);
                    var iconX = lblDateRange.Width - iconSize.Width - 10;
                    var iconY = (lblDateRange.Height - iconSize.Height) / 2;
                    e.Graphics.DrawString(iconText, iconFont, iconBrush, iconX, iconY);
                }
            };
        }

        private async void DashboardForm_Load(object sender, EventArgs e)
        {
            try
            {
                AddButtonTooltips();
                var encryptedPosId = System.Configuration.ConfigurationManager.AppSettings["Username"] ?? "0";
                var decryptedPosId = AesEncryptionHelper.Decrypt(encryptedPosId);
                StartHeartbeatTimer(decryptedPosId);

                _autoRefreshTimer.Stop();

                _startDate = DateTime.Today.AddDays(-7);
                _endDate = DateTime.Today;
                dtpStartDate.Value = _startDate;
                dtpEndDate.Value = _endDate;
                UpdateDateRangeLabel();
                ApplyDataGridStyles();

                await RunSingleLoad(async () =>
                {
                    await LoadAndShowInvoicesAsync();
                    await LoadAndShowLogsAsync();
                });

                _lastInvoiceCount = _invoiceCache?.Count ?? 0;
                _lastSyncedInvoiceCount = _invoiceCache?.Count(i => i.IsSynced == "Yes") ?? 0;
                _lastLogCount = _logCache?.Count ?? 0;
                _lastRefreshTime = DateTime.Now;

                InvoicesDataGridView.DataError += dataGridView_DataError;
                LogsDataGridView.DataError += dataGridView_DataError;
                _isInitialLoad = false;

                if (_autoRefreshEnabled)
                {
                    _autoRefreshTimer.Start();
                }
            }
            catch (Exception ex)
            {
                WindowsLocalAppNotification.Show("Dashboard Error", $"Error loading dashboard: {ex.Message}");
                AlertManager.ShowError($"Error loading dashboard: {ex.Message}");
            }
        }

        // Load Invoices with Virtual Mode
        private async Task LoadAndShowInvoicesAsync(bool skipDateFilter = false)
        {
            try
            {
                using var scope = _provider.CreateScope();
                var fileService = scope.ServiceProvider.GetRequiredService<IFileRecordService>();

                var response = await fileService.GetAllAsync();

                if (response?.Data == null || !response.Data.Any())
                {
                    _invoiceCache = new List<InvoiceDisplayModel>();
                    InvoicesDataGridView.RowCount = 0;
                    AlertManager.ShowWarning("No invoices found.");
                    lblLastSync.Text = "Last Sync: N/A";
                    return;
                }

                IEnumerable<dynamic> filteredInvoices = response.Data;

                if (!skipDateFilter)
                {
                    filteredInvoices = filteredInvoices.Where(i =>
                        i.DateCreated >= _startDate &&
                        i.DateCreated <= _endDate.AddDays(1).AddTicks(-1));
                }

                if (_filterSyncedOnly)
                    filteredInvoices = filteredInvoices.Where(i => i.IsSynced == 1);

                // Build cache for virtual mode
                _invoiceCache = filteredInvoices
                    .OrderByDescending(i => i.DateCreated)
                    .Select((inv, index) => new InvoiceDisplayModel
                    {
                        SerialNo = index + 1,
                        PosId = inv.POSID,
                        InvoiceNumber = inv.InvoiceNumber ?? "N/A",
                        IsSynced = inv.IsSynced == 1 ? "Yes" : "No",
                        DateCreated = inv.DateCreated.ToString("dd-MM-yyyy HH:mm:ss"),
                        IsSyncedRaw = inv.IsSynced,
                        AttemptCount = inv.AttemptCount,
                        DateCreatedRaw = inv.DateCreated
                    })
                    .ToList();

                // ✅ Set row count for virtual mode (instant)
                InvoicesDataGridView.RowCount = _invoiceCache.Count;
                InvoicesDataGridView.Invalidate();
                InvoicesDataGridView.Refresh();


                int syncedCount = _invoiceCache.Count(i => i.IsSynced == "Yes");
                int pendingCount = _invoiceCache.Count - syncedCount;

                labelAllInvoices.Text = _invoiceCache.Count.ToString();
                labelPendingInvoice.Text = pendingCount.ToString();
                labelPaidInvoices.Text = syncedCount.ToString();
                _pendingCount = pendingCount;
                _syncedCount = syncedCount;
                DrawInvoicePieChart(panelinvoicechart, _syncedCount, _pendingCount);

                InvoicesDataGridView.ClearSelection();
                InvoicesDataGridView.CurrentCell = null;

                var lastSyncedInvoice = _invoiceCache
                    .Where(i => i.IsSynced == "Yes")
                    .FirstOrDefault();

                if (lastSyncedInvoice != null)
                {
                    lblLastSync.Text = "Last Synced Invoice: " + lastSyncedInvoice.DateCreated;
                    lblLastSync.ForeColor = Color.FromArgb(34, 197, 94);
                }
                else
                {
                    lblLastSync.Text = "Last Synced Invoice: N/A";
                    lblLastSync.ForeColor = Color.FromArgb(220, 38, 38);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading invoices: {ex}");
                WindowsLocalAppNotification.Show("Invoices Error", $"Error loading invoices: {ex.Message}");
                AlertManager.ShowError($"Error loading invoices: {ex.Message}");
                lblLastSync.Text = "Last Sync: Error";
                lblLastSync.ForeColor = Color.FromArgb(220, 38, 38);
            }
        }

        private async Task LoadAndShowLogsAsync(
            IEnumerable<LogDto>? preloadedLogs = null,
            bool skipDateFilter = false,
            string? logTypeFilter = null)
        {
            try
            {
                IEnumerable<LogDto> logs;
                IEnumerable<LogDto> allLogsForStats; // ✅ Keep unfiltered logs for statistics

                if (preloadedLogs != null)
                {
                    logs = preloadedLogs;
                    allLogsForStats = preloadedLogs;
                }
                else
                {
                    var response = await _logService.GetAllAsync();

                    if (response?.Data == null || !response.Data.Any())
                    {
                        _logCache = new List<LogDisplayModel>();
                        LogsDataGridView.RowCount = 0;

                        WindowsLocalAppNotification.Show("Logs", "No logs available to display");
                        AlertManager.ShowWarning("No logs available to display");

                        CalculateLogStatistics(null);
                        UpdateLogStatisticsDisplay();
                        return;
                    }

                    logs = response.Data;
                    allLogsForStats = response.Data;
                }

                // ✅ Apply date filter (affects both stats + display)
                if (!skipDateFilter)
                {
                    logs = logs.Where(l =>
                        l.CreatedAtPk >= _startDate &&
                        l.CreatedAtPk <= _endDate.AddDays(1).AddTicks(-1));

                    allLogsForStats = allLogsForStats.Where(l =>
                        l.CreatedAtPk >= _startDate &&
                        l.CreatedAtPk <= _endDate.AddDays(1).AddTicks(-1));
                }

                // ✅ Calculate statistics BEFORE applying type filter
                CalculateLogStatistics(allLogsForStats);
                UpdateLogStatisticsDisplay();

                // ✅ Apply log type filter (for grid display only)
                if (!string.IsNullOrWhiteSpace(logTypeFilter))
                {
                    string filter = logTypeFilter.Trim().ToLower();

                    logs = logs.Where(l =>
                    {
                        var type = (l.Type ?? string.Empty).ToLower();

                        return filter switch
                        {
                            // Grouped filters
                            "error" or "exception" => type.Contains("error") || type.Contains("exception"),
                            "info" or "information" => type.Contains("info") || type.Contains("information"),
                            "warning" => type.Contains("warning"),
                            "success" => type.Contains("success"),
                            _ => true
                        };
                    });
                }

                var logsList = logs.OrderByDescending(l => l.CreatedAtPk).ToList();

                // ✅ Handle empty results after filter
                if (!logsList.Any())
                {
                    _logCache = new List<LogDisplayModel>();
                    LogsDataGridView.RowCount = 0;

                    WindowsLocalAppNotification.Show("Logs", $"No {logTypeFilter ?? "filtered"} logs available to display");
                    AlertManager.ShowWarning($"No {logTypeFilter ?? "filtered"} logs available to display");
                    return;
                }

                // ✅ Build log cache for display
                _logCache = logsList
                    .Select((log, index) => new LogDisplayModel
                    {
                        SerialNo = index + 1,
                        Message = log.Message ?? "No message",
                        Type = log.Type ?? "N/A",
                        DateCreated = log.CreatedAtPk.ToString("dd-MM-yyyy HH:mm:ss"),
                        DateCreatedRaw = log.CreatedAtPk
                    })
                    .ToList();

                // ✅ Refresh DataGridView safely
                LogsDataGridView.SuspendLayout();
                LogsDataGridView.RowCount = _logCache.Count;
                LogsDataGridView.ResumeLayout(false);
                LogsDataGridView.Invalidate();
                LogsDataGridView.Refresh();
                await Task.Yield();

                LogsDataGridView.ClearSelection();
                LogsDataGridView.CurrentCell = null;
            }
            catch (Exception ex)
            {
                CalculateLogStatistics(null);
                UpdateLogStatisticsDisplay();

                WindowsLocalAppNotification.Show("Logs Error", $"Error loading logs: {ex.Message}");
                AlertManager.ShowError($"Error loading logs: {ex.Message}");
            }
        }

        private async void btnExportInvoice_Click(object sender, EventArgs e)
        {
            try
            {
                if (_invoiceCache == null || _invoiceCache.Count == 0)
                {
                    MessageBox.Show("No invoices available to export.", "Export Invoices", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = "CSV Files (*.csv)|*.csv";
                    sfd.FileName = $"Local_Invoices_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    sfd.OverwritePrompt = true;

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        StringBuilder csvContent = new StringBuilder();
                        csvContent.AppendLine("Sr. No.,POS ID,Invoice Number,Is Synced,Date Created");

                        foreach (var invoice in _invoiceCache)
                        {
                            csvContent.AppendLine($"{invoice.SerialNo},{invoice.PosId},{EscapeCsvField(invoice.InvoiceNumber)},{invoice.IsSynced},{invoice.DateCreated}");
                        }

                        await File.WriteAllTextAsync(sfd.FileName, csvContent.ToString(), Encoding.UTF8);

                        MessageBox.Show($"Invoices exported successfully!\n\nFile saved to:\n{sfd.FileName}",
                            "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Access denied. Please choose a different location or check file permissions.",
                    "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IOException ioEx)
            {
                MessageBox.Show($"File error: {ioEx.Message}\n\nPlease ensure the file is not open in another program.",
                    "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting invoices: {ex.Message}",
                    "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "";

            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }

            return field;
        }

        private async void btnSyncLogs_Click(object sender, EventArgs e)
        {
            btnSyncLogs.Enabled = false;
            btnSyncLogs.Text = "Syncing...";
            try
            {
                await _sendLogToCloudService.SyncLogAsync();
            }
            catch (Exception ex)
            {
                AlertManager.ShowError($"Error syncing logs: {ex.Message}");
                WindowsLocalAppNotification.Show("Logs Sync Error", $"Error syncing logs: {ex.Message}");
            }
            finally
            {
                btnSyncLogs.Enabled = true;
                btnSyncLogs.Text = "Sync Logs";
            }
        }

        private List<LogDto> MergeLogs(IEnumerable<LogDto>? localLogs, IEnumerable<LogDto>? cloudLogs)
        {
            var merged = new List<LogDto>();

            if (localLogs != null)
                merged.AddRange(localLogs);

            if (cloudLogs != null)
                merged.AddRange(cloudLogs);

            var deduped = merged
                .GroupBy(l => new
                {
                    Message = l.Message?.Trim() ?? "",
                    Type = l.Type?.Trim() ?? "",
                    Timestamp = l.CreatedAtPk.ToString("yyyy-MM-dd HH:mm:ss")
                })
                .Select(g => g.First())
                .OrderByDescending(l => l.CreatedAtPk)
                .ToList();

            return deduped;
        }

        private async void btnExportLogs_Click(object sender, EventArgs e)
        {
            try
            {
                if (_logCache == null || _logCache.Count == 0)
                {
                    MessageBox.Show("No logs available to export.", "Export Logs", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = "CSV Files (*.csv)|*.csv";
                    sfd.FileName = $"Logs_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        StringBuilder csvContent = new StringBuilder();
                        csvContent.AppendLine("Sr. No.,Message,Type,Date/Time");

                        foreach (var log in _logCache)
                        {
                            csvContent.AppendLine($"{log.SerialNo},{EscapeCsvField(log.Message)},{log.Type},{log.DateCreated}");
                        }

                        await File.WriteAllTextAsync(sfd.FileName, csvContent.ToString(), Encoding.UTF8);

                        MessageBox.Show("Logs exported successfully!", "Export Logs", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting logs: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ResetSortToDefault()
        {
            foreach (DataGridViewColumn col in InvoicesDataGridView.Columns)
                col.HeaderCell.SortGlyphDirection = SortOrder.None;

            _isSyncedSortDescending = true;
            btnFilterSynced.Text = "Show Synced First";
        }

        private async void btnFilterInvoices_Click(object sender, EventArgs e) => await RunSingleLoad(async () =>
        {
            await LoadAndShowInvoicesAsync();
            await LoadAndShowLogsAsync();
        });

        private void btnFilterSynced_Click(object sender, EventArgs e)
        {
            if (_invoiceCache == null || _invoiceCache.Count == 0) return;

            if (_isSyncedSortDescending)
            {
                _invoiceCache = _invoiceCache.OrderByDescending(i => i.IsSynced).ToList();
                btnFilterSynced.Text = "Show Unsynced First";
            }
            else
            {
                _invoiceCache = _invoiceCache.OrderBy(i => i.IsSynced).ToList();
                btnFilterSynced.Text = "Show Synced First";
            }

            _isSyncedSortDescending = !_isSyncedSortDescending;
            InvoicesDataGridView.Invalidate();
        }

        private async void btnToday_Click(object sender, EventArgs e)
        {
            ResetSortToDefault();
            _startDate = DateTime.Today;
            _endDate = DateTime.Today;
            dtpStartDate.Value = _startDate;
            dtpEndDate.Value = _endDate;
            UpdateDateRangeLabel();

            await RunSingleLoad(async () =>
            {
                await LoadAndShowInvoicesAsync(skipDateFilter: false);
                await LoadAndShowLogsAsync(skipDateFilter: false);
            });
        }

        private async void btnClearFilter_Click(object sender, EventArgs e)
        {
            ResetSortToDefault();
            _filterSyncedOnly = false;

            _startDate = DateTime.Today.AddDays(-7);
            _endDate = DateTime.Today;
            dtpStartDate.Value = _startDate;
            dtpEndDate.Value = _endDate;
            UpdateDateRangeLabel();

            await RunSingleLoad(async () =>
            {
                await LoadAndShowInvoicesAsync(skipDateFilter: false);
                await LoadAndShowLogsAsync(skipDateFilter: false);
            });
        }
        private void ClearDataGridView(DataGridView dataGridView)
        {
            if (dataGridView == null) return;

            try
            {
                dataGridView.SuspendLayout();
                dataGridView.DataSource = null;
                dataGridView.RowCount = 0;
                dataGridView.ClearSelection();
                dataGridView.CurrentCell = null;

                GC.Collect();
                GC.WaitForPendingFinalizers();

                dataGridView.Refresh();
                dataGridView.Invalidate();
                Application.DoEvents();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing DataGridView: {ex.Message}");
            }
            finally
            {
                dataGridView.ResumeLayout(true);
            }
        }

        private void FixSerialNumberHeaderColor()
        {
            InvoicesDataGridView.ClearSelection();
            LogsDataGridView.ClearSelection();

            if (InvoicesDataGridView.Rows.Count > 0)
            {
                InvoicesDataGridView.CurrentCell = null;
            }

            if (LogsDataGridView.Rows.Count > 0)
            {
                LogsDataGridView.CurrentCell = null;
            }
        }

        private void ClearAllGrids()
        {
            ClearDataGridView(InvoicesDataGridView);
            ClearDataGridView(LogsDataGridView);

            labelAllInvoices.Text = "0";
            labelPendingInvoice.Text = "0";
            labelPaidInvoices.Text = "0";

            this.Refresh();
            Application.DoEvents();
        }


        // Disables the button for 3 seconds but doesn't block the UI.
        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            // Disable the button right away (no delay)
            btnRefresh.Enabled = false;

            ResetSortToDefault();

            bool wasAutoRefreshEnabled = _autoRefreshEnabled;
            if (wasAutoRefreshEnabled)
                _autoRefreshTimer.Stop();

            try
            {
                _invoiceCache?.Clear();
                _logCache?.Clear();
                InvoicesDataGridView.RowCount = 0;
                LogsDataGridView.RowCount = 0;

                bool skipDateFilter = false;
                _filterSyncedOnly = false;

                // Refresh the data immediately
                await RunSingleLoad(async () =>
                {
                    await LoadAndShowInvoicesAsync(skipDateFilter);
                    await LoadAndShowLogsAsync(skipDateFilter: skipDateFilter);
                });

                _lastInvoiceCount = _invoiceCache?.Count ?? 0;
                _lastSyncedInvoiceCount = _invoiceCache?.Count(i => i.IsSynced == "Yes") ?? 0;
                _lastLogCount = _logCache?.Count ?? 0;
            }
            catch (Exception ex)
            {
                AlertManager.ShowError($"Error refreshing data: {ex.Message}");
            }
            finally
            {
                if (wasAutoRefreshEnabled)
                    _autoRefreshTimer.Start();

                // Re-enable the button after 3 seconds
                _ = Task.Run(async () =>
                {
                    await Task.Delay(3000);
                    if (btnRefresh.IsHandleCreated)
                    {
                        btnRefresh.Invoke(() => btnRefresh.Enabled = true);
                    }
                });
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (_autoRefreshTimer != null)
            {
                _autoRefreshTimer.Stop();
                _autoRefreshTimer.Dispose();
                _heartbeatTimer?.Stop();
                _heartbeatTimer?.Dispose();
            }
        }

        private void dataGridView_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
            if (sender is DataGridView grid && e.RowIndex >= 0 && e.ColumnIndex >= 0)
                grid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "N/A";
        }

        private void SetButtonImage(Button btn, Image img, Color bgColor)
        {
            if (btn == null) return;

            btn.BackColor = bgColor;

            if (img == null)
            {
                btn.Image = null;
                return;
            }

            if (btn.Image != null)
            {
                btn.Image.Dispose();
                btn.Image = null;
            }

            int padding = 8;
            int maxWidth = btn.Width - padding;
            int maxHeight = btn.Height - padding;

            double ratioX = (double)maxWidth / img.Width;
            double ratioY = (double)maxHeight / img.Height;
            double ratio = Math.Min(ratioX, ratioY);

            int newWidth = (int)(img.Width * ratio);
            int newHeight = (int)(img.Height * ratio);

            Image resized = new Bitmap(img, new Size(newWidth, newHeight));

            btn.Image = resized;
            btn.ImageAlign = ContentAlignment.MiddleCenter;
            btn.Text = "";
            btn.FlatStyle = FlatStyle.Flat;
            btn.BackgroundImageLayout = ImageLayout.None;
        }

        private void AddImageToPanelRight(Panel panel, Image image, int width = 60)
        {
            if (panel == null || image == null) return;

            PictureBox pic = new PictureBox
            {
                Image = image,
                SizeMode = PictureBoxSizeMode.Zoom,
                Dock = DockStyle.Right,
                Width = width,
                Margin = new Padding(5)
            };

            panel.Controls.Add(pic);

            foreach (Control ctrl in panel.Controls)
            {
                if (ctrl != pic && ctrl is Label lbl)
                {
                    lbl.Dock = DockStyle.None;
                    lbl.AutoSize = false;
                    lbl.ForeColor = Color.White;

                    if (lbl.Font.Size > 20)
                    {
                        lbl.TextAlign = ContentAlignment.TopLeft;
                        lbl.Location = new Point(15, 15);
                        lbl.Width = panel.Width - width - 30;
                        lbl.Height = (int)(panel.Height * 0.6);
                    }
                    else
                    {
                        lbl.TextAlign = ContentAlignment.MiddleLeft;
                        lbl.Location = new Point(15, (int)(panel.Height * 0.55));
                        lbl.Width = panel.Width - width - 30;
                        lbl.Height = (int)(panel.Height * 0.35);
                    }
                }
            }
        }

        private void ApplyGradientBackground(Control control, Color startColor, Color endColor)
        {
            control.Paint += (s, e) =>
            {
                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    control.ClientRectangle,
                    startColor,
                    endColor,
                    System.Drawing.Drawing2D.LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(brush, control.ClientRectangle);
                }
            };

            control.BackColor = Color.Transparent;
            control.Invalidate();
        }

        private void StyleDataGridView(DataGridView dgv)
        {
            dgv.BorderStyle = BorderStyle.None;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.GridColor = Color.FromArgb(226, 232, 240);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(59, 130, 246);
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv.BackgroundColor = Color.White;
            dgv.RowHeadersVisible = false;
            dgv.EnableHeadersVisualStyles = false;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeRows = false;
            dgv.ReadOnly = true;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(48, 59, 78);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(0, 10, 0, 10);
            //dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv.ColumnHeadersHeight = 52;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = dgv.ColumnHeadersDefaultCellStyle.BackColor;
            dgv.ColumnHeadersDefaultCellStyle.SelectionForeColor = dgv.ColumnHeadersDefaultCellStyle.ForeColor;

            dgv.DefaultCellStyle.BackColor = Color.White;
            dgv.DefaultCellStyle.ForeColor = Color.FromArgb(55, 65, 81);
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 10F);
            dgv.DefaultCellStyle.Padding = new Padding(0, 4, 0, 4);
            dgv.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.False;

            dgv.RowTemplate.Height = 40;
            dgv.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
        }

        private void StyleInvoicesDataGridView()
        {
            InvoicesDataGridView.Columns.Clear();
            StyleDataGridView(InvoicesDataGridView);

            var colId = new DataGridViewTextBoxColumn
            {
                Name = "colId",
                HeaderText = "  Sr No",
                Width = 120,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.Automatic,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.FromArgb(107, 114, 128),
                    Font = new Font("Segoe UI", 9F)
                }
            };
            colId.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colId.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

            var colPosId = new DataGridViewTextBoxColumn
            {
                Name = "colPosId",
                HeaderText = "     POS ID",
                Width = 120,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.Automatic,
            };
            colPosId.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colPosId.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

            var colInvoiceNumber = new DataGridViewTextBoxColumn
            {
                Name = "colInvoiceNumber",
                HeaderText = "Invoice Number",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.Automatic,
            };

            var colIsSynced = new DataGridViewTextBoxColumn
            {
                Name = "colIsSynced",
                HeaderText = " Invoice Synced",
                Width = 180,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.Automatic,
            };
            colIsSynced.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colIsSynced.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

            var colPrint = new DataGridViewTextBoxColumn
            {
                Name = "colPrint",
                HeaderText = "     View",
                Width = 180,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.Automatic,
            };
            colPrint.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colPrint.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

            var colDateCreated = new DataGridViewTextBoxColumn
            {
                Name = "colDateCreated",
                HeaderText = "Date Created",
                Width = 220,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.Automatic,
            };
            colDateCreated.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colDateCreated.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

            InvoicesDataGridView.Columns.AddRange(new DataGridViewColumn[]
            {
                colId, colPosId, colInvoiceNumber, colIsSynced, colPrint, colDateCreated
            });

            InvoicesDataGridView.CellPainting += InvoicesDataGridView_CellPainting;
            InvoicesDataGridView.CellMouseEnter += InvoicesDataGridView_CellMouseEnter;
            InvoicesDataGridView.CellMouseLeave += InvoicesDataGridView_CellMouseLeave;
            InvoicesDataGridView.CellClick += InvoicesDataGridView_CellClick;
            InvoicesDataGridView.CellMouseMove += InvoicesDataGridView_CellMouseMove;
        }

        private void InvoicesDataGridView_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == InvoicesDataGridView.Columns["colPrint"].Index)
            {
                _hoveredCell = InvoicesDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
                InvoicesDataGridView.InvalidateCell(_hoveredCell);
            }
        }

        private void InvoicesDataGridView_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (_hoveredCell != null)
            {
                var cell = _hoveredCell;
                _hoveredCell = null;
                InvoicesDataGridView.InvalidateCell(cell);
            }
        }

        private void InvoicesDataGridView_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == InvoicesDataGridView.Columns["colPrint"].Index)
            {
                var cellBounds = InvoicesDataGridView.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
                var mousePoint = new Point(e.X + cellBounds.X, e.Y + cellBounds.Y);

                if (_printLinkBounds.Contains(mousePoint))
                {
                    InvoicesDataGridView.Cursor = Cursors.Hand;
                }
                else
                {
                    InvoicesDataGridView.Cursor = Cursors.Default;
                }
            }
            else
            {
                InvoicesDataGridView.Cursor = Cursors.Default;
            }
        }

        private async void InvoicesDataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // 1. Validate the click
            if (e.RowIndex < 0 || e.ColumnIndex != InvoicesDataGridView.Columns["colPrint"].Index)
                return;

            var mousePos = InvoicesDataGridView.PointToClient(Cursor.Position);
            if (!_printLinkBounds.Contains(mousePos))
                return;

            if (_invoiceCache == null || e.RowIndex >= _invoiceCache.Count)
                return;

            var invoice = _invoiceCache[e.RowIndex];
            var invoiceNumber = invoice.InvoiceNumber;

            _printingRowIndex = e.RowIndex;

            try
            {
                // 2. UI setup
                InvoicesDataGridView.InvalidateCell(e.ColumnIndex, e.RowIndex);
                InvoicesDataGridView.Enabled = false;
                this.Cursor = Cursors.WaitCursor;

                if (progressBar != null)
                {
                    CenterProgressBar();
                    progressBar.Style = ProgressBarStyle.Marquee;
                    progressBar.MarqueeAnimationSpeed = 30;
                    progressBar.Visible = true;
                    progressBar.BringToFront();
                }

                // 3. Load invoice data from API/service
                var response = await Task.Run(() =>
                    _invoiceService.GetInvoiceWithItems(invoiceNumber).GetAwaiter().GetResult());


                if (response?.Data == null)
                {
                    MessageBox.Show("⚠️ No data found for this invoice.",
                        "Data Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 4. Print thread setup
                var tcs = new TaskCompletionSource<object?>();

                response.Data.InvoiceNumber = invoiceNumber;

                Thread printThread = new Thread(() =>
                {
                    try
                    {
                        using (var printForm = new InvoiceReport(response.Data))
                        {
                            // Hide progress bar only after RDLC finishes rendering
                            printForm.ReportLoaded += (s, args) =>
                            {
                                this.Invoke(new Action(() =>
                                {
                                    if (progressBar != null)
                                    {
                                        progressBar.Visible = false;
                                        progressBar.Style = ProgressBarStyle.Continuous;
                                    }
                                }));
                            };

                            printForm.FormClosed += (s, args) => tcs.TrySetResult(null);
                            Application.Run(printForm);
                        }
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                    }
                });

                printThread.SetApartmentState(ApartmentState.STA);
                printThread.IsBackground = true;
                printThread.Start();

                await tcs.Task;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error viewing invoice: {ex.Message}",
                    "View Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // 5. UI cleanup
                _printingRowIndex = -1;
                InvoicesDataGridView.Enabled = true;
                this.Cursor = Cursors.Default;

                if (progressBar != null)
                {
                    progressBar.Visible = false;
                    progressBar.Style = ProgressBarStyle.Continuous;
                }

                InvoicesDataGridView.InvalidateCell(e.ColumnIndex, e.RowIndex);
            }
        }


        private void InvoicesDataGridView_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == InvoicesDataGridView.Columns["colIsSynced"]?.Index)
            {
                e.PaintBackground(e.CellBounds, true);

                string value = e.Value?.ToString() ?? "";

                // Use cached images
                Image icon = value.Equals("Yes", StringComparison.OrdinalIgnoreCase)
                    ? _greenTickCached
                    : _redCrossCached;

                if (icon != null)
                {
                    int iconSize = 28;
                    int x = e.CellBounds.X + (e.CellBounds.Width - iconSize) / 2;
                    int y = e.CellBounds.Y + (e.CellBounds.Height - iconSize) / 2;

                    e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
                    e.Graphics.DrawImage(icon, new Rectangle(x, y, iconSize, iconSize));
                }

                e.Handled = true;
            }

            if (e.RowIndex >= 0 && e.ColumnIndex == InvoicesDataGridView.Columns["colPrint"]?.Index)
            {
                e.PaintBackground(e.CellBounds, true);

                bool isHovered = _hoveredCell != null &&
                                _hoveredCell.RowIndex == e.RowIndex &&
                                _hoveredCell.ColumnIndex == e.ColumnIndex;

                bool isSelected = InvoicesDataGridView.Rows[e.RowIndex].Selected;
                bool isPrinting = _printingRowIndex == e.RowIndex;

                string linkText = isPrinting ? "Viewing..." : "View";

                Color linkColor;
                if (isSelected)
                {
                    linkColor = Color.White;
                }
                else if (isPrinting)
                {
                    linkColor = Color.Gray;
                }
                else
                {
                    linkColor = isHovered ? Color.FromArgb(34, 197, 94) : Color.FromArgb(59, 130, 246);
                }

                FontStyle fontStyle = isHovered && !isPrinting ? (FontStyle.Bold | FontStyle.Underline) : FontStyle.Bold;

                using (var font = new Font("Segoe UI", 9.5F, fontStyle))
                {
                    var textSize = e.Graphics.MeasureString(linkText, font);
                    float x = e.CellBounds.X + (e.CellBounds.Width - textSize.Width) / 2;
                    float y = e.CellBounds.Y + (e.CellBounds.Height - textSize.Height) / 2;

                    _printLinkBounds = new Rectangle(
                        (int)x,
                        (int)y,
                        (int)textSize.Width,
                        (int)textSize.Height
                    );

                    using (var brush = new SolidBrush(linkColor))
                    {
                        e.Graphics.DrawString(linkText, font, brush, x, y);
                    }
                }

                e.Handled = true;
            }

            if (e.RowIndex >= 0 && e.ColumnIndex == InvoicesDataGridView.Columns["colInvoiceNumber"]?.Index)
            {
                e.PaintBackground(e.CellBounds, true);

                string value = e.Value?.ToString() ?? "";

                using (var font = new Font("Segoe UI", 10F, FontStyle.Bold))
                using (var brush = new SolidBrush(e.CellStyle.ForeColor))
                {
                    var stringFormat = new StringFormat
                    {
                        Alignment = StringAlignment.Near,
                        LineAlignment = StringAlignment.Center,
                        Trimming = StringTrimming.EllipsisCharacter
                    };

                    var textRect = new RectangleF(
                        e.CellBounds.X + 8,
                        e.CellBounds.Y,
                        e.CellBounds.Width - 16,
                        e.CellBounds.Height
                    );

                    e.Graphics.DrawString(value, font, brush, textRect, stringFormat);
                }

                e.Handled = true;
            }
        }

        private void StyleLogsDataGridView()
        {
            LogsDataGridView.Columns.Clear();
            StyleDataGridView(LogsDataGridView);

            var colLogID = new DataGridViewTextBoxColumn
            {
                Name = "colLogID",
                HeaderText = "   Sr No",
                Width = 120,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.Automatic,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.FromArgb(107, 114, 128),
                    Font = new Font("Segoe UI", 9F),
                    Alignment = DataGridViewContentAlignment.MiddleCenter // Ensure cell content is centered
                }
            };

            // Force header alignment more explicitly
            colLogID.HeaderCell.Style = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(48, 59, 78),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Padding = new Padding(0, 10, 0, 10)
            };

            var colMessage = new DataGridViewTextBoxColumn
            {
                Name = "colMessage",
                HeaderText = "Message",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.Automatic,
            };

            var colException = new DataGridViewTextBoxColumn
            {
                Name = "colException",
                HeaderText = "Type",
                Width = 170,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.Automatic,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                }
            };

            colException.HeaderCell.Style = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(48, 59, 78),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Padding = new Padding(0, 10, 0, 10)
            };

            var colDateTime = new DataGridViewTextBoxColumn
            {
                Name = "logdatetime",
                HeaderText = "Date/Time",
                Width = 220,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.Automatic,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                }
            };

            colDateTime.HeaderCell.Style = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(48, 59, 78),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Padding = new Padding(0, 10, 0, 10)
            };

            LogsDataGridView.Columns.AddRange(new DataGridViewColumn[]
            {
        colLogID, colMessage, colException, colDateTime
            });

            LogsDataGridView.CellPainting += LogsDataGridView_CellPainting;

            foreach (DataGridViewColumn column in LogsDataGridView.Columns)
            {
                if (column.Name == "colLogID" || column.Name == "colException" || column.Name == "logdatetime")
                {
                    column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
            }
        }
        private void LogsDataGridView_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex == 2 && e.RowIndex >= 0)
            {
                var value = e.Value?.ToString() ?? "";
                Color badgeColor = Color.FromArgb(209, 250, 229);
                Color textColor = Color.FromArgb(5, 150, 105);

                if (value.Equals("Information", StringComparison.OrdinalIgnoreCase) ||
                    value.Equals("Info", StringComparison.OrdinalIgnoreCase))
                {
                    badgeColor = Color.FromArgb(219, 234, 254);
                    textColor = Color.FromArgb(37, 99, 235);
                }
                else if (value.Equals("Warning", StringComparison.OrdinalIgnoreCase))
                {
                    badgeColor = Color.FromArgb(254, 243, 199);
                    textColor = Color.FromArgb(217, 119, 6);
                }
                else if (value.Equals("Exception", StringComparison.OrdinalIgnoreCase) ||
                         value.Equals("Error", StringComparison.OrdinalIgnoreCase))
                {
                    badgeColor = Color.FromArgb(254, 226, 226);
                    textColor = Color.FromArgb(220, 38, 38);
                }

                if (!string.IsNullOrEmpty(value))
                {
                    e.PaintBackground(e.CellBounds, true);

                    int badgeWidth = 120;
                    int padding = 8;
                    int badgeHeight = e.CellBounds.Height - padding;

                    int x = e.CellBounds.X + 15;
                    int y = e.CellBounds.Y + (e.CellBounds.Height - badgeHeight) / 2;

                    using (var path = GetRoundedRect(new Rectangle(x, y, badgeWidth, badgeHeight), 6))
                    {
                        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        using (var brush = new SolidBrush(badgeColor))
                        {
                            e.Graphics.FillPath(brush, path);
                        }
                    }

                    TextRenderer.DrawText(
                        e.Graphics,
                        value,
                        new Font("Segoe UI", 8.5F, FontStyle.Regular),
                        new Rectangle(x, y, badgeWidth, badgeHeight),
                        textColor,
                        TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter
                    );

                    e.Handled = true;
                }
            }
        }

        private System.Drawing.Drawing2D.GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            int diameter = radius * 2;

            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }

        private void ApplyDataGridStyles()
        {
            StyleInvoicesDataGridView();
            StyleLogsDataGridView();
        }

        private void Panel_Resize(object? sender, EventArgs e)
        {
            if (sender is Panel panel)
            {
                // Only redraw if data actually changed OR chart doesn't exist
                if (_cachedPieChart == null ||
                    _lastPieChartSynced != _syncedCount ||
                    _lastPieChartPending != _pendingCount)
                {
                    DrawInvoicePieChart(panel, _syncedCount, _pendingCount);
                    _lastPieChartSynced = _syncedCount;
                    _lastPieChartPending = _pendingCount;
                }
            }
        }
        private void DrawInvoicePieChart(Panel panel, int syncedCount, int pendingCount)
        {
            if (panel == null) return;

            int panelWidth = panel.Width;
            int panelHeight = panel.Height;

            if (panelWidth <= 0 || panelHeight <= 0) return;

            panel.Controls.Clear();

            List<int> values = new List<int> { syncedCount, pendingCount };
            List<Color> colors = new List<Color>
            {
                ColorTranslator.FromHtml("#66BB6A"),
                ColorTranslator.FromHtml("#EF5350")
            };
            List<string> labels = new List<string> { "Synced", "Not Synced" };

            float total = values.Sum();
            if (total == 0) total = 1;

            Label header = new Label
            {
                Text = "ALL INVOICES DETAIL",
                AutoSize = false,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.Black,
                TextAlign = ContentAlignment.MiddleCenter,
                Width = panelWidth,
                Height = 25
            };
            header.Location = new Point(0, 8);
            panel.Controls.Add(header);

            int legendHeight = 40;
            int availableHeight = panelHeight - header.Bottom - legendHeight - 20;
            int pieSize = (int)(Math.Min(panelWidth, availableHeight) * 0.75);

            if (pieSize <= 0) return;

            int pieX = (panelWidth - pieSize) / 2;
            int pieY = header.Bottom + ((availableHeight - pieSize) / 2) + 10;

            Bitmap bmp = new Bitmap(pieSize, pieSize);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                Rectangle rect = new Rectangle(0, 0, pieSize, pieSize);
                float startAngle = 0;

                for (int i = 0; i < values.Count; i++)
                {
                    float sweepAngle = values[i] / total * 360f;
                    using (Brush brush = new SolidBrush(colors[i]))
                        g.FillPie(brush, rect, startAngle, sweepAngle);
                    startAngle += sweepAngle;
                }
            }

            PictureBox pic = new PictureBox
            {
                Image = bmp,
                SizeMode = PictureBoxSizeMode.Normal,
                Width = pieSize,
                Height = pieSize,
                Location = new Point(pieX, pieY),
                BackColor = Color.Transparent
            };
            panel.Controls.Add(pic);

            int legendY = panelHeight - legendHeight + 5;
            int spacingX = 120;
            int totalLegendWidth = values.Count * spacingX;
            int legendX = (panelWidth - totalLegendWidth) / 2;
            int boxSize = 14;

            for (int i = 0; i < values.Count; i++)
            {
                int offsetX = i * spacingX;

                Panel colorBox = new Panel
                {
                    BackColor = colors[i],
                    Size = new Size(boxSize, boxSize),
                    Location = new Point(legendX + offsetX, legendY)
                };
                panel.Controls.Add(colorBox);

                Label lbl = new Label
                {
                    Text = $"{labels[i]}: {values[i]}",
                    ForeColor = Color.Black,
                    Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                    Location = new Point(colorBox.Right + 5, colorBox.Top - 1),
                    AutoSize = true,
                    BackColor = Color.Transparent
                };
                panel.Controls.Add(lbl);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateLogStatisticsLayout();
        }
    }

    // ===== DISPLAY MODELS (ADD AT END OF FILE) =====
    public class InvoiceDisplayModel
    {
        public int SerialNo { get; set; }
        public int PosId { get; set; }
        public string InvoiceNumber { get; set; }
        public string IsSynced { get; set; }
        public string DateCreated { get; set; }
        public int IsSyncedRaw { get; set; }
        public int AttemptCount { get; set; }
        public DateTime DateCreatedRaw { get; set; }
    }

    public class LogDisplayModel
    {
        public int SerialNo { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public string DateCreated { get; set; }
        public DateTime DateCreatedRaw { get; set; }
    }
}