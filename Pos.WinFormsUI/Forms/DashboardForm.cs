using Microsoft.Extensions.DependencyInjection;
using Pos.Application.DTOs;
using Pos.Application.DTOs.ClientDtos;
using Pos.Application.DTOs.CommanDtos;
using Pos.Application.DTOs.FiscalDtos;
using Pos.Application.DTOs.LogDtos;
using Pos.Application.Services.CloudSyncService.CloudSyncLogService;
using Pos.Application.Services.FileRecordService;
using Pos.Application.Services.InvoiceService;
using Pos.Application.Services.LogService;
using Pos.Application.Utility;
using Pos.DTOs.LogDTOs;
using Pos.SecurityEncryption;
using Pos.WinFormsUI.AlertClasses;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing.Drawing2D;
using System.Net.Http.Json;
using System.Text;
namespace Pos.WinFormsUI.Forms
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

        private int currentInvoicePage = 1;
        private int invoiceRecordsPerPage = 500;
        private int totalInvoicePages = 1;
        private int totalInvoiceRecords = 0;

        private int currentLogsPage = 1;
        private int logsRecordsPerPage = 500;
        private int totalLogsPages = 1;
        private int totalLogsRecords = 0;

        private ModernPaginationControl paginationInvoices;
        private ModernPaginationControl paginationLogs;

        private Dictionary<string, Point> _originalButtonPositions = new Dictionary<string, Point>();


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

            InitializePaginationControls();
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

        private void InitializePaginationControls()
        {
            // Initialize button position storage if not already done
            if (_originalButtonPositions == null)
            {
                _originalButtonPositions = new Dictionary<string, Point>();
            }

            // ----- INVOICES PAGINATION -----
            paginationInvoices = new ModernPaginationControl
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            // Position pagination responsively
            PositionInvoicePagination();

            paginationInvoices.PageChanged += async (s, page) =>
            {
                currentInvoicePage = page;
                await RunSingleLoad(async () => await LoadAndShowInvoicesAsync());
            };

            panelInvoices.Controls.Add(paginationInvoices);
            paginationInvoices.BringToFront();

            // Handle resize events
            panelInvoices.Resize += (s, e) => PositionInvoicePagination();
            this.Resize += (s, e) => PositionInvoicePagination();


            // ----- LOGS PAGINATION -----
            paginationLogs = new ModernPaginationControl
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            PositionLogsPagination();

            paginationLogs.PageChanged += async (s, page) =>
            {
                currentLogsPage = page;
                await RunSingleLoad(async () => await LoadAndShowLogsAsync());
            };

            panelLogs.Controls.Add(paginationLogs);
            paginationLogs.BringToFront();

            // Handle resize events
            panelLogs.Resize += (s, e) => PositionLogsPagination();
            this.Resize += (s, e) => PositionLogsPagination();
        }

        private void PositionInvoicePagination()
        {
            if (paginationInvoices == null || panelInvoices == null) return;

            var lblInvoices = panelInvoices.Controls
                .OfType<Label>()
                .FirstOrDefault(l => l.Text.Contains("INVOICES", StringComparison.OrdinalIgnoreCase));

            if (lblInvoices == null) return;

            int screenWidth = this.Width;
            int availableWidth = panelInvoices.Width;

            // Handle button visibility and repositioning based on screen width
            HandleInvoiceButtonLayout(screenWidth);

            // Calculate available space after the label
            int spaceAfterLabel = availableWidth - lblInvoices.Right;

            // Pagination control width (approximately 250px)
            int paginationWidth = 250;

            // Minimum safe spacing
            int minSpacing = 5;

            //if (screenWidth <= 1280)
            //{
            //    // For small screens, position below the label
            //    paginationInvoices.Location = new Point(lblInvoices.Left, lblInvoices.Bottom + 5);
            //}
            //else if (spaceAfterLabel < paginationWidth + minSpacing)
            //{
            //    // Not enough horizontal space, position below
            //    paginationInvoices.Location = new Point(lblInvoices.Left, lblInvoices.Bottom + 5);
            //}
            //else
            //{
            // Enough space, position next to label
            paginationInvoices.Location = new Point(lblInvoices.Right + minSpacing, lblInvoices.Top - 3);
            //}
        }
        private void HandleInvoiceButtonLayout(int screenWidth)
        {

            // Find the buttons and calendar control
            if (btnFilterSynced == null || btnExportInvoice == null) return;

            // Find the date range label (calendar control)
            Control lblDateRange = this.Controls.Find("lblDateRange", true).FirstOrDefault();
            if (lblDateRange == null) return;

            // Store original sizes on first call
            if (!_originalButtonPositions.ContainsKey("btnFilterSyncedSize"))
            {
                _originalButtonPositions["btnFilterSyncedSize"] = new Point(btnFilterSynced.Width, btnFilterSynced.Height);
            }

            if (!_originalButtonPositions.ContainsKey("btnExportInvoiceSize"))
            {
                _originalButtonPositions["btnExportInvoiceSize"] = new Point(btnExportInvoice.Width, btnExportInvoice.Height);
            }

            // Define spacing between buttons and calendar
            int spacingBetweenButtons = 10; // Space between Export and FilterSynced
            int spacingToCalendar = 5;     // Space between buttons and calendar

            // Threshold for button overlap
            int overlapThreshold = 1366;

            if (screenWidth <= overlapThreshold)
            {
                btnExportInvoice.Text = "  EXPORT  ";
                btnExportInvoice.Font = new Font("Arial", 10.8f);
                btnExportInvoice.AutoSize = true;


                // COMPACT MODE: Hide btnFilterSynced
                btnFilterSynced.Visible = false;

                // Position btnExportInvoice to the left of calendar
                int exportLeft = lblDateRange.Left - _originalButtonPositions["btnExportInvoiceSize"].X - spacingToCalendar;

                btnExportInvoice.Location = new Point(
                    exportLeft,
                    lblDateRange.Top + ((lblDateRange.Height - _originalButtonPositions["btnExportInvoiceSize"].Y) / 2) // Vertically center with calendar
                );

                btnExportInvoice.Size = new Size(
                    _originalButtonPositions["btnExportInvoiceSize"].X,
                    _originalButtonPositions["btnExportInvoiceSize"].Y
                );
            }
            else
            {
                btnExportInvoice.Font = new Font("Arial", 12F);
                btnExportInvoice.Text = "📄 Export Invoices";


                // 
                // NORMAL MODE: Show both buttons
                btnFilterSynced.Visible = true;


                // Position btnFilterSynced to the left of calendar
                int syncedLeft = lblDateRange.Left - _originalButtonPositions["btnFilterSyncedSize"].X - spacingToCalendar;

                btnFilterSynced.Location = new Point(
                    syncedLeft,
                    lblDateRange.Top + ((lblDateRange.Height - _originalButtonPositions["btnFilterSyncedSize"].Y) / 2) // Vertically center with calendar
                );

                btnFilterSynced.Size = new Size(190, 37);


                // Position btnExportInvoice to the left of btnFilterSynced
                int exportLeft = btnFilterSynced.Left - _originalButtonPositions["btnExportInvoiceSize"].X - spacingBetweenButtons;

                btnExportInvoice.Location = new Point(
                    exportLeft,
                    lblDateRange.Top + ((lblDateRange.Height - _originalButtonPositions["btnExportInvoiceSize"].Y) / 2) // Vertically center with calendar
                );

                btnExportInvoice.Size = new Size(182, 37);

            }
        }
        private void PositionLogsPagination()
        {
            if (paginationLogs == null || panelLogs == null) return;

            var lblLogs = panelLogs.Controls
                .OfType<Label>()
                .FirstOrDefault(l => l.Text.Contains("LOGS", StringComparison.OrdinalIgnoreCase));

            if (lblLogs == null) return;

            int screenWidth = this.Width;
            int availableWidth = panelLogs.Width;

            // Calculate available space after the label
            int spaceAfterLabel = availableWidth - lblLogs.Right;

            // Pagination control width (approximately 250px)
            int paginationWidth = 250;

            // Minimum safe spacing
            int minSpacing = 15;

            //if (screenWidth <= 1280)
            //{
            //    // For small screens, position below the label
            //    paginationLogs.Location = new Point(lblLogs.Left, lblLogs.Bottom + 5);
            //}
            //else if (spaceAfterLabel < paginationWidth + minSpacing)
            //{
            //    // Not enough horizontal space, position below
            //    paginationLogs.Location = new Point(lblLogs.Left, lblLogs.Bottom + 5);
            //}
            //else
            //{
            // Enough space, position next to label
            paginationLogs.Location = new Point(lblLogs.Right + minSpacing, lblLogs.Top - 3);
            //}
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
                // Get environment and token from appsettings
                string selectedEnvironment = ConfigurationManager.AppSettings["Environment"]!;
                string token = ConfigurationManager.AppSettings["Token"]!; // CHANGED: get token from appsettings

                var fullUrl = $"{_baseUrl}{Endpoints.HeartBeat}";

                // Prepare request body
                var requestBody = new GetByPosIdDto
                {
                    PosId = posId,
                    Environment = selectedEnvironment
                };
                var json = JsonContent.Create(requestBody);

                // Create HttpRequestMessage to add headers
                using var request = new HttpRequestMessage(HttpMethod.Post, fullUrl)
                {
                    Content = json
                };

                // CHANGED: Add Authorization header for middleware
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // Send request
                var response = await _httpClient.SendAsync(request);

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

        private async Task<bool> QuickCheckForChangesAsync(bool skipDateFilter = false)
        {
            if (!await _checkSemaphore.WaitAsync(0))
                return false;

            try
            {
                using var scope = _provider.CreateScope();
                var fileService = scope.ServiceProvider.GetRequiredService<IFileRecordService>();
                var logService = scope.ServiceProvider.GetRequiredService<ILogService>();

                var startDate = skipDateFilter ? DateTime.MinValue : _startDate;
                var endDate = skipDateFilter ? DateTime.MaxValue : _endDate.AddDays(1).AddTicks(-1);

                var invoiceDto = new GetAllFileRecordDto
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    PageNumber = currentInvoicePage,
                    NumberOfRecords = invoiceRecordsPerPage
                };

                var logDto = new GetAllLogsDto
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    PageNumber = currentLogsPage,
                    NumberOfRecords = logsRecordsPerPage
                };

                var invoiceResponse = await fileService.GetAllAsync(invoiceDto);
                var logResponse = await logService.GetAllAsync(logDto);

                if (invoiceResponse?.Data == null || logResponse?.Data == null)
                    return false;

                int currentInvoiceCount = invoiceResponse.Data.TotalRecords ?? 0;
                int currentLogCount = logResponse.Data.TotalRecords ?? 0;

                bool hasChanges = (currentInvoiceCount != _lastInvoiceCount) ||
                                  (currentLogCount != _lastLogCount);

                if (hasChanges)
                {
                    _lastInvoiceCount = currentInvoiceCount;
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

                // Dispose pagination controls
                paginationInvoices?.Dispose();
                paginationLogs?.Dispose();

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
            // If start date is after end date, automatically adjust end date to match
            if (dtpStartDate.Value > dtpEndDate.Value)
            {
                dtpEndDate.Value = dtpStartDate.Value;
            }

            _startDate = dtpStartDate.Value.Date;
            UpdateDateRangeLabel();
        }

        private void DtpEndDate_ValueChanged(object sender, EventArgs e)
        {
            // If end date is before start date, automatically adjust start date to match
            if (dtpEndDate.Value < dtpStartDate.Value)
            {
                dtpStartDate.Value = dtpEndDate.Value;
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
                var encryptedPosId = ConfigurationManager.AppSettings["Username"] ?? "0";
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

                var startDate = skipDateFilter ? DateTime.MinValue : _startDate;
                var endDate = skipDateFilter ? DateTime.MaxValue : _endDate.AddDays(1).AddTicks(-1);

                var dto = new GetAllFileRecordDto
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    PageNumber = currentInvoicePage,
                    NumberOfRecords = invoiceRecordsPerPage
                };

                var response = await fileService.GetAllAsync(dto);

                if (response?.Data?.Items == null || !response.Data.Items.Any())
                {
                    _invoiceCache = new List<InvoiceDisplayModel>();
                    InvoicesDataGridView.RowCount = 0;
                    AlertManager.ShowWarning("No invoices found.");
                    lblLastSync.Text = "Last Sync: N/A";
                    UpdateInvoicePageInfo(1, 1);

                    // ✅ ADD THIS: Update panel labels to show zeros
                    labelAllInvoices.Text = "0";
                    labelPendingInvoice.Text = "0";
                    labelPaidInvoices.Text = "0";
                    _pendingCount = 0;
                    _syncedCount = 0;

                    // ✅ ADD THIS: Redraw pie chart with zeros
                    DrawInvoicePieChart(panelinvoicechart, 0, 0);

                    return;
                }

                totalInvoiceRecords = response.Data.TotalRecords ?? 0;
                totalInvoicePages = response.Data.TotalPages ?? 1;

                var invoices = response.Data.Items;

                _invoiceCache = invoices
                    .Select((inv, index) => new InvoiceDisplayModel
                    {
                        SerialNo = ((currentInvoicePage - 1) * invoiceRecordsPerPage) + index + 1,
                        PosId = inv.POSID,
                        InvoiceNumber = inv.InvoiceNumber ?? "N/A",
                        IsSynced = inv.IsSynced == 1 ? "Yes" : "No",
                        DateCreated = inv.DateCreated.ToString("dd-MM-yyyy HH:mm:ss"),
                        IsSyncedRaw = inv.IsSynced,
                        AttemptCount = inv.AttemptCount,
                        DateCreatedRaw = inv.DateCreated
                    })
                    .ToList();

                InvoicesDataGridView.SuspendLayout();
                InvoicesDataGridView.RowCount = _invoiceCache.Count;
                InvoicesDataGridView.ResumeLayout(false);
                InvoicesDataGridView.Invalidate();
                InvoicesDataGridView.Refresh();

                InvoicesDataGridView.ClearSelection();
                InvoicesDataGridView.CurrentCell = null;

                int syncedCount = response.Data.TotalSyncedInvoices ?? 0;
                int pendingCount = response.Data.UnsyncedInvoices ?? 0;

                labelAllInvoices.Text = totalInvoiceRecords.ToString();
                labelPendingInvoice.Text = pendingCount.ToString();
                labelPaidInvoices.Text = syncedCount.ToString();
                _pendingCount = pendingCount;
                _syncedCount = syncedCount;

                DrawInvoicePieChart(panelinvoicechart, _syncedCount, _pendingCount);
                UpdateInvoicePageInfo(currentInvoicePage, totalInvoicePages);

                var lastSyncedInvoice = _invoiceCache
                    .Where(i => i.IsSynced == "Yes")
                    .OrderByDescending(i => i.DateCreatedRaw)
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

                // ✅ ADD THIS: Also update to zeros on error
                labelAllInvoices.Text = "0";
                labelPendingInvoice.Text = "0";
                labelPaidInvoices.Text = "0";
                _pendingCount = 0;
                _syncedCount = 0;
                DrawInvoicePieChart(panelinvoicechart, 0, 0);
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
                IEnumerable<LogDto> allLogsForStats;

                if (preloadedLogs != null)
                {
                    logs = preloadedLogs;
                    allLogsForStats = preloadedLogs;
                    totalLogsRecords = logs.Count();
                    totalLogsPages = (int)Math.Ceiling(totalLogsRecords / (double)logsRecordsPerPage);
                }
                else
                {
                    var startDate = skipDateFilter ? DateTime.MinValue : _startDate;
                    var endDate = skipDateFilter ? DateTime.MaxValue : _endDate.AddDays(1).AddTicks(-1);

                    var Logsdto = new GetAllLogsDto
                    {
                        PageNumber = currentLogsPage,
                        NumberOfRecords = logsRecordsPerPage,
                        StartDate = startDate,
                        EndDate = endDate
                    };

                    var response = await _logService.GetAllAsync(Logsdto);

                    if (response?.Data?.Items == null || !response.Data.Items.Any())
                    {
                        _logCache = new List<LogDisplayModel>();
                        LogsDataGridView.RowCount = 0;
                        WindowsLocalAppNotification.Show("Logs", "No logs available to display");
                        AlertManager.ShowWarning("No logs available to display");
                        CalculateLogStatistics(null);
                        UpdateLogStatisticsDisplay();
                        UpdateLogsPageInfo(1, 1);
                        return;
                    }

                    totalLogsRecords = response.Data.TotalRecords ?? 0;
                    totalLogsPages = response.Data.TotalPages ?? 1;
                    logs = response.Data.Items;
                    allLogsForStats = response.Data.Items;
                }

                CalculateLogStatistics(allLogsForStats);
                UpdateLogStatisticsDisplay();

                if (!string.IsNullOrWhiteSpace(logTypeFilter))
                {
                    string filter = logTypeFilter.Trim().ToLower();

                    logs = logs.Where(l =>
                    {
                        var type = (l.Type ?? string.Empty).ToLower();
                        return filter switch
                        {
                            "error" or "exception" => type.Contains("error") || type.Contains("exception"),
                            "info" or "information" => type.Contains("info") || type.Contains("information"),
                            "warning" => type.Contains("warning"),
                            "success" => type.Contains("success"),
                            _ => true
                        };
                    });
                }

                var logsList = logs.OrderByDescending(l => l.CreatedAtPk).ToList();

                if (!logsList.Any())
                {
                    _logCache = new List<LogDisplayModel>();
                    LogsDataGridView.RowCount = 0;
                    WindowsLocalAppNotification.Show("Logs", $"No {logTypeFilter ?? "filtered"} logs available");
                    AlertManager.ShowWarning($"No {logTypeFilter ?? "filtered"} logs available");
                    UpdateLogsPageInfo(1, 1);
                    return;
                }

                _logCache = logsList
                    .Select((log, index) => new LogDisplayModel
                    {
                        SerialNo = ((currentLogsPage - 1) * logsRecordsPerPage) + index + 1,
                        Message = log.Message ?? "No message",
                        Type = log.Type ?? "N/A",
                        DateCreated = log.CreatedAtPk.ToString("dd-MM-yyyy HH:mm:ss"),
                        DateCreatedRaw = log.CreatedAtPk
                    })
                    .ToList();

                LogsDataGridView.SuspendLayout();
                LogsDataGridView.RowCount = _logCache.Count;
                LogsDataGridView.ResumeLayout(false);
                LogsDataGridView.Invalidate();
                LogsDataGridView.Refresh();

                UpdateLogsPageInfo(currentLogsPage, totalLogsPages);

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

        private void UpdateInvoicePageInfo(int currentPage, int totalPages)
        {
            if (paginationInvoices != null)
            {
                paginationInvoices.CurrentPage = currentPage;
                paginationInvoices.TotalPages = totalPages;
            }
        }

        private void UpdateLogsPageInfo(int currentPage, int totalPages)
        {
            if (paginationLogs != null)
            {
                paginationLogs.CurrentPage = currentPage;
                paginationLogs.TotalPages = totalPages;
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
                string selectedEnvironment = ConfigurationManager.AppSettings["Environment"];

                await _sendLogToCloudService.SyncLogAsync(selectedEnvironment, null);
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

        private async void btnFilterInvoices_Click(object sender, EventArgs e)
        {
            currentInvoicePage = 1;
            currentLogsPage = 1;
            await RunSingleLoad(async () =>
            {
                await LoadAndShowInvoicesAsync();
                await LoadAndShowLogsAsync();
            });
        }

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
            currentInvoicePage = 1;
            currentLogsPage = 1;
            _startDate = DateTime.Today;
            _endDate = DateTime.Today;
            dtpStartDate.Value = _startDate;
            dtpEndDate.Value = _endDate;
            UpdateDateRangeLabel();
            await RunSingleLoad(async () =>
            {
                await LoadAndShowInvoicesAsync();
                await LoadAndShowLogsAsync();
            });
        }

        private async void btnClearFilter_Click(object sender, EventArgs e)
        {
            ResetSortToDefault();
            _filterSyncedOnly = false;
            currentInvoicePage = 1;
            currentLogsPage = 1;

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
                System.Windows.Forms.Application.DoEvents();
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
            System.Windows.Forms.Application.DoEvents();
        }


        // Disables the button for 3 seconds but doesn't block the UI.
        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            btnRefresh.Enabled = false;
            ResetSortToDefault();
            currentInvoicePage = 1; // ✅ Reset pagination
            currentLogsPage = 1;

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
            //dgv.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
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
                HeaderText = " Invoice Number",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.Automatic,
            };
            colInvoiceNumber.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colInvoiceNumber.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

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
                SafeInvalidateCell(e.RowIndex, e.ColumnIndex);
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
                    _invoiceService.GetInvoiceWithItems(128762).GetAwaiter().GetResult());


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
                            System.Windows.Forms.Application.Run(printForm);
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

                SafeInvalidateCell(e.RowIndex, e.ColumnIndex);
            }
        }

        private void SafeInvalidateCell(int rowIndex, int columnIndex)
        {
            if (InvoicesDataGridView == null || InvoicesDataGridView.IsDisposed)
                return;

            if (rowIndex < 0 || rowIndex >= InvoicesDataGridView.RowCount)
                return;

            if (columnIndex < 0 || columnIndex >= InvoicesDataGridView.ColumnCount)
                return;

            InvoicesDataGridView.InvalidateCell(columnIndex, rowIndex);
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

            // Replace this section in InvoicesDataGridView_CellPainting method:

            if (e.RowIndex >= 0 && e.ColumnIndex == InvoicesDataGridView.Columns["colInvoiceNumber"]?.Index)
            {
                e.PaintBackground(e.CellBounds, true);

                string value = e.Value?.ToString() ?? "";

                using (var font = new Font("Segoe UI", 10F, FontStyle.Bold))
                using (var brush = new SolidBrush(e.CellStyle.ForeColor))
                {
                    var stringFormat = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center,
                        Trimming = StringTrimming.EllipsisCharacter
                    };

                    var textRect = new RectangleF(
                        e.CellBounds.X,
                        e.CellBounds.Y,
                        e.CellBounds.Width,
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
                //if (column.Name == "colLogID" || column.Name == "colException" || column.Name == "logdatetime")
                //{
                column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                //}
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

    #region Pagination Control
    public class ModernPaginationControl : FlowLayoutPanel
    {
        private int _currentPage = 1;
        private int _totalPages = 1;

        public event EventHandler<int> PageChanged;

        private Button btnFirstPage;
        private Button btnPrevPage;
        private Button btnNextPage;
        private Button btnLastPage;
        private TextBox txtPageNumber;
        private Label lblTotalPages;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                if (_currentPage != value && value >= 1 && value <= _totalPages)
                {
                    _currentPage = value;
                    UpdatePaginationUI();
                    PageChanged?.Invoke(this, _currentPage);
                }
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int TotalPages
        {
            get => _totalPages;
            set
            {
                if (_totalPages != value && value > 0)
                {
                    _totalPages = value;
                    if (_currentPage > _totalPages)
                        _currentPage = _totalPages;
                    UpdatePaginationUI();
                }
            }
        }

        public ModernPaginationControl()
        {
            InitializeControl();
        }

        private void InitializeControl()
        {
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.FlowDirection = FlowDirection.LeftToRight;
            this.WrapContents = false;
            this.Padding = new Padding(0);
            this.Margin = new Padding(0);
            this.BackColor = Color.Transparent;

            // Compact button size for lower resolutions
            int buttonSize = 28; // Reduced from 32

            // Navigation buttons with compact size
            btnFirstPage = CreateNavButton("⏮", "First Page", buttonSize);
            btnPrevPage = CreateNavButton("◀", "Previous Page", buttonSize);
            btnNextPage = CreateNavButton("▶", "Next Page", buttonSize);
            btnLastPage = CreateNavButton("⏭", "Last Page", buttonSize);

            // Compact page number textbox
            txtPageNumber = new TextBox
            {
                Width = 45, // Reduced from 50
                Height = buttonSize,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular), // Smaller font
                TextAlign = HorizontalAlignment.Center,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(2, 4, 2, 2), // Reduced margins
                BackColor = Color.White
            };

            // Compact total pages label
            lblTotalPages = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular), // Smaller font
                ForeColor = Color.FromArgb(55, 65, 81),
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(0, 7, 4, 0), // Adjusted padding
                Margin = new Padding(0, 0, 2, 0)
            };

            // Event handlers
            btnFirstPage.Click += (s, e) => CurrentPage = 1;
            btnPrevPage.Click += (s, e) => { if (_currentPage > 1) CurrentPage--; };
            btnNextPage.Click += (s, e) => { if (_currentPage < _totalPages) CurrentPage++; };
            btnLastPage.Click += (s, e) => CurrentPage = _totalPages;

            txtPageNumber.KeyPress += TxtPageNumber_KeyPress;
            txtPageNumber.Leave += TxtPageNumber_Leave;
            txtPageNumber.Enter += (s, e) => txtPageNumber.SelectAll();

            // Add all controls
            this.Controls.Add(btnFirstPage);
            this.Controls.Add(btnPrevPage);
            this.Controls.Add(txtPageNumber);
            this.Controls.Add(lblTotalPages);
            this.Controls.Add(btnNextPage);
            this.Controls.Add(btnLastPage);

            UpdatePaginationUI();
        }

        private void TxtPageNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }

            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                NavigateToEnteredPage();
            }
        }

        private void TxtPageNumber_Leave(object sender, EventArgs e)
        {
            NavigateToEnteredPage();
        }

        private void NavigateToEnteredPage()
        {
            if (int.TryParse(txtPageNumber.Text, out int pageNumber))
            {
                if (pageNumber >= 1 && pageNumber <= _totalPages)
                {
                    CurrentPage = pageNumber;
                }
                else
                {
                    txtPageNumber.Text = _currentPage.ToString();
                }
            }
            else
            {
                txtPageNumber.Text = _currentPage.ToString();
            }
        }

        private Button CreateNavButton(string text, string tooltip, int buttonSize)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(buttonSize, buttonSize),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold), // Smaller font
                BackColor = Color.White,
                ForeColor = Color.FromArgb(55, 65, 81),
                Cursor = Cursors.Hand,
                Margin = new Padding(1, 2, 1, 2), // Reduced margins
                TabStop = false
            };

            btn.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(243, 244, 246);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(229, 231, 235);

            return btn;
        }

        private void UpdatePaginationUI()
        {
            if (txtPageNumber.InvokeRequired)
            {
                txtPageNumber.Invoke(new Action(() =>
                {
                    txtPageNumber.Text = _currentPage.ToString();
                }));
            }
            else
            {
                txtPageNumber.Text = _currentPage.ToString();
            }

            if (lblTotalPages.InvokeRequired)
            {
                lblTotalPages.Invoke(new Action(() =>
                {
                    lblTotalPages.Text = $"of {_totalPages}";
                }));
            }
            else
            {
                lblTotalPages.Text = $"of {_totalPages}";
            }

            btnFirstPage.Enabled = _currentPage > 1;
            btnPrevPage.Enabled = _currentPage > 1;
            btnNextPage.Enabled = _currentPage < _totalPages;
            btnLastPage.Enabled = _currentPage < _totalPages;

            UpdateButtonAppearance(btnFirstPage);
            UpdateButtonAppearance(btnPrevPage);
            UpdateButtonAppearance(btnNextPage);
            UpdateButtonAppearance(btnLastPage);
        }

        private void UpdateButtonAppearance(Button btn)
        {
            if (!btn.Enabled)
            {
                btn.ForeColor = Color.FromArgb(156, 163, 175);
                btn.BackColor = Color.FromArgb(249, 250, 251);
                btn.FlatAppearance.BorderColor = Color.FromArgb(229, 231, 235);
                btn.Cursor = Cursors.Default;
            }
            else
            {
                btn.BackColor = Color.White;
                btn.ForeColor = Color.FromArgb(55, 65, 81);
                btn.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
                btn.Cursor = Cursors.Hand;
            }
        }

        public void Reset()
        {
            _currentPage = 1;
            _totalPages = 1;
            UpdatePaginationUI();
        }
    }
    #endregion


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
