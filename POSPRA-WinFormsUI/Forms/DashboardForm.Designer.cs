
// DashboardForm.Designer.cs
namespace POSPRA_WinFormsUI.Forms
{
    partial class DashboardForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Designer fields

        private System.Windows.Forms.ToolStripMenuItem settings;
        private System.Windows.Forms.ToolStripMenuItem logout;

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelTop;
        private System.Windows.Forms.Panel panelAll;
        private System.Windows.Forms.Panel panelPending;
        private System.Windows.Forms.Panel panelPaid;
        private System.Windows.Forms.Panel panelInProgress;

        private System.Windows.Forms.Label lblAllTitle;
        private System.Windows.Forms.Label labelAllInvoices;
        private System.Windows.Forms.Label lblPendingTitle;
        private System.Windows.Forms.Label labelPendingInvoice;
        private System.Windows.Forms.Label lblPaidTitle;
        private System.Windows.Forms.Label labelPaidInvoices;
        private System.Windows.Forms.Label lblInProgressTitle;
        private System.Windows.Forms.Label labelInProgressInvc;

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelMain;
        private System.Windows.Forms.Panel panelInvoices;
        private Panel panelinvoicechart;
        private Panel panellogchart;
        private System.Windows.Forms.Label labelInvoicesTitle;
        private System.Windows.Forms.DataGridView InvoicesDataGridView;

        private System.Windows.Forms.Panel panelLogs;
        private System.Windows.Forms.Label labelLogsTitle;
        private System.Windows.Forms.DataGridView LogsDataGridView;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelLogStats;

        // Invoices panel
        private System.Windows.Forms.DateTimePicker dtpStartDate;
        private System.Windows.Forms.DateTimePicker dtpEndDate;
        private System.Windows.Forms.Label lblDateRange;


        private System.Windows.Forms.Button btnToday;

        #endregion

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            settings = new ToolStripMenuItem();
            logout = new ToolStripMenuItem();
            tableLayoutPanelTop = new TableLayoutPanel();
            panelAll = new Panel();
            lblAllTitle = new Label();
            labelAllInvoices = new Label();
            panelPending = new Panel();
            lblPendingTitle = new Label();
            labelPendingInvoice = new Label();
            panelPaid = new Panel();
            lblPaidTitle = new Label();
            labelPaidInvoices = new Label();
            tableLayoutPanelMain = new TableLayoutPanel();
            panelInvoices = new Panel();
            btnExportInvoice = new Button();
            btnFilter = new Button();
            btnRefresh = new Button();
            progressBar = new ProgressBar();
            btnFilterSynced = new Button();
            btnClearFilter = new Button();
            btnToday = new Button();
            lblDateRange = new Label();
            dtpStartDate = new DateTimePicker();
            dtpEndDate = new DateTimePicker();
            InvoicesDataGridView = new DataGridView();
            labelInvoicesTitle = new Label();
            panelLogs = new Panel();
            btnExportLogs = new Button();
            LogsDataGridView = new DataGridView();
            labelLogsTitle = new Label();
            panelinvoicechart = new Panel();
            panellogchart = new Panel();
            tableLayoutPanelLogStats = new TableLayoutPanel();
            panelTotalLogs = new Panel();
            lblTotalLogsTitle = new Label();
            lblTotalLogsCount = new Label();
            panelErrorLogs = new Panel();
            lblErrorLogsTitle = new Label();
            lblErrorLogsCount = new Label();
            panelWarningLogs = new Panel();
            lblWarningLogsTitle = new Label();
            lblWarningLogsCount = new Label();
            panelInfoLogs = new Panel();
            lblInfoLogsTitle = new Label();
            lblInfoLogsCount = new Label();
            colLogID = new DataGridViewTextBoxColumn();
            colMessage = new DataGridViewTextBoxColumn();
            colException = new DataGridViewTextBoxColumn();
            logdatetime = new DataGridViewTextBoxColumn();
            tableLayoutPanelTop.SuspendLayout();
            panelAll.SuspendLayout();
            panelPending.SuspendLayout();
            panelPaid.SuspendLayout();
            tableLayoutPanelMain.SuspendLayout();
            panelInvoices.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)InvoicesDataGridView).BeginInit();
            panelLogs.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)LogsDataGridView).BeginInit();
            panellogchart.SuspendLayout();
            tableLayoutPanelLogStats.SuspendLayout();
            panelTotalLogs.SuspendLayout();
            panelErrorLogs.SuspendLayout();
            panelWarningLogs.SuspendLayout();
            panelInfoLogs.SuspendLayout();
            SuspendLayout();
            // 
            // settings
            // 
            settings.Name = "settings";
            settings.Size = new Size(32, 19);
            // 
            // logout
            // 
            logout.Name = "logout";
            logout.Size = new Size(32, 19);
            // 
            // tableLayoutPanelTop
            // 
            tableLayoutPanelTop.AutoSize = true;
            tableLayoutPanelTop.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanelTop.ColumnCount = 3;
            tableLayoutPanelTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
            tableLayoutPanelTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
            tableLayoutPanelTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
            tableLayoutPanelTop.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 23F));
            tableLayoutPanelTop.Controls.Add(panelAll, 0, 0);
            tableLayoutPanelTop.Controls.Add(panelPending, 1, 0);
            tableLayoutPanelTop.Controls.Add(panelPaid, 2, 0);
            tableLayoutPanelTop.Dock = DockStyle.Top;
            tableLayoutPanelTop.Location = new Point(0, 0);
            tableLayoutPanelTop.Margin = new Padding(0);
            tableLayoutPanelTop.Name = "tableLayoutPanelTop";
            tableLayoutPanelTop.Padding = new Padding(10, 11, 10, 11);
            tableLayoutPanelTop.RowCount = 1;
            tableLayoutPanelTop.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanelTop.Size = new Size(1781, 155);
            tableLayoutPanelTop.TabIndex = 1;
            // 
            // panelAll
            // 
            panelAll.BackColor = Color.White;
            panelAll.Controls.Add(lblAllTitle);
            panelAll.Controls.Add(labelAllInvoices);
            panelAll.Dock = DockStyle.Fill;
            panelAll.Location = new Point(18, 19);
            panelAll.Margin = new Padding(8);
            panelAll.Name = "panelAll";
            panelAll.Padding = new Padding(11, 12, 11, 12);
            panelAll.Size = new Size(570, 117);
            panelAll.TabIndex = 0;
            // 
            // lblAllTitle
            // 
            lblAllTitle.BackColor = Color.Transparent;
            lblAllTitle.Dock = DockStyle.Bottom;
            lblAllTitle.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            lblAllTitle.ForeColor = Color.Black;
            lblAllTitle.Location = new Point(11, 60);
            lblAllTitle.Name = "lblAllTitle";
            lblAllTitle.Size = new Size(548, 45);
            lblAllTitle.TabIndex = 1;
            lblAllTitle.Text = "All Invoices";
            // 
            // labelAllInvoices
            // 
            labelAllInvoices.AutoSize = true;
            labelAllInvoices.Dock = DockStyle.Fill;
            labelAllInvoices.Font = new Font("Microsoft Sans Serif", 28.2F, FontStyle.Bold);
            labelAllInvoices.ForeColor = Color.White;
            labelAllInvoices.Location = new Point(11, 12);
            labelAllInvoices.Name = "labelAllInvoices";
            labelAllInvoices.Size = new Size(50, 54);
            labelAllInvoices.TabIndex = 0;
            labelAllInvoices.Text = "0";
            labelAllInvoices.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // panelPending
            // 
            panelPending.BackColor = Color.White;
            panelPending.Controls.Add(lblPendingTitle);
            panelPending.Controls.Add(labelPendingInvoice);
            panelPending.Dock = DockStyle.Fill;
            panelPending.Location = new Point(604, 19);
            panelPending.Margin = new Padding(8);
            panelPending.Name = "panelPending";
            panelPending.Padding = new Padding(11, 12, 11, 12);
            panelPending.Size = new Size(570, 117);
            panelPending.TabIndex = 1;
            // 
            // lblPendingTitle
            // 
            lblPendingTitle.BackColor = Color.Transparent;
            lblPendingTitle.Dock = DockStyle.Bottom;
            lblPendingTitle.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblPendingTitle.ForeColor = Color.Black;
            lblPendingTitle.Location = new Point(11, 60);
            lblPendingTitle.Name = "lblPendingTitle";
            lblPendingTitle.Size = new Size(548, 45);
            lblPendingTitle.TabIndex = 1;
            lblPendingTitle.Text = "Not Synced";
            // 
            // labelPendingInvoice
            // 
            labelPendingInvoice.AutoSize = true;
            labelPendingInvoice.Dock = DockStyle.Fill;
            labelPendingInvoice.Font = new Font("Microsoft Sans Serif", 28.2F, FontStyle.Bold);
            labelPendingInvoice.ForeColor = Color.White;
            labelPendingInvoice.Location = new Point(11, 12);
            labelPendingInvoice.Name = "labelPendingInvoice";
            labelPendingInvoice.Size = new Size(50, 54);
            labelPendingInvoice.TabIndex = 0;
            labelPendingInvoice.Text = "0";
            labelPendingInvoice.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // panelPaid
            // 
            panelPaid.BackColor = Color.White;
            panelPaid.Controls.Add(lblPaidTitle);
            panelPaid.Controls.Add(labelPaidInvoices);
            panelPaid.Dock = DockStyle.Fill;
            panelPaid.Location = new Point(1190, 19);
            panelPaid.Margin = new Padding(8);
            panelPaid.Name = "panelPaid";
            panelPaid.Padding = new Padding(11, 12, 11, 12);
            panelPaid.Size = new Size(573, 117);
            panelPaid.TabIndex = 2;
            // 
            // lblPaidTitle
            // 
            lblPaidTitle.BackColor = Color.Transparent;
            lblPaidTitle.Dock = DockStyle.Bottom;
            lblPaidTitle.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            lblPaidTitle.ForeColor = Color.Black;
            lblPaidTitle.Location = new Point(11, 60);
            lblPaidTitle.Name = "lblPaidTitle";
            lblPaidTitle.Size = new Size(551, 45);
            lblPaidTitle.TabIndex = 1;
            lblPaidTitle.Text = "Synced Invoices";
            // 
            // labelPaidInvoices
            // 
            labelPaidInvoices.AutoSize = true;
            labelPaidInvoices.Dock = DockStyle.Fill;
            labelPaidInvoices.Font = new Font("Microsoft Sans Serif", 28.2F, FontStyle.Bold);
            labelPaidInvoices.ForeColor = Color.White;
            labelPaidInvoices.Location = new Point(11, 12);
            labelPaidInvoices.Name = "labelPaidInvoices";
            labelPaidInvoices.Size = new Size(50, 54);
            labelPaidInvoices.TabIndex = 0;
            labelPaidInvoices.Text = "0";
            labelPaidInvoices.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanelMain
            // 
            tableLayoutPanelMain.ColumnCount = 2;
            tableLayoutPanelMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80.52243F));
            tableLayoutPanelMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 19.47757F));
            tableLayoutPanelMain.Controls.Add(panelInvoices, 0, 0);
            tableLayoutPanelMain.Controls.Add(panelLogs, 0, 1);
            tableLayoutPanelMain.Controls.Add(panelinvoicechart, 1, 0);
            tableLayoutPanelMain.Controls.Add(panellogchart, 1, 1);
            tableLayoutPanelMain.Dock = DockStyle.Fill;
            tableLayoutPanelMain.Location = new Point(0, 155);
            tableLayoutPanelMain.Margin = new Padding(0);
            tableLayoutPanelMain.Name = "tableLayoutPanelMain";
            tableLayoutPanelMain.Padding = new Padding(10, 11, 10, 11);
            tableLayoutPanelMain.RowCount = 2;
            tableLayoutPanelMain.RowStyles.Add(new RowStyle(SizeType.Percent, 55.5045853F));
            tableLayoutPanelMain.RowStyles.Add(new RowStyle(SizeType.Percent, 44.4954147F));
            tableLayoutPanelMain.Size = new Size(1781, 676);
            tableLayoutPanelMain.TabIndex = 0;
            // 
            // panelInvoices
            // 
            panelInvoices.BackColor = Color.White;
            panelInvoices.Controls.Add(btnExportInvoice);
            panelInvoices.Controls.Add(btnFilter);
            panelInvoices.Controls.Add(btnRefresh);
            panelInvoices.Controls.Add(progressBar);
            panelInvoices.Controls.Add(btnFilterSynced);
            panelInvoices.Controls.Add(btnClearFilter);
            panelInvoices.Controls.Add(btnToday);
            panelInvoices.Controls.Add(lblDateRange);
            panelInvoices.Controls.Add(dtpStartDate);
            panelInvoices.Controls.Add(dtpEndDate);
            panelInvoices.Controls.Add(InvoicesDataGridView);
            panelInvoices.Controls.Add(labelInvoicesTitle);
            panelInvoices.Dock = DockStyle.Fill;
            panelInvoices.Location = new Point(16, 16);
            panelInvoices.Margin = new Padding(6, 5, 6, 5);
            panelInvoices.Name = "panelInvoices";
            panelInvoices.Padding = new Padding(8);
            panelInvoices.Size = new Size(1406, 353);
            panelInvoices.TabIndex = 0;
            // 
            // btnExportInvoice
            // 
            btnExportInvoice.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnExportInvoice.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnExportInvoice.BackColor = Color.SeaGreen;
            btnExportInvoice.FlatAppearance.BorderSize = 0;
            btnExportInvoice.FlatStyle = FlatStyle.Flat;
            btnExportInvoice.Font = new Font("Arial", 12F);
            btnExportInvoice.ForeColor = Color.White;
            btnExportInvoice.Location = new Point(469, 8);
            btnExportInvoice.Name = "btnExportInvoice";
            btnExportInvoice.Size = new Size(184, 37);
            btnExportInvoice.TabIndex = 10;
            btnExportInvoice.Text = "📄 Export Invoices";
            btnExportInvoice.UseVisualStyleBackColor = false;
            // 
            // btnFilter
            // 
            btnFilter.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnFilter.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnFilter.BackColor = Color.Black;
            btnFilter.FlatAppearance.BorderSize = 0;
            btnFilter.FlatStyle = FlatStyle.Flat;
            btnFilter.Font = new Font("Microsoft Sans Serif", 10.8F);
            btnFilter.ForeColor = Color.White;
            btnFilter.Location = new Point(1107, 8);
            btnFilter.Name = "btnFilter";
            btnFilter.Size = new Size(64, 37);
            btnFilter.TabIndex = 12;
            btnFilter.UseVisualStyleBackColor = false;
            // 
            // btnRefresh
            // 
            btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRefresh.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnRefresh.BackColor = Color.SeaGreen;
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.Font = new Font("Microsoft Sans Serif", 10.8F);
            btnRefresh.ForeColor = Color.White;
            btnRefresh.Location = new Point(1247, 8);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(64, 37);
            btnRefresh.TabIndex = 0;
            btnRefresh.UseVisualStyleBackColor = false;
            // 
            // progressBar
            // 
            progressBar.Anchor = AnchorStyles.None;
            progressBar.ForeColor = Color.SeaGreen;
            progressBar.Location = new Point(118, -59);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(400, 25);
            progressBar.Step = 1;
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.TabIndex = 0;
            progressBar.Visible = false;
            // 
            // btnFilterSynced
            // 
            btnFilterSynced.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnFilterSynced.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnFilterSynced.BackColor = Color.FromArgb(48, 59, 78);
            btnFilterSynced.FlatAppearance.BorderSize = 0;
            btnFilterSynced.FlatStyle = FlatStyle.Flat;
            btnFilterSynced.Font = new Font("Arial", 10.8F);
            btnFilterSynced.ForeColor = Color.White;
            btnFilterSynced.Location = new Point(659, 8);
            btnFilterSynced.Name = "btnFilterSynced";
            btnFilterSynced.Size = new Size(190, 37);
            btnFilterSynced.TabIndex = 5;
            btnFilterSynced.Text = "Show Synced First";
            btnFilterSynced.UseVisualStyleBackColor = false;
            // 
            // btnClearFilter
            // 
            btnClearFilter.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClearFilter.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnClearFilter.BackColor = Color.Red;
            btnClearFilter.FlatAppearance.BorderSize = 0;
            btnClearFilter.FlatStyle = FlatStyle.Flat;
            btnClearFilter.Font = new Font("Microsoft Sans Serif", 10.8F);
            btnClearFilter.ForeColor = Color.White;
            btnClearFilter.Location = new Point(1177, 8);
            btnClearFilter.Name = "btnClearFilter";
            btnClearFilter.Size = new Size(64, 37);
            btnClearFilter.TabIndex = 4;
            btnClearFilter.UseVisualStyleBackColor = false;
            // 
            // btnToday
            // 
            btnToday.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnToday.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnToday.BackColor = Color.FromArgb(99, 102, 241);
            btnToday.FlatAppearance.BorderSize = 0;
            btnToday.FlatStyle = FlatStyle.Flat;
            btnToday.Font = new Font("Arial", 10.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnToday.ForeColor = Color.White;
            btnToday.Location = new Point(1318, 8);
            btnToday.Name = "btnToday";
            btnToday.Size = new Size(76, 37);
            btnToday.TabIndex = 3;
            btnToday.Text = "Today";
            btnToday.UseVisualStyleBackColor = false;
            // 
            // lblDateRange
            // 
            lblDateRange.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblDateRange.BackColor = Color.White;
            lblDateRange.Cursor = Cursors.Hand;
            lblDateRange.Font = new Font("Segoe UI", 9F);
            lblDateRange.ForeColor = Color.FromArgb(55, 65, 81);
            lblDateRange.Location = new Point(860, 8);
            lblDateRange.Name = "lblDateRange";
            lblDateRange.Padding = new Padding(12, 7, 30, 7);
            lblDateRange.Size = new Size(240, 37);
            lblDateRange.TabIndex = 9;
            lblDateRange.Text = "📅  Wed, Sep 25, 2025 - Thu, Oct 2, 2025";
            lblDateRange.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // dtpStartDate
            // 
            dtpStartDate.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            dtpStartDate.Format = DateTimePickerFormat.Short;
            dtpStartDate.Location = new Point(903, 16);
            dtpStartDate.Name = "dtpStartDate";
            dtpStartDate.Size = new Size(15, 27);
            dtpStartDate.TabIndex = 10;
            dtpStartDate.Visible = false;
            // 
            // dtpEndDate
            // 
            dtpEndDate.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            dtpEndDate.Format = DateTimePickerFormat.Short;
            dtpEndDate.Location = new Point(1013, 16);
            dtpEndDate.Name = "dtpEndDate";
            dtpEndDate.Size = new Size(15, 27);
            dtpEndDate.TabIndex = 11;
            dtpEndDate.Visible = false;
            // 
            // InvoicesDataGridView
            // 
            InvoicesDataGridView.AllowUserToAddRows = false;
            InvoicesDataGridView.AllowUserToDeleteRows = false;
            InvoicesDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            InvoicesDataGridView.BackgroundColor = Color.White;
            InvoicesDataGridView.BorderStyle = BorderStyle.None;
            InvoicesDataGridView.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            InvoicesDataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            InvoicesDataGridView.ColumnHeadersHeight = 40;
            InvoicesDataGridView.Dock = DockStyle.Fill;
            InvoicesDataGridView.EnableHeadersVisualStyles = false;
            InvoicesDataGridView.GridColor = Color.FromArgb(240, 240, 240);
            InvoicesDataGridView.Location = new Point(8, 55);
            InvoicesDataGridView.Margin = new Padding(3, 4, 3, 4);
            InvoicesDataGridView.Name = "InvoicesDataGridView";
            InvoicesDataGridView.ReadOnly = true;
            InvoicesDataGridView.RowHeadersVisible = false;
            InvoicesDataGridView.RowHeadersWidth = 51;
            InvoicesDataGridView.RowTemplate.Height = 35;
            InvoicesDataGridView.Size = new Size(1390, 290);
            InvoicesDataGridView.TabIndex = 0;
            // 
            // labelInvoicesTitle
            // 
            labelInvoicesTitle.AutoSize = true;
            labelInvoicesTitle.Dock = DockStyle.Top;
            labelInvoicesTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            labelInvoicesTitle.ForeColor = Color.FromArgb(30, 30, 30);
            labelInvoicesTitle.Location = new Point(8, 8);
            labelInvoicesTitle.Name = "labelInvoicesTitle";
            labelInvoicesTitle.Padding = new Padding(6, 5, 0, 5);
            labelInvoicesTitle.Size = new Size(301, 47);
            labelInvoicesTitle.TabIndex = 1;
            labelInvoicesTitle.Text = "\U0001f9fe INVOICES LISTING";
            labelInvoicesTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // panelLogs
            // 
            panelLogs.BackColor = Color.White;
            panelLogs.Controls.Add(btnExportLogs);
            panelLogs.Controls.Add(LogsDataGridView);
            panelLogs.Controls.Add(labelLogsTitle);
            panelLogs.Dock = DockStyle.Fill;
            panelLogs.Location = new Point(16, 379);
            panelLogs.Margin = new Padding(6, 5, 6, 5);
            panelLogs.Name = "panelLogs";
            panelLogs.Padding = new Padding(8);
            panelLogs.Size = new Size(1406, 281);
            panelLogs.TabIndex = 1;
            // 
            // btnExportLogs
            // 
            btnExportLogs.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnExportLogs.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnExportLogs.BackColor = Color.SeaGreen;
            btnExportLogs.FlatAppearance.BorderSize = 0;
            btnExportLogs.FlatStyle = FlatStyle.Flat;
            btnExportLogs.Font = new Font("Arial", 12F);
            btnExportLogs.ForeColor = Color.White;
            btnExportLogs.Location = new Point(1228, 8);
            btnExportLogs.Name = "btnExportLogs";
            btnExportLogs.Size = new Size(167, 39);
            btnExportLogs.TabIndex = 9;
            btnExportLogs.Text = "📄 Export Logs";
            btnExportLogs.UseVisualStyleBackColor = false;
            // 
            // LogsDataGridView
            // 
            LogsDataGridView.AllowUserToAddRows = false;
            LogsDataGridView.AllowUserToDeleteRows = false;
            LogsDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            LogsDataGridView.Dock = DockStyle.Fill;
            LogsDataGridView.Location = new Point(8, 55);
            LogsDataGridView.Margin = new Padding(3, 4, 3, 4);
            LogsDataGridView.Name = "LogsDataGridView";
            LogsDataGridView.ReadOnly = true;
            LogsDataGridView.RowHeadersVisible = false;
            LogsDataGridView.RowHeadersWidth = 51;
            LogsDataGridView.Size = new Size(1390, 218);
            LogsDataGridView.TabIndex = 0;
            // 
            // labelLogsTitle
            // 
            labelLogsTitle.AutoSize = true;
            labelLogsTitle.Dock = DockStyle.Top;
            labelLogsTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            labelLogsTitle.ForeColor = Color.FromArgb(30, 30, 30);
            labelLogsTitle.Location = new Point(8, 8);
            labelLogsTitle.Name = "labelLogsTitle";
            labelLogsTitle.Padding = new Padding(6, 5, 0, 5);
            labelLogsTitle.Size = new Size(135, 47);
            labelLogsTitle.TabIndex = 1;
            labelLogsTitle.Text = "📝 LOGS";
            // 
            // panelinvoicechart
            // 
            panelinvoicechart.BackColor = Color.White;
            panelinvoicechart.Dock = DockStyle.Fill;
            panelinvoicechart.Location = new Point(1434, 16);
            panelinvoicechart.Margin = new Padding(6, 5, 6, 5);
            panelinvoicechart.Name = "panelinvoicechart";
            panelinvoicechart.Padding = new Padding(8);
            panelinvoicechart.Size = new Size(331, 353);
            panelinvoicechart.TabIndex = 2;
            // 
            // panellogchart
            // 
            panellogchart.BackColor = Color.White;
            panellogchart.Controls.Add(tableLayoutPanelLogStats);
            panellogchart.Dock = DockStyle.Fill;
            panellogchart.Location = new Point(1434, 379);
            panellogchart.Margin = new Padding(6, 5, 6, 5);
            panellogchart.Name = "panellogchart";
            panellogchart.Padding = new Padding(8);
            panellogchart.Size = new Size(331, 281);
            panellogchart.TabIndex = 3;
            // 
            // tableLayoutPanelLogStats
            // 
            tableLayoutPanelLogStats.ColumnCount = 2;
            tableLayoutPanelLogStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanelLogStats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanelLogStats.Controls.Add(panelTotalLogs, 0, 0);
            tableLayoutPanelLogStats.Controls.Add(panelErrorLogs, 1, 0);
            tableLayoutPanelLogStats.Controls.Add(panelWarningLogs, 0, 1);
            tableLayoutPanelLogStats.Controls.Add(panelInfoLogs, 1, 1);
            tableLayoutPanelLogStats.Dock = DockStyle.Fill;
            tableLayoutPanelLogStats.Location = new Point(8, 8);
            tableLayoutPanelLogStats.Margin = new Padding(0);
            tableLayoutPanelLogStats.Name = "tableLayoutPanelLogStats";
            tableLayoutPanelLogStats.RowCount = 2;
            tableLayoutPanelLogStats.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanelLogStats.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanelLogStats.Size = new Size(315, 265);
            tableLayoutPanelLogStats.TabIndex = 0;
            // 
            // panelTotalLogs
            // 
            panelTotalLogs.BackColor = Color.FromArgb(240, 248, 255);
            panelTotalLogs.BorderStyle = BorderStyle.FixedSingle;
            panelTotalLogs.Controls.Add(lblTotalLogsTitle);
            panelTotalLogs.Controls.Add(lblTotalLogsCount);
            panelTotalLogs.Dock = DockStyle.Fill;
            panelTotalLogs.Location = new Point(5, 5);
            panelTotalLogs.Margin = new Padding(5);
            panelTotalLogs.Name = "panelTotalLogs";
            panelTotalLogs.Padding = new Padding(8);
            panelTotalLogs.Size = new Size(147, 122);
            panelTotalLogs.TabIndex = 0;
            // 
            // lblTotalLogsTitle
            // 
            lblTotalLogsTitle.Dock = DockStyle.Bottom;
            lblTotalLogsTitle.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblTotalLogsTitle.ForeColor = Color.FromArgb(55, 65, 81);
            lblTotalLogsTitle.Location = new Point(8, 77);
            lblTotalLogsTitle.Name = "lblTotalLogsTitle";
            lblTotalLogsTitle.Size = new Size(129, 35);
            lblTotalLogsTitle.TabIndex = 1;
            lblTotalLogsTitle.Text = "Success Logs";
            lblTotalLogsTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblTotalLogsCount
            // 
            lblTotalLogsCount.AutoSize = true;
            lblTotalLogsCount.Dock = DockStyle.Top;
            lblTotalLogsCount.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTotalLogsCount.ForeColor = Color.FromArgb(59, 130, 246);
            lblTotalLogsCount.Location = new Point(8, 8);
            lblTotalLogsCount.Name = "lblTotalLogsCount";
            lblTotalLogsCount.Size = new Size(35, 41);
            lblTotalLogsCount.TabIndex = 0;
            lblTotalLogsCount.Text = "0";
            lblTotalLogsCount.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // panelErrorLogs
            // 
            panelErrorLogs.BackColor = Color.FromArgb(255, 241, 242);
            panelErrorLogs.BorderStyle = BorderStyle.FixedSingle;
            panelErrorLogs.Controls.Add(lblErrorLogsTitle);
            panelErrorLogs.Controls.Add(lblErrorLogsCount);
            panelErrorLogs.Dock = DockStyle.Fill;
            panelErrorLogs.Location = new Point(162, 5);
            panelErrorLogs.Margin = new Padding(5);
            panelErrorLogs.Name = "panelErrorLogs";
            panelErrorLogs.Padding = new Padding(8);
            panelErrorLogs.Size = new Size(148, 122);
            panelErrorLogs.TabIndex = 1;
            // 
            // lblErrorLogsTitle
            // 
            lblErrorLogsTitle.Dock = DockStyle.Bottom;
            lblErrorLogsTitle.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblErrorLogsTitle.ForeColor = Color.FromArgb(55, 65, 81);
            lblErrorLogsTitle.Location = new Point(8, 77);
            lblErrorLogsTitle.Name = "lblErrorLogsTitle";
            lblErrorLogsTitle.Size = new Size(130, 35);
            lblErrorLogsTitle.TabIndex = 1;
            lblErrorLogsTitle.Text = "Error Logs";
            lblErrorLogsTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblErrorLogsCount
            // 
            lblErrorLogsCount.AutoSize = true;
            lblErrorLogsCount.Dock = DockStyle.Top;
            lblErrorLogsCount.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblErrorLogsCount.ForeColor = Color.FromArgb(239, 68, 68);
            lblErrorLogsCount.Location = new Point(8, 8);
            lblErrorLogsCount.Name = "lblErrorLogsCount";
            lblErrorLogsCount.Size = new Size(35, 41);
            lblErrorLogsCount.TabIndex = 0;
            lblErrorLogsCount.Text = "0";
            lblErrorLogsCount.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // panelWarningLogs
            // 
            panelWarningLogs.BackColor = Color.FromArgb(255, 250, 235);
            panelWarningLogs.BorderStyle = BorderStyle.FixedSingle;
            panelWarningLogs.Controls.Add(lblWarningLogsTitle);
            panelWarningLogs.Controls.Add(lblWarningLogsCount);
            panelWarningLogs.Dock = DockStyle.Fill;
            panelWarningLogs.Location = new Point(5, 137);
            panelWarningLogs.Margin = new Padding(5);
            panelWarningLogs.Name = "panelWarningLogs";
            panelWarningLogs.Padding = new Padding(8);
            panelWarningLogs.Size = new Size(147, 123);
            panelWarningLogs.TabIndex = 2;
            // 
            // lblWarningLogsTitle
            // 
            lblWarningLogsTitle.Dock = DockStyle.Bottom;
            lblWarningLogsTitle.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblWarningLogsTitle.ForeColor = Color.FromArgb(55, 65, 81);
            lblWarningLogsTitle.Location = new Point(8, 78);
            lblWarningLogsTitle.Name = "lblWarningLogsTitle";
            lblWarningLogsTitle.Size = new Size(129, 35);
            lblWarningLogsTitle.TabIndex = 1;
            lblWarningLogsTitle.Text = "Warning Logs";
            lblWarningLogsTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblWarningLogsCount
            // 
            lblWarningLogsCount.AutoSize = true;
            lblWarningLogsCount.Dock = DockStyle.Top;
            lblWarningLogsCount.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblWarningLogsCount.ForeColor = Color.FromArgb(245, 158, 11);
            lblWarningLogsCount.Location = new Point(8, 8);
            lblWarningLogsCount.Name = "lblWarningLogsCount";
            lblWarningLogsCount.Size = new Size(35, 41);
            lblWarningLogsCount.TabIndex = 0;
            lblWarningLogsCount.Text = "0";
            lblWarningLogsCount.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // panelInfoLogs
            // 
            panelInfoLogs.BackColor = Color.FromArgb(236, 253, 245);
            panelInfoLogs.BorderStyle = BorderStyle.FixedSingle;
            panelInfoLogs.Controls.Add(lblInfoLogsTitle);
            panelInfoLogs.Controls.Add(lblInfoLogsCount);
            panelInfoLogs.Dock = DockStyle.Fill;
            panelInfoLogs.Location = new Point(162, 137);
            panelInfoLogs.Margin = new Padding(5);
            panelInfoLogs.Name = "panelInfoLogs";
            panelInfoLogs.Padding = new Padding(8);
            panelInfoLogs.Size = new Size(148, 123);
            panelInfoLogs.TabIndex = 3;
            // 
            // lblInfoLogsTitle
            // 
            lblInfoLogsTitle.Dock = DockStyle.Bottom;
            lblInfoLogsTitle.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblInfoLogsTitle.ForeColor = Color.FromArgb(55, 65, 81);
            lblInfoLogsTitle.Location = new Point(8, 78);
            lblInfoLogsTitle.Name = "lblInfoLogsTitle";
            lblInfoLogsTitle.Size = new Size(130, 35);
            lblInfoLogsTitle.TabIndex = 1;
            lblInfoLogsTitle.Text = "Info Logs";
            lblInfoLogsTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblInfoLogsCount
            // 
            lblInfoLogsCount.AutoSize = true;
            lblInfoLogsCount.Dock = DockStyle.Top;
            lblInfoLogsCount.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblInfoLogsCount.ForeColor = Color.FromArgb(16, 185, 129);
            lblInfoLogsCount.Location = new Point(8, 8);
            lblInfoLogsCount.Name = "lblInfoLogsCount";
            lblInfoLogsCount.Size = new Size(35, 41);
            lblInfoLogsCount.TabIndex = 0;
            lblInfoLogsCount.Text = "0";
            lblInfoLogsCount.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // colLogID
            // 
            colLogID.MinimumWidth = 6;
            colLogID.Name = "colLogID";
            colLogID.Width = 125;
            // 
            // colMessage
            // 
            colMessage.MinimumWidth = 6;
            colMessage.Name = "colMessage";
            colMessage.Width = 125;
            // 
            // colException
            // 
            colException.MinimumWidth = 6;
            colException.Name = "colException";
            colException.Width = 125;
            // 
            // logdatetime
            // 
            logdatetime.MinimumWidth = 6;
            logdatetime.Name = "logdatetime";
            logdatetime.Width = 125;
            // 
            // DashboardForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1781, 831);
            Controls.Add(tableLayoutPanelMain);
            Controls.Add(tableLayoutPanelTop);
            Name = "DashboardForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Dashboard";
            tableLayoutPanelTop.ResumeLayout(false);
            panelAll.ResumeLayout(false);
            panelAll.PerformLayout();
            panelPending.ResumeLayout(false);
            panelPending.PerformLayout();
            panelPaid.ResumeLayout(false);
            panelPaid.PerformLayout();
            tableLayoutPanelMain.ResumeLayout(false);
            panelInvoices.ResumeLayout(false);
            panelInvoices.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)InvoicesDataGridView).EndInit();
            panelLogs.ResumeLayout(false);
            panelLogs.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)LogsDataGridView).EndInit();
            panellogchart.ResumeLayout(false);
            tableLayoutPanelLogStats.ResumeLayout(false);
            panelTotalLogs.ResumeLayout(false);
            panelTotalLogs.PerformLayout();
            panelErrorLogs.ResumeLayout(false);
            panelErrorLogs.PerformLayout();
            panelWarningLogs.ResumeLayout(false);
            panelWarningLogs.PerformLayout();
            panelInfoLogs.ResumeLayout(false);
            panelInfoLogs.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnClearFilter;
        private Button btnRefresh;
        private Button btnFilterSynced;
        private ProgressBar progressBar;
        private Button btnExportLogs;
        private DataGridViewTextBoxColumn colId;
        private DataGridViewTextBoxColumn colPosId;
        private DataGridViewTextBoxColumn colInvoiceNumber;
        private DataGridViewTextBoxColumn colIsSynced;
        private DataGridViewTextBoxColumn colAttemptCount;
        private DataGridViewTextBoxColumn colDateCreated;
        private DataGridViewTextBoxColumn colLogID;
        private DataGridViewTextBoxColumn colMessage;
        private DataGridViewTextBoxColumn colException;
        private DataGridViewTextBoxColumn logdatetime;
        private Button btnFilter;
        private Panel panelTotalLogs;
        private Label lblTotalLogsTitle;
        private Label lblTotalLogsCount;
        private Panel panelErrorLogs;
        private Label lblErrorLogsTitle;
        private Label lblErrorLogsCount;
        private Panel panelWarningLogs;
        private Label lblWarningLogsTitle;
        private Label lblWarningLogsCount;
        private Panel panelInfoLogs;
        private Label lblInfoLogsTitle;
        private Label lblInfoLogsCount;
        private Button btnExportInvoice;
    }
}