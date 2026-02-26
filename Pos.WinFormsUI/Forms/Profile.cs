using Pos.SecurityEncryption;
using System;
using System.Configuration;
using System.Windows.Forms;

namespace Pos.WinFormsUI.Forms
{
    public partial class Profile : Form
    {
        public Profile()
        {
            InitializeComponent();
            LoadUserProfile();
        }

        private void LoadUserProfile()
        {
            try
            {
                var settings = AppSettingsReader.Load();
                SetUserDetails(settings);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load profile: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetUserDetails(WinformsAppSettings settings)
        {
            lblUserPosID.Text = settings.PosID ?? "N/A";
            lblUserBranchAddress.Text = settings.BranchAddress ?? "N/A";
            lblUserBusinessName.Text = settings.BusinessName ?? "N/A";
            lblUserBranchName.Text = settings.BranchName ?? "N/A";
            lblUserPhoneNo.Text = settings.PhoneNumber ?? "N/A";
        }
    }

    /// <summary>
    /// Strongly-typed representation of application settings used in the Profile view.
    /// </summary>
    public class WinformsAppSettings
    {
        public string PosID { get; set; } = string.Empty;
        public string? BranchName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? BranchAddress { get; set; }
        public string? BusinessName { get; set; }
    }

    /// <summary>
    /// Responsible for reading and decrypting application configuration values.
    /// </summary>
    public static class AppSettingsReader
    {
        public static WinformsAppSettings Load()
        {
            return new WinformsAppSettings
            {
                PosID = AesEncryptionHelper.Decrypt(ConfigurationManager.AppSettings["Username"]!),
                BranchName = ConfigurationManager.AppSettings["branchName"],
                PhoneNumber = ConfigurationManager.AppSettings["phoneNumber"],
                BranchAddress = ConfigurationManager.AppSettings["branchAddress"],
                BusinessName = ConfigurationManager.AppSettings["businessName"]
            };
        }
    }
}