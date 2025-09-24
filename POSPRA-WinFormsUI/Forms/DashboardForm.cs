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

        private bool _isInitialLoad = true; // skip notifications on first load
        private bool _startDateSelected = false;
        private bool _endDateSelected = false;
        private bool _filterSyncedOnly = false;

        // Atomic flag (0 = not loading, 1 = loading)
        private int _isLoadingFlag = 0;

        public DashboardForm(IServiceProvider provider, ILogService logService, IFiscalService fiscalService)
        {
            InitializeComponent();
            // Center the progress bar on load
            // Position progress bar in the center of InvoicesDataGridView
            this.Load += (s, e) =>
            {
                CenterProgressBar();
            };

            // Keep centered if form resized
            this.Resize += (s, e) =>
            {
                CenterProgressBar();
            };


            this.Load += DashboardForm_Load;

            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.MinimumSize = new Size(1024, 600);

            _provider = provider;
            _fiscalService = fiscalService ?? throw new ArgumentNullException(nameof(fiscalService));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));

            // Remove form chrome
            this.FormBorderStyle = FormBorderStyle.None;
            this.ControlBox = false;
            this.ShowIcon = false;
            this.Text = string.Empty;

            // Date pickers initially blank
            dtpInvoicesStart.ValueChanged += dtpInvoicesStart_ValueChanged;
            dtpInvoicesEnd.ValueChanged += dtpInvoicesEnd_ValueChanged;
            dtpInvoicesStart.Format = DateTimePickerFormat.Custom;
            dtpInvoicesStart.CustomFormat = "'Select Start Date'";

            dtpInvoicesEnd.Format = DateTimePickerFormat.Custom;
            dtpInvoicesEnd.CustomFormat = "'Select End Date'";

            // Hook filter buttons
            btnFilterInvoices.Click += btnFilterInvoices_Click;
            btnToday.Click += btnToday_Click;
            btnClearFilter.Click += btnClearFilter_Click;
            btnRefresh.Click += btnRefresh_Click;
            btnFilterSynced.Click += btnFilterSynced_Click;

            if (progressBar != null)
            {
                progressBar.Visible = false;
            }
        }
        private void CenterProgressBar()
        {
            if (progressBar != null && InvoicesDataGridView != null)
            {
                // Get the bounds of the invoices grid
                var gridBounds = InvoicesDataGridView.Bounds;

                // Center progress bar inside that region
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
                await RunSingleLoad(async () =>
                {
                    await LoadAndShowInvoicesAsync(skipDateFilter: true);
                    await LoadAndShowLogsAsync(skipDateFilter: true);
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


        private async void btnFilterInvoices_Click(object sender, EventArgs e)
        {
            await RunSingleLoad(async () =>
            {
                await LoadAndShowInvoicesAsync();
                await LoadAndShowLogsAsync();
            });
        }

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

                var filteredInvoices = skipDateFilter || !_startDateSelected || !_endDateSelected
                    ? response.Data
                    : response.Data.Where(i =>
                        i.DateCreated >= dtpInvoicesStart.Value.Date &&
                        i.DateCreated <= dtpInvoicesEnd.Value.Date.AddDays(1).AddTicks(-1));

                if (_filterSyncedOnly)
                    filteredInvoices = filteredInvoices.Where(i => i.IsSynced == 1);
                var invoicesList = filteredInvoices.OrderByDescending(i => i.DateCreated).ToList();
                int totalInvoices = invoicesList.Count;

                if (progressBar != null)
                {
                    progressBar.Style = ProgressBarStyle.Continuous; // optional, for determinate animation
                    progressBar.Minimum = 0;
                    progressBar.Maximum = totalInvoices;
                    progressBar.Value = 0;
                    //progressBar.Visible = true;
                    progressBar.BringToFront();
                }

                foreach (var inv in invoicesList)
                {
                    int rowIndex = InvoicesDataGridView.Rows.Add();
                    var row = InvoicesDataGridView.Rows[rowIndex];

                    row.Cells["colId"].Value = inv.ID;
                    row.Cells["colPosId"].Value = inv.POSID;
                    row.Cells["colInvoiceNumber"].Value = inv.InvoiceNumber ?? "N/A";
                    row.Cells["colIsSynced"].Value = inv.IsSynced == 1 ? "Yes" : "No";
                    row.Cells["colAttemptCount"].Value = inv.AttemptCount;
                    row.Cells["colDateCreated"].Value = inv.DateCreated.ToString("yyyy-MM-dd");

                    row.Tag = new { inv.IsSynced, inv.AttemptCount };

                    // Update the progress bar
                    if (progressBar != null)
                    {
                        progressBar.Value = Math.Min(progressBar.Value + 1, progressBar.Maximum);
                        progressBar.Refresh();
                    }

                    // Yield control so the UI updates (including progress bar animation)
                    await Task.Yield();
                }

                if (progressBar != null)
                {
                    progressBar.Visible = false; // hide when done
                }


                labelAllInvoices.Text = InvoicesDataGridView.Rows.Count.ToString();
                labelPendingInvoice.Text = InvoicesDataGridView.Rows.Cast<DataGridViewRow>()
                    .Count(r => ((dynamic)r.Tag).IsSynced == 0).ToString();
                labelPaidInvoices.Text = InvoicesDataGridView.Rows.Cast<DataGridViewRow>()
                    .Count(r => ((dynamic)r.Tag).IsSynced == 1).ToString();
                labelInProgressInvc.Text = InvoicesDataGridView.Rows.Cast<DataGridViewRow>()
                    .Count(r => ((dynamic)r.Tag).AttemptCount > 0).ToString();

                if (!skipDateFilter)
                {
                    string message = _filterSyncedOnly
                        ? "Synced invoices loaded successfully"
                        : "Invoices loaded successfully";
                    WindowsLocalAppNotification.Show("Invoices", message);
                    AlertManager.ShowSuccess(message);
                }
            }
            catch (Exception ex)
            {
                WindowsLocalAppNotification.Show("Invoices Error", $"Error loading invoices: {ex.Message}");
                AlertManager.ShowError($"Error loading invoices: {ex.Message}");
            }
        }

        private async Task LoadAndShowLogsAsync(bool skipDateFilter = false)
        {
            try
            {
                var response = await _logService.GetAllAsync();
                LogsDataGridView.Rows.Clear();

                if (response?.Data != null && response.Data.Any())
                {
                    IEnumerable<LogDto> filteredLogs = response.Data;

                    if (!skipDateFilter)
                    {
                        DateTime? startDate = dtpInvoicesStart.Format != DateTimePickerFormat.Custom
                            ? dtpInvoicesStart.Value.Date : null;

                        DateTime? endDate = dtpInvoicesEnd.Format != DateTimePickerFormat.Custom
                            ? dtpInvoicesEnd.Value.Date.AddDays(1).AddTicks(-1) : null;

                        if (startDate.HasValue && endDate.HasValue)
                        {
                            filteredLogs = filteredLogs.Where(l =>
                                l.CreatedAtPk >= startDate.Value &&
                                l.CreatedAtPk <= endDate.Value);
                        }
                    }
                    var logsList = filteredLogs.OrderByDescending(l => l.Id).ToList();
                    int totalLogs = logsList.Count;

                    if (progressBar != null)
                    {
                        progressBar.Style = ProgressBarStyle.Continuous;
                        progressBar.Minimum = 0;
                        progressBar.Maximum = totalLogs;
                        progressBar.Value = 0;
                        //progressBar.Visible = true;
                        progressBar.BringToFront();
                    }

                    foreach (var log in logsList)
                    {
                        int rowIndex = LogsDataGridView.Rows.Add();
                        var row = LogsDataGridView.Rows[rowIndex];

                        row.Cells["colLogID"].Value = log.Id;
                        row.Cells["colMessage"].Value = log.Message ?? "No message";
                        row.Cells["colException"].Value = log.Type ?? "N/A";
                        row.Cells["logdatetime"].Value = log.CreatedAtPk.ToString("dd-MM-yyyy HH:mm:ss");

                        if (progressBar != null)
                        {
                            progressBar.Value = Math.Min(progressBar.Value + 1, progressBar.Maximum);
                            progressBar.Refresh();
                        }

                        await Task.Yield(); // allow UI to update
                    }

                    if (progressBar != null)
                    {
                        progressBar.Visible = false;
                    }

                }
                else
                {
                    WindowsLocalAppNotification.Show("Logs", "No logs available to display");
                    AlertManager.ShowWarning("No logs available to display");
                }
            }
            catch (Exception ex)
            {
                WindowsLocalAppNotification.Show("Logs Error", $"Error loading logs: {ex.Message}");
                AlertManager.ShowError($"Error loading logs: {ex.Message}");
            }
        }

        private async void btnFilterSynced_Click(object sender, EventArgs e)
        {
            await RunSingleLoad(async () =>
            {
                _filterSyncedOnly = true;
                await LoadAndShowInvoicesAsync();
            });
        }

        private async void btnToday_Click(object sender, EventArgs e)
        {
            dtpInvoicesStart.ValueChanged -= dtpInvoicesStart_ValueChanged;
            dtpInvoicesEnd.ValueChanged -= dtpInvoicesEnd_ValueChanged;

            dtpInvoicesStart.Value = DateTime.Today;
            dtpInvoicesEnd.Value = DateTime.Today;

            dtpInvoicesStart.Format = DateTimePickerFormat.Short;
            dtpInvoicesEnd.Format = DateTimePickerFormat.Short;

            _startDateSelected = true;
            _endDateSelected = true;

            dtpInvoicesStart.ValueChanged += dtpInvoicesStart_ValueChanged;
            dtpInvoicesEnd.ValueChanged += dtpInvoicesEnd_ValueChanged;

            await RunSingleLoad(async () =>
            {
                await LoadAndShowInvoicesAsync();
                await LoadAndShowLogsAsync();
            });
        }

        private async void btnClearFilter_Click(object sender, EventArgs e)
        {
            dtpInvoicesStart.Format = DateTimePickerFormat.Custom;
            dtpInvoicesStart.CustomFormat = "'Select Start Date'";
            dtpInvoicesEnd.Format = DateTimePickerFormat.Custom;
            dtpInvoicesEnd.CustomFormat = "'Select End Date'";

            _startDateSelected = false;
            _endDateSelected = false;
            _filterSyncedOnly = false;

            await RunSingleLoad(async () =>
            {
                await LoadAndShowInvoicesAsync(skipDateFilter: true);
                await LoadAndShowLogsAsync(skipDateFilter: true);
            });
        }

        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            bool skipDateFilter = dtpInvoicesStart.Format == DateTimePickerFormat.Custom
                                  || dtpInvoicesEnd.Format == DateTimePickerFormat.Custom;

            await RunSingleLoad(async () =>
            {
                await LoadAndShowInvoicesAsync(skipDateFilter: skipDateFilter);
                await LoadAndShowLogsAsync(skipDateFilter: skipDateFilter);
            });
        }

        private void dataGridView_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;

            if (sender is DataGridView grid && e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                grid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "N/A";
            }
        }

        private void UpdateProgress(int value, int max)
        {
            if (progressBar == null) return;
            progressBar.Maximum = max;
            progressBar.Value = Math.Min(value, max);
            progressBar.Refresh();
        }
    }
}
