using LiteDB;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pos.Application.DTOs.FiscalDtos;
using Pos.Application.DTOs.LogDTOs;
using Pos.Application.Services.ReferenceService.InvoiceTypeService;
using Pos.Application.Services.ReferenceService.PaymentService;
using Pos.Application.Services.ReferenceService.ServicesRenderedService;
using Pos.Application.Services.ScriptService;
using Pos.Application.Utility;
using Pos.SecurityEncryption;
using Pos.SetupUI.Helpers.Reference;
using System.Configuration;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using WinFormsApp = System.Windows.Forms.Application;



namespace Pos.SetupUI
{
    public partial class ConfigForm2 : Form
    {
        #region Win32 API Imports

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
        int X, int Y, int cx, int cy, uint uFlags);

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_SHOWWINDOW = 0x0040;


        #endregion

        #region Fields

        private readonly string _xmlConfigPath;
        private readonly string _jsonWorkerPath;
        private readonly string _jsonMainPath;
        private readonly string _winformsConfigPath;
        private readonly string _setupConfigPath;
        private readonly string _installationInfo;

        private int _isLoadingFlag = 0;
        private System.Windows.Forms.Timer _messageHideTimer;

        private readonly string _defaultIMSPath;
        private readonly string _defaultPassword;
        private readonly string _backupDir;
        private readonly string _workerServiceName;

        private bool _isServiceAvailable = false;

        private readonly IScriptService _scriptservice;

        private readonly IPaymentService _paymentservice;
        private readonly IInvoiceTypeService _invoiceTypeService;
        private readonly IServicesRenderedService _servicesRenderedService;

        #endregion


        #region Constructor

        public ConfigForm2(
            string xmlConfigPath,
            string jsonWorkerPath,
            string jsonMainPath,
            string setupConfigPath,
            string winformsConfigPath,
            IScriptService scriptservice,
            IPaymentService paymentservice,
            IInvoiceTypeService invoiceTypeService,
            IServicesRenderedService servicesRenderedService,
            string installationInfo)
        {
            InitializeComponent();

            _xmlConfigPath = xmlConfigPath;
            _jsonWorkerPath = jsonWorkerPath;
            _jsonMainPath = jsonMainPath;
            _setupConfigPath = setupConfigPath;
            _winformsConfigPath = winformsConfigPath;
            _installationInfo = installationInfo;

            _defaultIMSPath = ConfigurationManager.AppSettings["DefaulIMStFilePath"]!;
            _defaultPassword = ConfigurationManager.AppSettings["DbPassword"]!;
            _backupDir = ConfigurationManager.AppSettings["backupDir"]!;
            _workerServiceName = ConfigurationManager.AppSettings["FiscalServiceName"]!;

            _scriptservice = scriptservice;

            InitializeFormSettings();
            InitializeEventHandlers();
            InitializeMessageTimer();
            LoadLogoImage();
            CheckServiceAvailability();
            LoadDefaultPaths();
            _paymentservice = paymentservice;
            _invoiceTypeService = invoiceTypeService;
            _servicesRenderedService = servicesRenderedService;
        }

        #endregion

        #region Initialization Methods

        private void InitializeFormSettings()
        {
            this.TopMost = true;
            this.BringToFront();
            this.Activate();
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.AcceptButton = btnOk;

            if (progressBar != null)
                progressBar.Visible = false;
        }

        private void InitializeEventHandlers()
        {
            txtUsername.KeyPress += txtUsername_KeyPress;
            txtUsername.KeyDown += txtUsername_KeyDown;
            txtUsername.TextChanged += txtUsername_TextChanged;

            txtPassword.KeyPress += txtPassword_KeyPress;

            txtUsername.TextChanged += ValidateForm;
            txtPassword.TextChanged += ValidateForm;
            btnBrowse.Click += btnBrowseMain_Click;
            btnBrowseOLD.Click += btnBrowseOld_Click;
            btnOk.Click += btnOk_Click;
            btnCancel.Click += btnCancel_Click;
            btnupdateLOGO.Click += btnupdateLOGO_Click;

            rdoSandbox.Click += rdoSandbox_Click;
            rdoProduction.Click += rdoProduction_Click;

            toolTip1.SetToolTip(btnupdateLOGO,
                "Logo Upload Guidelines:\n" +
                "• Allowed formats: PNG, jpg\n" +
                "• Size: 2448×2448 pixels\n" +
                "• File size < 2 MB\n" +
                "• The logo will appear across all forms after upload.");

        }

        private void InitializeMessageTimer()
        {
            _messageHideTimer = new System.Windows.Forms.Timer
            {
                Interval = 5000
            };
            _messageHideTimer.Tick += (s, e) => HideMessage();
        }

        private void LoadLogoImage()
        {
            string logoKey = ConfigurationManager.AppSettings["LOGO"];
            if (!string.IsNullOrEmpty(logoKey))
            {
                var res = Resource.ResourceManager.GetObject(logoKey);
                if (res is Image img)
                {
                    LOGO_img.Image = img;
                    LOGO_img.SizeMode = PictureBoxSizeMode.Zoom;
                }
            }

            string logoKey2 = ConfigurationManager.AppSettings["PRAL"];
            if (!string.IsNullOrEmpty(logoKey2))
            {
                var res = Resource.ResourceManager.GetObject(logoKey2);
                if (res is Image img)
                {
                    pictureBox1.Image = img;
                    pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                }
            }
        }

        private void CheckServiceAvailability()
        {
            try
            {
                _isServiceAvailable = IsWorkerServiceInstalled();

                if (_isServiceAvailable)
                {
                    StopWorkerService();

                    var result = MessageBox.Show(
                        "The Fiscal service was detected and stopped.\nDo you want to uninstall the existing service?",
                        "Uninstall Service",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button1,
                        MessageBoxOptions.DefaultDesktopOnly
                    );


                    if (result == DialogResult.Yes)
                    {
                        //UninstallWorkerService();
                    }

                    ShowMessage("Fiscal service detected. Old database migration enabled.", true, true);
                }
                else
                {
                    ShowMessage("Fiscal service not found. Old database migration disabled.", false, true);
                    DisableOldDatabaseControls();
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Error checking service: {ex.Message}", false, true);
                DisableOldDatabaseControls();
            }
        }


        private void DisableOldDatabaseControls()
        {
            txtOldDB.Text = "";
            txtOldDB.Enabled = false;
            txtOldDB.ReadOnly = true;
            txtOldDB.BackColor = Color.FromArgb(240, 240, 240);
            btnBrowseOLD.Enabled = false;
            btnBrowseOLD.BackColor = Color.FromArgb(51, 51, 51);
        }

        private void LoadDefaultPaths()
        {
            ClearAllFields();

            string defaultPath = ConfigurationManager.AppSettings["DefaultDBFilePath"];
            if (!string.IsNullOrWhiteSpace(defaultPath))
            {
                txtFilePath.Text = defaultPath;
            }

            if (_isServiceAvailable && !string.IsNullOrWhiteSpace(_defaultIMSPath))
            {
                txtOldDB.Text = _defaultIMSPath;
            }
        }

        #endregion

        #region Service Check Methods

        private bool IsWorkerServiceInstalled()
        {
            try
            {
                using (var controller = new ServiceController(_workerServiceName))
                {
                    var status = controller.Status;
                    return true;
                }
            }
            catch (InvalidOperationException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private void StopWorkerService()
        {
            try
            {
                using (var controller = new ServiceController(_workerServiceName))
                {
                    if (controller.Status != ServiceControllerStatus.Stopped &&
                        controller.Status != ServiceControllerStatus.StopPending)
                    {
                        controller.Stop();
                        controller.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                        ShowMessage("Fiscal service stopped successfully.", true, true);
                    }
                    else
                    {
                        ShowMessage("Fiscal service is already stopped.", true, true);
                    }
                }
            }
            catch (InvalidOperationException)
            {
                ShowMessage("Service not found when trying to stop.", false, true);
            }
            catch (System.ServiceProcess.TimeoutException)
            {
                ShowMessage("Timeout occurred while stopping the service.", false, true);
            }
            catch (Exception ex)
            {
                ShowMessage($"Error stopping service: {ex.Message}", false, true);
            }
        }
        private void UninstallWorkerService()
        {
            try
            {
                // Path to service EXE (adjust if needed)
                string serviceExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _workerServiceName);

                // Unregister the Windows service
                Process process = new Process();
                process.StartInfo.FileName = "sc.exe";
                process.StartInfo.Arguments = $"delete \"{_workerServiceName}\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                process.WaitForExit();

                ShowMessage("Service uninstalled successfully.", true, true);

                // ✅ Delete EXE file after uninstall
                if (File.Exists(serviceExePath))
                {
                    try
                    {
                        File.Delete(serviceExePath);
                        ShowMessage("Service executable deleted successfully.", true, true);
                    }
                    catch (IOException)
                    {
                        ShowMessage("Unable to delete EXE file (it may still be in use).", false, true);
                    }
                }
                else
                {
                    ShowMessage("Service executable not found on disk.", false, true);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Error uninstalling service: {ex.Message}", false, true);
            }
        }

        private async Task<bool> IsWorkerServiceRunningAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (!_isServiceAvailable)
                        return false;

                    using (var controller = new ServiceController(_workerServiceName))
                    {
                        controller.Refresh();
                        return controller.Status == ServiceControllerStatus.Running;
                    }
                }
                catch
                {
                    return false;
                }
            });
        }

        #endregion

        #region Form Event Handlers
        public void RemoveConnectionStrings(string jsonFilePath)
        {
            try
            {
                // Ensure the file exists before attempting to modify it
                if (!File.Exists(jsonFilePath))
                {

                    return;
                }

                // Read the JSON file content
                string jsonContent = File.ReadAllText(jsonFilePath);

                // Parse the content into a JObject
                JObject jsonObject = JObject.Parse(jsonContent);

                // Check if "ConnectionStrings" section exists and remove it
                if (jsonObject["ConnectionStrings"] != null)
                {
                    jsonObject.Remove("ConnectionStrings");

                }
                else
                {

                }

                // Save the updated JSON back to the same file
                File.WriteAllText(jsonFilePath, jsonObject.ToString(Newtonsoft.Json.Formatting.Indented));

            }
            catch (Exception ex)
            {

            }
        }
        protected override void OnLoad(EventArgs e)
        {
            // Ensure that the ConnectionStrings section is removed before proceeding
            RemoveConnectionStrings(_jsonMainPath);
            base.OnLoad(e);
            this.CenterToScreen();

            // Bring it to the very top once
            this.TopMost = true;
            this.BringToFront();
            this.Activate();

            // Force it to appear above all windows briefly
            SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);

            // After a short delay, remove TopMost so other windows can appear above later
            Task.Delay(1000).ContinueWith(_ =>
            {
                if (!this.IsDisposed)
                {
                    this.Invoke((Action)(() =>
                    {
                        this.TopMost = false;
                        SetWindowPos(this.Handle, HWND_NOTOPMOST, 0, 0, 0, 0,
                            SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
                    }));
                }
            });
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // Center and show above everything once
            this.CenterToScreen();
            this.TopMost = true;
            this.BringToFront();
            this.Activate();

            // Force top-most window Z-order once
            SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);

            // After a short delay, remove TopMost flag
            _ = Task.Run(async () =>
            {
                await Task.Delay(800); // short & smooth
                if (!this.IsDisposed && this.IsHandleCreated)
                {
                    this.Invoke(() =>
                    {
                        this.TopMost = false;
                        SetWindowPos(this.Handle, HWND_NOTOPMOST, 0, 0, 0, 0,
                            SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
                    });
                }
            });
        }



        private async void btnOk_Click(object sender, EventArgs e)
        {
            try
            {
                btnOk.Enabled = false;
                btnOk.Text = "Processing...";

                // 🟢 Get the selected environment value from radio buttons
                string selectedEnvironment = null;

                if (rdoSandbox.Checked)
                {
                    selectedEnvironment = "Sandbox";
                }
                else if (rdoProduction.Checked)
                {
                    selectedEnvironment = "Production";
                }
                else
                {
                    ShowMessage("Please select an environment first (Sandbox or Production).", false, false);
                    return;
                }

                UpdateWorkerPathInJson(_jsonMainPath);

                // Pass the selected environment to your setup method
                await ProcessSetupAsync(selectedEnvironment);
            }
            catch (Exception ex)
            {
                ShowMessage($"Error: {ex.Message}", false, false);
            }
            finally
            {
                btnOk.Enabled = true;
                btnOk.Text = "OK";
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to cancel?",
                "Cancel Setup",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                ExitApplication(1602);
            }
        }

        //Upload Logo
        private void btnupdateLOGO_Click(object sender, EventArgs e)
        {
            try
            {
                using var ofd = new OpenFileDialog
                {
                    Filter = "Image Files|*.png;*.jpg",
                    Title = "Select Company Logo"
                };

                if (ofd.ShowDialog() != DialogResult.OK)
                    return;

                var fileInfo = new FileInfo(ofd.FileName);
                if (!fileInfo.Exists)
                {
                    ShowMessage(" File not found.", false, true);
                    return;
                }

                if (fileInfo.Length > 1024 * 1024) // 2,048 KB KB limit
                {
                    ShowMessage(" Logo size too large. Please select an image under 2 MB.",
                        false, true);
                    return;
                }

                // Convert image → Base64
                string base64;
                using (var img = Image.FromFile(ofd.FileName))
                using (var ms = new MemoryStream())
                {
                    img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    base64 = Convert.ToBase64String(ms.ToArray());
                }

                // Define shared config path
                string dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "POSPRA"
                );
                Directory.CreateDirectory(dir);

                string configFile = Path.Combine(dir, "AppSettings.config");

                // Load or create XML config
                var xml = new XmlDocument();
                if (File.Exists(configFile))
                    xml.Load(configFile);
                else
                {
                    xml.AppendChild(xml.CreateXmlDeclaration("1.0", "utf-8", null));
                    xml.AppendChild(xml.CreateElement("appSettings"));
                }

                var appSettings = xml.SelectSingleNode("//appSettings");
                if (appSettings == null)
                {
                    appSettings = xml.CreateElement("appSettings");
                    xml.AppendChild(appSettings);
                }

                // Update or create logo entry
                var node = appSettings.SelectSingleNode("add[@key='CompLogobase64']") as XmlElement;
                if (node == null)
                {
                    node = xml.CreateElement("add");
                    node.SetAttribute("key", "CompLogobase64");
                    appSettings.AppendChild(node);
                }

                node.SetAttribute("value", base64);
                xml.Save(configFile);
                ShowMessage(" Logo saved successfully!", true, true);
            }
            catch (Exception ex)
            {
                ShowMessage($" Failed to update logo: {ex.Message}", false, true);
            }
        }
        #endregion

        #region Main Setup Process

        private async Task ProcessSetupAsync(string selectedEnvironment)
        {
            try
            {
                if (!ValidateInputs(out string username, out string password, out string dbPath, out string oldDbPath))
                    return;

                ShowProgressBar(true);

                ShowMessage("Starting setup process...", true, false);
                await Task.Delay(500);

                CreateDatabaseDirectory(dbPath);

                ShowMessage("Retrieving system information...", true, false);
                var mac = TryGetMacAddress();
                await Task.Delay(300);

                ShowMessage("Authenticating with server...", true, false);
                var json = await AuthenticateAsync(username, password, mac, selectedEnvironment);
                if (json == null)
                {
                    ShowProgressBar(false);
                    return;
                }

                var (branchName, branchAddress, businessName, IsActive, AccessCode, PhoneNO, NTN, e_Key, LocalDBPassword) = ExtractBranchDetails(json);

                if (!VerifyAuthentication(json))
                {
                    ShowProgressBar(false);
                    return;
                }

                ShowMessage("Saving configurations...", true, false);
                SaveAllConfigs(username, password, mac, txtPassword.Text, dbPath, branchName, branchAddress, businessName, AccessCode, selectedEnvironment, PhoneNO, NTN, e_Key, LocalDBPassword);
                UpdateSetupConfig(dbPath);

                ShowMessage("Creating database file...", true, false);

                if (!CreateEncryptedDatabaseFile(dbPath, LocalDBPassword))
                {
                    ShowProgressBar(false);
                    return;
                }
                await Task.Delay(300);

                await SyncReferenceApisAfterDbSetupAsync(selectedEnvironment, txtPassword.Text);
                await Task.Delay(300);

                string DBconnection = GetDbConnectionStringForConfig(dbPath, LocalDBPassword);
                await Task.Delay(300);

                ShowMessage("Initializing database...", true, false);
                if (!InitializeDatabase(DBconnection))
                {
                    ShowProgressBar(false);
                    return;
                }
                await Task.Delay(300);

                if (_isServiceAvailable && !string.IsNullOrWhiteSpace(oldDbPath))
                {
                    ShowMessage("Migrating old database...", true, false);
                    await MigrateOldDatabaseAsync(oldDbPath, username, password, dbPath);
                }

                if (_isServiceAvailable)
                {
                    ShowMessage("Validating old database...", true, false);
                    if (!ValidateOldDatabase(oldDbPath))
                    {
                        ShowProgressBar(false);
                        return;
                    }

                    ShowMessage("Creating backup...", true, false);
                    CreateSafeBackup(oldDbPath);
                    await Task.Delay(300);
                }

                ShowProgressBar(false);
                ShowMessage("Setup completed successfully!", true, false);
                await Task.Delay(2000);

                // Install info block unchanged
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                ShowProgressBar(false);
                ShowMessage($"Fatal error: {ex.Message}", false, false);
            }
        }

        private async Task SyncReferenceApisAfterDbSetupAsync(string selectedEnvironment, string password)
        {
            var syncer = new SyncReferenceApis(
                _paymentservice,
                _invoiceTypeService,
                _servicesRenderedService,
                _isServiceAvailable,
                ShowMessage
            );
            await syncer.SyncAllAfterDbSetupAsync(selectedEnvironment, password);
        }

        private bool CreateEncryptedDatabaseFile(string dbPath, string password)
        {
            try
            {
                var connectionStringBuilder = new SqliteConnectionStringBuilder
                {
                    DataSource = dbPath,
                    Mode = SqliteOpenMode.ReadWriteCreate,
                    Password = password
                };

                using var connection = new SqliteConnection(connectionStringBuilder.ToString());
                connection.Open();   // THIS CREATES THE FILE

                using var cmd = connection.CreateCommand();
                cmd.CommandText = "PRAGMA journal_mode=WAL;";
                cmd.ExecuteNonQuery();

                return true;
            }
            catch (Exception ex)
            {
                ShowMessage($"Database creation failed: {ex.Message}", false, false);
                return false;
            }
        }

        private string GetDbConnectionStringForConfig(string dbPath, string dbPassword)
        {
            if (string.IsNullOrWhiteSpace(dbPath))
                throw new ArgumentException("dbPath cannot be null or empty.", nameof(dbPath));

            // Replace single backslashes with double for storing in config
            string escapedPath = dbPath.Replace("\\", "\\\\");

            // Return only path + mode + password, no "Data Source="
            string connectionString = $"{escapedPath};Mode=ReadWriteCreate;Password={dbPassword}";

            return connectionString;
        }

        private void mainPanel_Paint(object sender, PaintEventArgs e)
        {

        }
        #endregion

        #region Helper Method for Progress Bar

        private void ShowProgressBar(bool show)
        {
            if (progressBar == null) return;

            if (show)
            {
                progressBar.Style = ProgressBarStyle.Marquee;
                progressBar.MarqueeAnimationSpeed = 30;
                progressBar.Visible = true;
                progressBar.BringToFront();
                progressBar.Refresh();
                WinFormsApp.DoEvents();
            }
            else
            {
                progressBar.Visible = false;
                progressBar.Style = ProgressBarStyle.Continuous;
            }
        }

        #endregion

        #region Validation Methods

        private bool ValidateDatabasePath(string dbPath)
        {
            if (string.IsNullOrWhiteSpace(dbPath))
            {
                ShowMessage("Database path cannot be empty.", false, true);
                return false;
            }

            try
            {
                var directory = Path.GetDirectoryName(dbPath);
                if (string.IsNullOrWhiteSpace(directory))
                {
                    ShowMessage("Invalid database file path.", false, true);
                    return false;
                }

                // Check if directory exists or can be created
                if (!Directory.Exists(directory))
                {
                    try
                    {
                        Directory.CreateDirectory(directory);
                    }
                    catch (Exception ex)
                    {
                        ShowMessage($"Cannot create directory: {ex.Message}", false, true);
                        return false;
                    }
                }

                // Check file extension
                var extension = Path.GetExtension(dbPath);
                if (string.IsNullOrEmpty(extension) || !extension.Equals(".db", StringComparison.OrdinalIgnoreCase))
                {
                    ShowMessage("Database file must have .db extension.", false, true);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                ShowMessage($"Invalid database path: {ex.Message}", false, true);
                return false;
            }
        }

        private bool ValidateInputs(out string username, out string password, out string dbPath, out string oldDbPath)
        {
            username = txtUsername.Text.Trim();
            password = txtPassword.Text.Trim();
            dbPath = txtFilePath.Text.Trim();
            oldDbPath = _isServiceAvailable ? txtOldDB.Text.Trim() : string.Empty;

            if (string.IsNullOrWhiteSpace(username))
            {
                ShowMessage("Please enter POS ID.", false, true);
                return false;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ShowMessage("Please enter Token.", false, true);
                return false;
            }

            if (string.IsNullOrWhiteSpace(dbPath))
            {
                ShowMessage("Please select a database file path.", false, true);
                return false;
            }

            // Only require old DB path if service is available
            if (_isServiceAvailable && string.IsNullOrWhiteSpace(oldDbPath))
            {
                ShowMessage("Please select an old database file path.", false, true);
                return false;
            }

            if (!ValidateDatabasePath(dbPath))
                return false;

            return true;
        }

        private bool ValidateOldDatabase(string oldDbPath)
        {
            if (!_isServiceAvailable)
                return true;

            if (!File.Exists(oldDbPath))
            {
                ShowMessage($"Old database file not found at: {oldDbPath}", false, true);
                txtOldDB.Text = string.Empty;
                return false;
            }

            try
            {
                var validationResult = ValidateImsFile(oldDbPath, _defaultPassword);

                if (!validationResult.IsValid)
                {
                    if (validationResult.IsCorrupted)
                    {
                        ShowMessage($"Database is corrupted: {validationResult.ErrorMessage}", false, true);
                    }
                    else if (validationResult.IsEmpty)
                    {
                        ShowMessage($"Database is empty: {validationResult.ErrorMessage}", false, true);
                    }
                    else
                    {
                        ShowMessage($"Validation failed: {validationResult.ErrorMessage}", false, true);
                    }
                    return false;
                }

                if (validationResult.IsEmpty)
                {
                    ShowMessage("Old database has no data. Please select a database with existing records.", false, true);
                    return false;
                }

                // Only sum counts that are greater than or equal to 0 (exclude corrupted collections with -1)
                int totalRecords = validationResult.CollectionCounts.Values.Where(count => count >= 0).Sum();
                int corruptedCollections = validationResult.CollectionCounts.Values.Count(count => count < 0);

                string message = $"Database validated: {validationResult.CollectionNames.Count} collections, {totalRecords} total records";
                if (corruptedCollections > 0)
                {
                    message += $" ({corruptedCollections} corrupted collection(s) will be skipped)";
                }

                ShowMessage(message, true, false);

                return true;
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to validate old database: {ex.Message}", false, true);
                return false;
            }
        }

        private void ValidateForm(object sender, EventArgs e)
        {
            btnOk.Enabled = !string.IsNullOrWhiteSpace(txtUsername.Text);
        }

        #endregion

        #region IMS File Validation Methods

        /// <summary>
        /// Validates IMS file for corruption and accessibility
        /// </summary>
        private ImsValidationResult ValidateImsFile(string imsFilePath, string password)
        {
            var result = new ImsValidationResult();

            try
            {
                if (!File.Exists(imsFilePath))
                {
                    result.IsValid = false;
                    result.ErrorMessage = "IMS file does not exist.";
                    return result;
                }

                var fileInfo = new FileInfo(imsFilePath);
                if (fileInfo.Length == 0)
                {
                    result.IsValid = false;
                    result.IsEmpty = true;
                    result.ErrorMessage = "IMS file is empty (0 bytes).";
                    return result;
                }

                if (!IsValidLiteDbFile(imsFilePath))
                {
                    result.IsValid = false;
                    result.IsCorrupted = true;
                    result.ErrorMessage = "File does not appear to be a valid LiteDB database.";
                    return result;
                }

                using (var db = new LiteDatabase($"Filename={imsFilePath};Password={password}"))
                {
                    try
                    {
                        result.CollectionNames = db.GetCollectionNames().ToList();

                        if (result.CollectionNames.Count == 0)
                        {
                            result.IsValid = true;
                            result.IsEmpty = true;
                            result.ErrorMessage = "Database is empty (no collections).";
                            return result;
                        }

                        bool hasAnyData = false;
                        foreach (var collectionName in result.CollectionNames)
                        {
                            try
                            {
                                var collection = db.GetCollection(collectionName);
                                int count = collection.Count();
                                result.CollectionCounts[collectionName] = count;

                                if (count > 0)
                                    hasAnyData = true;
                            }
                            catch (Exception ex)
                            {
                                result.CollectionCounts[collectionName] = -1;
                                result.ErrorMessage += $" Warning: Collection '{collectionName}' is corrupted: {ex.Message}";
                            }
                        }

                        if (!hasAnyData)
                        {
                            result.IsValid = true;
                            result.IsEmpty = true;
                            result.ErrorMessage = "Database has no data in any collection.";
                            return result;
                        }

                        result.IsValid = true;
                        return result;
                    }
                    catch (LiteException ex)
                    {
                        result.IsValid = false;
                        result.IsCorrupted = true;
                        result.ErrorMessage = $"Database corruption detected: {ex.Message}";
                        return result;
                    }
                }
            }
            catch (LiteException ex) when (ex.ErrorCode == 123)
            {
                result.IsValid = false;
                result.ErrorMessage = "Invalid password or encrypted database.";
                return result;
            }
            catch (UnauthorizedAccessException)
            {
                result.IsValid = false;
                result.ErrorMessage = "Access denied. File is locked or insufficient permissions.";
                return result;
            }
            catch (IOException ex)
            {
                result.IsValid = false;
                result.ErrorMessage = $"IO Error: {ex.Message}";
                return result;
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.IsCorrupted = true;
                result.ErrorMessage = $"Unexpected error: {ex.Message}";
                return result;
            }
        }

        /// <summary>
        /// Checks if file has valid LiteDB header
        /// </summary>
        private bool IsValidLiteDbFile(string filePath)
        {
            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    if (fs.Length < 8192)
                        return false;

                    byte[] header = new byte[7];
                    fs.Read(header, 0, 7);

                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region IMS Data Migration Methods

        /// <summary>
        /// Migrates data from old IMS database to API
        /// </summary>
        /// <summary>
        /// Migrates data from old IMS database to API
        /// </summary>
        private async Task MigrateOldDatabaseAsync(string oldDbPath, string username, string password, string dbPath)
        {
            try
            {
                ShowMessage("Loading data from old database...", true, false);

                // Load data from IMS file
                var fileRecords = await Task.Run(() => LoadFileRecordsFromIms(oldDbPath));
                var logs = await Task.Run(() => LoadLogsFromIms(oldDbPath));

                ShowMessage($"Loaded {fileRecords.Count} file records and {logs.Count} logs", true, false);
                await Task.Delay(500);

                // Show summary in MessageBox
                ShowMigrationSummary(fileRecords, logs);

                ShowMessage("Sending data to server...", true, false);
                // Prepare data for API
                await PrepareDataForApiAsync(fileRecords, logs, username, password, dbPath);
            }
            catch (Exception ex)
            {
                ShowMessage($"Migration failed: {ex.Message}", false, true);
                throw;
            }
        }
        /// <summary>
        /// Shows migration summary in MessageBox
        /// </summary>
        private void ShowMigrationSummary(List<FileRecordDto> fileRecords, List<SyncLogDto> logs)
        {
            int totalFileRecords = fileRecords.Count;
            int totalLogs = logs.Count;
            int totalRecords = totalFileRecords + totalLogs;

            // Also show in the form message
            ShowMessage($"Migration ready: {totalFileRecords} file records, {totalLogs} logs - Total: {totalRecords} records", true, false);
        }

        /// <summary>
        /// Prepares data for API transmission (ready for API implementation)
        /// </summary>
        private async Task PrepareDataForApiAsync(
            List<FileRecordDto> fileRecords,
            List<SyncLogDto> logs,
            string username,
            string password,
            string dbPath)
        {
            try
            {
                if ((fileRecords == null || fileRecords.Count == 0) &&
                    (logs == null || logs.Count == 0))
                {
                    ShowMessage("No data to send.", false, true);
                    return;
                }

                //if (fileRecords != null && int.TryParse(username, out int userPosId))
                //{
                //    fileRecords = fileRecords
                //        .Where(x => x.POSID == userPosId)
                //        .ToList();
                //}
                if (fileRecords != null &&
     int.TryParse(username, out int userPosId) &&
     fileRecords.Any(x => x.POSID != userPosId))
                {
                    MessageBox.Show(
                        "POSID does not match. Old data migration failed.",
                        "Validation Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    ); return;
                }
                var payload = new ScriptDTO
                {
                    FileRecord = fileRecords,
                    Log = logs,
                    NewDbPath = dbPath
                };

                ShowMessage($"Preparing {fileRecords?.Count ?? 0} file records and {logs?.Count ?? 0} logs for API...", true, false);

                bool success = await SendDataToApiAsync(payload);

                if (success)
                {
                    ShowMessage("Data successfully sent to API!", true, false);
                }
                else
                {
                    ShowMessage("Failed to send data to API", false, true);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Error preparing API data: {ex.Message}", false, true);
            }
        }

        /// <summary>
        /// Ready-to-use method for sending data to API (commented out for now)
        /// </summary>
        private async Task<bool> SendDataToApiAsync(ScriptDTO payload)
        {
            try
            {
                var response = await _scriptservice.CreateScript(payload);

                if (response.StatusCode == "200")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Error sending data to API: {ex.Message}", false, true);
                return false;
            }
        }

        /// <summary>
        /// Loads FileRecords from IMS database for API transmission
        /// </summary>
        private List<FileRecordDto> LoadFileRecordsFromIms(string imsFilePath)
        {
            var fileRecords = new List<FileRecordDto>();

            try
            {
                using (var db = new LiteDatabase($"Filename={imsFilePath};Password={_defaultPassword}"))
                {
                    var collection = db.GetCollection("filerecords");
                    var documents = collection.FindAll().ToList();

                    foreach (var doc in documents)
                    {
                        try
                        {
                            var fileRecord = new FileRecordDto
                            {
                                ID = GetIntValue(doc, "_id", "ID", "Id"),
                                POSID = GetIntValue(doc, "POSID", "PosId"),
                                InvoiceData = GetStringValue(doc, "InvoiceData"),
                                InvoiceNumber = GetStringValue(doc, "InvoiceNumber"),
                                IsSynced = GetIntValue(doc, "IsSynced"),
                                AttemptCount = GetIntValue(doc, "AttemptCount"),
                                DateCreated = GetDateTimeValue(doc, "DateCreated"),
                                DateModified = GetDateTimeValue(doc, "DateModified")
                            };

                            fileRecords.Add(fileRecord);
                        }
                        catch (Exception ex)
                        {
                            ShowMessage($"Error parsing FileRecord: {ex.Message}", false, false);
                        }
                    }
                }

                return fileRecords;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load FileRecords: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Loads Logs from IMS database for API transmission
        /// </summary>
        private List<SyncLogDto> LoadLogsFromIms(string imsFilePath)
        {
            var logs = new List<SyncLogDto>();

            using (var db = new LiteDatabase($"Filename={imsFilePath};Password={_defaultPassword};Mode=ReadOnly"))
            {
                var collection = db.GetCollection("logs");

                // Stream instead of .ToList() to reduce memory & improve speed
                foreach (var doc in collection.FindAll())
                {
                    try
                    {
                        var log = new SyncLogDto
                        {
                            Id = doc.TryGetValue("_id", out var idVal) ? idVal.AsInt64 : 0,
                            Message = doc.TryGetValue("Message", out var msgVal) ? msgVal.AsString : string.Empty,
                            Type = doc.TryGetValue("TypeId", out var typeVal) ? typeVal.ToString() : string.Empty,
                            IsSynced = doc.TryGetValue("IsSynced", out var syncVal) && syncVal.AsBoolean,
                        };

                        logs.Add(log);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error parsing log record: {ex.Message}");
                    }
                }
            }

            return logs;
        }

        #endregion

        #region BsonDocument Helper Methods

        /// <summary>
        /// Gets integer value from BsonDocument with fallback keys
        /// </summary>
        private int GetIntValue(BsonDocument doc, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (doc.ContainsKey(key))
                {
                    try
                    {
                        return doc[key].AsInt32;
                    }
                    catch
                    {
                        try
                        {
                            return (int)doc[key].AsInt64;
                        }
                        catch
                        {
                            // Continue to next key
                        }
                    }
                }
            }
            return 0;
        }

        /// <summary>
        /// Gets string value from BsonDocument with fallback keys
        /// </summary>
        private string GetStringValue(BsonDocument doc, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (doc.ContainsKey(key))
                {
                    try
                    {
                        return doc[key].AsString;
                    }
                    catch
                    {
                        // Continue to next key
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets DateTime value from BsonDocument with fallback keys
        /// </summary>
        private DateTime GetDateTimeValue(BsonDocument doc, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (doc.ContainsKey(key))
                {
                    try
                    {
                        return doc[key].AsDateTime;
                    }
                    catch
                    {
                        // Continue to next key
                    }
                }
            }
            return DateTime.MinValue;
        }

        #endregion

        #region Database Operations

        private void CreateDatabaseDirectory(string dbPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath));
        }

        private void CreateSafeBackup(string dbPath)
        {
            try
            {
                // Read from config first
                string backupDir = ConfigurationManager.AppSettings["backupDir"];

                // Fallback to default if not set
                if (string.IsNullOrWhiteSpace(backupDir))
                {
                    backupDir = Path.Combine(Path.GetDirectoryName(dbPath), "Backups");
                }

                // Ensure directory exists
                Directory.CreateDirectory(backupDir);

                // Build the backup filename
                string backupFile = Path.Combine(
                    backupDir,
                    $"{Path.GetFileNameWithoutExtension(dbPath)}_backup_{DateTime.Now:yyyyMMdd_HHmmss}.ims"
                );

                // Create backup safely — read-while-in-use supported
                using (var source = new FileStream(dbPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var destination = new FileStream(backupFile, FileMode.Create, FileAccess.Write))
                {
                    source.CopyTo(destination);
                }

                ShowMessage($"Backup created successfully at: {backupFile}", true, true);
            }
            catch (Exception ex)
            {
                ShowMessage($"Backup failed: {ex.Message}", false, true);
            }
        }


        private bool InitializeDatabase(string dbPath)
        {
            try
            {
                var sqliteOptions = new DbContextOptionsBuilder<SqliteDbContext>()
                    .UseSqlite($"Data Source={dbPath}")
                    .Options;

                using var context = new SqliteDbContext(sqliteOptions);
                context.Database.EnsureCreated();
                return true;
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to initialize database: {ex.Message}", false, false);
                return false;
            }
        }

        #endregion

        #region Authentication
        private string TryGetMacAddress()
        {
            try
            {
                var mac = GetMacAddress();
                if (mac == "UNKNOWN" || string.IsNullOrWhiteSpace(mac))
                {
                    // Generate a persistent machine identifier as fallback
                    mac = GenerateMachineId();
                    ShowMessage("Using generated machine identifier.", true, false);
                }
                return mac;
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to read MAC address: {ex.Message}. Using fallback identifier.", false, true);
                return GenerateMachineId();
            }
        }

        private string GenerateMachineId()
        {
            // Create a persistent machine identifier based on machine name and other factors
            var machineName = Environment.MachineName;
            var userName = Environment.UserName;
            var combined = $"{machineName}_{userName}_{Environment.OSVersion.Version}";

            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(combined));
                return BitConverter.ToString(hash).Replace("-", "").Substring(0, 12);
            }
        }

        private string GetMacAddress()
        {
            try
            {
                var nic = NetworkInterface.GetAllNetworkInterfaces()
                    .FirstOrDefault(n => n.OperationalStatus == OperationalStatus.Up &&
                                         n.NetworkInterfaceType != NetworkInterfaceType.Loopback);

                if (nic == null)
                    return "UNKNOWN";

                var bytes = nic.GetPhysicalAddress().GetAddressBytes();
                return string.Join("-", bytes.Select(b => b.ToString("X2")));
            }
            catch
            {
                return "UNKNOWN";
            }
        }

        private async Task<JObject> AuthenticateAsync(string username, string password, string mac, string selectedEnvironment)
        {
            try
            {
                var payload = new
                {
                    posId = username,
                    macAddress = mac,
                    token = password,
                    Environment = selectedEnvironment
                };


                // string apiUrl = ConfigurationManager.AppSettings["ApiUrl"];

                var _baseUrl = ConfigurationManager.AppSettings["BaseUrl"] ?? "";
                var apiUrl = $"{_baseUrl}{Endpoints.Authenticate}";

                if (string.IsNullOrWhiteSpace(apiUrl))
                {
                    ShowMessage("API URL is missing in configuration.", false, false);
                    return null;
                }

                // CHANGED: Use HttpRequestMessage to add headers
                using var request = new HttpRequestMessage(HttpMethod.Post, apiUrl)
                {
                    Content = new StringContent(
                        JsonConvert.SerializeObject(payload),
                        Encoding.UTF8,
                        "application/json"
                    )
                };

                // CHANGED: Add Authorization header for middleware
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", password);

                // Send request
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Accept", "application/json");

                var response = await client.SendAsync(request);

                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    ShowMessage(
                        $"Authentication failed: {(int)response.StatusCode} - {response.ReasonPhrase}",
                        false,
                        false
                    );
                    return null;
                }

                return ParseAuthResponse(responseBody);
            }
            catch (HttpRequestException ex) when (ex.InnerException is SocketException se && se.SocketErrorCode == SocketError.HostUnreachable)
            {
                ShowMessage("Unable to reach the host. Please check your network connection or server address.", false, true);
                return null;
            }
            catch (HttpRequestException ex)
            {
                ShowMessage($"Network error: {ex.Message}", false, true);
                return null;
            }
            catch (TaskCanceledException)
            {
                ShowMessage("Request timed out. Please try again later.", false, true);
                return null;
            }
            catch (Exception ex)
            {
                ShowMessage($"Unexpected error: {ex.Message}", false, true);
                return null;
            }
        }

        private async Task<JObject> SetEnvironmentAsync(string POSID1, string selectedEnvironment)
        {
            try
            {
                var payload = new
                {
                    POSID = POSID1,
                    Environment = selectedEnvironment
                };

                string apiUrl = ConfigurationManager.AppSettings["EnvironmentApiUrl"];
                if (string.IsNullOrWhiteSpace(apiUrl))
                {
                    ShowMessage("API URL is missing in configuration.", false, false);
                    return null;
                }

                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
                client.DefaultRequestHeaders.Add("Accept", "application/json");

                var jsonContent = new StringContent(
                    JsonConvert.SerializeObject(payload),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync(apiUrl, jsonContent);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    ShowMessage(
                        $"Authentication failed: {(int)response.StatusCode} - {response.ReasonPhrase}",
                        false,
                        false
                    );
                    return null;
                }

                return ParseAuthResponse(responseBody);
            }
            catch (Exception ex)
            {
                ShowMessage($"API error: {ex.Message}", false, false);
                return null;
            }
        }

        private JObject ParseAuthResponse(string responseBody)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(responseBody))
                {
                    throw new ArgumentException("Empty response body");
                }

                var json = JToken.Parse(responseBody);

                // Handle string responses that contain JSON
                if (json.Type == JTokenType.String)
                {
                    return JObject.Parse(json.ToString());
                }

                // Handle nested response property
                if (json is JObject jobj && jobj["response"] != null)
                {
                    if (jobj["response"].Type == JTokenType.String)
                    {
                        return JObject.Parse(jobj["response"].ToString());
                    }
                    return jobj["response"] as JObject;
                }

                return json as JObject;
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to parse authentication response: {ex.Message}", false, false);
                return null;
            }
        }

        private bool VerifyAuthentication(JObject json)
        {
            try
            {
                string statusCode = json["statusCode"]?.ToString();
                string message = json["message"]?.ToString()?.ToLower();
                string serverMsg = json["message"]?.ToString() ?? "";

                if (string.IsNullOrWhiteSpace(statusCode))
                {
                    ShowMessage("Response missing status code.", false, false);
                    return false;
                }
                if (statusCode == "200")
                {
                    return true;
                }
                else
                {
                    ShowMessage(message, false, false);
                    return false;
                }

            }
            catch (Exception ex)
            {
                ShowMessage($"Authentication verification failed: {ex.Message}", false, false);
                return false;
            }
        }

        private bool ShowError(string msg)
        {
            ShowMessage(msg, false, false);
            return false;
        }

        #endregion

        #region Configuration Save Methods

        private (string branchName, string branchAddress, string businessName, string IsActive, string AccessCode, string PhoneNumber, string NTN, string EC, string LocalDBPassword) ExtractBranchDetails(JObject json)
        {
            try
            {
                var data = json["data"];
                if (data != null && data.Type == JTokenType.Object)
                {
                    return (
                        data["branchName"]?.ToString() ?? "N/A",
                        data["branchAddress"]?.ToString() ?? "N/A",
                        data["businessName"]?.ToString() ?? "N/A",
                        data["isActive"]?.ToString() ?? "N/A",
                        data["password"]?.ToString() ?? "N/A",
                        data["phoneNumber"]?.ToString() ?? "N/A",
                        data["ntn"]?.ToString() ?? "N/A",
                        data["e_Key"]?.ToString() ?? "N/A",
                        data["localDBPassword"]?.ToString() ?? "N/A"
                    );
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Error extracting branch details: {ex.Message}", false, true);
            }
            return ("N/A", "N/A", "N/A", "N/A", "N/A", "N/A", "N/A", "N/A", "N/A");
        }

        private void SaveAllConfigs(string username, string password, string mac, string Token, string dbPath,
            string branchName, string branchAddress, string businessName, string AccessCode, string selectedEnvironment, string PhoneNO, string NTN, string e_Key, string LocalDBPassword)
        {
            SaveJsonConfigs(dbPath, username, selectedEnvironment, Token, e_Key, LocalDBPassword);
            SaveWinFormsConfigComplete(username, AccessCode, mac, Token, dbPath, branchName, branchAddress, businessName, selectedEnvironment, PhoneNO, NTN, e_Key, LocalDBPassword);
        }

        private void SaveJsonConfigs(string dbPath, string username, string selectedEnvironment, string Token, string e_Key, string LocalDBPassword)
        {
            try
            {
                SaveDbPathToJson(_jsonWorkerPath, dbPath, username, Token, selectedEnvironment, LocalDBPassword);
                SaveDbPathToJson(_jsonMainPath, dbPath, username, Token, selectedEnvironment, LocalDBPassword, e_Key);
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to update JSON configs: {ex.Message}", false, true);
            }
        }

        private void SaveWinFormsConfigComplete(
            string username, string AccessCode, string mac, string Token, string dbPath,
            string branchName, string branchAddress, string businessName, string selectedEnvironment, string PhoneNO, string NTN, string e_Key, string LocalDBPassword)
        {
            try
            {
                if (!File.Exists(_winformsConfigPath))
                {
                    // Create a basic App.config structure if it doesn't exist
                    var newConfig = new XDocument(
                        new XElement("configuration",
                            new XElement("appSettings")
                        )
                    );
                    newConfig.Save(_winformsConfigPath);
                }

                var xml = XDocument.Load(_winformsConfigPath);
                var appSettingsNode = xml.Root.Element("appSettings");
                if (appSettingsNode == null)
                {
                    appSettingsNode = new XElement("appSettings");
                    xml.Root.Add(appSettingsNode);
                }

                // Read existing encrypted blob
                var encryptedElement = appSettingsNode.Elements("add")
                    .FirstOrDefault(x => x.Attribute("key")?.Value == "EncryptedSettings");

                JObject settings;
                if (encryptedElement != null)
                {
                    try
                    {
                        string encryptedBlob = encryptedElement.Attribute("value")?.Value;
                        string decryptedJson = AesEncryptionHelper.Decrypt(encryptedBlob);
                        settings = JObject.Parse(decryptedJson);
                    }
                    catch
                    {
                        // Decryption failed, start fresh
                        settings = new JObject();
                    }
                }
                else
                {
                    settings = new JObject();
                }
                var encUser = AesEncryptionHelper.Encrypt(username);
                var encAccessCode = AesEncryptionHelper.Encrypt(AccessCode);

                // Update values inside the blob
                settings["Username"] = encUser;
                settings["Password"] = encAccessCode;
                settings["MacAddress"] = mac;
                settings["DefaultDBFilePath"] = dbPath;
                settings["branchName"] = branchName;
                settings["branchAddress"] = branchAddress;
                settings["businessName"] = businessName;
                settings["Environment"] = selectedEnvironment;
                settings["phoneNumber"] = PhoneNO;
                settings["Token"] = Token;
                settings["NTN"] = NTN;
                settings["EC"] = e_Key;
                settings["DefaultDBPassword"] = LocalDBPassword;

                // Encrypt the updated blob
                string updatedJson = settings.ToString(Newtonsoft.Json.Formatting.None);
                string newEncryptedBlob = AesEncryptionHelper.Encrypt(updatedJson);

                // Update or add the EncryptedSettings key
                if (encryptedElement != null)
                    encryptedElement.SetAttributeValue("value", newEncryptedBlob);
                else
                    appSettingsNode.Add(new XElement("add",
                        new XAttribute("key", "EncryptedSettings"),
                        new XAttribute("value", newEncryptedBlob)));

                // Save updated App.config
                xml.Save(_winformsConfigPath);
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to update {Path.GetFileName(_winformsConfigPath)}: {ex.Message}", false, true);
            }
        }
        private void UpdateOrCreateNode(XmlDocument doc, string key, string value)
        {
            var node = doc.SelectSingleNode($"//appSettings/add[@key='{key}']");
            if (node == null)
            {
                var appSettings = doc.SelectSingleNode("//appSettings") ?? doc.CreateElement("appSettings");
                if (appSettings.ParentNode == null)
                    doc.DocumentElement.AppendChild(appSettings);

                XmlElement newNode = doc.CreateElement("add");
                newNode.SetAttribute("key", key);
                newNode.SetAttribute("value", value);
                appSettings.AppendChild(newNode);
            }
            else
            {
                node.Attributes["value"].Value = value;
            }
        }



        private void UpdateEnvironmentInJson(string jsonFilePath, string selectedEnvironment)
        {
            try
            {
                JObject appSettings = new JObject();

                // Try to read existing encrypted settings
                if (File.Exists(jsonFilePath))
                {
                    try
                    {
                        string jsonContent = File.ReadAllText(jsonFilePath);
                        var root = JObject.Parse(jsonContent);

                        // Try to find and decrypt existing blob
                        if (root["AppSettings"]?["EncryptedSettings"] != null)
                        {
                            string encryptedBlob = root["AppSettings"]["EncryptedSettings"].ToString();
                            string decryptedJson = AesEncryptionHelper.Decrypt(encryptedBlob);
                            appSettings = JObject.Parse(decryptedJson);
                        }
                    }
                    catch
                    {
                        // If decryption fails, start fresh
                        appSettings = new JObject();
                    }
                }

                // Update environment (this preserves the type - string, number, etc.)
                appSettings["Environment"] = selectedEnvironment;

                // Convert to string and encrypt
                string json = appSettings.ToString(Newtonsoft.Json.Formatting.None);
                string newEncryptedBlob = AesEncryptionHelper.Encrypt(json);

                // Create root structure
                var newRoot = new JObject
                {
                    ["AppSettings"] = new JObject
                    {
                        ["EncryptedSettings"] = newEncryptedBlob
                    }
                };

                // Preserve other sections if they exist (like Logging)
                if (File.Exists(jsonFilePath))
                {
                    try
                    {
                        string existingContent = File.ReadAllText(jsonFilePath);
                        var existingRoot = JObject.Parse(existingContent);

                        foreach (var property in existingRoot.Properties())
                        {
                            if (property.Name != "AppSettings")
                            {
                                newRoot[property.Name] = property.Value;
                            }
                        }
                    }
                    catch { /* ignore */ }
                }

                File.WriteAllText(jsonFilePath, newRoot.ToString(Newtonsoft.Json.Formatting.Indented));
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to update {Path.GetFileName(jsonFilePath)}: {ex.Message}", false, true);
            }
        }



        private async void UpdateWorkerPathInJson(string jsonFilePath)
        {
            JObject root;

            // Read install path from PRAL file
            string? installPath = await ReadPralFileAsync("install_info.txt");

            // Open or create JSON
            if (File.Exists(jsonFilePath))
            {
                string text = File.ReadAllText(jsonFilePath);
                root = string.IsNullOrWhiteSpace(text) ? new JObject() : JObject.Parse(text);
            }
            else
            {
                root = new JObject();
            }
            // ⚡ Correct WorkerConfigPath formatting
            if (installPath != null)
            {
                string workerPath = Path.Combine(
                    installPath,
                    "appsettings.worker.json"
                );
                root["WorkerConfigPath"] = workerPath;

            }
            // Save JSON
            File.WriteAllText(jsonFilePath, root.ToString(Newtonsoft.Json.Formatting.Indented));
        }

        public async Task<string?> ReadPralFileAsync(string fileName)
        {
            try
            {
                const string key = "InstallPath=";
                // Build full path
                string fullPath = Path.Combine(_installationInfo, fileName);

                // Check if file exists
                if (!File.Exists(fullPath))
                    return null;

                // Read file content asynchronously
                string content = await File.ReadAllTextAsync(fullPath);

                if (string.IsNullOrWhiteSpace(content))
                    return null;

                // Find where "InstallPath=" starts
                int index = content.IndexOf(key, StringComparison.OrdinalIgnoreCase);
                if (index == -1)
                    return null;

                // Slice out the value after InstallPath=
                string path = content[(index + key.Length)..].Trim();

                return path;
            }
            catch (Exception ex)
            {
                return null; // or rethrow if needed
            }
        }


        private void SaveDbPathToJson(string jsonFilePath, string dbPath, string posId, string Token, string selectedEnvironment, string LocalDBPassword, string e_Key = null)
        {
            try
            {
                JObject appSettingsToEncrypt = new JObject();

                // Add new settings
                appSettingsToEncrypt["DefaultDBFilePath"] = dbPath;
                appSettingsToEncrypt["POS"] = posId;
                appSettingsToEncrypt["Environment"] = selectedEnvironment;
                appSettingsToEncrypt["Token"] = Token;
                appSettingsToEncrypt["DefaultDBPassword"] = LocalDBPassword;

                if (e_Key != null)
                    appSettingsToEncrypt["EC"] = e_Key;

                // If file exists and has encrypted settings, decrypt and merge
                if (File.Exists(jsonFilePath))
                {
                    try
                    {
                        string existingContent = File.ReadAllText(jsonFilePath);
                        var existingRoot = JObject.Parse(existingContent);

                        // Check if there's an existing encrypted blob
                        if (existingRoot["AppSettings"]?["EncryptedSettings"] != null)
                        {
                            string existingBlob = existingRoot["AppSettings"]["EncryptedSettings"].ToString();
                            string decryptedJson = AesEncryptionHelper.Decrypt(existingBlob);
                            var existingSettings = JObject.Parse(decryptedJson);

                            // Merge existing settings (new values override old ones)
                            foreach (var prop in existingSettings.Properties())
                            {
                                if (appSettingsToEncrypt[prop.Name] == null)
                                {
                                    appSettingsToEncrypt[prop.Name] = prop.Value;
                                }
                            }
                        }
                    }
                    catch
                    {
                        // If can't decrypt or parse, just use new settings
                    }
                }

                // Convert JObject to string and encrypt
                string appSettingsJson = appSettingsToEncrypt.ToString(Newtonsoft.Json.Formatting.None);
                string encryptedBlob = AesEncryptionHelper.Encrypt(appSettingsJson);

                // Create the encrypted JSON structure
                var root = new JObject
                {
                    ["AppSettings"] = new JObject
                    {
                        ["EncryptedSettings"] = encryptedBlob
                    }
                };

                // Preserve other sections if they exist (like Logging)
                if (File.Exists(jsonFilePath))
                {
                    try
                    {
                        string existingContent = File.ReadAllText(jsonFilePath);
                        var existingRoot = JObject.Parse(existingContent);

                        foreach (var property in existingRoot.Properties())
                        {
                            if (property.Name != "AppSettings")
                            {
                                root[property.Name] = property.Value;
                            }
                        }
                    }
                    catch
                    {
                        // If can't parse existing, just use new structure
                    }
                }

                File.WriteAllText(jsonFilePath, root.ToString(Newtonsoft.Json.Formatting.Indented));
                ShowMessage($"{Path.GetFileName(jsonFilePath)} saved with encryption", true, false);
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to update {Path.GetFileName(jsonFilePath)}: {ex.Message}", false, true);
            }
        }
        private void rdoSandbox_Click(object sender, EventArgs e)
        {
            SaveEnvironmentToApiConfig("Sandbox");
            UpdateEnvironmentInJson(_jsonWorkerPath, "Sandbox");
        }

        private void rdoProduction_Click(object sender, EventArgs e)
        {
            SaveEnvironmentToApiConfig("Production");
            UpdateEnvironmentInJson(_jsonWorkerPath, "Production");
        }

        private void SaveEnvironmentToApiConfig(string environment)
        {
            try
            {
                // Update both main and worker config files with encryption
                UpdateEnvironmentInJson(_jsonMainPath, environment);
                UpdateEnvironmentInJson(_jsonWorkerPath, environment);

                ShowMessage($"Environment updated to {environment}", true, false);
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to update configuration: {ex.Message}", false, false);
            }
        }

        private void UpdateSetupConfig(string dbPath)
        {
            try
            {
                var docSetup = new XmlDocument();
                docSetup.Load(_setupConfigPath);
                UpdateOrCreateNode(docSetup, "DefaultDBFilePath", dbPath);
                docSetup.Save(_setupConfigPath);
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to update SetupUI config: {ex.Message}", false, true);
            }
        }

        #endregion

        #region File Browser Methods

        private void btnBrowseMain_Click(object sender, EventArgs e)
        {
            BrowseAndSelectNewDatabaseFile(txtFilePath);
        }

        private void btnBrowseOld_Click(object sender, EventArgs e)
        {
            if (!_isServiceAvailable)
            {
                ShowMessage("Old database migration is disabled. Worker service not found.", false, true);
                return;
            }

            BrowseAndSelectExistingFile(txtOldDB, "Select IMS Database File", "IMS files (*.ims)|*.ims|All files (*.*)|*.*");
        }

        private void BrowseAndSelectNewDatabaseFile(TextBox targetTextBox)
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Title = "Select or create SQLite DB file";
                dialog.Filter = "SQLite DB (*.db)|*.db|All files (*.*)|*.*";
                dialog.FileName = "POSPRA.db";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    targetTextBox.Text = dialog.FileName;
                }
            }
        }

        private void BrowseAndSelectExistingFile(TextBox targetTextBox, string title, string filter)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = title;
                dialog.Filter = filter;
                dialog.CheckFileExists = true;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    targetTextBox.Text = dialog.FileName;
                }
            }
        }

        #endregion

        #region Message Display

        private void ShowMessage(string message, bool isSuccess, bool autoHide = true)
        {
            lblMessage.Text = message;
            lblMessage.Visible = true;
            lblMessage.BringToFront();

            if (isSuccess)
            {
                lblMessage.ForeColor = Color.FromArgb(76, 175, 80);
                lblMessage.BackColor = Color.FromArgb(232, 245, 233);
            }
            else
            {
                lblMessage.ForeColor = Color.FromArgb(211, 47, 47);
                lblMessage.BackColor = Color.FromArgb(255, 235, 238);
            }

            lblMessage.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            lblMessage.Padding = new Padding(10, 8, 10, 8);
            lblMessage.TextAlign = ContentAlignment.MiddleCenter;
            lblMessage.AutoSize = false;
            lblMessage.Height = 60;
            lblMessage.Width = 370;

            if (autoHide)
            {
                _messageHideTimer.Stop();
                _messageHideTimer.Start();
            }
        }

        private void HideMessage()
        {
            _messageHideTimer.Stop();
            lblMessage.Visible = false;
            lblMessage.Text = "";
        }

        #endregion

        #region Progress Bar Helper

        private async Task RunSingleLoad(Func<Task> work)
        {
            // Check if already loading
            if (Interlocked.CompareExchange(ref _isLoadingFlag, 1, 0) == 1)
            {
                ShowMessage("An operation is already in progress...", false, true);
                return;
            }

            try
            {
                // Show progress bar
                if (progressBar != null)
                {
                    progressBar.Style = ProgressBarStyle.Marquee;
                    progressBar.MarqueeAnimationSpeed = 30;
                    progressBar.Visible = true;
                    progressBar.BringToFront();

                    // Force UI update
                    progressBar.Refresh();
                    WinFormsApp.DoEvents();
                }

                // Disable buttons during operation
                if (btnOk != null) btnOk.Enabled = false;
                if (btnCancel != null) btnCancel.Enabled = false;

                // Execute the work
                await work();
            }
            catch (Exception ex)
            {
                ShowMessage($"Error: {ex.Message}", false, true);
            }
            finally
            {
                // Hide progress bar
                if (progressBar != null)
                {
                    progressBar.Visible = false;
                    progressBar.Style = ProgressBarStyle.Continuous;
                }

                // Re-enable buttons
                if (btnOk != null) btnOk.Enabled = true;
                if (btnCancel != null) btnCancel.Enabled = true;

                // Reset flag
                Interlocked.Exchange(ref _isLoadingFlag, 0);
            }
        }

        #endregion

        #region Helper Methods

        private void ClearAllFields()
        {
            txtUsername.Clear();
            txtPassword.Clear();
        }

        private void txtUsername_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox tb = (TextBox)sender;

            // Allow only digits and control keys (like Backspace)
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
                return;
            }

            // Limit to maximum 9 digits
            if (!char.IsControl(e.KeyChar) && tb.Text.Length >= 9)
            {
                e.Handled = true;
            }
        }

        private void txtUsername_KeyDown(object sender, KeyEventArgs e)
        {
            // Allow paste — will be sanitized in TextChanged
        }

        private void txtUsername_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;

            // Keep only digits
            string digitsOnly = new string(tb.Text.Where(char.IsDigit).ToArray());

            // Trim to 9 digits maximum
            if (digitsOnly.Length > 9)
                digitsOnly = digitsOnly.Substring(0, 9);

            // Apply correction if needed
            if (tb.Text != digitsOnly)
            {
                int cursorPos = tb.SelectionStart - (tb.Text.Length - digitsOnly.Length);
                tb.Text = digitsOnly;
                tb.SelectionStart = Math.Max(0, Math.Min(cursorPos, tb.Text.Length));
            }
        }


        private void txtPassword_KeyPress(object sender, KeyPressEventArgs e) { }

        private void ExitApplication(int exitCode)
        {
            try
            {
                this.TopMost = false;
                SetWindowPos(this.Handle, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
                //Application.ExitThread();
                //Application.Exit();
                Environment.Exit(exitCode);
            }
            catch
            {
                Environment.Exit(exitCode);
            }
        }

        #endregion
    }

    #region IMS Validation Result Class

    /// <summary>
    /// Result of IMS file validation
    /// </summary>
    public class ImsValidationResult
    {
        public bool IsValid { get; set; }
        public bool IsCorrupted { get; set; }
        public bool IsEmpty { get; set; }
        public string ErrorMessage { get; set; }
        public List<string> CollectionNames { get; set; } = new List<string>();
        public Dictionary<string, int> CollectionCounts { get; set; } = new Dictionary<string, int>();
    }

    #endregion
}