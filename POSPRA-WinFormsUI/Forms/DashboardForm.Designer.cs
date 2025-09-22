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
        private System.Windows.Forms.Label labelInvoicesTitle;
        private System.Windows.Forms.DataGridView InvoicesDataGridView;

        private System.Windows.Forms.Panel panelLogs;
        private System.Windows.Forms.Label labelLogsTitle;
        private System.Windows.Forms.DataGridView LogsDataGridView;

        // Invoice grid columns
        private System.Windows.Forms.DataGridViewTextBoxColumn colId;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPosId;
        private System.Windows.Forms.DataGridViewTextBoxColumn colInvoiceData;
        private System.Windows.Forms.DataGridViewTextBoxColumn colInvoiceNumber;
        private System.Windows.Forms.DataGridViewTextBoxColumn colIsSynced;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAttemptCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDateCreated;

        // Logs grid columns
        private System.Windows.Forms.DataGridViewTextBoxColumn colID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMessage;
        private System.Windows.Forms.DataGridViewTextBoxColumn colException;
        private System.Windows.Forms.DataGridViewTextBoxColumn logdatetime;

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
            labelAllInvoices = new Label();
            lblAllTitle = new Label();
            panelPending = new Panel();
            labelPendingInvoice = new Label();
            lblPendingTitle = new Label();
            panelPaid = new Panel();
            labelPaidInvoices = new Label();
            lblPaidTitle = new Label();
            panelInProgress = new Panel();
            labelInProgressInvc = new Label();
            lblInProgressTitle = new Label();
            tableLayoutPanelMain = new TableLayoutPanel();
            panelInvoices = new Panel();
            InvoicesDataGridView = new DataGridView();
            colId = new DataGridViewTextBoxColumn();
            colPosId = new DataGridViewTextBoxColumn();
            colInvoiceData = new DataGridViewTextBoxColumn();
            colInvoiceNumber = new DataGridViewTextBoxColumn();
            colIsSynced = new DataGridViewTextBoxColumn();
            colAttemptCount = new DataGridViewTextBoxColumn();
            colDateCreated = new DataGridViewTextBoxColumn();
            labelInvoicesTitle = new Label();
            panelLogs = new Panel();
            LogsDataGridView = new DataGridView();
            colID = new DataGridViewTextBoxColumn();
            colMessage = new DataGridViewTextBoxColumn();
            colException = new DataGridViewTextBoxColumn();
            logdatetime = new DataGridViewTextBoxColumn();
            labelLogsTitle = new Label();
            tableLayoutPanelTop.SuspendLayout();
            panelAll.SuspendLayout();
            panelPending.SuspendLayout();
            panelPaid.SuspendLayout();
            panelInProgress.SuspendLayout();
            tableLayoutPanelMain.SuspendLayout();
            panelInvoices.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)InvoicesDataGridView).BeginInit();
            panelLogs.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)LogsDataGridView).BeginInit();
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
            tableLayoutPanelTop.ColumnCount = 4;
            tableLayoutPanelTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tableLayoutPanelTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tableLayoutPanelTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tableLayoutPanelTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tableLayoutPanelTop.Controls.Add(panelAll, 0, 0);
            tableLayoutPanelTop.Controls.Add(panelPending, 1, 0);
            tableLayoutPanelTop.Controls.Add(panelPaid, 2, 0);
            tableLayoutPanelTop.Controls.Add(panelInProgress, 3, 0);
            tableLayoutPanelTop.Dock = DockStyle.Top;
            tableLayoutPanelTop.Location = new Point(0, 0);
            tableLayoutPanelTop.Margin = new Padding(0);
            tableLayoutPanelTop.Name = "tableLayoutPanelTop";
            tableLayoutPanelTop.Padding = new Padding(10);
            tableLayoutPanelTop.RowCount = 1;
            tableLayoutPanelTop.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanelTop.Size = new Size(1200, 136);
            tableLayoutPanelTop.TabIndex = 1;
            // 
            // panelAll
            // 
            panelAll.BackColor = Color.White;
            panelAll.Controls.Add(labelAllInvoices);
            panelAll.Controls.Add(lblAllTitle);
            panelAll.Dock = DockStyle.Fill;
            panelAll.Location = new Point(18, 18);
            panelAll.Margin = new Padding(8);
            panelAll.Name = "panelAll";
            panelAll.Padding = new Padding(12);
            panelAll.Size = new Size(279, 100);
            panelAll.TabIndex = 0;
            // 
            // labelAllInvoices
            // 
            labelAllInvoices.AutoSize = true;
            labelAllInvoices.Dock = DockStyle.Fill;
            labelAllInvoices.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
            labelAllInvoices.ForeColor = Color.FromArgb(40, 40, 40);
            labelAllInvoices.Location = new Point(12, 37);
            labelAllInvoices.Name = "labelAllInvoices";
            labelAllInvoices.Size = new Size(43, 50);
            labelAllInvoices.TabIndex = 0;
            labelAllInvoices.Text = "0";
            labelAllInvoices.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblAllTitle
            // 
            lblAllTitle.AutoSize = true;
            lblAllTitle.Dock = DockStyle.Top;
            lblAllTitle.Font = new Font("Segoe UI", 11F);
            lblAllTitle.ForeColor = Color.DimGray;
            lblAllTitle.Location = new Point(12, 12);
            lblAllTitle.Name = "lblAllTitle";
            lblAllTitle.Size = new Size(107, 25);
            lblAllTitle.TabIndex = 1;
            lblAllTitle.Text = "All Invoices";
            // 
            // panelPending
            // 
            panelPending.BackColor = Color.White;
            panelPending.Controls.Add(labelPendingInvoice);
            panelPending.Controls.Add(lblPendingTitle);
            panelPending.Dock = DockStyle.Fill;
            panelPending.Location = new Point(313, 18);
            panelPending.Margin = new Padding(8);
            panelPending.Name = "panelPending";
            panelPending.Padding = new Padding(12);
            panelPending.Size = new Size(279, 100);
            panelPending.TabIndex = 1;
            // 
            // labelPendingInvoice
            // 
            labelPendingInvoice.AutoSize = true;
            labelPendingInvoice.Dock = DockStyle.Fill;
            labelPendingInvoice.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
            labelPendingInvoice.ForeColor = Color.FromArgb(40, 40, 40);
            labelPendingInvoice.Location = new Point(12, 37);
            labelPendingInvoice.Name = "labelPendingInvoice";
            labelPendingInvoice.Size = new Size(43, 50);
            labelPendingInvoice.TabIndex = 0;
            labelPendingInvoice.Text = "0";
            labelPendingInvoice.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblPendingTitle
            // 
            lblPendingTitle.AutoSize = true;
            lblPendingTitle.Dock = DockStyle.Top;
            lblPendingTitle.Font = new Font("Segoe UI", 11F);
            lblPendingTitle.ForeColor = Color.DimGray;
            lblPendingTitle.Location = new Point(12, 12);
            lblPendingTitle.Name = "lblPendingTitle";
            lblPendingTitle.Size = new Size(107, 25);
            lblPendingTitle.TabIndex = 1;
            lblPendingTitle.Text = "Not Synced";
            // 
            // panelPaid
            // 
            panelPaid.BackColor = Color.White;
            panelPaid.Controls.Add(labelPaidInvoices);
            panelPaid.Controls.Add(lblPaidTitle);
            panelPaid.Dock = DockStyle.Fill;
            panelPaid.Location = new Point(608, 18);
            panelPaid.Margin = new Padding(8);
            panelPaid.Name = "panelPaid";
            panelPaid.Padding = new Padding(12);
            panelPaid.Size = new Size(279, 100);
            panelPaid.TabIndex = 2;
            // 
            // labelPaidInvoices
            // 
            labelPaidInvoices.AutoSize = true;
            labelPaidInvoices.Dock = DockStyle.Fill;
            labelPaidInvoices.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
            labelPaidInvoices.ForeColor = Color.FromArgb(40, 40, 40);
            labelPaidInvoices.Location = new Point(12, 37);
            labelPaidInvoices.Name = "labelPaidInvoices";
            labelPaidInvoices.Size = new Size(43, 50);
            labelPaidInvoices.TabIndex = 0;
            labelPaidInvoices.Text = "0";
            labelPaidInvoices.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblPaidTitle
            // 
            lblPaidTitle.AutoSize = true;
            lblPaidTitle.Dock = DockStyle.Top;
            lblPaidTitle.Font = new Font("Segoe UI", 11F);
            lblPaidTitle.ForeColor = Color.DimGray;
            lblPaidTitle.Location = new Point(12, 12);
            lblPaidTitle.Name = "lblPaidTitle";
            lblPaidTitle.Size = new Size(144, 25);
            lblPaidTitle.TabIndex = 1;
            lblPaidTitle.Text = "Synced Invoices";
            // 
            // panelInProgress
            // 
            panelInProgress.BackColor = Color.White;
            panelInProgress.Controls.Add(labelInProgressInvc);
            panelInProgress.Controls.Add(lblInProgressTitle);
            panelInProgress.Dock = DockStyle.Fill;
            panelInProgress.Location = new Point(903, 18);
            panelInProgress.Margin = new Padding(8);
            panelInProgress.Name = "panelInProgress";
            panelInProgress.Padding = new Padding(12);
            panelInProgress.Size = new Size(279, 100);
            panelInProgress.TabIndex = 3;
            // 
            // labelInProgressInvc
            // 
            labelInProgressInvc.AutoSize = true;
            labelInProgressInvc.Dock = DockStyle.Fill;
            labelInProgressInvc.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
            labelInProgressInvc.ForeColor = Color.FromArgb(40, 40, 40);
            labelInProgressInvc.Location = new Point(12, 37);
            labelInProgressInvc.Name = "labelInProgressInvc";
            labelInProgressInvc.Size = new Size(43, 50);
            labelInProgressInvc.TabIndex = 0;
            labelInProgressInvc.Text = "0";
            labelInProgressInvc.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblInProgressTitle
            // 
            lblInProgressTitle.AutoSize = true;
            lblInProgressTitle.Dock = DockStyle.Top;
            lblInProgressTitle.Font = new Font("Segoe UI", 11F);
            lblInProgressTitle.ForeColor = Color.DimGray;
            lblInProgressTitle.Location = new Point(12, 12);
            lblInProgressTitle.Name = "lblInProgressTitle";
            lblInProgressTitle.Size = new Size(106, 25);
            lblInProgressTitle.TabIndex = 1;
            lblInProgressTitle.Text = "In Progress";
            // 
            // tableLayoutPanelMain
            // 
            tableLayoutPanelMain.ColumnCount = 1;
            tableLayoutPanelMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanelMain.Controls.Add(panelInvoices, 0, 0);
            tableLayoutPanelMain.Controls.Add(panelLogs, 0, 1);
            tableLayoutPanelMain.Dock = DockStyle.Fill;
            tableLayoutPanelMain.Location = new Point(0, 136);
            tableLayoutPanelMain.Margin = new Padding(0);
            tableLayoutPanelMain.Name = "tableLayoutPanelMain";
            tableLayoutPanelMain.Padding = new Padding(10);
            tableLayoutPanelMain.RowCount = 2;
            tableLayoutPanelMain.RowStyles.Add(new RowStyle(SizeType.Percent, 65F));
            tableLayoutPanelMain.RowStyles.Add(new RowStyle(SizeType.Percent, 35F));
            tableLayoutPanelMain.Size = new Size(1200, 764);
            tableLayoutPanelMain.TabIndex = 0;
            // 
            // panelInvoices
            // 
            panelInvoices.BackColor = Color.White;
            panelInvoices.Controls.Add(InvoicesDataGridView);
            panelInvoices.Controls.Add(labelInvoicesTitle);
            panelInvoices.Dock = DockStyle.Fill;
            panelInvoices.Location = new Point(16, 16);
            panelInvoices.Margin = new Padding(6);
            panelInvoices.Name = "panelInvoices";
            panelInvoices.Padding = new Padding(8);
            panelInvoices.Size = new Size(1168, 471);
            panelInvoices.TabIndex = 0;
            // 
            // InvoicesDataGridView
            // 
            InvoicesDataGridView.AllowUserToAddRows = false;
            InvoicesDataGridView.AllowUserToDeleteRows = false;
            InvoicesDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            InvoicesDataGridView.Columns.AddRange(new DataGridViewColumn[] { colId, colPosId, colInvoiceData, colInvoiceNumber, colIsSynced, colAttemptCount, colDateCreated });
            InvoicesDataGridView.Dock = DockStyle.Fill;
            InvoicesDataGridView.Location = new Point(8, 57);
            InvoicesDataGridView.Margin = new Padding(3, 4, 3, 4);
            InvoicesDataGridView.Name = "InvoicesDataGridView";
            InvoicesDataGridView.ReadOnly = true;
            InvoicesDataGridView.RowHeadersVisible = false;
            InvoicesDataGridView.RowHeadersWidth = 51;
            InvoicesDataGridView.Size = new Size(1152, 406);
            InvoicesDataGridView.TabIndex = 0;
            // 
            // colId
            // 
            colId.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colId.HeaderText = "ID";
            colId.MinimumWidth = 50;
            colId.Name = "colId";
            colId.ReadOnly = true;
            colId.Width = 80;
            // 
            // colPosId
            // 
            colPosId.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colPosId.HeaderText = "POS ID";
            colPosId.MinimumWidth = 6;
            colPosId.Name = "colPosId";
            colPosId.ReadOnly = true;
            // 
            // colInvoiceData
            // 
            colInvoiceData.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colInvoiceData.HeaderText = "Invoice Data";
            colInvoiceData.MinimumWidth = 6;
            colInvoiceData.Name = "colInvoiceData";
            colInvoiceData.ReadOnly = true;
            // 
            // colInvoiceNumber
            // 
            colInvoiceNumber.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colInvoiceNumber.HeaderText = "Invoice Number";
            colInvoiceNumber.MinimumWidth = 6;
            colInvoiceNumber.Name = "colInvoiceNumber";
            colInvoiceNumber.ReadOnly = true;
            // 
            // colIsSynced
            // 
            colIsSynced.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colIsSynced.HeaderText = "Invoice Synced";
            colIsSynced.MinimumWidth = 6;
            colIsSynced.Name = "colIsSynced";
            colIsSynced.ReadOnly = true;
            // 
            // colAttemptCount
            // 
            colAttemptCount.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colAttemptCount.HeaderText = "Attempt Count";
            colAttemptCount.MinimumWidth = 6;
            colAttemptCount.Name = "colAttemptCount";
            colAttemptCount.ReadOnly = true;
            // 
            // colDateCreated
            // 
            colDateCreated.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colDateCreated.HeaderText = "Date Created";
            colDateCreated.MinimumWidth = 6;
            colDateCreated.Name = "colDateCreated";
            colDateCreated.ReadOnly = true;
            // 
            // labelInvoicesTitle
            // 
            labelInvoicesTitle.AutoSize = true;
            labelInvoicesTitle.Dock = DockStyle.Top;
            labelInvoicesTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            labelInvoicesTitle.ForeColor = Color.FromArgb(30, 30, 30);
            labelInvoicesTitle.Location = new Point(8, 8);
            labelInvoicesTitle.Name = "labelInvoicesTitle";
            labelInvoicesTitle.Padding = new Padding(6, 6, 0, 6);
            labelInvoicesTitle.Size = new Size(221, 49);
            labelInvoicesTitle.TabIndex = 1;
            labelInvoicesTitle.Text = "Invoices Listing";
            // 
            // panelLogs
            // 
            panelLogs.BackColor = Color.White;
            panelLogs.Controls.Add(LogsDataGridView);
            panelLogs.Controls.Add(labelLogsTitle);
            panelLogs.Dock = DockStyle.Fill;
            panelLogs.Location = new Point(16, 499);
            panelLogs.Margin = new Padding(6);
            panelLogs.Name = "panelLogs";
            panelLogs.Padding = new Padding(8);
            panelLogs.Size = new Size(1168, 249);
            panelLogs.TabIndex = 1;
            // 
            // LogsDataGridView
            // 
            LogsDataGridView.AllowUserToAddRows = false;
            LogsDataGridView.AllowUserToDeleteRows = false;
            LogsDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            LogsDataGridView.Columns.AddRange(new DataGridViewColumn[] { colID, colMessage, colException, logdatetime });
            LogsDataGridView.Dock = DockStyle.Fill;
            LogsDataGridView.Location = new Point(8, 57);
            LogsDataGridView.Margin = new Padding(3, 4, 3, 4);
            LogsDataGridView.Name = "LogsDataGridView";
            LogsDataGridView.ReadOnly = true;
            LogsDataGridView.RowHeadersVisible = false;
            LogsDataGridView.RowHeadersWidth = 51;
            LogsDataGridView.Size = new Size(1152, 184);
            LogsDataGridView.TabIndex = 0;
            // 
            // colId
            // 
            colID.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colID.HeaderText = "ID";
            colID.MinimumWidth = 50;
            colID.Name = "colID";
            colID.ReadOnly = true;
            colID.Width = 80;
            // 
            // colMessage
            // 
            colMessage.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colMessage.HeaderText = "Message";
            colMessage.MinimumWidth = 6;
            colMessage.Name = "colMessage";
            colMessage.ReadOnly = true;
            // 
            // colException
            // 
            colException.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colException.HeaderText = "Message";
            colException.MinimumWidth = 6;
            colException.Name = "colException";
            colException.ReadOnly = true;
            // 
            // logdatetime
            // 
            logdatetime.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            logdatetime.HeaderText = "Date and Time";
            logdatetime.MinimumWidth = 6;
            logdatetime.Name = "logdatetime";
            logdatetime.ReadOnly = true;
            logdatetime.Width = 300;
            // 
            // labelLogsTitle
            // 
            labelLogsTitle.AutoSize = true;
            labelLogsTitle.Dock = DockStyle.Top;
            labelLogsTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            labelLogsTitle.ForeColor = Color.FromArgb(30, 30, 30);
            labelLogsTitle.Location = new Point(8, 8);
            labelLogsTitle.Name = "labelLogsTitle";
            labelLogsTitle.Padding = new Padding(6, 6, 0, 6);
            labelLogsTitle.Size = new Size(83, 49);
            labelLogsTitle.TabIndex = 1;
            labelLogsTitle.Text = "Logs";
            // 
            // DashboardForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1200, 900);
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
            panelInProgress.ResumeLayout(false);
            panelInProgress.PerformLayout();
            tableLayoutPanelMain.ResumeLayout(false);
            panelInvoices.ResumeLayout(false);
            panelInvoices.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)InvoicesDataGridView).EndInit();
            panelLogs.ResumeLayout(false);
            panelLogs.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)LogsDataGridView).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
