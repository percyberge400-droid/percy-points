using POSPRA.Application.Services.FiscalService;
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
        private readonly ILogService _logService;
        private readonly IFiscalService _fiscalService;

        private bool _isInitialLoad = true;
        private bool _startDateSelected = false;
        private bool _endDateSelected = false;
        private bool _filterSyncedOnly = false;

        private bool _isSyncedSortDescending = true; // Default: Yes on top

        private int _isLoadingFlag = 0;

        public DashboardForm(IServiceProvider provider, ILogService logService, IFiscalService fiscalService)
        {
            InitializeComponent();

            this.Load += (s, e) => CenterProgressBar();
            this.Resize += (s, e) => CenterProgressBar();
            this.Load += DashboardForm_Load;

            _provider = provider;
            _fiscalService = fiscalService ?? throw new ArgumentNullException(nameof(fiscalService));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));

            FormBorderStyle = FormBorderStyle.None;
            ControlBox = false;
            ShowIcon = false;
            Text = string.Empty;

            dtpInvoicesStart.ValueChanged += dtpInvoicesStart_ValueChanged;
            dtpInvoicesEnd.ValueChanged += dtpInvoicesEnd_ValueChanged;

            dtpInvoicesStart.Format = DateTimePickerFormat.Custom;
            dtpInvoicesStart.CustomFormat = "' Select Start Date'";
            dtpInvoicesEnd.Format = DateTimePickerFormat.Custom;
            dtpInvoicesEnd.CustomFormat = "' Select End Date'";

            btnFilterInvoices.Click += btnFilterInvoices_Click;
            btnToday.Click += btnToday_Click;
            btnClearFilter.Click += btnClearFilter_Click;
            btnRefresh.Click += btnRefresh_Click;
            btnFilterSynced.Click += btnFilterSynced_Click;
            btnLoadFullData.Click += btnLoadFullData_Click;

            btnExportLogs.Click += btnExportLogs_Click;

            if (progressBar != null) progressBar.Visible = false;
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

        private void dtpInvoicesStart_ValueChanged(object sender, EventArgs e)
        {
            dtpInvoicesStart.Format = DateTimePickerFormat.Short;
            _startDateSelected = true;
        }

        private void dtpInvoicesEnd_ValueChanged(object sender, EventArgs e)
        {
            dtpInvoicesEnd.Format = DateTimePickerFormat.Short;
            _endDateSelected = true;
        }

        private async void DashboardForm_Load(object sender, EventArgs e)
        {
            try
            {
                // Initial load: last 7 days only
                dtpInvoicesStart.Value = DateTime.Today.AddDays(-7);
                dtpInvoicesEnd.Value = DateTime.Today;
                _startDateSelected = true;
                _endDateSelected = true;

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
                var response = await _fiscalService.GetAllAsync();
                InvoicesDataGridView.Rows.Clear();

                if (response?.Data == null || !response.Data.Any())
                {
                    //WindowsLocalAppNotification.Show("Invoices", "No invoices found.");
                    AlertManager.ShowWarning("No invoices found.");
                    return;
                }

                IEnumerable<dynamic> filteredInvoices = response.Data;

                if (!skipDateFilter && _startDateSelected && _endDateSelected)
                {
                    filteredInvoices = filteredInvoices.Where(i =>
                        i.DateCreated >= dtpInvoicesStart.Value.Date &&
                        i.DateCreated <= dtpInvoicesEnd.Value.Date.AddDays(1).AddTicks(-1));
                }

                if (_filterSyncedOnly)
                    filteredInvoices = filteredInvoices.Where(i => i.IsSynced == 1);

                // ✅ Order by most recent (DateCreated DESC)
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
                }

                var rows = new List<DataGridViewRow>();
                int syncedCount = 0, pendingCount = 0, inProgressCount = 0;

                for (int i = 0; i < invoicesList.Count; i++)
                {
                    var inv = invoicesList[i];
                    var row = new DataGridViewRow();
                    row.CreateCells(InvoicesDataGridView);

                    // ✅ Sr No. (1 = most recent invoice)
                    row.Cells[InvoicesDataGridView.Columns["colId"].Index].Value = i + 1;

                    row.Cells[InvoicesDataGridView.Columns["colPosId"].Index].Value = inv.POSID;
                    row.Cells[InvoicesDataGridView.Columns["colInvoiceNumber"].Index].Value = inv.InvoiceNumber ?? "N/A";
                    row.Cells[InvoicesDataGridView.Columns["colIsSynced"].Index].Value = inv.IsSynced == 1 ? "Yes" : "No";
                    row.Cells[InvoicesDataGridView.Columns["colAttemptCount"].Index].Value = inv.AttemptCount;
                    row.Cells[InvoicesDataGridView.Columns["colDateCreated"].Index].Value = inv.DateCreated.ToString("dd-MM-yyyy HH:mm:ss");
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

                // ✅ Update summary labels
                labelAllInvoices.Text = totalInvoices.ToString();
                labelPendingInvoice.Text = pendingCount.ToString();
                labelPaidInvoices.Text = syncedCount.ToString();
                labelInProgressInvc.Text = inProgressCount.ToString();

                if (progressBar != null)
                {
                    progressBar.Visible = false;
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
                    return;
                }

                IEnumerable<LogDto> filteredLogs = response.Data;

                if (!skipDateFilter && _startDateSelected && _endDateSelected)
                {
                    filteredLogs = filteredLogs.Where(l =>
                        l.CreatedAtPk >= dtpInvoicesStart.Value.Date &&
                        l.CreatedAtPk <= dtpInvoicesEnd.Value.Date.AddDays(1).AddTicks(-1));
                }

                // ✅ Order by CreatedAtPk DESC (latest log first)
                var logsList = filteredLogs
                    .OrderByDescending(l => l.CreatedAtPk)
                    .ToList();

                int totalLogs = logsList.Count;

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
            }
            catch (Exception ex)
            {
                LogsDataGridView.ResumeLayout(true);
                if (progressBar != null) progressBar.Visible = false;

                WindowsLocalAppNotification.Show("Logs Error", $"Error loading logs: {ex.Message}");
                AlertManager.ShowError($"Error loading logs: {ex.Message}");
            }
        }


        // ----------------------------------------
        // Full Data Button
        // ----------------------------------------
        private async void btnLoadFullData_Click(object sender, EventArgs e)
        {
            ResetSortToDefault();
            _startDateSelected = false;
            _endDateSelected = false;

            await RunSingleLoad(async () =>
            {
                await LoadAndShowInvoicesAsync(skipDateFilter: true);
                await LoadAndShowLogsAsync(skipDateFilter: true);
            });
        }

        // ----------------------------------------
        // Export Invoice Button
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
            dtpInvoicesStart.Value = DateTime.Today;
            dtpInvoicesEnd.Value = DateTime.Today;
            _startDateSelected = true;
            _endDateSelected = true;
            await RunSingleLoad(async () =>
            {
                await LoadAndShowInvoicesAsync();
                await LoadAndShowLogsAsync();
            });
        }

        private async void btnClearFilter_Click(object sender, EventArgs e)
        {
            ResetSortToDefault();
            _startDateSelected = false;
            _endDateSelected = false;
            _filterSyncedOnly = false;

            // ✅ Reset backend values safely
            dtpInvoicesStart.Value = DateTime.Today; // start can default to today
            dtpInvoicesEnd.Value = DateTime.Today.AddDays(1);

            // ✅ Reset UI placeholders
            dtpInvoicesStart.Format = DateTimePickerFormat.Custom;
            dtpInvoicesStart.CustomFormat = "' Select Start Date'";
            dtpInvoicesEnd.Format = DateTimePickerFormat.Custom;
            dtpInvoicesEnd.CustomFormat = "' Select End Date'";

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

        private void ClearAllGrids()
        {
            // Clear both grids
            ClearDataGridView(InvoicesDataGridView);
            ClearDataGridView(LogsDataGridView);

            // Reset all statistics labels
            labelAllInvoices.Text = "0";
            labelPendingInvoice.Text = "0";
            labelPaidInvoices.Text = "0";
            labelInProgressInvc.Text = "0";

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
                bool skipDateFilter = !_startDateSelected || !_endDateSelected;

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
    }
}
