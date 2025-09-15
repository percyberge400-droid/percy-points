using POSPRA.Application.Services.FiscalService;

namespace POSPRA_WinFormsUI.Forms
{
    public partial class Main : Form
    {
        private readonly IFiscalService _fiscalService;
        public Main()
        {
            InitializeComponent();

            this.IsMdiContainer = true;

            panInvoiceSelection.Visible = false;
            panItemEntry.Visible = false;
            panExportInvoice.Visible = false;

            btnDashboard.ForeColor = ColorTranslator.FromHtml("#686DF4"); // Highlight color

            Form childForm = new DashboardForm();
            childForm.MdiParent = this;
            childForm.Dock = DockStyle.Fill;
            childForm.Show();
        }

        private void ResetNavStyles()
        {
            btnDashboard.ForeColor = Color.Black;
            btnInvoiceSelection.ForeColor = Color.Black;
            btnItemEntry.ForeColor = Color.Black;
            btnExportInvoice.ForeColor = Color.Black;

            panDashboard.Visible = false;
            panInvoiceSelection.Visible = false;
            panItemEntry.Visible = false;
            panExportInvoice.Visible = false;
        }

        private void btnDashboard_Click(object sender, EventArgs e)
        {
            ResetNavStyles();
            btnDashboard.ForeColor = ColorTranslator.FromHtml("#686DF4");
            panDashboard.Visible = true;

            LoadView("Dashboard");
        }

        public void LoadView(string v)
        {
            // Close currently active child form if one exists
            if (this.ActiveMdiChild != null)
            {
                this.ActiveMdiChild.Close();
            }

            Form childForm = null;

            switch (v)
            {
                case "Dashboard":
                    childForm = new DashboardForm();
                    break;

                case "Invoice Selection":
                    childForm = new InvoiceEntry(_fiscalService);
                    break;

                case "Invoice Entry":
                    childForm = new item_entry(_fiscalService);
                    break;

                case "Invoice Export":
                    // Uncomment when ready
                    // childForm = new InvoiceExportForm();
                    break;
            }

            if (childForm != null)
            {
                // Apply all properties to remove title bar
                childForm.TopLevel = false; // Very important!
                childForm.FormBorderStyle = FormBorderStyle.None;
                childForm.ControlBox = false;
                childForm.MaximizeBox = false;
                childForm.MinimizeBox = false;
                childForm.ShowInTaskbar = false;
                childForm.Text = "";
                childForm.WindowState = FormWindowState.Maximized;

                // Set as MDI child
                childForm.MdiParent = this;
                childForm.Dock = DockStyle.Fill;
                childForm.Show();
            }
        }


        private void btnInvoiceSelection_Click(object sender, EventArgs e)
        {
            ResetNavStyles();
            btnInvoiceSelection.ForeColor = ColorTranslator.FromHtml("#686DF4");
            panInvoiceSelection.Visible = true;

            LoadView("Invoice Entry");
        }

        private void btnExportInvoice_Click(object sender, EventArgs e)
        {
            ResetNavStyles();
            btnExportInvoice.ForeColor = ColorTranslator.FromHtml("#686DF4");
            panExportInvoice.Visible = true;
        }

        private void btnItemEntry_Click(object sender, EventArgs e)
        {
            ResetNavStyles();
            btnItemEntry.ForeColor = ColorTranslator.FromHtml("#686DF4");
            panItemEntry.Visible = true;

            LoadView("Item Entry");
        }

        private void Main_Load(object sender, EventArgs e)
        {
            // 🔒 Disable Item Entry at startup
            btnItemEntry.Enabled = false;
            btnItemEntry.ForeColor = Color.Gray;
        }
    }
}
