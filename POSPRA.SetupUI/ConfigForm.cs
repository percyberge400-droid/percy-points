using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using POSPRA.Infrastructure.Context;
using POSPRA.SecurityEncryption;
using System.Configuration;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

namespace POSPRA.SetupUI
{
    public partial class ConfigForm : Form
    {
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
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_SHOWWINDOW = 0x0040;

        private readonly string _xmlConfigPath;
        private readonly string _jsonWorkerPath; //worker json path
        private readonly string _jsonMainPath;   //API json path
        private readonly string _winformsConfigPath; // WinFormsUI config path
        private string _setupConfigPath;

        private int _isLoadingFlag = 0;
        private bool _isLoading = false;

        public ConfigForm(string xmlConfigPath, string jsonWorkerPath, string jsonMainPath, string setupConfigPath, string winformsConfigPath)
        {
            InitializeComponent();
            // Always stay above all other windows
            this.TopMost = true;

            // Make sure it stays focused
            this.BringToFront();
            this.Activate();

            // Optional: Prevent user from sending it to back
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            _xmlConfigPath = xmlConfigPath;
            _jsonWorkerPath = jsonWorkerPath;
            _jsonMainPath = jsonMainPath;
            _setupConfigPath = setupConfigPath;
            _winformsConfigPath = winformsConfigPath;

            txtUsername.KeyPress += txtUsername_KeyPress;
            txtPassword.KeyPress += txtPassword_KeyPress;
            txtUsername.TextChanged += ValidateForm;
            txtPassword.TextChanged += ValidateForm;

            this.AcceptButton = btnOk;

            // Hide progress bar initially
            if (progressBar != null) progressBar.Visible = false;

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

        #region Progress Bar

        private async Task RunSingleLoad(Func<Task> work)
        {
            if (Interlocked.Exchange(ref _isLoadingFlag, 1) == 1) return;

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

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            this.CenterToScreen();
            this.TopMost = true;

            SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0,
                         SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
            this.Activate();
            this.BringToFront();
        }

        private void ConfigForm_Load(object sender, EventArgs e)
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);

            try
            {
                var doc = new XmlDocument();
                doc.Load(_xmlConfigPath); // ✅ FIXED

                var userNode = doc.SelectSingleNode("//appSettings/add[@key='Username']");
                var passNode = doc.SelectSingleNode("//appSettings/add[@key='Password']");
                var dbPathNode = doc.SelectSingleNode("//appSettings/add[@key='DefaultDBFilePath']");

                if (userNode != null)
                    txtUsername.Text = AesEncryptionHelper.Decrypt(userNode.Attributes["value"].Value);

                if (passNode != null)
                    txtPassword.Text = AesEncryptionHelper.Decrypt(passNode.Attributes["value"].Value);

                if (dbPathNode != null)
                    txtFilePath.Text = dbPathNode.Attributes["value"].Value; // not encrypted
            }
            catch (Exception ex)
            {
                //WindowsLocalAppNotification.Show($"Error reading config: {ex.Message}");
                MessageBox.Show($"Error reading config: {ex.Message}");
            }
        }

        // ✅ Browse button for selecting DB file path
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Title = "Select or create SQLite DB file";
                dialog.Filter = "SQLite DB (*.db)|*.db|All files (*.*)|*.*";
                dialog.FileName = "POSPRA.db";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtFilePath.Text = dialog.FileName;
                }
            }
        }

        private void txtUsername_KeyPress(object sender, KeyPressEventArgs e)
        {
            //if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            //    e.Handled = true;

            //if (txtUsername.Text.Length >= 6 && !char.IsControl(e.KeyChar))
            //    e.Handled = true;
        }

        private void txtPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            //if (!char.IsControl(e.KeyChar) && !char.IsLetterOrDigit(e.KeyChar))
            //    e.Handled = true;

            //if (txtPassword.Text.Length >= 8 && !char.IsControl(e.KeyChar))
            //    e.Handled = true;
        }

        private void ValidateForm(object sender, EventArgs e)
        {
            //bool validUsername = System.Text.RegularExpressions.Regex.IsMatch(txtUsername.Text, @"^\d{6}$");
            // bool validPassword = System.Text.RegularExpressions.Regex.IsMatch(txtPassword.Text, @"^[a-zA-Z0-9]{8}$");
            //  btnOk.Enabled = validUsername;
        }

        private async void btnOk_Click(object sender, EventArgs e)
        {
            if (_isLoading)
            {
                MessageBox.Show("Configuration is already in progress. Please wait...",
                    "Please Wait", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Disable button immediately
            btnOk.Enabled = false;
            btnOk.Text = "Processing...";

            try
            {
                string username = txtUsername.Text.Trim();
                string password = txtPassword.Text.Trim();
                string dbPath = txtFilePath.Text.Trim();

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Please enter both POID and Access Code.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(dbPath))
                {
                    MessageBox.Show("Please select a database file path.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                await RunSingleLoad(async () =>
                {
                    try
                    {
                        _isLoading = true;

                        // ✅ Auto-create folder if missing
                        string folderPath = Path.GetDirectoryName(dbPath);
                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                        }

                        // ✅ Get MAC address
                        string macAddress = ConfigurationManager.AppSettings["mac"] ?? GetMacAddress();

                        // ✅ Prepare authentication payload
                        var payload = new
                        {
                            posId = int.Parse(username),
                            macAddress = macAddress,
                            token = password
                        };

                        // ✅ Get API URL from App.config
                        string apiUrl = ConfigurationManager.AppSettings["ApiUrl"];
                        if (string.IsNullOrWhiteSpace(apiUrl))
                        {
                            //MessageBox.ShowError("API URL is missing in App.config.");
                            MessageBox.Show("API URL is missing in App.config.", "Configuration Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        using (var client = new HttpClient())
                        {
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Add("accept", "text/plain");

                            var jsonContent = new StringContent(
                                JsonConvert.SerializeObject(payload),
                                Encoding.UTF8,
                                "application/json"
                            );

                            // ✅ Call API
                            var response = await client.PostAsync(apiUrl, jsonContent);
                            string responseBody = await response.Content.ReadAsStringAsync();

                            if (!response.IsSuccessStatusCode)
                            {
                                throw new Exception("Authentication failed: Invalid credentials or MAC address.");
                            }

                            // ✅ Parse JSON response
                            var json = JObject.Parse(responseBody);
                            string statusCode = json["statusCode"]?.ToString();
                            bool data = json["data"]?.ToObject<bool>() ?? false;
                            string message = json["message"]?.ToString();

                            if (statusCode != "200" || !data)
                            {
                                throw new Exception("Authentication failed: " + (message ?? "Unknown error"));
                            }

                            // ✅ Success message
                            MessageBox.Show("API URL is missing in App.config.");
                        }

                        // ✅ Save only Username, Password, and MacAddress to XML (skip DB path)
                        var doc = new XmlDocument();
                        doc.Load(_xmlConfigPath);

                        // 🔹 Step 4: Initialize SQLite database
                        var sqliteOptions = new DbContextOptionsBuilder<SqliteDbContext>()
                            .UseSqlite($"Data Source={dbPath}")
                            .Options;

                        using (var context = new SqliteDbContext(sqliteOptions))
                        {
                            context.Database.EnsureCreated();
                        }

                        // 🔹 Step 5: Build DI container (if needed later)
                        var services = new ServiceCollection();
                        services.AddDbContext<SqliteDbContext>(opt => opt.UseSqlite($"Data Source={dbPath}"));

                        UpdateOrCreateNode(doc, "Username", AesEncryptionHelper.Encrypt(username));
                        UpdateOrCreateNode(doc, "Password", AesEncryptionHelper.Encrypt(password));
                        UpdateOrCreateNode(doc, "MacAddress", macAddress);

                        doc.Save(_xmlConfigPath);

                        // ✅ Save DB path only in JSON files
                        SaveDbPathToJson(_jsonWorkerPath, dbPath);  // appsettings.worker.json
                        SaveDbPathToJson(_jsonMainPath, dbPath);    // appsettings.json

                        // ✅ Save DB path in POSPRA-WinFormsUI.dll.config
                        SaveDbPathToWinFormsConfig(dbPath);

                        // ✅ Save DB path also in POSPRA.SetupUI.dll.config
                        try
                        {
                            var docSetup = new XmlDocument();
                            docSetup.Load(_setupConfigPath);
                            UpdateOrCreateNode(docSetup, "DefaultDBFilePath", dbPath);
                            docSetup.Save(_setupConfigPath);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Failed to update SetupUI config: " + ex.Message,
                                "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }

                        MessageBox.Show("Configuration saved successfully.", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        Environment.Exit(0);
                    }
                    catch (Exception ex)
                    {
                        // ✅ Show error to user but do NOT terminate installer
                        MessageBox.Show($"Error: {ex.Message}");

                        // Do NOT call Environment.Exit(1);
                        // Installer will continue to allow retry or close.
                    }
                    finally
                    {
                        _isLoading = false;
                    }
                });
            }
            finally
            {
                btnOk.Enabled = true;
                btnOk.Text = "OK";
            }
        }

        private void UpdateOrCreateNode(XmlDocument doc, string key, string value)
        {
            var node = doc.SelectSingleNode($"//appSettings/add[@key='{key}']");
            if (node == null)
            {
                var appSettings = doc.SelectSingleNode("//appSettings");
                if (appSettings == null)
                {
                    appSettings = doc.CreateElement("appSettings");
                    doc.DocumentElement.AppendChild(appSettings);
                }

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

        // ✅ JSON updater
        private void SaveDbPathToJson(string jsonFilePath, string dbPath)
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
                {
                    root["AppSettings"] = new JObject();
                }

                root["AppSettings"]["DefaultDBFilePath"] = dbPath;

                File.WriteAllText(jsonFilePath, root.ToString(Newtonsoft.Json.Formatting.Indented));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update {Path.GetFileName(jsonFilePath)}: " + ex.Message,
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ✅ Save DB path to WinFormsUI config (POSPRA-WinFormsUI.dll.config)
        private void SaveDbPathToWinFormsConfig(string dbPath)
        {
            try
            {
                var doc = new XmlDocument();
                doc.Load(_winformsConfigPath);

                var node = doc.SelectSingleNode("//appSettings/add[@key='DefaultDBFilePath']");
                if (node == null)
                {
                    var appSettings = doc.SelectSingleNode("//appSettings");
                    if (appSettings == null)
                    {
                        appSettings = doc.CreateElement("appSettings");
                        doc.DocumentElement.AppendChild(appSettings);
                    }

                    XmlElement newNode = doc.CreateElement("add");
                    newNode.SetAttribute("key", "DefaultDBFilePath");
                    newNode.SetAttribute("value", dbPath);
                    appSettings.AppendChild(newNode);
                }
                else
                {
                    node.Attributes["value"].Value = dbPath;
                }

                doc.Save(_winformsConfigPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update WinForms config: {ex.Message}", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ✅ Get MAC address of first active network adapter
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
                // Return MSI error code for cancel
                Environment.Exit(1602);
            }
        }

        private void btnBrowse_Click_1(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Title = "Select or create SQLite DB file";
                dialog.Filter = "SQLite DB (*.db)|*.db|All files (*.*)|*.*";
                dialog.FileName = "POSPRA.db";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtFilePath.Text = dialog.FileName;
                }
            }
        }
    }
}