using POSPRA.Application.Services.FileRecordService;
using POSPRA.Application.Services.InvoiceService;
using POSPRA.Application.Services.LogService;
using POSPRA.DTOs.LogDtos;
using POSPRA_WinFormsUI.AlertClasses;
using System.ComponentModel;
using System.Data;
using System.Text;
namespace POSPRA_WinFormsUI.Forms

{
    public partial class DashboardForm : Form
    {
        private readonly IServiceProvider _provider;
        private readonly IFileRecordService _fileRecordService;
        private readonly ILogService _logService;
        private readonly IInvoiceService _invoiceService;
        private bool _isInitialLoad = true;
        private bool _filterSyncedOnly = false;

        private bool _isSyncedSortDescending = true; // Default: Yes on top

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

        public DashboardForm(IServiceProvider provider, ILogService logService, IInvoiceService invoiceService, IFileRecordService fileRecordService)
        {
            InitializeComponent();

            this.Load += (s, e) => CenterProgressBar();
            this.Resize += (s, e) => CenterProgressBar();
            this.Load += DashboardForm_Load;

            _provider = provider;
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));

            FormBorderStyle = FormBorderStyle.None;
            ControlBox = false;
            ShowIcon = false;
            Text = string.Empty;

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

            btnExportLogs.Click += btnExportLogs_Click;
            panelinvoicechart.Resize -= Panel_Resize;
            panelinvoicechart.Resize += Panel_Resize;

            panellogchart.Resize -= LogChartPanel_Resize;
            panellogchart.Resize += LogChartPanel_Resize;

            if (progressBar != null) progressBar.Visible = false;
            ApplyGradientBackground(
                panelAll,
                ColorTranslator.FromHtml("#8860C1"),
                ColorTranslator.FromHtml("#584ABC")
            );
            ApplyGradientBackground(
                panelPending,
                ColorTranslator.FromHtml("#E53935"),
                ColorTranslator.FromHtml("#EF5350")
            );
            ApplyGradientBackground(
                panelPaid,
                ColorTranslator.FromHtml("#43A047"),
                ColorTranslator.FromHtml("#66BB6A")
            );
            AddImageToPanelRight(panelAll, Resources.InvoiceAll);
            AddImageToPanelRight(panelPending, Resources.NotSynced);
            AddImageToPanelRight(panelPaid, Resources.Synced);
            SetButtonImage(btnFilter, Resources.Filter, ColorTranslator.FromHtml("#0D7351"));
            SetButtonImage(btnRefresh, Resources.refresh, ColorTranslator.FromHtml("#5BBBB3"));
            SetButtonImage(btnClearFilter, Resources.clearFilter, ColorTranslator.FromHtml("#DC2626"));

            InitializeLogStatistics();
            _invoiceService = invoiceService;
            _fileRecordService = fileRecordService;
        }
        private void InitializeLogStatistics()
        {
            // Set initial values
            UpdateLogStatisticsDisplay();
        }
        private void UpdateLogStatisticsDisplay()
        {
            // Update the labels with current counts
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

            // Adjust font sizes based on panel size
            int panelHeight = panellogchart.Height;
            int titleFontSize = Math.Max(8, Math.Min(12, panelHeight / 25));
            int countFontSize = Math.Max(12, Math.Min(24, panelHeight / 12));

            // Update fonts for all statistic panels
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
                countLabel.TextAlign = ContentAlignment.MiddleCenter; // Ensure centered
                countLabel.AutoSize = false; // Disable auto-size for better control
                countLabel.Dock = DockStyle.None; // Remove dock for manual positioning
            }

            if (titleLabel != null)
            {
                titleLabel.Font = new Font("Segoe UI", titleSize, FontStyle.Regular);
                titleLabel.TextAlign = ContentAlignment.MiddleCenter; // Ensure centered
                titleLabel.AutoSize = false; // Disable auto-size for better control
                titleLabel.Dock = DockStyle.None; // Remove dock for manual positioning
            }

            // Adjust positioning within the panel for centered layout
            if (panel != null && countLabel != null && titleLabel != null)
            {
                int padding = 8;

                // Position count label - centered horizontally, top portion
                countLabel.Width = panel.Width - (padding * 2);
                countLabel.Height = (int)(panel.Height * 0.6); // 60% of panel height for number
                countLabel.Left = padding;
                countLabel.Top = padding;
                countLabel.TextAlign = ContentAlignment.MiddleCenter;

                // Position title label - centered horizontally, bottom portion
                titleLabel.Width = panel.Width - (padding * 2);
                titleLabel.Height = (int)(panel.Height * 0.3); // 30% of panel height for title
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
        private void LblDateRange_Click(object sender, EventArgs e)
        {
            dtpStartDate.Visible = true;
            dtpStartDate.Focus();
            SendKeys.Send("%{DOWN}");
        }

        private void DtpStartDate_ValueChanged(object sender, EventArgs e)
        {
            _startDate = dtpStartDate.Value.Date;
            UpdateDateRangeLabel();
        }

        private void DtpEndDate_ValueChanged(object sender, EventArgs e)
        {
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
                // Draw rounded border
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

                // Draw calendar icon on right
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

                InvoicesDataGridView.DataError += dataGridView_DataError;
                LogsDataGridView.DataError += dataGridView_DataError;
                _isInitialLoad = false;
            }
            catch (Exception ex)
            {
                WindowsLocalAppNotification.Show("Dashboard Error", $"Error loading dashboard: {ex.Message}");
                AlertManager.ShowError($"Error loading dashboard: {ex.Message}");
            }
        }

        private async Task RunSingleLoad(Func<Task> work)
        {
            if (Interlocked.Exchange(ref _isLoadingFlag, 1) == 1) return;

            try
            {
                if (progressBar != null)
                {
                    progressBar.Visible = true;
                    progressBar.BringToFront();
                    progressBar.Value = 0;
                    progressBar.Update();
                }

                await work();
            }
            finally
            {
                if (progressBar != null)
                    progressBar.Visible = false;

                Interlocked.Exchange(ref _isLoadingFlag, 0);
            }
        }

        // ----------------------------------------
        // Optimized Load Invoices
        // ----------------------------------------

        private async Task LoadAndShowInvoicesAsync(bool skipDateFilter = false)
        {
            try
            {
                var response = await _fileRecordService.GetAllAsync();
                InvoicesDataGridView.Rows.Clear();

                if (response?.Data == null || !response.Data.Any())
                {
                    AlertManager.ShowWarning("No invoices found.");
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

                var invoicesList = filteredInvoices
                    .OrderByDescending(i => i.DateCreated)
                    .ToList();

                int totalInvoices = invoicesList.Count;

                InvoicesDataGridView.SuspendLayout();

                if (progressBar != null && totalInvoices > 100)
                {
                    progressBar.Style = ProgressBarStyle.Continuous;
                    progressBar.Minimum = 0;
                    progressBar.Maximum = totalInvoices;
                    progressBar.Value = 0;
                    progressBar.Visible = true;
                    progressBar.BringToFront();
                }

                var rows = new List<DataGridViewRow>();
                int syncedCount = 0, pendingCount = 0, inProgressCount = 0;

                for (int i = 0; i < invoicesList.Count; i++)
                {
                    var inv = invoicesList[i];
                    var row = new DataGridViewRow();
                    row.CreateCells(InvoicesDataGridView);

                    // Set cell values
                    SetCellValue(row, "colId", i + 1);
                    SetCellValue(row, "colPosId", inv.POSID);
                    SetCellValue(row, "colInvoiceNumber", inv.InvoiceNumber ?? "N/A");
                    SetCellValue(row, "colIsSynced", inv.IsSynced == 1 ? "Yes" : "No");
                    SetCellValue(row, "colPrint", "Print"); // Set Print text
                    SetCellValue(row, "colDateCreated", inv.DateCreated.ToString("dd-MM-yyyy HH:mm:ss"));

                    row.Tag = new { inv.IsSynced, inv.AttemptCount };
                    rows.Add(row);

                    if (inv.IsSynced == 1) syncedCount++;
                    else pendingCount++;
                    if (inv.AttemptCount > 0) inProgressCount++;

                    if (progressBar != null && totalInvoices > 100 && i % 10 == 0)
                    {
                        progressBar.Value = Math.Min(i + 1, progressBar.Maximum);
                        Application.DoEvents();
                    }
                }

                InvoicesDataGridView.Rows.AddRange(rows.ToArray());
                InvoicesDataGridView.ResumeLayout(true);

                labelAllInvoices.Text = totalInvoices.ToString();
                labelPendingInvoice.Text = pendingCount.ToString();
                labelPaidInvoices.Text = syncedCount.ToString();
                _pendingCount = pendingCount;
                _syncedCount = syncedCount;
                DrawInvoicePieChart(panelinvoicechart, _syncedCount, _pendingCount);

                if (progressBar != null)
                {
                    progressBar.Visible = false;
                }
                InvoicesDataGridView.ClearSelection();
                if (InvoicesDataGridView.Rows.Count > 0)
                {
                    InvoicesDataGridView.CurrentCell = null;
                }
            }
            catch (Exception ex)
            {
                InvoicesDataGridView.ResumeLayout(true);
                if (progressBar != null) progressBar.Visible = false;

                WindowsLocalAppNotification.Show("Invoices Error", $"Error loading invoices: {ex.Message}");
                AlertManager.ShowError($"Error loading invoices: {ex.Message}");
            }
        }
        private void SetCellValue(DataGridViewRow row, string columnName, object value)
        {
            if (InvoicesDataGridView.Columns.Contains(columnName))
            {
                row.Cells[InvoicesDataGridView.Columns[columnName].Index].Value = value;
            }
        }


        // ----------------------------------------
        // Optimized Load Logs
        // ----------------------------------------
        private async Task LoadAndShowLogsAsync(bool skipDateFilter = false)
        {
            try
            {
                var response = await _logService.GetAllAsync();
                LogsDataGridView.Rows.Clear();

                if (response?.Data == null || !response.Data.Any())
                {
                    WindowsLocalAppNotification.Show("Logs", "No logs available to display");
                    AlertManager.ShowWarning("No logs available to display");

                    // Reset log statistics
                    CalculateLogStatistics(null);
                    UpdateLogStatisticsDisplay();
                    return;
                }

                IEnumerable<LogDto> filteredLogs = response.Data;

                if (!skipDateFilter)
                {
                    filteredLogs = filteredLogs.Where(l =>
                        l.CreatedAtPk >= _startDate &&
                        l.CreatedAtPk <= _endDate.AddDays(1).AddTicks(-1));
                }

                // ✅ Order by CreatedAtPk DESC (latest log first)
                var logsList = filteredLogs
                    .OrderByDescending(l => l.CreatedAtPk)
                    .ToList();

                int totalLogs = logsList.Count;

                // Calculate log statistics
                CalculateLogStatistics(logsList);
                UpdateLogStatisticsDisplay();

                LogsDataGridView.SuspendLayout();

                if (progressBar != null && totalLogs > 100)
                {
                    progressBar.Style = ProgressBarStyle.Continuous;
                    progressBar.Minimum = 0;
                    progressBar.Maximum = totalLogs;
                    progressBar.Value = 0;
                    progressBar.Visible = true;
                }

                var rows = new List<DataGridViewRow>();

                for (int i = 0; i < logsList.Count; i++)
                {
                    var log = logsList[i];
                    var row = new DataGridViewRow();
                    row.CreateCells(LogsDataGridView);

                    // ✅ Sr No. (1 = latest log at the top)
                    row.Cells[LogsDataGridView.Columns["colLogID"].Index].Value = i + 1;
                    row.Cells[LogsDataGridView.Columns["colMessage"].Index].Value = log.Message ?? "No message";
                    row.Cells[LogsDataGridView.Columns["colException"].Index].Value = log.Type ?? "N/A";
                    row.Cells[LogsDataGridView.Columns["logdatetime"].Index].Value = log.CreatedAtPk.ToString("dd-MM-yyyy HH:mm:ss");

                    rows.Add(row);

                    if (progressBar != null && totalLogs > 100 && i % 10 == 0)
                    {
                        progressBar.Value = Math.Min(i + 1, progressBar.Maximum);
                        Application.DoEvents();
                    }
                }

                LogsDataGridView.Rows.AddRange(rows.ToArray());
                LogsDataGridView.ResumeLayout(true);

                if (progressBar != null)
                {
                    progressBar.Visible = false;
                }
                LogsDataGridView.ClearSelection();
                if (LogsDataGridView.Rows.Count > 0)
                {
                    LogsDataGridView.CurrentCell = null;
                }
            }
            catch (Exception ex)
            {
                LogsDataGridView.ResumeLayout(true);
                if (progressBar != null) progressBar.Visible = false;

                // Reset log statistics on error
                CalculateLogStatistics(null);
                UpdateLogStatisticsDisplay();

                WindowsLocalAppNotification.Show("Logs Error", $"Error loading logs: {ex.Message}");
                AlertManager.ShowError($"Error loading logs: {ex.Message}");
            }
        }

        // ----------------------------------------
        // Export Invoices Button
        // ----------------------------------------
        private async void btnExportInvoice_Click(object sender, EventArgs e)
        {
            try
            {
                if (InvoicesDataGridView.Rows.Count == 0)
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

                        // ✅ Write headers
                        var headers = InvoicesDataGridView.Columns
                            .Cast<DataGridViewColumn>()
                            .Where(c => c.Visible)
                            .Select(c => EscapeCsvField(c.HeaderText));
                        csvContent.AppendLine(string.Join(",", headers));

                        // ✅ Write rows
                        foreach (DataGridViewRow row in InvoicesDataGridView.Rows)
                        {
                            if (!row.IsNewRow)
                            {
                                var cells = row.Cells.Cast<DataGridViewCell>()
                                    .Where(c => c.OwningColumn.Visible)
                                    .Select(c => EscapeCsvField(c.Value?.ToString() ?? ""));
                                csvContent.AppendLine(string.Join(",", cells));
                            }
                        }

                        // ✅ Save file
                        await File.WriteAllTextAsync(sfd.FileName, csvContent.ToString(), Encoding.UTF8);

                        // Show success message with file location
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

        // Helper method for proper CSV field escaping
        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "";

            // If field contains comma, quote, or newline, wrap in quotes and escape existing quotes
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }

            return field;
        }


        // ----------------------------------------
        // Export Logs Button
        // ----------------------------------------
        private async void btnExportLogs_Click(object sender, EventArgs e)
        {
            try
            {
                if (LogsDataGridView.Rows.Count == 0)
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

                        // ✅ Write headers
                        var headers = LogsDataGridView.Columns
                            .Cast<DataGridViewColumn>()
                            .Where(c => c.Visible) // only visible columns
                            .Select(c => c.HeaderText);
                        csvContent.AppendLine(string.Join(",", headers));

                        // ✅ Write rows
                        foreach (DataGridViewRow row in LogsDataGridView.Rows)
                        {
                            if (!row.IsNewRow)
                            {
                                var cells = row.Cells.Cast<DataGridViewCell>()
                                    .Where(c => c.OwningColumn.Visible)
                                    .Select(c => c.Value?.ToString().Replace(",", " ") ?? ""); // prevent CSV break
                                csvContent.AppendLine(string.Join(",", cells));
                            }
                        }

                        // ✅ Save file
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


        // ----------------------------------------
        // Reset Sort Helper Method
        // ----------------------------------------
        private void ResetSortToDefault()
        {
            // Clear sort glyphs
            foreach (DataGridViewColumn col in InvoicesDataGridView.Columns)
                col.HeaderCell.SortGlyphDirection = SortOrder.None;

            // Sort by ID descending (default)
            InvoicesDataGridView.Sort(InvoicesDataGridView.Columns["colId"], ListSortDirection.Descending);

            // Reset synced sort state
            _isSyncedSortDescending = true;
            btnFilterSynced.Text = "Show Synced First";
        }


        // ----------------------------------------
        // Other Buttons
        // ----------------------------------------
        private async void btnFilterInvoices_Click(object sender, EventArgs e) => await RunSingleLoad(async () =>
        {
            await LoadAndShowInvoicesAsync();
            await LoadAndShowLogsAsync();
        });

        private void btnFilterSynced_Click(object sender, EventArgs e)
        {
            // Toggle sort order
            if (_isSyncedSortDescending)
            {
                // Yes (synced) on top
                InvoicesDataGridView.Sort(InvoicesDataGridView.Columns["colIsSynced"], ListSortDirection.Descending);
                btnFilterSynced.Text = "Show Unsynced First";
            }
            else
            {
                // No (unsynced) on top
                InvoicesDataGridView.Sort(InvoicesDataGridView.Columns["colIsSynced"], ListSortDirection.Ascending);
                btnFilterSynced.Text = "Show Synced First";
            }

            // Flip flag for next click
            _isSyncedSortDescending = !_isSyncedSortDescending;
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
                await LoadAndShowInvoicesAsync();
                await LoadAndShowLogsAsync();
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
            lblDateRange.Text = "Select Date From-To";

            await RunSingleLoad(async () =>
            {
                await LoadAndShowInvoicesAsync(skipDateFilter: true);
                await LoadAndShowLogsAsync(skipDateFilter: true);
            });
        }


        private void ClearDataGridView(DataGridView dataGridView)
        {

            if (dataGridView == null) return;

            try
            {
                // Suspend layout to prevent flickering
                dataGridView.SuspendLayout();

                // Clear all data sources
                dataGridView.DataSource = null;

                // Clear all rows
                dataGridView.Rows.Clear();

                // Clear selection
                dataGridView.ClearSelection();

                // Reset current cell
                dataGridView.CurrentCell = null;

                // Force garbage collection on disposed rows
                GC.Collect();
                GC.WaitForPendingFinalizers();

                // Force immediate UI update
                dataGridView.Refresh();
                dataGridView.Invalidate();

                // Additional refresh
                Application.DoEvents();
            }
            catch (Exception ex)
            {
                // Log any errors but don't throw
                Console.WriteLine($"Error clearing DataGridView: {ex.Message}");
            }
            finally
            {
                // Always resume layout
                dataGridView.ResumeLayout(true);
            }
        }

        private void FixSerialNumberHeaderColor()
        {
            // Disable selection on load to prevent initial blue highlight
            InvoicesDataGridView.ClearSelection();
            LogsDataGridView.ClearSelection();

            // Set current cell to null to remove focus
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
            // Clear both grids
            ClearDataGridView(InvoicesDataGridView);
            ClearDataGridView(LogsDataGridView);

            // Reset all statistics labels
            labelAllInvoices.Text = "0";
            labelPendingInvoice.Text = "0";
            labelPaidInvoices.Text = "0";

            // Force form refresh
            this.Refresh();
            Application.DoEvents();
        }

        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            ResetSortToDefault();
            try
            {
                // Show loading indicator
                if (progressBar != null)
                {
                    progressBar.Visible = true;
                    progressBar.Style = ProgressBarStyle.Marquee;
                    progressBar.MarqueeAnimationSpeed = 30;
                }

                // Completely clear all grids first
                ClearAllGrids();

                // Small delay to ensure UI is cleared
                await Task.Delay(200);

                // Determine if we should skip date filter
                bool skipDateFilter = false;
                // Reset synced filter
                _filterSyncedOnly = false;

                // Always run single load to prevent overlapping loads
                await RunSingleLoad(async () =>
                {
                    // Reload both grids with fresh data
                    await LoadAndShowInvoicesAsync(skipDateFilter);
                    await LoadAndShowLogsAsync(skipDateFilter);
                });
            }
            catch (Exception ex)
            {
                // Handle any errors during refresh
                AlertManager.ShowError($"Error refreshing data: {ex.Message}");
            }
            finally
            {
                // Hide loading indicator
                if (progressBar != null)
                {
                    progressBar.Visible = false;
                    progressBar.Style = ProgressBarStyle.Continuous;
                }
            }
        }


        private void dataGridView_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
            if (sender is DataGridView grid && e.RowIndex >= 0 && e.ColumnIndex >= 0)
                grid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "N/A";
        }

        /// <summary>
        /// Sets an image inside a button, scales it properly, centers it, and applies a background color.
        /// </summary>
        /// <param name="btn">The Button to apply the image to</param>
        /// <param name="img">The Image to set</param>
        /// <param name="bgColor">Background color of the button</param>
        private void SetButtonImage(Button btn, Image img, Color bgColor)
        {
            if (btn == null) return;

            // Set background color
            btn.BackColor = bgColor;

            if (img == null)
            {
                btn.Image = null;
                return;
            }

            // Dispose previous image to prevent memory leaks
            if (btn.Image != null)
            {
                btn.Image.Dispose();
                btn.Image = null;
            }

            // Calculate maximum size to fit inside button, leaving some padding
            int padding = 8;
            int maxWidth = btn.Width - padding;
            int maxHeight = btn.Height - padding;

            // Calculate scaled size while keeping aspect ratio
            double ratioX = (double)maxWidth / img.Width;
            double ratioY = (double)maxHeight / img.Height;
            double ratio = Math.Min(ratioX, ratioY);

            int newWidth = (int)(img.Width * ratio);
            int newHeight = (int)(img.Height * ratio);

            // Resize the image
            Image resized = new Bitmap(img, new Size(newWidth, newHeight));

            // Apply image to button
            btn.Image = resized;
            btn.ImageAlign = ContentAlignment.MiddleCenter; // center
            btn.Text = ""; // remove text if needed
            btn.FlatStyle = FlatStyle.Flat;
            btn.BackgroundImageLayout = ImageLayout.None;
        }

        /// <summary>
        /// Adds an image to the right side of a panel.
        /// </summary>
        /// <param name="panel">The panel to add the image to.</param>
        /// <param name="image">The image to display.</param>
        /// <param name="width">Optional: width of the image box (default 60).</param>
        private void AddImageToPanelRight(Panel panel, Image image, int width = 60)
        {
            if (panel == null || image == null) return;

            // Create PictureBox
            PictureBox pic = new PictureBox
            {
                Image = image,
                SizeMode = PictureBoxSizeMode.Zoom,
                Dock = DockStyle.Right,
                Width = width,
                Margin = new Padding(5)
            };

            // Add PictureBox to panel
            panel.Controls.Add(pic);

            // Reconfigure existing labels to position them correctly
            foreach (Control ctrl in panel.Controls)
            {
                if (ctrl != pic && ctrl is Label lbl)
                {
                    lbl.Dock = DockStyle.None;
                    lbl.AutoSize = false;
                    lbl.ForeColor = Color.White; // ✅ Always white text

                    if (lbl.Font.Size > 20) // ✅ Number label
                    {
                        lbl.TextAlign = ContentAlignment.TopLeft;
                        lbl.Location = new Point(15, 15);
                        lbl.Width = panel.Width - width - 30;
                        lbl.Height = (int)(panel.Height * 0.6);
                    }
                    else // ✅ Title label
                    {
                        lbl.TextAlign = ContentAlignment.MiddleLeft;
                        lbl.Location = new Point(15, (int)(panel.Height * 0.55)); // 🔼 slightly higher
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

            // To allow gradient to show behind children
            control.BackColor = Color.Transparent;
            control.Invalidate(); // Force repaint
        }

        private void StyleDataGridView(DataGridView dgv)
        {
            // General settings
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
            //dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Column header style
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(51, 51, 51);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(12, 10, 12, 10);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv.ColumnHeadersHeight = 52;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = dgv.ColumnHeadersDefaultCellStyle.BackColor;
            dgv.ColumnHeadersDefaultCellStyle.SelectionForeColor = dgv.ColumnHeadersDefaultCellStyle.ForeColor;


            // Cell style (adjusted)
            dgv.DefaultCellStyle.BackColor = Color.White;
            dgv.DefaultCellStyle.ForeColor = Color.FromArgb(55, 65, 81);
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 10F); // slightly bigger & cleaner
            dgv.DefaultCellStyle.Padding = new Padding(12, 6, 12, 6); // less vertical padding
            dgv.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            // Adjust row height so text fits nicely
            //dgv.RowTemplate.Height = dgv.DefaultCellStyle.Font.Height + dgv.DefaultCellStyle.Padding.Vertical + 12;
        }


        private void StyleInvoicesDataGridView()
        {
            // Clear existing columns first
            InvoicesDataGridView.Columns.Clear();

            // Apply base styling
            StyleDataGridView(InvoicesDataGridView);

            // Add columns with specific styling
            var colId = new DataGridViewTextBoxColumn
            {
                Name = "colId",
                HeaderText = "Sr. No.",
                Width = 120,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.Automatic,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.FromArgb(107, 114, 128),
                    Font = new Font("Segoe UI", 9F)
                }
            };

            var colPosId = new DataGridViewTextBoxColumn
            {
                Name = "colPosId",
                HeaderText = "POS ID",
                Width = 120,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.Automatic,
            };

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
                HeaderText = "Invoice Synced",
                Width = 180,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.Automatic,
            };
            colIsSynced.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colIsSynced.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

            var colPrint = new DataGridViewTextBoxColumn
            {
                Name = "colPrint",
                HeaderText = "     Print",
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

            // Attach custom cell painting for badges
            InvoicesDataGridView.CellPainting += InvoicesDataGridView_CellPainting;

            // NEW: Add mouse events for hyperlink hover and click
            InvoicesDataGridView.CellMouseEnter += InvoicesDataGridView_CellMouseEnter;
            InvoicesDataGridView.CellMouseLeave += InvoicesDataGridView_CellMouseLeave;
            InvoicesDataGridView.CellClick += InvoicesDataGridView_CellClick;
            InvoicesDataGridView.CellMouseMove += InvoicesDataGridView_CellMouseMove;
        }

        // NEW: Add these event handlers after StyleInvoicesDataGridView method

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
                // Check if mouse is within the text bounds
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
        private void InvoicesDataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == InvoicesDataGridView.Columns["colPrint"].Index)
            {
                // Get the actual mouse position
                var cellBounds = InvoicesDataGridView.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
                var mousePos = InvoicesDataGridView.PointToClient(Cursor.Position);

                // Check if click is within the text bounds
                if (_printLinkBounds.Contains(mousePos))
                {

                    var invoiceNumber = InvoicesDataGridView.Rows[e.RowIndex].Cells["colInvoiceNumber"].Value?.ToString() ?? "N/A";

                    // call invoice print generator
                    // invoiceNumber
                    var response = _invoiceService.GetInvoiceWithItems(invoiceNumber).Result;
                    MessageBox.Show(
                        $"Print button clicked for Invoice: {invoiceNumber}",
                        "Print Invoice",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
            }
        }

        private void StyleLogsDataGridView()
        {
            // Clear existing columns first
            LogsDataGridView.Columns.Clear();

            // Apply base styling
            StyleDataGridView(LogsDataGridView);

            // Add log-specific columns
            var colLogID = new DataGridViewTextBoxColumn
            {
                Name = "colLogID",
                HeaderText = "Sr. No.",
                Width = 120,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.Automatic,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.FromArgb(107, 114, 128),
                    Font = new Font("Segoe UI", 9F)
                }
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
                Width = 150,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.Automatic,

            };
            colException.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

            var colDateTime = new DataGridViewTextBoxColumn
            {
                Name = "logdatetime",
                HeaderText = "Date/Time",
                Width = 220,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.Automatic,
            };

            LogsDataGridView.Columns.AddRange(new DataGridViewColumn[]
            {
        colLogID, colMessage, colException, colDateTime
            });

            // Attach custom cell painting for badges in Type column
            LogsDataGridView.CellPainting += LogsDataGridView_CellPainting;
        }
        private void InvoicesDataGridView_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            // Handle IsSynced column with icons
            if (e.RowIndex >= 0 && e.ColumnIndex == InvoicesDataGridView.Columns["colIsSynced"].Index)
            {
                e.PaintBackground(e.CellBounds, true);

                string value = e.Value?.ToString() ?? "";
                Image icon = null;

                if (value.Equals("Yes", StringComparison.OrdinalIgnoreCase))
                    icon = Resources.GreenTick;
                else if (value.Equals("No", StringComparison.OrdinalIgnoreCase))
                    icon = Resources.RedCross;

                if (icon != null)
                {
                    int iconSize = (int)(e.CellBounds.Height * 0.7);
                    iconSize = Math.Max(24, Math.Min(iconSize, 32));

                    int x = e.CellBounds.X + (e.CellBounds.Width - iconSize) / 2;
                    int y = e.CellBounds.Y + (e.CellBounds.Height - iconSize) / 2;

                    e.Graphics.DrawImage(icon, new Rectangle(x, y, iconSize, iconSize));
                    e.Handled = true;
                }
            }

            // NEW: Handle Print hyperlink column
            if (e.RowIndex >= 0 && e.ColumnIndex == InvoicesDataGridView.Columns["colPrint"].Index)
            {
                e.PaintBackground(e.CellBounds, true);

                bool isHovered = _hoveredCell != null &&
                                _hoveredCell.RowIndex == e.RowIndex &&
                                _hoveredCell.ColumnIndex == e.ColumnIndex;

                bool isSelected = InvoicesDataGridView.Rows[e.RowIndex].Selected;

                // Hyperlink colors - white when selected, otherwise blue/green
                Color linkColor;
                if (isSelected)
                {
                    linkColor = Color.White;
                }
                else
                {
                    linkColor = isHovered ? Color.FromArgb(34, 197, 94) : Color.FromArgb(59, 130, 246);
                }

                FontStyle fontStyle = isHovered ? (FontStyle.Bold | FontStyle.Underline) : FontStyle.Bold;

                string linkText = "Print";
                using (var font = new Font("Segoe UI", 9.5F, fontStyle))
                using (var brush = new SolidBrush(linkColor))
                {
                    var textSize = e.Graphics.MeasureString(linkText, font);
                    float x = e.CellBounds.X + (e.CellBounds.Width - textSize.Width) / 2;
                    float y = e.CellBounds.Y + (e.CellBounds.Height - textSize.Height) / 2;

                    // Store the bounds of the text for hit testing
                    _printLinkBounds = new Rectangle(
                        (int)x,
                        (int)y,
                        (int)textSize.Width,
                        (int)textSize.Height
                    );

                    e.Graphics.DrawString(linkText, font, brush, x, y);
                }

                e.Handled = true;
            }
        }
        /// <summary>
        /// Sets an image inside a button, scales it properly, and centers it.
        /// </summary>
        /// <param name="btn">The Button to apply the image to</param>
        /// <param name="img">The Image to set</param>

        private void LogsDataGridView_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            // Check if it's the "Type" column (colException - index 2)
            if (e.ColumnIndex == 2 && e.RowIndex >= 0)
            {
                var value = e.Value?.ToString() ?? "";

                Color badgeColor = Color.LightGray;
                Color textColor = Color.Black;

                if (value.Equals("Information", StringComparison.OrdinalIgnoreCase) ||
                    value.Equals("Info", StringComparison.OrdinalIgnoreCase))
                {
                    badgeColor = Color.FromArgb(209, 250, 229);
                    textColor = Color.FromArgb(5, 150, 105);
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

                    // ✅ Fixed badge width, dynamic badge height
                    int badgeWidth = 120;
                    int padding = 8; // space above/below inside the row
                    int badgeHeight = e.CellBounds.Height - padding;

                    // Center vertically
                    int x = e.CellBounds.X + 15;
                    int y = e.CellBounds.Y + (e.CellBounds.Height - badgeHeight) / 2;

                    // Create rounded rectangle for badge
                    using (var path = GetRoundedRect(new Rectangle(x, y, badgeWidth, badgeHeight), 6))
                    {
                        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        using (var brush = new SolidBrush(badgeColor))
                        {
                            e.Graphics.FillPath(brush, path);
                        }
                    }

                    // Draw centered text
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



        // Call these methods in your DashboardForm_Load or InitializeComponent
        private void ApplyDataGridStyles()
        {
            StyleInvoicesDataGridView();
            StyleLogsDataGridView();
        }

        private void Panel_Resize(object? sender, EventArgs e)
        {
            if (sender is Panel panel)
            {
                DrawInvoicePieChart(panel, _syncedCount, _pendingCount);
            }
        }
        private void DrawInvoicePieChart(Panel panel, int pendingCount, int syncedCount)
        {
            if (panel == null) return;

            int panelWidth = panel.Width;
            int panelHeight = panel.Height;

            // ✅ Prevent invalid drawing when minimized or too small
            if (panelWidth <= 0 || panelHeight <= 0) return;

            panel.Controls.Clear();

            // Values and labels
            List<int> values = new List<int> { pendingCount, syncedCount };
            List<Color> colors = new List<Color>
            {
                ColorTranslator.FromHtml("#66BB6A"), // Synced
                ColorTranslator.FromHtml("#EF5350")  // Not Synced
            };
            List<string> labels = new List<string> { "Synced", "Not Synced" };

            float total = values.Sum();
            if (total == 0) total = 1; // prevent division by zero

            // ---------- Header ----------
            Label header = new Label
            {
                Text = "ALL INVOICES DETAIL",
                AutoSize = false,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold), // smaller than before
                ForeColor = Color.Black,
                TextAlign = ContentAlignment.MiddleCenter,
                Width = panelWidth,
                Height = 25
            };
            header.Location = new Point(0, 8); // some top padding
            panel.Controls.Add(header);

            // ---------- Pie chart ----------

            // Calculate pie chart size (75% of available space)
            int legendHeight = 40;
            int availableHeight = panelHeight - header.Bottom - legendHeight - 20;
            int pieSize = (int)(Math.Min(panelWidth, availableHeight) * 0.75);

            if (pieSize <= 0) return;

            // Center the pie chart
            int pieX = (panelWidth - pieSize) / 2;
            int pieY = header.Bottom + ((availableHeight - pieSize) / 2) + 10;

            // Draw pie chart
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

            // ---------- Legend ----------
            int legendY = panelHeight - legendHeight + 5;
            int spacingX = 120; // spacing between legend items
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

        // Optional: Add method to refresh log statistics independently
        public async Task RefreshLogStatisticsAsync()
        {
            try
            {
                var response = await _logService.GetAllAsync();
                if (response?.Data != null)
                {
                    IEnumerable<LogDto> filteredLogs = response.Data;

                    if (!_isInitialLoad) // Apply date filter after initial load
                    {
                        filteredLogs = filteredLogs.Where(l =>
                            l.CreatedAtPk >= _startDate &&
                            l.CreatedAtPk <= _endDate.AddDays(1).AddTicks(-1));
                    }

                    CalculateLogStatistics(filteredLogs);
                    UpdateLogStatisticsDisplay();
                }
            }
            catch (Exception ex)
            {
                // Silently handle errors in statistics refresh
                Console.WriteLine($"Error refreshing log statistics: {ex.Message}");
            }
        }

        private void DashboardForm_Load_1(object sender, EventArgs e)
        {

        }
    }
}