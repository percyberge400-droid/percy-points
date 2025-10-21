using LiteDB;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using POSPRA.Infrastructure.Context;
using POSPRA.SecurityEncryption;
using System.Configuration;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Xml;

namespace POSPRA.SetupUI
{
    public partial class ConfigForm : Form
    {
        #region Win32 API Imports

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int X,
            int Y,
            int cx,
            int cy,
            uint uFlags);

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

        private int _isLoadingFlag = 0;
        private System.Windows.Forms.Timer _messageHideTimer;

        private readonly string _defaultIMSPath;
        private readonly string _defaultPassword;
        private readonly string _backupDir;
        private readonly string _workerServiceName;

        private bool _isServiceAvailable = false;

        #endregion

        #region Constructor

        public ConfigForm(string xmlConfigPath, string jsonWorkerPath, string jsonMainPath, string setupConfigPath, string winformsConfigPath)
        {
            InitializeComponent();

            _xmlConfigPath = xmlConfigPath;
            _jsonWorkerPath = jsonWorkerPath;
            _jsonMainPath = jsonMainPath;
            _setupConfigPath = setupConfigPath;
            _winformsConfigPath = winformsConfigPath;

            _defaultIMSPath = ConfigurationManager.AppSettings["DefaulIMStFilePath"];
            _defaultPassword = ConfigurationManager.AppSettings["DbPassword"];
            _backupDir = ConfigurationManager.AppSettings["backupDir"];
            _workerServiceName = ConfigurationManager.AppSettings["FiscalServiceName"];

            InitializeFormSettings();
            InitializeEventHandlers();
            InitializeMessageTimer();
            LoadLogoImage();
            CheckServiceAvailability();
            LoadDefaultPaths();
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
            txtPassword.KeyPress += txtPassword_KeyPress;
            txtUsername.TextChanged += ValidateForm;
            txtPassword.TextChanged += ValidateForm;
            btnBrowse.Click += btnBrowseMain_Click;
            btnBrowseOLD.Click += btnBrowseOld_Click;
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
                    LOGO_img.AutoSize = true;
                }
            }
        }

        private void CheckServiceAvailability()
        {
            try
            {
                _isServiceAvailable = IsWorkerServiceInstalled();
                //_isServiceAvailable = false;
                if (_isServiceAvailable)
                {
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
            if (txtOldDB != null)
            {
                txtOldDB.Enabled = false;
                txtOldDB.ReadOnly = true;
                txtOldDB.BackColor = Color.FromArgb(240, 240, 240);
            }

            if (btnBrowseOLD != null)
            {
                btnBrowseOLD.Enabled = false;
            }
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
                    // Access the Status property to check if service exists
                    var status = controller.Status;
                    return true;
                }
            }
            catch (InvalidOperationException)
            {
                // Service does not exist
                return false;
            }
            catch (Exception)
            {
                return false;
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

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.CenterToScreen();
            this.TopMost = true;
            SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
            this.Activate();
            this.BringToFront();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            _ = ProcessSetupAsync();
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

        #endregion

        #region Main Setup Process

        private async Task ProcessSetupAsync()
        {
            try
            {
                if (!ValidateInputs(out string username, out string password, out string dbPath, out string oldDbPath))
                    return;

                // Only validate and backup old database if service is available
                if (_isServiceAvailable)
                {
                    if (!ValidateOldDatabase(oldDbPath))
                        return;

                    CreateOldDatabaseBackup(oldDbPath);
                }

                CreateDatabaseDirectory(dbPath);

                var mac = TryGetMacAddress();
                var json = await AuthenticateAsync(username, password, mac);
                if (json == null)
                    return;

                var (branchName, branchAddress, businessName) = ExtractBranchDetails(json);

                if (!VerifyAuthentication(json))
                    return;

                SaveAllConfigs(username, password, mac, dbPath, branchName, branchAddress, businessName);
                SaveEnvironmentSettings();
                UpdateSetupConfig(dbPath);

                if (!InitializeDatabase(dbPath))
                    return;

                ShowMessage("Setup completed successfully!", true, false);
                await Task.Delay(2000);
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                ShowMessage($"Fatal error: {ex.Message}", false, false);
            }
        }

        #endregion

        #region Validation Methods

        private bool ValidateInputs(out string username, out string password, out string dbPath, out string oldDbPath)
        {
            username = txtUsername.Text.Trim();
            password = txtPassword.Text.Trim();
            dbPath = txtFilePath.Text.Trim();
            oldDbPath = _isServiceAvailable ? txtOldDB.Text.Trim() : string.Empty;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ShowMessage("Please enter both POS ID and Access Code.", false, true);
                return false;
            }

            if (string.IsNullOrWhiteSpace(dbPath))
            {
                ShowMessage("Please select a database file path.", false, true);
                return false;
            }

            // Only validate old DB path if service is available
            if (_isServiceAvailable && string.IsNullOrWhiteSpace(oldDbPath))
            {
                ShowMessage("Please select an old database file path.", false, true);
                return false;
            }

            return true;
        }

        private bool ValidateOldDatabase(string oldDbPath)
        {
            // Skip validation if service is not available
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
                using (var db = new LiteDatabase($"Filename={oldDbPath};Password={_defaultPassword}"))
                {
                    var collectionNames = db.GetCollectionNames().ToList();

                    if (collectionNames.Count == 0)
                    {
                        ShowMessage("Old database is empty. No data found.", false, true);
                        return false;
                    }

                    bool hasData = false;
                    foreach (var collectionName in collectionNames)
                    {
                        var collection = db.GetCollection(collectionName);
                        if (collection.Count() > 0)
                        {
                            hasData = true;
                            break;
                        }
                    }

                    if (!hasData)
                    {
                        ShowMessage("Old database has no data. Please select a database with existing records.", false, true);
                        return false;
                    }

                    return true;
                }
            }
            catch (LiteException ex) when (ex.ErrorCode == 123)
            {
                ShowMessage("Invalid password for old database or file is corrupted.", false, true);
                return false;
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

        #region Database Operations

        private void CreateDatabaseDirectory(string dbPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath));
        }

        private void CreateOldDatabaseBackup(string oldDbPath)
        {
            // Skip backup if service is not available or path is empty
            if (!_isServiceAvailable || string.IsNullOrWhiteSpace(oldDbPath))
                return;

            try
            {
                string backupDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");
                Directory.CreateDirectory(backupDirectory);

                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string fileName = Path.GetFileNameWithoutExtension(oldDbPath);
                string backupPath = Path.Combine(backupDirectory, $"{fileName}_backup_{timestamp}.ims");

                File.Copy(oldDbPath, backupPath, overwrite: true);
                ShowMessage($"Backup created: {Path.GetFileName(backupPath)}", true, false);
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to create backup: {ex.Message}", false, true);
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
                return GetMacAddress();
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to read MAC address: {ex.Message}", false, true);
                return "UNKNOWN";
            }
        }

        private string GetMacAddress()
        {
            try
            {
                var nic = NetworkInterface.GetAllNetworkInterfaces()
                    .FirstOrDefault(n => n.OperationalStatus == OperationalStatus.Up &&
                                         n.NetworkInterfaceType != NetworkInterfaceType.Loopback);
                return nic?.GetPhysicalAddress().ToString() ?? "UNKNOWN";
            }
            catch
            {
                return "UNKNOWN";
            }
        }

        private async Task<JObject> AuthenticateAsync(string username, string password, string mac)
        {
            try
            {
                var payload = new
                {
                    posId = username,
                    macAddress = mac,
                    token = password
                };

                string apiUrl = ConfigurationManager.AppSettings["ApiUrl"];
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
            var json = JObject.Parse(responseBody);

            if (json.Type == JTokenType.String)
                return JObject.Parse(json.ToString());

            if (json["response"]?.Type == JTokenType.String)
                return JObject.Parse(json["response"].ToString());

            return json;
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

                return statusCode switch
                {
                    "200" when message?.Contains("record found") == true || message?.Contains("success") == true
                        => true,
                    "401" => ShowError("Invalid POS ID or access code."),
                    "403" => ShowError("Access denied. Unauthorized device."),
                    "404" => ShowError("MAC address verification failed."),
                    "500" => ShowError("Internal server error."),
                    _ => ShowError($"Unexpected response: {statusCode} - {serverMsg}")
                };
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

        private (string branchName, string branchAddress, string businessName) ExtractBranchDetails(JObject json)
        {
            try
            {
                var data = json["data"];
                if (data != null && data.Type == JTokenType.Object)
                {
                    return (
                        data["branchName"]?.ToString() ?? "N/A",
                        data["branchAddress"]?.ToString() ?? "N/A",
                        data["businessName"]?.ToString() ?? "N/A"
                    );
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Error extracting branch details: {ex.Message}", false, true);
            }
            return ("N/A", "N/A", "N/A");
        }

        private void SaveAllConfigs(string username, string password, string mac, string dbPath,
            string branchName, string branchAddress, string businessName)
        {
            SaveXmlConfig(username, password, mac);
            SaveJsonConfigs(dbPath, username);
            SaveWinFormsConfig(dbPath, branchName, branchAddress, businessName);
        }

        private void SaveXmlConfig(string username, string password, string mac)
        {
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(_xmlConfigPath);
                UpdateOrCreateNode(xmlDoc, "Username", AesEncryptionHelper.Encrypt(username));
                UpdateOrCreateNode(xmlDoc, "Password", AesEncryptionHelper.Encrypt(password));
                UpdateOrCreateNode(xmlDoc, "MacAddress", mac);
                xmlDoc.Save(_xmlConfigPath);
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to update XML config: {ex.Message}", false, true);
            }
        }

        private void SaveJsonConfigs(string dbPath, string username)
        {
            try
            {
                SaveDbPathToJson(_jsonWorkerPath, dbPath, username);
                SaveDbPathToJson(_jsonMainPath, dbPath, username);
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to update JSON configs: {ex.Message}", false, true);
            }
        }

        private void SaveWinFormsConfig(string dbPath, string branchName, string branchAddress, string businessName)
        {
            try
            {
                var doc = new XmlDocument();
                doc.Load(_winformsConfigPath);

                UpdateOrCreateNode(doc, "DefaultDBFilePath", dbPath);
                UpdateOrCreateNode(doc, "branchName", branchName);
                UpdateOrCreateNode(doc, "branchAddress", branchAddress);
                UpdateOrCreateNode(doc, "businessName", businessName);

                doc.Save(_winformsConfigPath);
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to update WinForms config: {ex.Message}", false, true);
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

        private void SaveDbPathToJson(string jsonFilePath, string dbPath, string posId)
        {
            try
            {
                JObject root;

                if (File.Exists(jsonFilePath))
                {
                    string text = File.ReadAllText(jsonFilePath);
                    root = string.IsNullOrWhiteSpace(text) ? new JObject() : JObject.Parse(text);
                }
                else
                {
                    root = new JObject();
                }

                if (root["AppSettings"] == null || root["AppSettings"].Type != JTokenType.Object)
                    root["AppSettings"] = new JObject();

                root["AppSettings"]["DefaultDBFilePath"] = dbPath;
                root["AppSettings"]["POS"] = posId;

                File.WriteAllText(jsonFilePath, root.ToString(Newtonsoft.Json.Formatting.Indented));
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to update {Path.GetFileName(jsonFilePath)}: {ex.Message}", false, true);
            }
        }

        private void SaveEnvironmentSettings()
        {
            try
            {
                if (rdoProduction.Checked)
                    SaveEnvironmentToApiConfig("Production");
                else if (rdoSandbox.Checked)
                    SaveEnvironmentToApiConfig("Sandbox");
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to save environment settings: {ex.Message}", false, true);
            }
        }

        private void SaveEnvironmentToApiConfig(string environment)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_jsonMainPath) || !File.Exists(_jsonMainPath))
                {
                    ShowMessage("API config file not found.", false, true);
                    return;
                }

                string json = File.ReadAllText(_jsonMainPath);
                dynamic config = JsonConvert.DeserializeObject(json) ?? new JObject();

                if (config["AppSettings"] == null)
                    config["AppSettings"] = new JObject();

                config["AppSettings"]["Environment"] = environment;
                string apiUrl = environment.Equals("Production", StringComparison.OrdinalIgnoreCase)
                    ? "https://api.yourdomain.com"
                    : "https://sandbox.api.yourdomain.com";
                config["AppSettings"]["ApiBaseUrl"] = apiUrl;

                bool isProd = environment.Equals("Production", StringComparison.OrdinalIgnoreCase);
                config["AppSettings"]["isProduction"] = isProd;

                File.WriteAllText(_jsonMainPath, JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented));
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to update API config: {ex.Message}", false, false);
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
            lblMessage.TextAlign = ContentAlignment.MiddleLeft;
            lblMessage.AutoSize = false;
            lblMessage.Height = 35;
            lblMessage.Width = 450;

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
            if (Interlocked.Exchange(ref _isLoadingFlag, 1) == 1)
                return;

            try
            {
                if (progressBar != null)
                {
                    progressBar.Style = ProgressBarStyle.Marquee;
                    progressBar.MarqueeAnimationSpeed = 30;
                    progressBar.Visible = true;
                    progressBar.BringToFront();
                    progressBar.Update();
                }

                await work();
            }
            finally
            {
                if (progressBar != null)
                {
                    progressBar.Visible = false;
                    progressBar.Style = ProgressBarStyle.Continuous;
                }

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

        private void txtUsername_KeyPress(object sender, KeyPressEventArgs e) { }

        private void txtPassword_KeyPress(object sender, KeyPressEventArgs e) { }

        private void ExitApplication(int exitCode)
        {
            try
            {
                this.TopMost = false;
                SetWindowPos(this.Handle, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
                Application.ExitThread();
                Application.Exit();
                Environment.Exit(exitCode);
            }
            catch
            {
                Environment.Exit(exitCode);
            }
        }

        #endregion
    }
}