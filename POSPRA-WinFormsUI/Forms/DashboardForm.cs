using POSPRA.Application.Services.FiscalService;
using POSPRA.Application.Services.LogService;
using POSPRA.DTOs.LogDtos;
using POSPRA_WinFormsUI.AlertClasses;
using System.Data;

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
            dtpInvoicesStart.CustomFormat = "'Select Start Date'";
            dtpInvoicesEnd.Format = DateTimePickerFormat.Custom;
            dtpInvoicesEnd.CustomFormat = "'Select End Date'";

            btnFilterInvoices.Click += btnFilterInvoices_Click;
            btnToday.Click += btnToday_Click;
            btnClearFilter.Click += btnClearFilter_Click;
            btnRefresh.Click += btnRefresh_Click;
            btnFilterSynced.Click += btnFilterSynced_Click;
            btnLoadFullData.Click += btnLoadFullData_Click;

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
                    WindowsLocalAppNotification.Show("Invoices", "No invoices found.");
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

                // ✅ Order by ID descending instead of DateCreated
                var invoicesList = filteredInvoices.OrderByDescending(i => i.ID).ToList();
                int totalInvoices = invoicesList.Count;

                // Suspend layout to prevent multiple redraws
                InvoicesDataGridView.SuspendLayout();

                // Optional: Show progress for very large datasets
                if (progressBar != null && totalInvoices > 100)
                {
                    progressBar.Style = ProgressBarStyle.Continuous;
                    progressBar.Minimum = 0;
                    progressBar.Maximum = totalInvoices;
                    progressBar.Value = 0;
                    progressBar.Visible = true;
                }

                // Batch add rows for better performance
                var rows = new List<DataGridViewRow>();
                int syncedCount = 0, pendingCount = 0, inProgressCount = 0;

                for (int i = 0; i < invoicesList.Count; i++)
                {
                    var inv = invoicesList[i];
                    var row = new DataGridViewRow();
                    row.CreateCells(InvoicesDataGridView);

                    row.Cells[InvoicesDataGridView.Columns["colId"].Index].Value = inv.ID;
                    row.Cells[InvoicesDataGridView.Columns["colPosId"].Index].Value = inv.POSID;
                    row.Cells[InvoicesDataGridView.Columns["colInvoiceNumber"].Index].Value = inv.InvoiceNumber ?? "N/A";
                    row.Cells[InvoicesDataGridView.Columns["colIsSynced"].Index].Value = inv.IsSynced == 1 ? "Yes" : "No";
                    row.Cells[InvoicesDataGridView.Columns["colAttemptCount"].Index].Value = inv.AttemptCount;
                    row.Cells[InvoicesDataGridView.Columns["colDateCreated"].Index].Value = inv.DateCreated.ToString("yyyy-MM-dd");
                    row.Tag = new { inv.IsSynced, inv.AttemptCount };

                    rows.Add(row);

                    // Count statistics while we're iterating
                    if (inv.IsSynced == 1) syncedCount++;
                    else pendingCount++;
                    if (inv.AttemptCount > 0) inProgressCount++;

                    // Update progress less frequently for better performance
                    if (progressBar != null && totalInvoices > 100 && i % 10 == 0)
                    {
                        progressBar.Value = Math.Min(i + 1, progressBar.Maximum);
                        Application.DoEvents(); // Allow UI to update occasionally
                    }
                }

                // Add all rows at once
                InvoicesDataGridView.Rows.AddRange(rows.ToArray());

                // Resume layout and refresh
                InvoicesDataGridView.ResumeLayout(true);

                // Update labels with pre-calculated counts
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

                var logsList = filteredLogs.OrderByDescending(l => l.Id).ToList();
                int totalLogs = logsList.Count;

                // Suspend layout to prevent multiple redraws
                LogsDataGridView.SuspendLayout();

                // Optional: Show progress for very large datasets
                if (progressBar != null && totalLogs > 100)
                {
                    progressBar.Style = ProgressBarStyle.Continuous;
                    progressBar.Minimum = 0;
                    progressBar.Maximum = totalLogs;
                    progressBar.Value = 0;
                    progressBar.Visible = true;
                }

                // Batch add rows for better performance
                var rows = new List<DataGridViewRow>();

                for (int i = 0; i < logsList.Count; i++)
                {
                    var log = logsList[i];
                    var row = new DataGridViewRow();
                    row.CreateCells(LogsDataGridView);

                    row.Cells[LogsDataGridView.Columns["colLogID"].Index].Value = log.Id;
                    row.Cells[LogsDataGridView.Columns["colMessage"].Index].Value = log.Message ?? "No message";
                    row.Cells[LogsDataGridView.Columns["colException"].Index].Value = log.Type ?? "N/A";
                    row.Cells[LogsDataGridView.Columns["logdatetime"].Index].Value = log.CreatedAtPk.ToString("dd-MM-yyyy HH:mm:ss");

                    rows.Add(row);

                    // Update progress less frequently for better performance
                    if (progressBar != null && totalLogs > 100 && i % 10 == 0)
                    {
                        progressBar.Value = Math.Min(i + 1, progressBar.Maximum);
                        Application.DoEvents(); // Allow UI to update occasionally
                    }
                }

                // Add all rows at once
                LogsDataGridView.Rows.AddRange(rows.ToArray());

                // Resume layout and refresh
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
            _startDateSelected = false;
            _endDateSelected = false;

            await RunSingleLoad(async () =>
            {
                await LoadAndShowInvoicesAsync(skipDateFilter: true);
                await LoadAndShowLogsAsync(skipDateFilter: true);
            });
        }

        // ----------------------------------------
        // Other Buttons
        // ----------------------------------------
        private async void btnFilterInvoices_Click(object sender, EventArgs e) => await RunSingleLoad(async () =>
        {
            await LoadAndShowInvoicesAsync();
            await LoadAndShowLogsAsync();
        });

        private async void btnFilterSynced_Click(object sender, EventArgs e)
        {
            _filterSyncedOnly = true;
            await RunSingleLoad(async () => await LoadAndShowInvoicesAsync());
        }

        private async void btnToday_Click(object sender, EventArgs e)
        {
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
            _startDateSelected = false;
            _endDateSelected = false;
            _filterSyncedOnly = false;
            dtpInvoicesStart.Format = DateTimePickerFormat.Custom;
            dtpInvoicesStart.CustomFormat = "'Select Start Date'";
            dtpInvoicesEnd.Format = DateTimePickerFormat.Custom;
            dtpInvoicesEnd.CustomFormat = "'Select End Date'";

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
