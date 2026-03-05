using System.Drawing;
using System.Windows.Forms;
namespace Pos.WinFormsUI.Dashboard
{

    partial class DashboardFormNew
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
        private System.Windows.Forms.Label labelInvoicesTitle;
        private System.Windows.Forms.DataGridView InvoicesDataGridView;

        private System.Windows.Forms.Panel panelLogs;

        // Invoices panel
        private System.Windows.Forms.DateTimePicker dtpStartDate;
        private System.Windows.Forms.DateTimePicker dtpEndDate;
        private System.Windows.Forms.Label lblDateRange;


        private System.Windows.Forms.Button btnToday;

        #endregion

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
            labelLogs = new Label();
            lblPaidTitle = new Label();
            labelPaidInvoices = new Label();
            btnExportLogs = new Button();
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
            tableLayoutPanelLog = new TableLayoutPanel();
            panelLogBtn = new Panel();
            btnRow = new TableLayoutPanel();
            panelExportLog = new Panel();
            panelSyncLog = new Panel();
            btnSyncLogs = new Button();
            panel1 = new Panel();
            PaginationPanel = new Panel();
            panelLogGridBox = new Panel();
            LogsDataGridView = new DataGridView();
            panel3 = new Panel();
            panel2 = new Panel();
            lblHeartbeat1 = new Label();
            lblLastSync1 = new Label();
            tableLayoutPanel2 = new TableLayoutPanel();
            panellogchart = new Panel();
            panelTotalLogs = new Panel();
            lblTotalLogsCount = new Label();
            lblTotalLogsTitle = new Label();
            panelWarningLogs = new Panel();
            lblWarningLogsTitle = new Label();
            lblWarningLogsCount = new Label();
            panelErrorLogs = new Panel();
            lblErrorLogsTitle = new Label();
            lblErrorLogsCount = new Label();
            panelInfoLogs = new Panel();
            lblInfoLogsTitle = new Label();
            lblInfoLogsCount = new Label();
            labelLogsTitle = new Label();
            lblLastSync = new Label();
            lblHeartbeat = new Label();
            tlpBtns = new TableLayoutPanel();
            tlpLogBtnInner = new TableLayoutPanel();
            tableLayoutPanelTop.SuspendLayout();
            panelAll.SuspendLayout();
            panelPending.SuspendLayout();
            panelPaid.SuspendLayout();
            tableLayoutPanelMain.SuspendLayout();
            panelInvoices.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)InvoicesDataGridView).BeginInit();
            panelLogs.SuspendLayout();
            tableLayoutPanelLog.SuspendLayout();
            panelLogBtn.SuspendLayout();
            btnRow.SuspendLayout();
            panelExportLog.SuspendLayout();
            panelSyncLog.SuspendLayout();
            panel1.SuspendLayout();
            panelLogGridBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)LogsDataGridView).BeginInit();
            panel3.SuspendLayout();
            panel2.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            panellogchart.SuspendLayout();
            panelTotalLogs.SuspendLayout();
            panelWarningLogs.SuspendLayout();
            panelErrorLogs.SuspendLayout();
            panelInfoLogs.SuspendLayout();
            tlpLogBtnInner.SuspendLayout();
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
            tableLayoutPanelTop.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 230F));
            tableLayoutPanelTop.Controls.Add(panelAll, 0, 0);
            tableLayoutPanelTop.Controls.Add(panelPending, 1, 0);
            tableLayoutPanelTop.Controls.Add(panelPaid, 2, 0);
            tableLayoutPanelTop.Dock = DockStyle.Top;
            tableLayoutPanelTop.Location = new Point(0, 0);
            tableLayoutPanelTop.Margin = new Padding(0);
            tableLayoutPanelTop.Name = "tableLayoutPanelTop";
            tableLayoutPanelTop.Padding = new Padding(8, 4, 8, 2);
            tableLayoutPanelTop.RowCount = 1;
            tableLayoutPanelTop.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanelTop.Size = new Size(1145, 106);
            tableLayoutPanelTop.TabIndex = 1;
            // 
            // panelAll
            // 
            panelAll.BackColor = Color.White;
            panelAll.Controls.Add(lblAllTitle);
            panelAll.Controls.Add(labelAllInvoices);
            panelAll.Dock = DockStyle.Fill;
            panelAll.Location = new Point(15, 10);
            panelAll.Margin = new Padding(7, 6, 7, 6);
            panelAll.Name = "panelAll";
            panelAll.Padding = new Padding(10, 9, 10, 9);
            panelAll.Size = new Size(362, 88);
            panelAll.TabIndex = 0;
            // 
            // lblAllTitle
            // 
            lblAllTitle.BackColor = Color.Transparent;
            lblAllTitle.Dock = DockStyle.Bottom;
            lblAllTitle.Font = new Font("Microsoft Sans Serif", 10.8F);
            lblAllTitle.ForeColor = Color.Black;
            lblAllTitle.Location = new Point(10, 61);
            lblAllTitle.Name = "lblAllTitle";
            lblAllTitle.Size = new Size(342, 18);
            lblAllTitle.TabIndex = 1;
            lblAllTitle.Text = "All Invoices";
            // 
            // labelAllInvoices
            // 
            labelAllInvoices.Dock = DockStyle.Top;
            labelAllInvoices.Font = new Font("Microsoft Sans Serif", 25.8000011F, FontStyle.Bold);
            labelAllInvoices.ForeColor = Color.White;
            labelAllInvoices.Location = new Point(10, 9);
            labelAllInvoices.Margin = new Padding(3, 0, 3, 2);
            labelAllInvoices.Name = "labelAllInvoices";
            labelAllInvoices.Size = new Size(342, 22);
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
            panelPending.Location = new Point(391, 10);
            panelPending.Margin = new Padding(7, 6, 7, 6);
            panelPending.Name = "panelPending";
            panelPending.Padding = new Padding(10, 9, 10, 9);
            panelPending.Size = new Size(362, 88);
            panelPending.TabIndex = 1;
            // 
            // lblPendingTitle
            // 
            lblPendingTitle.BackColor = Color.Transparent;
            lblPendingTitle.Dock = DockStyle.Bottom;
            lblPendingTitle.Font = new Font("Microsoft Sans Serif", 10.8F);
            lblPendingTitle.ForeColor = Color.Black;
            lblPendingTitle.Location = new Point(10, 61);
            lblPendingTitle.Name = "lblPendingTitle";
            lblPendingTitle.Size = new Size(342, 18);
            lblPendingTitle.TabIndex = 1;
            lblPendingTitle.Text = "Not Synced";
            // 
            // labelPendingInvoice
            // 
            labelPendingInvoice.Dock = DockStyle.Top;
            labelPendingInvoice.Font = new Font("Microsoft Sans Serif", 25.8000011F, FontStyle.Bold);
            labelPendingInvoice.ForeColor = Color.White;
            labelPendingInvoice.Location = new Point(10, 9);
            labelPendingInvoice.Margin = new Padding(3, 0, 3, 2);
            labelPendingInvoice.Name = "labelPendingInvoice";
            labelPendingInvoice.Size = new Size(342, 22);
            labelPendingInvoice.TabIndex = 0;
            labelPendingInvoice.Text = "0";
            labelPendingInvoice.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // panelPaid
            // 
            panelPaid.BackColor = Color.White;
            panelPaid.Controls.Add(labelLogs);
            panelPaid.Controls.Add(lblPaidTitle);
            panelPaid.Controls.Add(labelPaidInvoices);
            panelPaid.Dock = DockStyle.Fill;
            panelPaid.Location = new Point(767, 10);
            panelPaid.Margin = new Padding(7, 6, 7, 6);
            panelPaid.Name = "panelPaid";
            panelPaid.Padding = new Padding(10, 9, 10, 9);
            panelPaid.Size = new Size(363, 88);
            panelPaid.TabIndex = 2;
            // 
            // labelLogs
            // 
            labelLogs.Font = new Font("Segoe UI", 13.8F);
            labelLogs.ForeColor = Color.FromArgb(85, 85, 85);
            labelLogs.Location = new Point(160, 51);
            labelLogs.Margin = new Padding(0);
            labelLogs.Name = "labelLogs";
            labelLogs.Padding = new Padding(0, 0, 0, 1);
            labelLogs.Size = new Size(101, 28);
            labelLogs.TabIndex = 38;
            labelLogs.Text = "📝 LOGS";
            labelLogs.TextAlign = ContentAlignment.MiddleLeft;
            labelLogs.Visible = false;
            // 
            // lblPaidTitle
            // 
            lblPaidTitle.BackColor = Color.Transparent;
            lblPaidTitle.Dock = DockStyle.Bottom;
            lblPaidTitle.Font = new Font("Microsoft Sans Serif", 10.8F);
            lblPaidTitle.ForeColor = Color.Black;
            lblPaidTitle.Location = new Point(10, 61);
            lblPaidTitle.Name = "lblPaidTitle";
            lblPaidTitle.Size = new Size(343, 18);
            lblPaidTitle.TabIndex = 1;
            lblPaidTitle.Text = "Synced Invoices";
            // 
            // labelPaidInvoices
            // 
            labelPaidInvoices.Dock = DockStyle.Top;
            labelPaidInvoices.Font = new Font("Microsoft Sans Serif", 25.8000011F, FontStyle.Bold);
            labelPaidInvoices.ForeColor = Color.White;
            labelPaidInvoices.Location = new Point(10, 9);
            labelPaidInvoices.Margin = new Padding(3, 0, 3, 2);
            labelPaidInvoices.Name = "labelPaidInvoices";
            labelPaidInvoices.Size = new Size(343, 22);
            labelPaidInvoices.TabIndex = 0;
            labelPaidInvoices.Text = "0";
            labelPaidInvoices.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // btnExportLogs
            // 
            btnExportLogs.AutoSize = true;
            btnExportLogs.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnExportLogs.BackColor = Color.SeaGreen;
            btnExportLogs.Dock = DockStyle.Fill;
            btnExportLogs.FlatAppearance.BorderSize = 0;
            btnExportLogs.FlatStyle = FlatStyle.Flat;
            btnExportLogs.Font = new Font("Arial", 12F);
            btnExportLogs.ForeColor = Color.White;
            btnExportLogs.Location = new Point(0, 0);
            btnExportLogs.Margin = new Padding(3, 2, 3, 2);
            btnExportLogs.Name = "btnExportLogs";
            btnExportLogs.Size = new Size(83, 28);
            btnExportLogs.TabIndex = 40;
            btnExportLogs.Text = "📄 Export";
            btnExportLogs.UseVisualStyleBackColor = false;
            // 
            // tableLayoutPanelMain
            // 
            tableLayoutPanelMain.AutoSize = true;
            tableLayoutPanelMain.ColumnCount = 2;
            tableLayoutPanelMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 66.5632248F));
            tableLayoutPanelMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.43677F));
            tableLayoutPanelMain.Controls.Add(panelInvoices, 0, 0);
            tableLayoutPanelMain.Controls.Add(panelLogs, 1, 0);
            tableLayoutPanelMain.Dock = DockStyle.Fill;
            tableLayoutPanelMain.Location = new Point(0, 106);
            tableLayoutPanelMain.Margin = new Padding(0);
            tableLayoutPanelMain.Name = "tableLayoutPanelMain";
            tableLayoutPanelMain.Padding = new Padding(8, 1, 8, 4);
            tableLayoutPanelMain.RowCount = 1;
            tableLayoutPanelMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanelMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 427F));
            tableLayoutPanelMain.Size = new Size(1145, 432);
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
            panelInvoices.Location = new Point(14, 5);
            panelInvoices.Margin = new Padding(6, 4, 6, 4);
            panelInvoices.Name = "panelInvoices";
            panelInvoices.Padding = new Padding(7, 6, 7, 6);
            panelInvoices.Size = new Size(739, 419);
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
            btnExportInvoice.Location = new Point(92, 5);
            btnExportInvoice.Margin = new Padding(3, 2, 3, 2);
            btnExportInvoice.Name = "btnExportInvoice";
            btnExportInvoice.Size = new Size(90, 28);
            btnExportInvoice.TabIndex = 10;
            btnExportInvoice.Text = "📄 Export";
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
            btnFilter.Location = new Point(474, 5);
            btnFilter.Margin = new Padding(3, 2, 3, 2);
            btnFilter.Name = "btnFilter";
            btnFilter.Size = new Size(56, 28);
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
            btnRefresh.Location = new Point(589, 5);
            btnRefresh.Margin = new Padding(3, 2, 3, 2);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(56, 28);
            btnRefresh.TabIndex = 0;
            btnRefresh.UseVisualStyleBackColor = false;
            // 
            // progressBar
            // 
            progressBar.Anchor = AnchorStyles.None;
            progressBar.ForeColor = Color.SeaGreen;
            progressBar.Location = new Point(-141, 33);
            progressBar.Margin = new Padding(3, 2, 3, 2);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(350, 19);
            progressBar.Step = 1;
            progressBar.Style = ProgressBarStyle.Continuous;
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
            btnFilterSynced.Font = new Font("Arial", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnFilterSynced.ForeColor = Color.White;
            btnFilterSynced.Location = new Point(185, 5);
            btnFilterSynced.Margin = new Padding(3, 2, 3, 2);
            btnFilterSynced.Name = "btnFilterSynced";
            btnFilterSynced.Size = new Size(76, 28);
            btnFilterSynced.TabIndex = 5;
            btnFilterSynced.Text = "Synced";
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
            btnClearFilter.Location = new Point(531, 5);
            btnClearFilter.Margin = new Padding(3, 2, 3, 2);
            btnClearFilter.Name = "btnClearFilter";
            btnClearFilter.Size = new Size(56, 28);
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
            btnToday.Location = new Point(649, 5);
            btnToday.Margin = new Padding(3, 2, 3, 2);
            btnToday.Name = "btnToday";
            btnToday.Size = new Size(67, 28);
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
            lblDateRange.Location = new Point(261, 5);
            lblDateRange.Name = "lblDateRange";
            lblDateRange.Padding = new Padding(10, 5, 27, 5);
            lblDateRange.Size = new Size(207, 28);
            lblDateRange.TabIndex = 9;
            lblDateRange.Text = "📅  Wed, Sep 25, 2025 - Thu, Oct 2, 2025";
            lblDateRange.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // dtpStartDate
            // 
            dtpStartDate.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            dtpStartDate.Format = DateTimePickerFormat.Short;
            dtpStartDate.Location = new Point(298, 12);
            dtpStartDate.Margin = new Padding(3, 2, 3, 2);
            dtpStartDate.Name = "dtpStartDate";
            dtpStartDate.Size = new Size(14, 23);
            dtpStartDate.TabIndex = 10;
            dtpStartDate.Visible = false;
            // 
            // dtpEndDate
            // 
            dtpEndDate.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            dtpEndDate.Format = DateTimePickerFormat.Short;
            dtpEndDate.Location = new Point(394, 12);
            dtpEndDate.Margin = new Padding(3, 2, 3, 2);
            dtpEndDate.Name = "dtpEndDate";
            dtpEndDate.Size = new Size(14, 23);
            dtpEndDate.TabIndex = 11;
            dtpEndDate.Visible = false;
            // 
            // InvoicesDataGridView
            // 
            InvoicesDataGridView.AllowUserToAddRows = false;
            InvoicesDataGridView.AllowUserToDeleteRows = false;
            InvoicesDataGridView.BackgroundColor = Color.White;
            InvoicesDataGridView.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            InvoicesDataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            InvoicesDataGridView.ColumnHeadersHeight = 50;
            InvoicesDataGridView.Dock = DockStyle.Fill;
            InvoicesDataGridView.EnableHeadersVisualStyles = false;
            InvoicesDataGridView.GridColor = Color.FromArgb(240, 240, 240);
            InvoicesDataGridView.Location = new Point(7, 39);
            InvoicesDataGridView.Name = "InvoicesDataGridView";
            InvoicesDataGridView.ReadOnly = true;
            InvoicesDataGridView.RowHeadersVisible = false;
            InvoicesDataGridView.RowHeadersWidth = 51;
            InvoicesDataGridView.RowTemplate.Height = 40;
            InvoicesDataGridView.Size = new Size(725, 374);
            InvoicesDataGridView.TabIndex = 0;
            // 
            // labelInvoicesTitle
            // 
            labelInvoicesTitle.AutoSize = true;
            labelInvoicesTitle.Dock = DockStyle.Top;
            labelInvoicesTitle.Font = new Font("Segoe UI", 13.8F);
            labelInvoicesTitle.ForeColor = Color.FromArgb(85, 85, 85);
            labelInvoicesTitle.Location = new Point(7, 6);
            labelInvoicesTitle.Margin = new Padding(0);
            labelInvoicesTitle.Name = "labelInvoicesTitle";
            labelInvoicesTitle.Padding = new Padding(0, 4, 0, 4);
            labelInvoicesTitle.Size = new Size(186, 33);
            labelInvoicesTitle.TabIndex = 1;
            labelInvoicesTitle.Text = "INVOICES LISTING\U0001f9fe";
            labelInvoicesTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // panelLogs
            // 
            panelLogs.AutoSize = true;
            panelLogs.BackColor = Color.White;
            panelLogs.Controls.Add(tableLayoutPanelLog);
            panelLogs.Dock = DockStyle.Fill;
            panelLogs.Location = new Point(765, 5);
            panelLogs.Margin = new Padding(6, 4, 6, 4);
            panelLogs.Name = "panelLogs";
            panelLogs.Padding = new Padding(1);
            panelLogs.Size = new Size(366, 419);
            panelLogs.TabIndex = 1;
            // 
            // tableLayoutPanelLog
            // 
            tableLayoutPanelLog.ColumnCount = 1;
            tableLayoutPanelLog.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanelLog.Controls.Add(panelLogBtn, 0, 0);
            tableLayoutPanelLog.Controls.Add(panelLogGridBox, 0, 1);
            tableLayoutPanelLog.Controls.Add(panel3, 0, 2);
            tableLayoutPanelLog.Dock = DockStyle.Fill;
            tableLayoutPanelLog.Location = new Point(1, 1);
            tableLayoutPanelLog.Margin = new Padding(3, 2, 3, 2);
            tableLayoutPanelLog.Name = "tableLayoutPanelLog";
            tableLayoutPanelLog.RowCount = 3;
            tableLayoutPanelLog.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            tableLayoutPanelLog.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanelLog.RowStyles.Add(new RowStyle(SizeType.Absolute, 124F));
            tableLayoutPanelLog.Size = new Size(364, 417);
            tableLayoutPanelLog.TabIndex = 0;
            // 
            // panelLogBtn
            // 
            panelLogBtn.Controls.Add(btnRow);
            panelLogBtn.Dock = DockStyle.Fill;
            panelLogBtn.Location = new Point(3, 2);
            panelLogBtn.Margin = new Padding(3, 2, 3, 2);
            panelLogBtn.Name = "panelLogBtn";
            panelLogBtn.Padding = new Padding(1);
            panelLogBtn.Size = new Size(358, 30);
            panelLogBtn.TabIndex = 0;
            // 
            // btnRow
            // 
            btnRow.AutoSize = true;
            btnRow.ColumnCount = 3;
            btnRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            btnRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            btnRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            btnRow.Controls.Add(panelExportLog, 2, 0);
            btnRow.Controls.Add(panelSyncLog, 1, 0);
            btnRow.Controls.Add(panel1, 0, 0);
            btnRow.Dock = DockStyle.Top;
            btnRow.Location = new Point(1, 1);
            btnRow.Margin = new Padding(0);
            btnRow.Name = "btnRow";
            btnRow.RowCount = 1;
            btnRow.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            btnRow.Size = new Size(356, 32);
            btnRow.TabIndex = 0;
            // 
            // panelExportLog
            // 
            panelExportLog.Controls.Add(btnExportLogs);
            panelExportLog.Dock = DockStyle.Fill;
            panelExportLog.Location = new Point(270, 2);
            panelExportLog.Margin = new Padding(3, 2, 3, 2);
            panelExportLog.Name = "panelExportLog";
            panelExportLog.Size = new Size(83, 28);
            panelExportLog.TabIndex = 41;
            // 
            // panelSyncLog
            // 
            panelSyncLog.Controls.Add(btnSyncLogs);
            panelSyncLog.Dock = DockStyle.Fill;
            panelSyncLog.Location = new Point(181, 2);
            panelSyncLog.Margin = new Padding(3, 2, 3, 2);
            panelSyncLog.Name = "panelSyncLog";
            panelSyncLog.Size = new Size(83, 28);
            panelSyncLog.TabIndex = 42;
            // 
            // btnSyncLogs
            // 
            btnSyncLogs.AutoSize = true;
            btnSyncLogs.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnSyncLogs.BackColor = Color.SlateBlue;
            btnSyncLogs.Dock = DockStyle.Fill;
            btnSyncLogs.FlatAppearance.BorderSize = 0;
            btnSyncLogs.FlatStyle = FlatStyle.Flat;
            btnSyncLogs.Font = new Font("Arial", 12F);
            btnSyncLogs.ForeColor = Color.White;
            btnSyncLogs.Location = new Point(0, 0);
            btnSyncLogs.Margin = new Padding(3, 2, 3, 2);
            btnSyncLogs.Name = "btnSyncLogs";
            btnSyncLogs.Size = new Size(83, 28);
            btnSyncLogs.TabIndex = 39;
            btnSyncLogs.Text = "🔄 Sync";
            btnSyncLogs.UseVisualStyleBackColor = false;
            // 
            // panel1
            // 
            panel1.Controls.Add(PaginationPanel);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(0, 0);
            panel1.Margin = new Padding(0);
            panel1.Name = "panel1";
            panel1.Size = new Size(178, 32);
            panel1.TabIndex = 43;
            // 
            // PaginationPanel
            // 
            PaginationPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            PaginationPanel.Dock = DockStyle.Fill;
            PaginationPanel.Location = new Point(0, 0);
            PaginationPanel.Margin = new Padding(0);
            PaginationPanel.Name = "PaginationPanel";
            PaginationPanel.Size = new Size(178, 32);
            PaginationPanel.TabIndex = 39;
            // 
            // panelLogGridBox
            // 
            panelLogGridBox.Controls.Add(LogsDataGridView);
            panelLogGridBox.Dock = DockStyle.Fill;
            panelLogGridBox.Location = new Point(3, 36);
            panelLogGridBox.Margin = new Padding(3, 2, 3, 2);
            panelLogGridBox.Name = "panelLogGridBox";
            panelLogGridBox.Size = new Size(358, 255);
            panelLogGridBox.TabIndex = 1;
            // 
            // LogsDataGridView
            // 
            LogsDataGridView.AllowUserToAddRows = false;
            LogsDataGridView.AllowUserToDeleteRows = false;
            LogsDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            LogsDataGridView.Dock = DockStyle.Fill;
            LogsDataGridView.Location = new Point(0, 0);
            LogsDataGridView.Name = "LogsDataGridView";
            LogsDataGridView.ReadOnly = true;
            LogsDataGridView.RowHeadersVisible = false;
            LogsDataGridView.RowHeadersWidth = 31;
            LogsDataGridView.RowTemplate.Height = 40;
            LogsDataGridView.Size = new Size(358, 255);
            LogsDataGridView.TabIndex = 37;
            // 
            // panel3
            // 
            panel3.Controls.Add(panel2);
            panel3.Controls.Add(tableLayoutPanel2);
            panel3.Dock = DockStyle.Fill;
            panel3.Location = new Point(3, 295);
            panel3.Margin = new Padding(3, 2, 3, 2);
            panel3.Name = "panel3";
            panel3.Size = new Size(358, 120);
            panel3.TabIndex = 2;
            // 
            // panel2
            // 
            panel2.Controls.Add(lblHeartbeat1);
            panel2.Controls.Add(lblLastSync1);
            panel2.Dock = DockStyle.Bottom;
            panel2.Location = new Point(0, 96);
            panel2.Margin = new Padding(3, 2, 3, 2);
            panel2.Name = "panel2";
            panel2.Size = new Size(358, 24);
            panel2.TabIndex = 23;
            // 
            // lblHeartbeat1
            // 
            lblHeartbeat1.AutoSize = true;
            lblHeartbeat1.Font = new Font("Segoe UI", 8F);
            lblHeartbeat1.ForeColor = Color.FromArgb(85, 85, 85);
            lblHeartbeat1.Location = new Point(4, -2);
            lblHeartbeat1.Margin = new Padding(0);
            lblHeartbeat1.Name = "lblHeartbeat1";
            lblHeartbeat1.Size = new Size(188, 13);
            lblHeartbeat1.TabIndex = 36;
            lblHeartbeat1.Text = "Last Heartbeat: 14-25-45 12:12:!2am";
            lblHeartbeat1.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblLastSync1
            // 
            lblLastSync1.AutoSize = true;
            lblLastSync1.Font = new Font("Segoe UI", 8F);
            lblLastSync1.ForeColor = Color.Red;
            lblLastSync1.Location = new Point(4, 11);
            lblLastSync1.Margin = new Padding(0);
            lblLastSync1.Name = "lblLastSync1";
            lblLastSync1.Size = new Size(212, 13);
            lblLastSync1.TabIndex = 37;
            lblLastSync1.Text = "Last Synced Invoice: 14-25-45 12:12:!2am";
            lblLastSync1.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 2;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.Controls.Add(panellogchart, 0, 0);
            tableLayoutPanel2.Controls.Add(panelWarningLogs, 0, 1);
            tableLayoutPanel2.Controls.Add(panelErrorLogs, 1, 0);
            tableLayoutPanel2.Controls.Add(panelInfoLogs, 1, 1);
            tableLayoutPanel2.Dock = DockStyle.Top;
            tableLayoutPanel2.Location = new Point(0, 0);
            tableLayoutPanel2.Margin = new Padding(0);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 2;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.Size = new Size(358, 93);
            tableLayoutPanel2.TabIndex = 22;
            // 
            // panellogchart
            // 
            panellogchart.BackColor = Color.White;
            panellogchart.Controls.Add(panelTotalLogs);
            panellogchart.Dock = DockStyle.Fill;
            panellogchart.Location = new Point(4, 3);
            panellogchart.Margin = new Padding(4, 3, 4, 3);
            panellogchart.Name = "panellogchart";
            panellogchart.Size = new Size(171, 40);
            panellogchart.TabIndex = 4;
            // 
            // panelTotalLogs
            // 
            panelTotalLogs.BackColor = Color.FromArgb(89, 193, 137);
            panelTotalLogs.BorderStyle = BorderStyle.FixedSingle;
            panelTotalLogs.Controls.Add(lblTotalLogsCount);
            panelTotalLogs.Controls.Add(lblTotalLogsTitle);
            panelTotalLogs.Dock = DockStyle.Fill;
            panelTotalLogs.Location = new Point(0, 0);
            panelTotalLogs.Margin = new Padding(3, 2, 3, 2);
            panelTotalLogs.Name = "panelTotalLogs";
            panelTotalLogs.Padding = new Padding(4, 3, 4, 3);
            panelTotalLogs.Size = new Size(171, 40);
            panelTotalLogs.TabIndex = 1;
            // 
            // lblTotalLogsCount
            // 
            lblTotalLogsCount.AutoSize = true;
            lblTotalLogsCount.Dock = DockStyle.Left;
            lblTotalLogsCount.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTotalLogsCount.ForeColor = Color.White;
            lblTotalLogsCount.Location = new Point(4, 3);
            lblTotalLogsCount.Name = "lblTotalLogsCount";
            lblTotalLogsCount.Size = new Size(28, 32);
            lblTotalLogsCount.TabIndex = 2;
            lblTotalLogsCount.Text = "0";
            lblTotalLogsCount.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblTotalLogsTitle
            // 
            lblTotalLogsTitle.Dock = DockStyle.Right;
            lblTotalLogsTitle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            lblTotalLogsTitle.ForeColor = Color.White;
            lblTotalLogsTitle.Location = new Point(52, 3);
            lblTotalLogsTitle.Name = "lblTotalLogsTitle";
            lblTotalLogsTitle.Size = new Size(113, 32);
            lblTotalLogsTitle.TabIndex = 1;
            lblTotalLogsTitle.Text = "Success Logs";
            lblTotalLogsTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // panelWarningLogs
            // 
            panelWarningLogs.BackColor = Color.FromArgb(250, 165, 81);
            panelWarningLogs.BorderStyle = BorderStyle.FixedSingle;
            panelWarningLogs.Controls.Add(lblWarningLogsTitle);
            panelWarningLogs.Controls.Add(lblWarningLogsCount);
            panelWarningLogs.Dock = DockStyle.Fill;
            panelWarningLogs.Location = new Point(3, 48);
            panelWarningLogs.Margin = new Padding(3, 2, 3, 2);
            panelWarningLogs.Name = "panelWarningLogs";
            panelWarningLogs.Padding = new Padding(4, 3, 4, 3);
            panelWarningLogs.Size = new Size(173, 43);
            panelWarningLogs.TabIndex = 2;
            // 
            // lblWarningLogsTitle
            // 
            lblWarningLogsTitle.Dock = DockStyle.Right;
            lblWarningLogsTitle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            lblWarningLogsTitle.ForeColor = Color.White;
            lblWarningLogsTitle.Location = new Point(52, 3);
            lblWarningLogsTitle.Name = "lblWarningLogsTitle";
            lblWarningLogsTitle.Size = new Size(115, 35);
            lblWarningLogsTitle.TabIndex = 1;
            lblWarningLogsTitle.Text = "Warning Logs";
            lblWarningLogsTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblWarningLogsCount
            // 
            lblWarningLogsCount.AutoSize = true;
            lblWarningLogsCount.Dock = DockStyle.Left;
            lblWarningLogsCount.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblWarningLogsCount.ForeColor = Color.White;
            lblWarningLogsCount.Location = new Point(4, 3);
            lblWarningLogsCount.Name = "lblWarningLogsCount";
            lblWarningLogsCount.Size = new Size(28, 32);
            lblWarningLogsCount.TabIndex = 0;
            lblWarningLogsCount.Text = "0";
            lblWarningLogsCount.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // panelErrorLogs
            // 
            panelErrorLogs.BackColor = Color.FromArgb(229, 100, 91);
            panelErrorLogs.BorderStyle = BorderStyle.FixedSingle;
            panelErrorLogs.Controls.Add(lblErrorLogsTitle);
            panelErrorLogs.Controls.Add(lblErrorLogsCount);
            panelErrorLogs.Dock = DockStyle.Fill;
            panelErrorLogs.Location = new Point(182, 2);
            panelErrorLogs.Margin = new Padding(3, 2, 3, 2);
            panelErrorLogs.Name = "panelErrorLogs";
            panelErrorLogs.Padding = new Padding(4, 3, 4, 3);
            panelErrorLogs.Size = new Size(173, 42);
            panelErrorLogs.TabIndex = 1;
            // 
            // lblErrorLogsTitle
            // 
            lblErrorLogsTitle.Dock = DockStyle.Right;
            lblErrorLogsTitle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            lblErrorLogsTitle.ForeColor = Color.White;
            lblErrorLogsTitle.Location = new Point(49, 3);
            lblErrorLogsTitle.Name = "lblErrorLogsTitle";
            lblErrorLogsTitle.Size = new Size(118, 34);
            lblErrorLogsTitle.TabIndex = 1;
            lblErrorLogsTitle.Text = "Error Logs";
            lblErrorLogsTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblErrorLogsCount
            // 
            lblErrorLogsCount.AutoSize = true;
            lblErrorLogsCount.Dock = DockStyle.Left;
            lblErrorLogsCount.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblErrorLogsCount.ForeColor = Color.White;
            lblErrorLogsCount.Location = new Point(4, 3);
            lblErrorLogsCount.Name = "lblErrorLogsCount";
            lblErrorLogsCount.Size = new Size(28, 32);
            lblErrorLogsCount.TabIndex = 0;
            lblErrorLogsCount.Text = "0";
            lblErrorLogsCount.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // panelInfoLogs
            // 
            panelInfoLogs.BackColor = Color.FromArgb(97, 158, 223);
            panelInfoLogs.BorderStyle = BorderStyle.FixedSingle;
            panelInfoLogs.Controls.Add(lblInfoLogsTitle);
            panelInfoLogs.Controls.Add(lblInfoLogsCount);
            panelInfoLogs.Dock = DockStyle.Fill;
            panelInfoLogs.Location = new Point(183, 50);
            panelInfoLogs.Margin = new Padding(4);
            panelInfoLogs.Name = "panelInfoLogs";
            panelInfoLogs.Padding = new Padding(3, 3, 4, 3);
            panelInfoLogs.Size = new Size(171, 39);
            panelInfoLogs.TabIndex = 3;
            // 
            // lblInfoLogsTitle
            // 
            lblInfoLogsTitle.Dock = DockStyle.Right;
            lblInfoLogsTitle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            lblInfoLogsTitle.ForeColor = Color.White;
            lblInfoLogsTitle.Location = new Point(49, 3);
            lblInfoLogsTitle.Name = "lblInfoLogsTitle";
            lblInfoLogsTitle.Size = new Size(116, 31);
            lblInfoLogsTitle.TabIndex = 1;
            lblInfoLogsTitle.Text = "Info Logs";
            lblInfoLogsTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblInfoLogsCount
            // 
            lblInfoLogsCount.AutoSize = true;
            lblInfoLogsCount.Dock = DockStyle.Left;
            lblInfoLogsCount.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblInfoLogsCount.ForeColor = Color.White;
            lblInfoLogsCount.Location = new Point(3, 3);
            lblInfoLogsCount.Name = "lblInfoLogsCount";
            lblInfoLogsCount.Size = new Size(28, 32);
            lblInfoLogsCount.TabIndex = 0;
            lblInfoLogsCount.Text = "0";
            lblInfoLogsCount.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // labelLogsTitle
            // 
            labelLogsTitle.Dock = DockStyle.Fill;
            labelLogsTitle.Font = new Font("Segoe UI", 13.8F);
            labelLogsTitle.ForeColor = Color.FromArgb(85, 85, 85);
            labelLogsTitle.Location = new Point(0, 0);
            labelLogsTitle.Margin = new Padding(0, 0, 3, 0);
            labelLogsTitle.Name = "labelLogsTitle";
            labelLogsTitle.Padding = new Padding(0, 5, 0, 5);
            labelLogsTitle.Size = new Size(197, 38);
            labelLogsTitle.TabIndex = 43;
            labelLogsTitle.Text = "📝 LOGS";
            labelLogsTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblLastSync
            // 
            lblLastSync.Dock = DockStyle.Fill;
            lblLastSync.Font = new Font("Segoe UI", 7F);
            lblLastSync.Location = new Point(0, 38);
            lblLastSync.Margin = new Padding(0);
            lblLastSync.Name = "lblLastSync";
            lblLastSync.Padding = new Padding(2, 0, 0, 0);
            lblLastSync.Size = new Size(200, 16);
            lblLastSync.TabIndex = 40;
            lblLastSync.Text = "Last Synced Invoice: 14-25-45 12:12:!2am";
            lblLastSync.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblHeartbeat
            // 
            lblHeartbeat.Dock = DockStyle.Fill;
            lblHeartbeat.Font = new Font("Segoe UI", 7F);
            lblHeartbeat.ForeColor = Color.FromArgb(85, 85, 85);
            lblHeartbeat.Location = new Point(0, 54);
            lblHeartbeat.Margin = new Padding(0);
            lblHeartbeat.Name = "lblHeartbeat";
            lblHeartbeat.Padding = new Padding(2, 0, 0, 0);
            lblHeartbeat.Size = new Size(200, 16);
            lblHeartbeat.TabIndex = 39;
            lblHeartbeat.Text = "Last Heartbeat: 14-25-45 12:12:!2am";
            lblHeartbeat.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // tlpBtns
            // 
            tlpBtns.ColumnCount = 2;
            tlpBtns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpBtns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpBtns.Dock = DockStyle.Fill;
            tlpBtns.Location = new Point(0, 70);
            tlpBtns.Margin = new Padding(0);
            tlpBtns.Name = "tlpBtns";
            tlpBtns.RowCount = 1;
            tlpBtns.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpBtns.Size = new Size(200, 30);
            tlpBtns.TabIndex = 44;
            // 
            // tlpLogBtnInner
            // 
            tlpLogBtnInner.ColumnCount = 1;
            tlpLogBtnInner.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpLogBtnInner.Controls.Add(labelLogsTitle, 0, 0);
            tlpLogBtnInner.Controls.Add(lblLastSync, 0, 1);
            tlpLogBtnInner.Controls.Add(lblHeartbeat, 0, 2);
            tlpLogBtnInner.Controls.Add(tlpBtns, 0, 3);
            tlpLogBtnInner.Dock = DockStyle.Fill;
            tlpLogBtnInner.Location = new Point(0, 0);
            tlpLogBtnInner.Margin = new Padding(0);
            tlpLogBtnInner.Name = "tlpLogBtnInner";
            tlpLogBtnInner.RowCount = 4;
            tlpLogBtnInner.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            tlpLogBtnInner.RowStyles.Add(new RowStyle(SizeType.Absolute, 16F));
            tlpLogBtnInner.RowStyles.Add(new RowStyle(SizeType.Absolute, 16F));
            tlpLogBtnInner.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpLogBtnInner.Size = new Size(200, 100);
            tlpLogBtnInner.TabIndex = 0;
            // 
            // DashboardFormNew
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1145, 538);
            Controls.Add(tableLayoutPanelMain);
            Controls.Add(tableLayoutPanelTop);
            Margin = new Padding(3, 2, 3, 2);
            Name = "DashboardFormNew";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Dashboard";
            tableLayoutPanelTop.ResumeLayout(false);
            panelAll.ResumeLayout(false);
            panelPending.ResumeLayout(false);
            panelPaid.ResumeLayout(false);
            tableLayoutPanelMain.ResumeLayout(false);
            tableLayoutPanelMain.PerformLayout();
            panelInvoices.ResumeLayout(false);
            panelInvoices.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)InvoicesDataGridView).EndInit();
            panelLogs.ResumeLayout(false);
            tableLayoutPanelLog.ResumeLayout(false);
            panelLogBtn.ResumeLayout(false);
            panelLogBtn.PerformLayout();
            btnRow.ResumeLayout(false);
            panelExportLog.ResumeLayout(false);
            panelExportLog.PerformLayout();
            panelSyncLog.ResumeLayout(false);
            panelSyncLog.PerformLayout();
            panel1.ResumeLayout(false);
            panelLogGridBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)LogsDataGridView).EndInit();
            panel3.ResumeLayout(false);
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            panellogchart.ResumeLayout(false);
            panelTotalLogs.ResumeLayout(false);
            panelTotalLogs.PerformLayout();
            panelWarningLogs.ResumeLayout(false);
            panelWarningLogs.PerformLayout();
            panelErrorLogs.ResumeLayout(false);
            panelErrorLogs.PerformLayout();
            panelInfoLogs.ResumeLayout(false);
            panelInfoLogs.PerformLayout();
            tlpLogBtnInner.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private void AddButtonTooltips()
        {
            ToolTip tooltip = new ToolTip();
            tooltip.SetToolTip(btnClearFilter, "Clear Flter");
            tooltip.SetToolTip(btnRefresh, "Refresh");
            tooltip.SetToolTip(btnFilterSynced, "Filter Synced/ Unsynced");
            tooltip.SetToolTip(btnExportLogs, "Export Logs");
            tooltip.SetToolTip(btnFilter, "Filter");
            tooltip.SetToolTip(btnExportInvoice, "Export Invoices");
            tooltip.SetToolTip(btnSyncLogs, "Sync Logs");
            tooltip.SetToolTip(btnToday, "Filter Today");
            tooltip.SetToolTip(panelTotalLogs, "Filter Success Logs");
            tooltip.SetToolTip(lblTotalLogsCount, "Filter Success Logs");
            tooltip.SetToolTip(lblTotalLogsTitle, "Filter Success Logs");
            tooltip.SetToolTip(panelErrorLogs, "Filter Error Logs");
            tooltip.SetToolTip(lblErrorLogsCount, "Filter Error Logs");
            tooltip.SetToolTip(lblErrorLogsTitle, "Filter Error Logs");
            tooltip.SetToolTip(panelInfoLogs, "Filter Info Logs");
            tooltip.SetToolTip(lblInfoLogsCount, "Filter Info Logs");
            tooltip.SetToolTip(lblInfoLogsTitle, "Filter Info Logs");
            tooltip.SetToolTip(panelWarningLogs, "Filter Warning Logs");
            tooltip.SetToolTip(lblWarningLogsTitle, "Filter Warning Logs");
            tooltip.SetToolTip(lblWarningLogsCount, "Filter Warning Logs");
        }

        private Button btnClearFilter;
        private Button btnRefresh;
        private Button btnFilterSynced;
        private ProgressBar progressBar;
        private Button btnFilter;
        private Button btnExportInvoice;
        private Panel panel4;
        private Label label5;
        private Label label6;
        private Panel panelExportLog;
        private Label lblLastSync1;
        private Label lblHeartbeat1;
        private Label labelLogs;
        private TableLayoutPanel tableLayoutPanelLog;
        private Panel panelLogGridBox;
        private TableLayoutPanel tableLayoutPanel2;
        private Panel panellogchart;
        private Panel panelTotalLogs;
        private Label lblTotalLogsCount;
        private Label lblTotalLogsTitle;
        private Panel panelWarningLogs;
        private Label lblWarningLogsTitle;
        private Label lblWarningLogsCount;
        private Panel panelErrorLogs;
        private Label lblErrorLogsTitle;
        private Label lblErrorLogsCount;
        private Panel panelInfoLogs;
        private Label lblInfoLogsTitle;
        private Label lblInfoLogsCount;
        private Panel panel3;
        //private Button btnSyncLogs;
        //private Button btnExportLogs;
        private Label lblLastSync;
        private Label lblHeartbeat;
        private Label labelLogsTitle;
        private DataGridView LogsDataGridView;
        private Panel panelLogBtn;
        private TableLayoutPanel tlpBtns;
        private TableLayoutPanel tlpLogBtnInner;
        //private Button button1;
        //private Button btnExportLog;
        private TableLayoutPanel btnRow;
        private Panel PaginationPanel;
        private Button btnSyncLogs;
        private Button btnExportLogs;
        private Panel panel2;
        private Panel panelSyncLog;
        private Panel panel1;
    }
}