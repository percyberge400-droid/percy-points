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
        // --- Win32 API to force window on top ---
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int X,
            int Y,
            int cx,
            int cy,
            uint uFlags);

        // HWND constants
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

        // Flags for SetWindowPos
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_SHOWWINDOW = 0x0040;

        // Paths for different config files
        private readonly string _xmlConfigPath;
        private readonly string _jsonWorkerPath;
        private readonly string _jsonMainPath;
        private readonly string _winformsConfigPath;
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

            //this.StartPosition = FormStartPosition.CenterScreen;

            //this.Load += ConfigForm_Load;

            // Optional: Prevent user from sending it to back
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            _xmlConfigPath = xmlConfigPath;
            _jsonWorkerPath = jsonWorkerPath;
            _jsonMainPath = jsonMainPath;
            _setupConfigPath = setupConfigPath;
            _winformsConfigPath = winformsConfigPath;

            // Hook validation events
            txtUsername.KeyPress += txtUsername_KeyPress;
            txtPassword.KeyPress += txtPassword_KeyPress;
            txtUsername.TextChanged += ValidateForm;
            txtPassword.TextChanged += ValidateForm;

            this.AcceptButton = btnOk;

            // Hide progress bar initially
            if (progressBar != null) progressBar.Visible = false;



            // Load logo from App.config if available
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
            string value = ConfigurationManager.AppSettings["DefaultDBFilePath"];
            ClearAllFields();

            // Load default DB path from App.config
            string defaultPath = ConfigurationManager.AppSettings["DefaultDBFilePath"];
            if (!string.IsNullOrWhiteSpace(defaultPath))
            {
                txtFilePath.Text = defaultPath;
            }

        }

        #region Progress Bar Helper
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

        // --- Ensure form opens on top of everything ---
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.CenterToScreen();  // center form
            this.TopMost = true;    // mark as topmost

            // Force Win32 TopMost in case another app steals focus
            SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0,
                         SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);

            this.Activate();        // bring focus
            this.BringToFront();    // make sure visible
        }

        // --- Browse button for selecting DB file path ---
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

        private void ClearAllFields()
        {
            txtUsername.Clear();
            txtPassword.Clear();
        }


        private void txtUsername_KeyPress(object sender, KeyPressEventArgs e) { }
        private void txtPassword_KeyPress(object sender, KeyPressEventArgs e) { }

        private void ValidateForm(object sender, EventArgs e)
        {
            btnOk.Enabled = !string.IsNullOrWhiteSpace(txtUsername.Text);
        }

        // --- OK button click handler ---
        private async void btnOk_Click(object sender, EventArgs e)
        {
            if (_isLoading)
            {
                MessageBox.Show("Configuration is already in progress. Please wait...",
                    "Please Wait", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

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

                        // Ensure DB folder exists
                        string folderPath = Path.GetDirectoryName(dbPath);
                        if (!Directory.Exists(folderPath))
                            Directory.CreateDirectory(folderPath);

                        // Get MAC
                        string macaddress = GetMacAddress();

                        // Prepare auth payload
                        var payload = new
                        {
                            posId = int.Parse(username),
                            macAddress = macaddress,
                            token = password
                        };

                        // Get API URL
                        string apiUrl = ConfigurationManager.AppSettings["ApiUrl"];
                        if (string.IsNullOrWhiteSpace(apiUrl))
                        {
                            //MessageBox.ShowError("API URL is missing in App.config.");
                            MessageBox.Show("API URL is missing in App.config.", "Configuration Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        // Call API
                        using (var client = new HttpClient())
                        {
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Add("accept", "text/plain");

                            var jsonContent = new StringContent(
                                JsonConvert.SerializeObject(payload),
                                Encoding.UTF8,
                                "application/json"
                            );

                            var response = await client.PostAsync(apiUrl, jsonContent);
                            string responseBody = await response.Content.ReadAsStringAsync();

                            if (!response.IsSuccessStatusCode)
                                throw new Exception("Authentication failed: Invalid credentials or MAC address.");

                            var json = JObject.Parse(responseBody);
                            string statusCode = json["statusCode"]?.ToString();
                            bool data = json["data"]?.ToObject<bool>() ?? false;
                            string message = json["message"]?.ToString();

                            if (statusCode != "200" || !data)
                                throw new Exception("Authentication failed: " + (message ?? "Unknown error"));
                        }

                        // Save XML config
                        var doc = new XmlDocument();
                        doc.Load(_xmlConfigPath);

                        var sqliteOptions = new DbContextOptionsBuilder<SqliteDbContext>()
                            .UseSqlite($"Data Source={dbPath}")
                            .Options;

                        using (var context = new SqliteDbContext(sqliteOptions))
                            context.Database.EnsureCreated();

                        var services = new ServiceCollection();
                        services.AddDbContext<SqliteDbContext>(opt => opt.UseSqlite($"Data Source={dbPath}"));

                        UpdateOrCreateNode(doc, "Username", AesEncryptionHelper.Encrypt(username));
                        UpdateOrCreateNode(doc, "Password", AesEncryptionHelper.Encrypt(password));
                        UpdateOrCreateNode(doc, "MacAddress", macaddress);

                        doc.Save(_xmlConfigPath);

                        SaveDbPathToJson(_jsonWorkerPath, dbPath);
                        SaveDbPathToJson(_jsonMainPath, dbPath);
                        SaveDbPathToWinFormsConfig(dbPath);

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

                        // ✅ release topmost before exiting
                        this.TopMost = false;
                        SetWindowPos(this.Handle, HWND_NOTOPMOST, 0, 0, 0, 0,
                                     SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);

                        Environment.Exit(0);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        // --- Update or create XML node ---
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

        // --- Save DB path to JSON ---
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
                    root["AppSettings"] = new JObject();

                root["AppSettings"]["DefaultDBFilePath"] = dbPath;

                File.WriteAllText(jsonFilePath, root.ToString(Newtonsoft.Json.Formatting.Indented));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update {Path.GetFileName(jsonFilePath)}: " + ex.Message,
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // --- Save DB path to WinForms config ---
        private void SaveDbPathToWinFormsConfig(string dbPath)
        {
            try
            {
                var doc = new XmlDocument();
                doc.Load(_winformsConfigPath);

                var node = doc.SelectSingleNode("//appSettings/add[@key='DefaultDBFilePath']");
                if (node == null)
                {
                    var appSettings = doc.SelectSingleNode("//appSettings") ?? doc.CreateElement("appSettings");
                    if (appSettings.ParentNode == null)
                        doc.DocumentElement.AppendChild(appSettings);

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

        // --- Get MAC address ---
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

        // --- Cancel button ---
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
                // ✅ release topmost before exiting
                this.TopMost = false;
                SetWindowPos(this.Handle, HWND_NOTOPMOST, 0, 0, 0, 0,
                             SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);

                // MSI cancel error code
                Environment.Exit(1602);
            }
        }
    }
}
