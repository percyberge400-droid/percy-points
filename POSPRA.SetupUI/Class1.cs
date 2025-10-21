//using LiteDB;
//using Microsoft.Extensions.Logging;
//using System.ComponentModel;
//using System.Configuration;
//using System.Data;

//using System.Diagnostics;
//using System.ServiceProcess;

//namespace PRA_POS
//{


//    public partial class DashboardForm : Form
//    {
//        public string connectionString;
//        private readonly IWindowsServiceManager _serviceManager;
//        string defaultPath = ConfigurationManager.AppSettings["DefaultFilePath"];
//        string defaultPassword = ConfigurationManager.AppSettings["DbPassword"];
//        string backupDir = ConfigurationManager.AppSettings["backupDir"];

//        private Panel overlayPanel;
//        private PictureBox loaderPicBox;
//        private List<InvoiceGridRow> displayedInvoices = new List<InvoiceGridRow>();
//        private int syncFilterState = 0; // 0 = All, 1 = OnlyNo, 2 = OnlyYes

//        // Variables for blinking
//        private bool _isBlinking = false;
//        private CancellationTokenSource _blinkerCts;
//        // Add these as form-level variables
//        private int _currentPage = 1;
//        private int _pageSize = 10;
//        private int _totalRecords = 0;
//        private int _totalPages = 0;
//        private System.Windows.Forms.Timer serviceCheckTimer;
//        private string lastServiceStatus = string.Empty;
//        private string _currentSortOrder = "Ascending";
//        private List<dynamic> allInvoices = new List<dynamic>();
//        public bool LogoutRequested { get; private set; } = false;
//        string backupConnectionString;
//        private bool sortAscending = true;
//        public DashboardForm(IWindowsServiceManager serviceManager)
//        {
//            InitializeComponent();
//            this.AutoScaleMode = AutoScaleMode.Dpi;
//            // Prevent breaking on very small screens
//            this.AutoScroll = true;
//            this.MinimumSize = new Size(1024, 600);
//            _serviceManager = serviceManager;
//            dgvInvoices.ReadOnly = true;
//            dgvlogs.ReadOnly = true;

//            startservice.FlatStyle = FlatStyle.Flat;
//            startservice.FlatAppearance.BorderSize = 0; // optional, cleaner look
//            //connectionString = ConfigurationManager.ConnectionStrings["LiteDbConnection"].ConnectionString;
//        }// Show loader
//        private void ShowLoader()
//        {
//            overlayPanel.Visible = true;
//            overlayPanel.BringToFront();
//            loaderPicBox.BringToFront();
//        }

//        // Hide loader
//        private void HideLoader()
//        {
//            overlayPanel.Visible = false;
//        }


//        public void DashboardForm_Load(object sender, EventArgs e)
//        {
//            try
//            {
//                btnSortSynced.Enabled = false;
//                btnClear.Enabled = false;
//                btnFilter.Enabled = false;
//                // Service status
//                string status = _serviceManager.GetStatus();
//                UpdateServiceStatusAndBlink();
//                labelServiceStatus.Text = status;

//                if (status.Equals("Online", StringComparison.OrdinalIgnoreCase))
//                {
//                    startservice.Enabled = false;   // disable start button
//                    startservice.BackColor = Color.LightGray;   // background grey
//                    startservice.ForeColor = Color.White;  // text white
//                }
//                else
//                {
//                    ToastNotificationForm toast = new ToastNotificationForm("⚠️ Service has been stopped!");
//                    toast.Show();

//                    //    MessageBox.Show("Alert: The service is currently stopped!", "Service Alert",
//                    //MessageBoxButtons.OK, MessageBoxIcon.Warning);
//                    startservice.Enabled = true;    // enable start button
//                    startservice.BackColor = Color.Green;  // background green
//                    startservice.ForeColor = Color.White;  // text white

//                }
//                // Setup timer for real-time monitoring
//                serviceCheckTimer = new System.Windows.Forms.Timer();
//                serviceCheckTimer.Interval = 5000; // check every 5 seconds
//                serviceCheckTimer.Tick += ServiceCheckTimer_Tick;
//                serviceCheckTimer.Start();

//                lastServiceStatus = status;

//                pg.Visible = false;

//                //loaderpicbox.Visible = false;
//                //loaderLabel.Visible = false;
//                //btnLoad.Enabled = true;
//                // Read default path from App.config

//                filepath.Text = defaultPath;

//                // Get directory from the full path
//                string directoryPath = Path.GetDirectoryName(defaultPath);
//                // Validate file            
//                if (Directory.Exists(directoryPath))
//                {
//                    // Check if any .ims file exists in this folder
//                    var imsFiles = Directory.GetFiles(directoryPath, "*.ims");

//                    if (imsFiles.Length > 0)
//                    {
//                        btnLoad.Enabled = true; // enable load button
//                    }
//                    else
//                    {
//                        btnLoad.Enabled = false;
//                        MessageBox.Show("File path not exist (.ims file not found).");

//                    }


//                    if (Path.GetExtension(imsFiles[0]).Equals(".ims", StringComparison.OrdinalIgnoreCase))
//                    {
//                        // Build LiteDB connection string with password
//                        connectionString = $"Filename={imsFiles[0]};Password={defaultPassword};Connection=shared;";

//                        filepath.Text = imsFiles[0];

//                        // ✅ Enable LoadData only if .ims file is selected
//                        btnLoad.Enabled = true;
//                    }
//                    else
//                    {
//                        MessageBox.Show("Please select a valid .ims file.");
//                        filepath.Text = string.Empty;
//                        btnLoad.Enabled = false; // disable if not .ims
//                    }
//                }
//                else
//                {
//                    btnLoad.Enabled = false;
//                    MessageBox.Show("File path not exist (directory not found).");
//                }
//            }
//            catch (Exception ex)
//            {
//                Logger.Log("APP", "Error while loading invoices", ex);
//                MessageBox.Show($"Error: {ex.Message}");
//                btnLoad.Enabled = false;
//            }


//        }
//        private void LoadInvoices()
//        {
//            try
//            {
//                //string connectionString = ConfigurationManager.ConnectionStrings["LiteDbConnection"].ConnectionString;

//                using (var db = new LiteDatabase(connectionString))
//                {
//                    var collection = db.GetCollection<FileRecords>("filerecords");

//                    // Load all records into a list
//                    var records = collection.FindAll().ToList();

//                    // Bind to DataGridView
//                    dgvInvoices.DataSource = records;
//                }
//            }
//            catch (Exception ex)
//            {
//                Logger.Log("APP", "Error while loading invoices", ex);
//                MessageBox.Show("Error loading file records: " + ex.Message);
//            }
//        }
//        private void LoadRecords()
//        {
//            _currentPage = 1;   // Always reset to page 1
//            LoadLogsPage();     // ✅ Reuse the same paging logic
//        }



//        private void label1_Click(object sender, EventArgs e)
//        {

//        }
//        private void UpdateServiceStatus()
//        {
//            try
//            {
//                using (ServiceController sc = new ServiceController("IMS_Fiscalization"))
//                {
//                    sc.Refresh();
//                    labelServiceStatus.Text = sc.Status.ToString();
//                }
//            }
//            catch (Exception ex)
//            {
//                Logger.Log("APP", "Error while loading invoices", ex);
//                labelServiceStatus.Text = $"Error: {ex.Message}";
//            }
//        }


//        private void dgvInvoices_CellContentClick(object sender, DataGridViewCellEventArgs e)
//        {

//        }

//        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
//        {

//        }

//        private void browse_Click(object sender, EventArgs e)
//        {
//            {
//                OpenFileDialog ofd = new OpenFileDialog();
//                ofd.Title = "Select LiteDB File";
//                ofd.Filter = "LiteDB files (*.ims;*.db)|*.ims;*.db|All files (*.*)|*.*";

//                if (ofd.ShowDialog() == DialogResult.OK)
//                {
//                    string filePath = ofd.FileName;
//                    // Check extension
//                    if (Path.GetExtension(filePath).Equals(".ims", StringComparison.OrdinalIgnoreCase))
//                    {
//                        // Build LiteDB connection string with password
//                        connectionString = $"Filename={filePath};Password={defaultPassword};Connection=shared;";

//                        filepath.Text = filePath;

//                        // ✅ Enable LoadData only if .ims file is selected
//                        btnLoad.Enabled = true;
//                    }
//                    else
//                    {
//                        MessageBox.Show("Please select a valid .ims file.");
//                        filepath.Text = string.Empty;
//                        btnLoad.Enabled = false; // disable if not .ims
//                    }

//                }
//            }
//        }
//        private void filepath_TextChanged(object sender, EventArgs e)
//        {
//            btnLoad.Enabled = !string.IsNullOrWhiteSpace(filepath.Text);
//        }

//        private async void LoadData_Click(object sender, EventArgs e)
//        {
//            try
//            {
//                btnLoad.Enabled = false;
//                pg.Style = ProgressBarStyle.Marquee;
//                pg.Visible = true;


//                // Run backup + invoices loading in background thread
//                await Task.Run(() =>
//                {
//                    //if (_serviceManager.GetStatus() == "Online")
//                    //{
//                    //    _serviceManager.Stop();
//                    //}

//                    string originalFilePath = filepath.Text;
//                    string backupDir = ConfigurationManager.AppSettings["backupDir"];

//                    if (!Directory.Exists(backupDir))
//                        Directory.CreateDirectory(backupDir);

//                    foreach (string file in Directory.GetFiles(backupDir))
//                    {
//                        try { File.Delete(file); } catch { }
//                    }

//                    string fileNameWithoutExt = Path.GetFileNameWithoutExtension(originalFilePath);
//                    string extension = Path.GetExtension(originalFilePath);
//                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
//                    string backupFileName = $"{fileNameWithoutExt}_{timestamp}{extension}";
//                    string backupFilePath = Path.Combine(backupDir, backupFileName);

//                    File.Copy(originalFilePath, backupFilePath, false);

//                    backupConnectionString = $"Filename={backupFilePath};Password={defaultPassword};Connection=shared;";

//                    // === Load filerecords ===
//                    using (var db = new LiteDatabase(backupConnectionString))
//                    {
//                        var collection = db.GetCollection<FileRecords>("filerecords");
//                        var records = collection.FindAll()
//                            .Select(r => new InvoiceGridRow
//                            {
//                                _Id = r._Id,
//                                POSID = r.POSID,
//                                InvoiceNumber = r.InvoiceNumber,
//                                IsSynced = r.IsSynced == 1 ? "Yes" : "No",
//                                AttemptCount = r.AttemptCount,
//                                DateCreated = r.DateCreated
//                            })
//                            .ToList();

//                        displayedInvoices = records;
//                        syncFilterState = 0;

//                        this.Invoke(new Action(() =>
//                        {
//                            dgvInvoices.DataSource = displayedInvoices;
//                            if (dgvInvoices.Columns.Contains("_Id"))
//                                dgvInvoices.Columns["_Id"].HeaderText = "Sr. No.";

//                            if (dgvInvoices.Columns.Contains("POSID"))
//                                dgvInvoices.Columns["POSID"].HeaderText = "POS ID";

//                            if (dgvInvoices.Columns.Contains("InvoiceNumber"))
//                                dgvInvoices.Columns["InvoiceNumber"].HeaderText = "Invoice No.";

//                            if (dgvInvoices.Columns.Contains("IsSynced"))
//                                dgvInvoices.Columns["IsSynced"].HeaderText = "Invoice Synced";  // 🔄 renamed

//                            if (dgvInvoices.Columns.Contains("AttemptCount"))
//                                dgvInvoices.Columns["AttemptCount"].HeaderText = "Attempt Count";

//                            if (dgvInvoices.Columns.Contains("DateCreated"))
//                                dgvInvoices.Columns["DateCreated"].HeaderText = "Date Created";

//                            btnSortSynced.Enabled = displayedInvoices.Any();
//                            btnFilter.Enabled = displayedInvoices.Any();
//                            btnClear.Enabled = displayedInvoices.Any();
//                            btnExport.Visible = true;
//                            btnExport.Enabled = true;

//                            lbltotalrecords.Text = $"Total Records: {records.Count}";
//                            lbltotalsynced.Text = $"Total Synced: {records.Count(r => r.IsSynced == "Yes")}";
//                            lbltotalnotsynced.Text = $"Total Not Synced: {records.Count(r => r.IsSynced == "No")}";
//                            if (records.Any())
//                                lbllastcreateddate.Text = $"Last Created: {records.Max(r => r.DateCreated):dd/MM/yyyy HH:mm:ss}";
//                            else
//                                lbllastcreateddate.Text = "Last Created: N/A";

//                            dgvInvoices.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
//                            dgvInvoices.ShowCellToolTips = true;
//                        }));
//                    }
//                });

//                // === Load logs AFTER invoices are ready ===
//                _currentPage = 1;
//                await LoadLogsPageAsync();

//                // ✅ Keep loader 1 second longer after logs are displayed
//                await Task.Delay(1000);

//                pg.Visible = false;
//                btnLoad.Enabled = true;


//                _serviceManager.Start();
//                labelServiceStatus.Text = _serviceManager.GetStatus();
//                UpdateServiceStatusAndBlink();

//                if (labelServiceStatus.Text == "Online")
//                {
//                    startservice.Enabled = false;
//                    startservice.BackColor = Color.LightGray;
//                    startservice.ForeColor = Color.White;
//                }
//                else
//                {
//                    startservice.Enabled = true;
//                    startservice.BackColor = Color.Green;
//                    startservice.ForeColor = Color.White;
//                }
//            }
//            catch (Exception ex)
//            {
//                string errorMessage = "Error loading records: " + ex.Message;
//                if (ex.InnerException != null)
//                    errorMessage += Environment.NewLine + ex.InnerException.Message;

//                Logger.Log("APP", "Error while loading invoices", ex);
//                MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//            }
//        }




//        private void label2_Click(object sender, EventArgs e)
//        {

//        }

//        private void label2_Click_1(object sender, EventArgs e)
//        {

//        }

//        private void startservice_Click(object sender, EventArgs e)
//        {
//            string status = _serviceManager.GetStatus();
//            if (status.Equals("Online", StringComparison.OrdinalIgnoreCase))
//            {
//                MessageBox.Show("Service is already running.");
//                startservice.Enabled = false;   // disable start button
//                startservice.BackColor = Color.LightGray;   // background grey
//                startservice.ForeColor = Color.White;  // text white
//            }
//            else
//            {
//                _serviceManager.Start();

//                MessageBox.Show("IMS service started.");
//                startservice.BackColor = Color.LightGray;   // background grey
//                startservice.ForeColor = Color.White;  // text white
//            }
//            UpdateServiceStatusAndBlink();

//            // Always refresh status after click
//            labelServiceStatus.Text = _serviceManager.GetStatus();
//            // Enable/disable button based on latest status
//            startservice.Enabled = !labelServiceStatus.Text.Equals("Online", StringComparison.OrdinalIgnoreCase);
//        }





//        private void filepath_TextChanged_1(object sender, EventArgs e)
//        {
//            btnLoad.Enabled = !string.IsNullOrWhiteSpace(filepath.Text);

//        }

//        private void loaderpicbox_Click(object sender, EventArgs e)
//        {

//        }

//        private void pictureBox1_Click(object sender, EventArgs e)
//        {

//        }




//        private void pg_Click(object sender, EventArgs e)
//        {

//        }
//        // --- METHOD UPDATED ---
//        // This method now handles starting the correct color blink (green or red).
//        private void UpdateServiceStatusAndBlink()
//        {
//            // First, stop any blink that is currently running.
//            if (_isBlinking)
//            {
//                _blinkerCts?.Cancel();
//                _blinkerCts?.Dispose();
//                _isBlinking = false;
//            }

//            // Get the latest status and update the label's text.
//            string status = _serviceManager.GetStatus();
//            labelServiceStatus.Text = status;

//            // Start the correct new blink based on the status.
//            if (status.Equals("Online", StringComparison.OrdinalIgnoreCase))
//            {
//                _isBlinking = true;
//                _blinkerCts = new CancellationTokenSource();
//                SoftBlink(labelServiceStatus, Color.Green, Color.LimeGreen, 1500, false, _blinkerCts.Token);
//                startservice.Enabled = false;   // disable start button
//                startservice.BackColor = Color.LightGray;   // background grey
//                startservice.ForeColor = Color.White;  // text white
//            }
//            else if (status.Equals("Offline", StringComparison.OrdinalIgnoreCase))

//            {
//                _isBlinking = true;
//                _blinkerCts = new CancellationTokenSource();
//                SoftBlink(labelServiceStatus, Color.DarkRed, Color.Red, 1500, false, _blinkerCts.Token);
//                startservice.Enabled = true;   // disable start button
//                startservice.BackColor = Color.DarkGreen;   // background grey
//                startservice.ForeColor = Color.White;  // text white
//            }
//            else // For any other status like "Pending", "Error", etc.
//            {
//                // Just set a default, non-blinking color.
//                labelServiceStatus.ForeColor = Color.Yellow;
//            }
//        }
//        private async void SoftBlink(Control ctrl, Color c1, Color c2, short CycleTime_ms, bool BkClr, CancellationToken token)
//        {
//            var sw = new Stopwatch();
//            sw.Start();
//            short halfCycle = (short)(CycleTime_ms * 0.5);

//            try
//            {
//                while (!token.IsCancellationRequested)
//                {
//                    await Task.Delay(10, token);
//                    var n = sw.ElapsedMilliseconds % CycleTime_ms;
//                    var per = (double)Math.Abs(n - halfCycle) / halfCycle;

//                    var red = (short)Math.Round((c2.R - c1.R) * per) + c1.R;
//                    var grn = (short)Math.Round((c2.G - c1.G) * per) + c1.G;
//                    var blw = (short)Math.Round((c2.B - c1.B) * per) + c1.B;

//                    var clr = Color.FromArgb(red, grn, blw);

//                    if (BkClr)
//                        ctrl.BackColor = clr;
//                    else
//                        ctrl.ForeColor = clr;
//                }
//            }
//            catch (TaskCanceledException)
//            {

//                // This is expected when stopping. No action needed.
//            }
//        }

//        private void dgvlogs_CellContentClick(object sender, DataGridViewCellEventArgs e)
//        {

//        }

//        private void lbl_Click(object sender, EventArgs e)
//        {

//        }

//        private void LoadLogsPage()
//        {
//            using (var db = new LiteDatabase(backupConnectionString)) // ✅ Always use backup DB
//            {
//                var collection = db.GetCollection<LogRecord>("logs");

//                // ✅ Get ALL logs first
//                var logs = collection.FindAll().ToList();

//                // ✅ Transform into display model
//                var records = logs.Select(r =>
//                {
//                    string messageText = r.Message ?? "";
//                    DateTime parsedDateTime = DateTime.MinValue;

//                    if (!string.IsNullOrWhiteSpace(r.Message) && r.Message.Contains("==>"))
//                    {
//                        var parts = r.Message.Split(new string[] { "==>" }, StringSplitOptions.None);

//                        if (parts.Length > 0 && DateTime.TryParse(parts[0].Trim(), out DateTime parsed))
//                        {
//                            parsedDateTime = parsed;
//                        }

//                        if (parts.Length > 1)
//                            messageText = parts[1].Trim();
//                    }

//                    return new
//                    {
//                        Date = parsedDateTime == DateTime.MinValue ? "" : parsedDateTime.ToString("dd-MMM-yyyy hh:mm:ss tt"),
//                        Message = messageText,
//                        IsSynced = r.IsSynced ? "Yes" : "No",
//                        SortDateTime = parsedDateTime
//                    };

//                })
//                .OrderByDescending(x => x.SortDateTime)   // ✅ newest first
//                .ToList();

//                // ✅ Update totals AFTER sorting
//                _totalRecords = records.Count;
//                _totalPages = (int)Math.Ceiling((double)_totalRecords / _pageSize);

//                // ✅ Apply paging
//                var pageRecords = records
//                    .Skip((_currentPage - 1) * _pageSize)
//                    .Take(_pageSize)
//                    .ToList();

//                // ✅ Bind to grid
//                // ✅ Bind to grid
//                this.Invoke(new Action(() =>
//                {
//                    dgvlogs.DataSource = pageRecords;

//                    // Hide helper column
//                    if (dgvlogs.Columns.Contains("SortDateTime"))
//                        dgvlogs.Columns["SortDateTime"].Visible = false;

//                    // 🔄 Rename headers
//                    if (dgvlogs.Columns.Contains("Date"))
//                        dgvlogs.Columns["Date"].HeaderText = "Date";

//                    if (dgvlogs.Columns.Contains("Message"))
//                        dgvlogs.Columns["Message"].HeaderText = "Message";

//                    if (dgvlogs.Columns.Contains("IsSynced"))
//                        dgvlogs.Columns["IsSynced"].HeaderText = "Invoice Synced";

//                    lblPageInfo.Text = $"Page {_currentPage} of {_totalPages}";
//                }));

//            }
//        }

//        private async Task LoadLogsPageAsync()
//        {
//            var records = await Task.Run(() =>
//            {
//                using (var db = new LiteDatabase(backupConnectionString))
//                {
//                    var collection = db.GetCollection<LogRecord>("logs");
//                    var logs = collection.FindAll().ToList();

//                    var parsedLogs = logs.Select(r =>
//                    {
//                        string messageText = r.Message ?? "";
//                        DateTime parsedDateTime = DateTime.MinValue;

//                        if (!string.IsNullOrWhiteSpace(r.Message) && r.Message.Contains("==>"))
//                        {
//                            var parts = r.Message.Split(new string[] { "==>" }, StringSplitOptions.None);

//                            if (parts.Length > 0 && DateTime.TryParse(parts[0].Trim(), out DateTime parsed))
//                                parsedDateTime = parsed;

//                            if (parts.Length > 1)
//                                messageText = parts[1].Trim();
//                        }

//                        return new
//                        {
//                            Date = parsedDateTime == DateTime.MinValue ? "" : parsedDateTime.ToString("dd-MMM-yyyy hh:mm:ss tt"),
//                            Message = messageText,
//                            IsSynced = r.IsSynced ? "Yes" : "No",
//                            SortDateTime = parsedDateTime
//                        };
//                    })
//                    .OrderByDescending(x => x.SortDateTime)
//                    .ToList();

//                    _totalRecords = parsedLogs.Count;
//                    _totalPages = (int)Math.Ceiling((double)_totalRecords / _pageSize);

//                    return parsedLogs;
//                }
//            });

//            // ✅ Update UI on main thread
//            this.Invoke(new Action(() =>
//            {
//                // Apply paging to logs grid
//                var pageRecords = records
//                    .Skip((_currentPage - 1) * _pageSize)
//                    .Take(_pageSize)
//                    .ToList();

//                dgvlogs.DataSource = pageRecords;
//                // Hide helper column
//                if (dgvlogs.Columns.Contains("SortDateTime"))
//                    dgvlogs.Columns["SortDateTime"].Visible = false;

//                // Rename columns
//                if (dgvlogs.Columns.Contains("Date"))
//                    dgvlogs.Columns["Date"].HeaderText = "Date";

//                if (dgvlogs.Columns.Contains("Message"))
//                    dgvlogs.Columns["Message"].HeaderText = "Message";

//                if (dgvlogs.Columns.Contains("IsSynced"))
//                    dgvlogs.Columns["IsSynced"].HeaderText = "Invoice Synced";

//                if (dgvlogs.Columns.Contains("SortDateTime"))
//                    dgvlogs.Columns["SortDateTime"].Visible = false;

//                lblPageInfo.Text = $"Page {_currentPage} of {_totalPages}";

//                // ✅ Update Last Created based on logs
//                var latestLogDate = records
//                    .Where(r => r.SortDateTime != DateTime.MinValue)
//                    .Select(r => r.SortDateTime)
//                    .DefaultIfEmpty(DateTime.MinValue)
//                    .Max();

//                if (latestLogDate != DateTime.MinValue)
//                    lbllastcreateddate.Text = $"Last Created: {latestLogDate:dd/MM/yyyy HH:mm:ss}";
//                else
//                    lbllastcreateddate.Text = "Last Created: N/A";
//            }));
//        }






//        // Button: Next Page
//        private void btnNext_Click(object sender, EventArgs e)
//        {
//            if (_currentPage < _totalPages)
//            {
//                _currentPage++;
//                LoadLogsPage();
//            }
//        }

//        // Button: Previous Page
//        private void btnPrevious_Click(object sender, EventArgs e)
//        {
//            if (_currentPage > 1)
//            {
//                _currentPage--;
//                LoadLogsPage();
//            }
//        }
//        private string _lastServiceStatus = string.Empty;

//        private void ServiceCheckTimer_Tick(object sender, EventArgs e)
//        {
//            try
//            {
//                string currentStatus = _serviceManager.GetStatus();

//                if (!currentStatus.Equals(_lastServiceStatus, StringComparison.OrdinalIgnoreCase))
//                {
//                    _lastServiceStatus = currentStatus;

//                    if (currentStatus.Equals("Online", StringComparison.OrdinalIgnoreCase))
//                    {
//                        Logger.Log("SERVICE", "IMS_Fiscalization service is ONLINE");
//                    }
//                    else if (currentStatus.Equals("Offline", StringComparison.OrdinalIgnoreCase))
//                    {
//                        Logger.Log("SERVICE", "IMS_Fiscalization service is OFFLINE");
//                        ToastNotificationForm toast = new ToastNotificationForm("⚠️ IMS service stopped!");
//                        toast.Show();
//                    }
//                    else
//                    {
//                        Logger.Log("SERVICE", $"IMS_Fiscalization service status: {currentStatus}");
//                    }

//                    labelServiceStatus.Text = currentStatus;
//                    UpdateServiceStatusAndBlink();
//                }
//            }
//            catch (Exception ex)
//            {
//                Logger.Log("SERVICE", "Error checking service status", ex);
//            }
//        }

//        private async void dgvInvoices_CellClick(object sender, DataGridViewCellEventArgs e)
//        {
//            try
//            {
//                if (e.RowIndex < 0 || e.ColumnIndex < 0)
//                    return;
//                string columnName = dgvInvoices.Columns[e.ColumnIndex].Name;
//                if (columnName == "InvoiceData")
//                {
//                    object posIdValue = null;

//                    if (dgvInvoices.Columns.Contains("BPOSID"))
//                        posIdValue = dgvInvoices.Rows[e.RowIndex].Cells["BPOSID"].Value;
//                    else if (dgvInvoices.Columns.Contains("POSID"))
//                        posIdValue = dgvInvoices.Rows[e.RowIndex].Cells["POSID"].Value;

//                    var encryptedInvoice = dgvInvoices.Rows[e.RowIndex].Cells["InvoiceData"].Value?.ToString();
//                    if (posIdValue == null || string.IsNullOrEmpty(encryptedInvoice))
//                    {
//                        MessageBox.Show("Missing POSID or InvoiceData");
//                        return;
//                    }
//                    int posId = Convert.ToInt32(posIdValue);
//                    var encryptedKey = await Helper.GetEncryptedKeyFromApi(posId);

//                    if (string.IsNullOrEmpty(encryptedKey))
//                    {
//                        MessageBox.Show("Encrypted key not found for POSID.");
//                        return;
//                    }
//                    // var encryptedInvoiceTest = "7lNk2wDADJmJzkb5M13RUZL8TNK1PtSKGvlG0kV1l0jD7AdEjtDsmATQPpPlnNT7YijjTV2ShS5YEkeovZex+uddHeN41AbLXF9U70P2A1W+lE2NSMxNf6c2PVxI/zxEhjtdGpx3fMPWJzsnDqomWuLtTWvDf9H0NB3jpmQOcWrrzj8rSQatgLDo1imMPoKD7L+isb0axx4mKsExyLRsBso4w/MY2a+z+KZmHiss4D1Z8IVXJeZESDME+wgI9qHhJox8kePqzHIvZBHa7UTzowi9Qlq002vNIptiFxrwcNDuvAj3+2kCUbAObScEFlUKj2HjnrZyxlqVU/AG1Yae9auaMh6xPkHNQbFwz0bmGj1UNeVWTYjzjONBkSrNJ1xizMQ5kWCiYoKQAXq0BYTyPON+xIygbkvuIe7pq+rrE63QCr9QaLIPNCYW2N6yfRpnd6OlIdj4UvKoFqeWCA/YlLAUxA/mMOSIvnGLe3XaxKwMChsq49muIf1Ha+i50nVDt9Bhcf+GOuHLVe7d5beYL8KehF1E1uZRuG/PVIe4MsHpaGceiS89jKzEZI30nKJF78wKh/Sr1jOnfSoo3tYKpNrpbHQMRl5D7iGl9+rVVGr8XL43TrZswboDZDwEScI/+eK6u96k8l5VofqDd8Gods5jfUgIIlmbTLqG3pyx2yscRuv48L8lcOp7nCRmWnQCFX8MnwNHF3cbbo7bbTrjClSdtwd3Wt5t3mGaMPFI9hnSChhdwYyxSVoaQUOJoKUbnIqNq9wx3bNeGb5UQEsssO46K0J2Vdt94O/QibPDar5r523YNuZ50EI92zvf0/l4W2dAl0sXozjqRYSukdU4YRWSGslNpJe4fxM/SQnIonkfHoUP6oJhp0IEcw8HmjlZViRIjYQ/7HXa4Uu/kcLKSGNZUurLiDafVJ2nTrNEVLDE+B8t3lapYr9W09p5tpojo5JXInvfZeCK6AseSrVytY4t5TJ+YmpH8htCGLK5M++/9W+zeyJK/Lh66/TXWtbhR5P+GIatrWUdQDdD8cix3l1ynSbAjEr/3LVuJKLbLW0Z+5XJv0fGCmx0S6LmuttPRZD/x35cOGhQThKKdccosruh7WIWWqXBlOqpwp4TSLdU5ClPX4azVQafmSosTbnyktQj6kUzTdFjoaFfYl1jBg9etd+q/8INgWMdop+b8nX0zQiD4gghgZuBYXFFSjzklzLnL0H628h/qA/u6FIMyg==";
//                    // string encryptedKeyTest = "36b8dd382b014af3e053ecf1322f6db2";
//                    //string decryptedString = Helper.DecryptInvoiceData(encryptedInvoiceTest, encryptedKey);
//                    string decryptedString = Helper.DecryptInvoiceData(encryptedInvoice, encryptedKey);
//                    MessageBox.Show(decryptedString, "Decrypted InvoiceData", MessageBoxButtons.OK, MessageBoxIcon.Information);
//                }
//            }
//            catch (Exception ex)
//            {
//                Logger.Log("APP", "Error while loading invoices", ex);
//                MessageBox.Show("Error while decrypting: " + ex.Message);
//            }
//        }

//        private void dgvInvoices_Click(object sender, EventArgs e)
//        {

//        }

//        private void btnInvcSync_Click(object sender, EventArgs e)
//        {

//            if (_currentSortOrder == "Ascending")
//            {
//                _currentSortOrder = "Descending";
//            }
//            else
//            {
//                _currentSortOrder = "Ascending";
//            }


//            ApplySort();
//        }
//        private void ApplySort()
//        {
//            try
//            {
//                if (displayedInvoices == null || !displayedInvoices.Any()) return;

//                if (_currentSortOrder == "Ascending")
//                {
//                    displayedInvoices = displayedInvoices.OrderBy(inv => inv.DateCreated).ToList();
//                }
//                else // "Descending"
//                {
//                    displayedInvoices = displayedInvoices.OrderByDescending(inv => inv.DateCreated).ToList();
//                }

//                // Refresh the grid
//                dgvInvoices.DataSource = null;
//                dgvInvoices.DataSource = displayedInvoices;
//            }
//            catch (Exception ex)
//            {
//                Logger.Log("APP", "Error while loading invoices", ex);
//                throw;
//            }
//        }

//        private void btnFilter_Click(object sender, EventArgs e)
//        {
//            try
//            {
//                DateTime fromDate = dtpFrom.Value.Date;
//                DateTime toDate = dtpTo.Value.Date.AddDays(1).AddTicks(-1); // Include entire day

//                using (var db = new LiteDatabase(backupConnectionString))
//                {
//                    var collection = db.GetCollection<FileRecords>("filerecords");

//                    var records = collection
//                        .Find(x => x.DateCreated >= fromDate && x.DateCreated <= toDate)
//                        .Select(r => new InvoiceGridRow
//                        {
//                            _Id = r._Id,
//                            POSID = r.POSID,
//                            InvoiceNumber = r.InvoiceNumber,
//                            IsSynced = r.IsSynced == 1 ? "Yes" : "No",
//                            AttemptCount = r.AttemptCount,
//                            DateCreated = r.DateCreated
//                        })
//                        .ToList();

//                    // Keep this filtered list in-memory
//                    displayedInvoices = records;
//                    syncFilterState = 0; // reset cycle to "All" for this date

//                    // Update grid on UI thread
//                    this.Invoke(new Action(() =>
//                    {
//                        dgvInvoices.DataSource = displayedInvoices;
//                        btnSortSynced.Enabled = displayedInvoices.Any();
//                        btnSortSynced.Text = "Not Synced";
//                    }));
//                }
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show("Error filtering records: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//                Logger.Log("APP", "Error while loading invoices", ex);
//            }
//        }






//        private void btnClear_Click(object sender, EventArgs e)
//        {
//            try
//            {
//                using (var db = new LiteDatabase(backupConnectionString))
//                {
//                    var collection = db.GetCollection<FileRecords>("filerecords");

//                    var records = collection.FindAll()
//                        .Select(r => new InvoiceGridRow
//                        {
//                            _Id = r._Id,
//                            POSID = r.POSID,
//                            InvoiceNumber = r.InvoiceNumber,
//                            IsSynced = r.IsSynced == 1 ? "Yes" : "No",
//                            AttemptCount = r.AttemptCount,
//                            DateCreated = r.DateCreated
//                        })
//                        .ToList();

//                    // ✅ Reset main in-memory list
//                    displayedInvoices = records;
//                    syncFilterState = 0;

//                    // ✅ Update grid safely from the UI thread
//                    this.Invoke(new Action(() =>
//                    {
//                        dgvInvoices.DataSource = displayedInvoices;
//                        btnSortSynced.Enabled = displayedInvoices.Any();
//                        btnSortSynced.Text = "Not Synced"; // reset button text
//                    }));
//                }
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show("Error clearing records: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//                Logger.Log("APP", "Error while loading invoices", ex);
//            }
//        }





//        private void btnSortSynced_Click(object sender, EventArgs e)
//        {
//            try
//            {
//                if (displayedInvoices == null || !displayedInvoices.Any())
//                    return;

//                if (syncFilterState == 0)
//                {
//                    // Show only "No"
//                    dgvInvoices.DataSource = displayedInvoices.Where(r => r.IsSynced == "No").ToList();
//                    btnSortSynced.Text = "Synced";
//                    syncFilterState = 1;
//                }
//                else // syncFilterState == 1
//                {
//                    // Back to all (date-filtered or loaded set)
//                    dgvInvoices.DataSource = displayedInvoices;
//                    btnSortSynced.Text = "Not Synced";
//                    syncFilterState = 0;
//                }
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show("Error filtering records: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//                Logger.Log("APP", "Error while filtering invoices", ex);
//            }
//        }







//        private void dgvInvoices_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
//        {

//        }


//        private void btnExport_Click(object sender, EventArgs e)
//        {
//            try
//            {
//                // Save dialog
//                SaveFileDialog saveFileDialog = new SaveFileDialog
//                {
//                    Filter = "Text Files (*.txt)|*.txt",
//                    FileName = $"Logs_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
//                };

//                if (saveFileDialog.ShowDialog() == DialogResult.OK)
//                {
//                    using (var db = new LiteDatabase(backupConnectionString)) // ✅ Use backup DB
//                    {
//                        var collection = db.GetCollection<LogRecord>("logs");
//                        var logs = collection.FindAll().ToList();

//                        // Parse + sort
//                        var records = logs.Select(r =>
//                        {
//                            string messageText = r.Message ?? "";
//                            DateTime parsedDateTime = DateTime.MinValue;

//                            if (!string.IsNullOrWhiteSpace(r.Message) && r.Message.Contains("==>"))
//                            {
//                                var parts = r.Message.Split(new string[] { "==>" }, StringSplitOptions.None);

//                                if (parts.Length > 0 && DateTime.TryParse(parts[0].Trim(), out DateTime parsed))
//                                {
//                                    parsedDateTime = parsed;
//                                }

//                                if (parts.Length > 1)
//                                    messageText = parts[1].Trim();
//                            }
//                            else if (r.Message != null && r.Message.StartsWith("DatabaseMetaData"))
//                            {
//                                // Try to extract "creationTime" from JSON
//                                var match = System.Text.RegularExpressions.Regex.Match(r.Message, "\"creationTime\":\"([^\"]+)\"");
//                                if (match.Success && DateTime.TryParse(match.Groups[1].Value, out DateTime metaDate))
//                                {
//                                    parsedDateTime = metaDate;
//                                }
//                            }

//                            return new
//                            {
//                                DateTimeCombined = parsedDateTime == DateTime.MinValue ? "" : parsedDateTime.ToString("dd-MMM-yyyy hh:mm:ss tt"),
//                                Message = messageText,
//                                IsSynced = r.IsSynced ? "Yes" : "No",
//                                SortDateTime = parsedDateTime
//                            };
//                        })
//                        .OrderByDescending(x => x.SortDateTime) // ✅ newest first
//                        .ToList();

//                        if (records.Count == 0)
//                        {
//                            MessageBox.Show("No logs found in the database.", "Export Logs", MessageBoxButtons.OK, MessageBoxIcon.Information);
//                            return;
//                        }

//                        using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
//                        {
//                            // Header
//                            writer.WriteLine("Date | Message | Invoice Synced");
//                            writer.WriteLine(new string('-', 120));

//                            // Rows
//                            foreach (var log in records)
//                            {
//                                string line = $"{log.DateTimeCombined} | {log.Message} | {log.IsSynced}";
//                                writer.WriteLine(line);
//                            }
//                        }
//                    }

//                    MessageBox.Show("✅ All logs exported successfully!", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
//                }
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show("Error exporting logs: " + ex.Message, "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//                Logger.Log("APP", "Error while exporting logs", ex);
//            }
//        }



//        private void labelServiceStatus_Click(object sender, EventArgs e)
//        {

//        }

//        private void lbl_Click_1(object sender, EventArgs e)
//        {

//        }

//        private void dgvlogs_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
//        {

//        }

//        private void btnlogout_Click_1(object sender, EventArgs e)
//        {
//            DialogResult result = MessageBox.Show("Are you sure you want to Exit?",
//                                                              "Confirm Exit",
//                                                              MessageBoxButtons.YesNo,
//                                                              MessageBoxIcon.Question);

//            if (result == DialogResult.Yes)
//            {
//                LogoutRequested = true;
//                this.Close(); // just close, Program.cs will reopen login
//            }
//        }

//        private void lblloading_Click(object sender, EventArgs e)
//        {

//        }

//        private void lbllastcreateddate_Click(object sender, EventArgs e)
//        {

//        }
//    }
//}



