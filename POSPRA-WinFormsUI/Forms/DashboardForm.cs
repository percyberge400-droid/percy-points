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



        // 1. CHECK YOUR CONSTRUCTOR - ADD THE MISSING EVENT HANDLER
        public DashboardForm(IServiceProvider provider, ILogService logService, IFiscalService fiscalService)
        {
            InitializeComponent();
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
                // Show all records unfiltered initially
                await LoadAndShowInvoicesAsync(skipDateFilter: true);
                await LoadAndShowLogsAsync(skipDateFilter: true);

                InvoicesDataGridView.DataError += dataGridView_DataError;
                LogsDataGridView.DataError += dataGridView_DataError;

                _isInitialLoad = false; // mark initial load done
            }
            catch (Exception ex)
            {
                WindowsLocalAppNotification.Show("Dashboard Error", $"Error loading dashboard: {ex.Message}");
                AlertManager.ShowError($"Error loading dashboard: {ex.Message}");
            }
        }

        // ================== MASTER FILTER BUTTON ==================
        private async void btnFilterInvoices_Click(object sender, EventArgs e)
        {
            await LoadAndShowInvoicesAsync();
            await LoadAndShowLogsAsync();
        }

        // ================== INVOICES GRID ==================
        private async Task LoadAndShowInvoicesAsync(bool skipDateFilter = false)
        {
            try
            {
                var response = await _fiscalService.GetAllAsync();
                if (response?.Data == null || !response.Data.Any())
                {
                    WindowsLocalAppNotification.Show("Invoices", "No invoices found.");
                    AlertManager.ShowWarning("No invoices found.");
                    return;
                }

                var filteredInvoices = skipDateFilter || !_startDateSelected || !_endDateSelected
                    ? response.Data
                    : response.Data
                        .Where(i => i.DateCreated >= dtpInvoicesStart.Value.Date &&
                                    i.DateCreated <= dtpInvoicesEnd.Value.Date.AddDays(1).AddTicks(-1));

                // Apply synced filter if enabled
                if (_filterSyncedOnly)
                {
                    filteredInvoices = filteredInvoices.Where(i => i.IsSynced == 1);
                }

                filteredInvoices = filteredInvoices.OrderByDescending(i => i.DateCreated);

                InvoicesDataGridView.Rows.Clear();

                foreach (var inv in filteredInvoices)
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
                }

                // Update top summary labels
                labelAllInvoices.Text = InvoicesDataGridView.Rows.Count.ToString();
                labelPendingInvoice.Text = InvoicesDataGridView.Rows.Cast<DataGridViewRow>().Count(r => ((dynamic)r.Tag).IsSynced == 0).ToString();
                labelPaidInvoices.Text = InvoicesDataGridView.Rows.Cast<DataGridViewRow>().Count(r => ((dynamic)r.Tag).IsSynced == 1).ToString();
                labelInProgressInvc.Text = InvoicesDataGridView.Rows.Cast<DataGridViewRow>().Count(r => ((dynamic)r.Tag).AttemptCount > 0).ToString();

                if (!skipDateFilter)
                {
                    string message = _filterSyncedOnly ? "Synced invoices loaded successfully" : "Invoices loaded successfully";
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

        // ================== LOGS GRID ==================
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
                        // Determine filter bounds
                        DateTime? startDate = dtpInvoicesStart.Format != DateTimePickerFormat.Custom
                            ? dtpInvoicesStart.Value.Date
                            : null;

                        DateTime? endDate = dtpInvoicesEnd.Format != DateTimePickerFormat.Custom
                            ? dtpInvoicesEnd.Value.Date.AddDays(1).AddTicks(-1)
                            : null;

                        if (startDate.HasValue && endDate.HasValue)
                        {
                            filteredLogs = filteredLogs.Where(l => l.CreatedAtPk >= startDate.Value && l.CreatedAtPk <= endDate.Value);
                        }
                    }

                    filteredLogs = filteredLogs.OrderByDescending(l => l.Id);

                    foreach (var log in filteredLogs)
                    {
                        int rowIndex = LogsDataGridView.Rows.Add();
                        var row = LogsDataGridView.Rows[rowIndex];
                        row.Cells["colLogID"].Value = log.Id;
                        row.Cells["colMessage"].Value = log.Message ?? "No message";
                        row.Cells["colException"].Value = log.Type ?? "N/A";
                        row.Cells["logdatetime"].Value = log.CreatedAtPk.ToString("dd-MM-yyyy HH:mm:ss");
                    }

                    if (!_isInitialLoad)
                    {
                        WindowsLocalAppNotification.Show("Logs", "Logs loaded successfully");
                        AlertManager.ShowSuccess("Logs loaded successfully");
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

        // ================== SYNC FILTER BUTTON ==================
        private async void btnFilterSynced_Click(object sender, EventArgs e)
        {
            _filterSyncedOnly = true; // Enable synced filter
            await LoadAndShowInvoicesAsync();
        }

        // ================== TODAY BUTTON ==================
        private async void btnToday_Click(object sender, EventArgs e)
        {
            dtpInvoicesStart.ValueChanged -= dtpInvoicesStart_ValueChanged;
            dtpInvoicesEnd.ValueChanged -= dtpInvoicesEnd_ValueChanged;

            dtpInvoicesStart.Value = DateTime.Today;
            dtpInvoicesEnd.Value = DateTime.Today;

            dtpInvoicesStart.Format = DateTimePickerFormat.Short;
            dtpInvoicesEnd.Format = DateTimePickerFormat.Short;

            // Force them as selected for filtering
            _startDateSelected = true;
            _endDateSelected = true;

            // Reattach events
            dtpInvoicesStart.ValueChanged += dtpInvoicesStart_ValueChanged;
            dtpInvoicesEnd.ValueChanged += dtpInvoicesEnd_ValueChanged;

            // Load filtered data
            await LoadAndShowInvoicesAsync();
            await LoadAndShowLogsAsync();
        }

        // ================== CLEAR FILTER BUTTON ==================

        private async void btnClearFilter_Click(object sender, EventArgs e)
        {
            // Reset date pickers
            dtpInvoicesStart.Format = DateTimePickerFormat.Custom;
            dtpInvoicesStart.CustomFormat = "'Select Start Date'";
            dtpInvoicesEnd.Format = DateTimePickerFormat.Custom;
            dtpInvoicesEnd.CustomFormat = "'Select End Date'";

            // Reset flags
            _startDateSelected = false;
            _endDateSelected = false;
            _filterSyncedOnly = false; // Reset synced filter

            // Reload all data
            await LoadAndShowInvoicesAsync(skipDateFilter: true);
            await LoadAndShowLogsAsync(skipDateFilter: true);
        }


        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            // Determine if date pickers have a real date selected
            bool skipDateFilter = dtpInvoicesStart.Format == DateTimePickerFormat.Custom
                                  || dtpInvoicesEnd.Format == DateTimePickerFormat.Custom;

            // Reload invoices and logs using current date filter if selected
            await LoadAndShowInvoicesAsync(skipDateFilter: skipDateFilter);
            await LoadAndShowLogsAsync(skipDateFilter: skipDateFilter);
        }


        private void dataGridView_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;

            if (sender is DataGridView grid && e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                grid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "N/A";
            }
        }
    }
}
