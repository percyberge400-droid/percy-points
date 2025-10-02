using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Runtime.InteropServices;
using POSPRA.SecurityEncryption;

namespace POSPRA.SetupUI
{
    public partial class ConfigForm : Form
    {
        //
        // TOP Most Window
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
        //

        private readonly string _configPath;

        public ConfigForm(string configPath)
        {
            InitializeComponent();
            _configPath = configPath;

            txtUsername.KeyPress += txtUsername_KeyPress;
            txtPassword.KeyPress += txtPassword_KeyPress;

            // 🔑 Live validation
            txtUsername.TextChanged += ValidateForm;
            txtPassword.TextChanged += ValidateForm;

            btnOk.Enabled = false; // disabled at start
            this.AcceptButton = btnOk;
        }

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
                if (!File.Exists(_configPath))
                {
                    MessageBox.Show("Config file not found:\n" + _configPath);
                    return;
                }

                var doc = new XmlDocument();
                doc.Load(_configPath);

                var userNode = doc.SelectSingleNode("//appSettings/add[@key='Username']");
                var passNode = doc.SelectSingleNode("//appSettings/add[@key='Password']");

                if (userNode != null)
                    txtUsername.Text = AesEncryptionHelper.Decrypt(userNode.Attributes["value"].Value);

                if (passNode != null)
                    txtPassword.Text = AesEncryptionHelper.Decrypt(passNode.Attributes["value"].Value);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reading config: " + ex.Message);
            }
        }

        // Restrict txtUsername to numeric only (max 6)
        private void txtUsername_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;

            if (txtUsername.Text.Length >= 6 && !char.IsControl(e.KeyChar))
                e.Handled = true;
        }

        // Restrict txtPassword to alphanumeric only (max 8)
        private void txtPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsLetterOrDigit(e.KeyChar))
                e.Handled = true;

            if (txtPassword.Text.Length >= 8 && !char.IsControl(e.KeyChar))
                e.Handled = true;
        }

        // Live validation → enable OK only when valid
        private void ValidateForm(object sender, EventArgs e)
        {
            bool validUsername = System.Text.RegularExpressions.Regex.IsMatch(txtUsername.Text, @"^\d{6}$");
            bool validPassword = System.Text.RegularExpressions.Regex.IsMatch(txtPassword.Text, @"^[a-zA-Z0-9]{8}$");

            btnOk.Enabled = validUsername && validPassword;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            try
            {
                string username = txtUsername.Text.Trim();
                string password = txtPassword.Text.Trim();

                // Final strict validation
                if (!System.Text.RegularExpressions.Regex.IsMatch(username, @"^\d{6}$"))
                {
                    MessageBox.Show("Username must be exactly 6 numeric digits.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"^[a-zA-Z0-9]{8}$"))
                {
                    MessageBox.Show("Password must be exactly 8 alphanumeric characters (no special chars).",
                        "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var doc = new XmlDocument();
                doc.Load(_configPath);

                var userNode = doc.SelectSingleNode("//appSettings/add[@key='Username']");
                var passNode = doc.SelectSingleNode("//appSettings/add[@key='Password']");

                if (userNode != null) userNode.Attributes["value"].Value = AesEncryptionHelper.Encrypt(username);
                if (passNode != null) passNode.Attributes["value"].Value = AesEncryptionHelper.Encrypt(password);

                doc.Save(_configPath);

                MessageBox.Show("POS ID and Access Code saved. Press Okay", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                Environment.Exit(0); // success
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating POS ID and Access Code: " + ex.Message);
                Environment.Exit(1); // error
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to cancel the installation?",
                "Cancel Installation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                Environment.Exit(1602); // MSI cancel code
            }
        }
    }
}
