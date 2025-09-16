using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Button = System.Windows.Forms.Button;

namespace POSPRA_WinFormsUI.Forms
{
    partial class DashboardForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            settings = new ToolStripMenuItem();
            logout = new ToolStripMenuItem();
            panel2 = new Panel();
            button1 = new Button();
            label5 = new Label();
            panel3 = new Panel();
            button5 = new Button();
            label11 = new Label();
            button2 = new Button();
            label4 = new Label();
            label6 = new Label();
            lblPendingInvoices = new Label();
            panel4 = new Panel();
            button6 = new Button();
            label12 = new Label();
            button3 = new Button();
            label7 = new Label();
            label8 = new Label();
            lblPaidInvoices = new Label();
            panel5 = new Panel();
            button7 = new Button();
            label13 = new Label();
            button4 = new Button();
            label9 = new Label();
            label10 = new Label();
            lblInProgressInvoices = new Label();
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel2 = new TableLayoutPanel();
            panel7 = new Panel();
            dataGridView2 = new DataGridView();
            colMessage = new DataGridViewComboBoxColumn();
            colInvoiceSyncedStatus = new DataGridViewTextBoxColumn();
            label3 = new Label();
            panel6 = new Panel();
            btnNewInvoice = new Button();
            label2 = new Label();
            dataGridView1 = new DataGridView();
            colAll = new DataGridViewCheckBoxColumn();
            colSrNo = new DataGridViewTextBoxColumn();
            colInvoiceNo = new DataGridViewTextBoxColumn();
            colPosId = new DataGridViewTextBoxColumn();
            colInvoiceSynced = new DataGridViewTextBoxColumn();
            colDueDate = new DataGridViewTextBoxColumn();
            colTotal = new DataGridViewTextBoxColumn();
            colStatus = new DataGridViewTextBoxColumn();
            panel2.SuspendLayout();
            panel3.SuspendLayout();
            panel4.SuspendLayout();
            panel5.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            panel7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView2).BeginInit();
            panel6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
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
            // panel2
            // 
            panel2.BackColor = Color.White;
            panel2.Controls.Add(button1);
            panel2.Controls.Add(label5);
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new Point(11, 13);
            panel2.Margin = new Padding(11, 13, 11, 13);
            panel2.Name = "panel2";
            panel2.Size = new Size(440, 191);
            panel2.TabIndex = 2;
            // 
            // button1
            // 
            button1.FlatAppearance.BorderSize = 0;
            button1.FlatStyle = FlatStyle.Flat;
            button1.Font = new Font("Segoe UI", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            button1.Location = new Point(63, 55);
            button1.Margin = new Padding(3, 4, 3, 4);
            button1.Name = "button1";
            button1.Size = new Size(159, 51);
            button1.TabIndex = 1;
            button1.Text = "All Invoices";
            button1.TextImageRelation = TextImageRelation.ImageBeforeText;
            button1.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 20F, FontStyle.Bold, GraphicsUnit.Pixel);
            label5.Location = new Point(73, 111);
            label5.Name = "label5";
            label5.Size = new Size(118, 28);
            label5.TabIndex = 0;
            label5.Text = "20,000,000";
            // 
            // panel3
            // 
            panel3.BackColor = Color.White;
            panel3.Controls.Add(button5);
            panel3.Controls.Add(label11);
            panel3.Controls.Add(button2);
            panel3.Controls.Add(label4);
            panel3.Controls.Add(label6);
            panel3.Controls.Add(lblPendingInvoices);
            panel3.Dock = DockStyle.Fill;
            panel3.Location = new Point(473, 13);
            panel3.Margin = new Padding(11, 13, 11, 13);
            panel3.Name = "panel3";
            panel3.Size = new Size(440, 191);
            panel3.TabIndex = 2;
            // 
            // button5
            // 
            button5.FlatAppearance.BorderSize = 0;
            button5.FlatStyle = FlatStyle.Flat;
            button5.Font = new Font("Segoe UI", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            button5.Location = new Point(141, 48);
            button5.Margin = new Padding(3, 4, 3, 4);
            button5.Name = "button5";
            button5.Size = new Size(189, 51);
            button5.TabIndex = 6;
            button5.Text = "Pending Invoices";
            button5.TextImageRelation = TextImageRelation.ImageBeforeText;
            button5.UseVisualStyleBackColor = true;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Font = new Font("Segoe UI", 20F, FontStyle.Bold, GraphicsUnit.Pixel);
            label11.Location = new Point(151, 104);
            label11.Name = "label11";
            label11.Size = new Size(118, 28);
            label11.TabIndex = 5;
            label11.Text = "20,000,000";
            // 
            // button2
            // 
            button2.FlatAppearance.BorderSize = 0;
            button2.FlatStyle = FlatStyle.Flat;
            button2.Location = new Point(147, 48);
            button2.Margin = new Padding(3, 4, 3, 4);
            button2.Name = "button2";
            button2.Size = new Size(135, 51);
            button2.TabIndex = 4;
            button2.Text = "All Invoices";
            button2.TextImageRelation = TextImageRelation.ImageBeforeText;
            button2.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 20F, FontStyle.Bold, GraphicsUnit.Pixel);
            label4.Location = new Point(158, 104);
            label4.Name = "label4";
            label4.Size = new Size(118, 28);
            label4.TabIndex = 3;
            label4.Text = "20,000,000";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Segoe UI", 20F, FontStyle.Bold, GraphicsUnit.Pixel);
            label6.Location = new Point(152, 97);
            label6.Name = "label6";
            label6.Size = new Size(118, 28);
            label6.TabIndex = 1;
            label6.Text = "20,000,000";
            // 
            // lblPendingInvoices
            // 
            lblPendingInvoices.AutoSize = true;
            lblPendingInvoices.Font = new Font("Segoe UI", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblPendingInvoices.Location = new Point(152, 55);
            lblPendingInvoices.Name = "lblPendingInvoices";
            lblPendingInvoices.Size = new Size(88, 21);
            lblPendingInvoices.TabIndex = 2;
            lblPendingInvoices.Text = "All Invoices";
            lblPendingInvoices.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // panel4
            // 
            panel4.BackColor = Color.White;
            panel4.Controls.Add(button6);
            panel4.Controls.Add(label12);
            panel4.Controls.Add(button3);
            panel4.Controls.Add(label7);
            panel4.Controls.Add(label8);
            panel4.Controls.Add(lblPaidInvoices);
            panel4.Dock = DockStyle.Fill;
            panel4.Location = new Point(935, 13);
            panel4.Margin = new Padding(11, 13, 11, 13);
            panel4.Name = "panel4";
            panel4.Size = new Size(440, 191);
            panel4.TabIndex = 2;
            // 
            // button6
            // 
            button6.FlatAppearance.BorderSize = 0;
            button6.FlatStyle = FlatStyle.Flat;
            button6.Font = new Font("Segoe UI", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            button6.Location = new Point(141, 48);
            button6.Margin = new Padding(3, 4, 3, 4);
            button6.Name = "button6";
            button6.Size = new Size(159, 51);
            button6.TabIndex = 6;
            button6.Text = "Paid Invoices";
            button6.TextImageRelation = TextImageRelation.ImageBeforeText;
            button6.UseVisualStyleBackColor = true;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Font = new Font("Segoe UI", 20F, FontStyle.Bold, GraphicsUnit.Pixel);
            label12.Location = new Point(151, 104);
            label12.Name = "label12";
            label12.Size = new Size(118, 28);
            label12.TabIndex = 5;
            label12.Text = "20,000,000";
            // 
            // button3
            // 
            button3.FlatAppearance.BorderSize = 0;
            button3.FlatStyle = FlatStyle.Flat;
            button3.Location = new Point(147, 48);
            button3.Margin = new Padding(3, 4, 3, 4);
            button3.Name = "button3";
            button3.Size = new Size(135, 51);
            button3.TabIndex = 4;
            button3.Text = "All Invoices";
            button3.TextImageRelation = TextImageRelation.ImageBeforeText;
            button3.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Segoe UI", 20F, FontStyle.Bold, GraphicsUnit.Pixel);
            label7.Location = new Point(158, 104);
            label7.Name = "label7";
            label7.Size = new Size(118, 28);
            label7.TabIndex = 3;
            label7.Text = "20,000,000";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Segoe UI", 20F, FontStyle.Bold, GraphicsUnit.Pixel);
            label8.Location = new Point(152, 97);
            label8.Name = "label8";
            label8.Size = new Size(118, 28);
            label8.TabIndex = 1;
            label8.Text = "20,000,000";
            // 
            // lblPaidInvoices
            // 
            lblPaidInvoices.AutoSize = true;
            lblPaidInvoices.Font = new Font("Segoe UI", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblPaidInvoices.Location = new Point(152, 55);
            lblPaidInvoices.Name = "lblPaidInvoices";
            lblPaidInvoices.Size = new Size(88, 21);
            lblPaidInvoices.TabIndex = 2;
            lblPaidInvoices.Text = "All Invoices";
            lblPaidInvoices.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // panel5
            // 
            panel5.BackColor = Color.White;
            panel5.Controls.Add(button7);
            panel5.Controls.Add(label13);
            panel5.Controls.Add(button4);
            panel5.Controls.Add(label9);
            panel5.Controls.Add(label10);
            panel5.Controls.Add(lblInProgressInvoices);
            panel5.Dock = DockStyle.Fill;
            panel5.Location = new Point(1397, 13);
            panel5.Margin = new Padding(11, 13, 11, 13);
            panel5.Name = "panel5";
            panel5.Size = new Size(441, 191);
            panel5.TabIndex = 2;
            // 
            // button7
            // 
            button7.FlatAppearance.BorderSize = 0;
            button7.FlatStyle = FlatStyle.Flat;
            button7.Font = new Font("Segoe UI", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            button7.Location = new Point(142, 48);
            button7.Margin = new Padding(3, 4, 3, 4);
            button7.Name = "button7";
            button7.Size = new Size(219, 51);
            button7.TabIndex = 6;
            button7.Text = "In Progress Invoices";
            button7.TextImageRelation = TextImageRelation.ImageBeforeText;
            button7.UseVisualStyleBackColor = true;
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Font = new Font("Segoe UI", 20F, FontStyle.Bold, GraphicsUnit.Pixel);
            label13.Location = new Point(152, 104);
            label13.Name = "label13";
            label13.Size = new Size(118, 28);
            label13.TabIndex = 5;
            label13.Text = "20,000,000";
            // 
            // button4
            // 
            button4.FlatAppearance.BorderSize = 0;
            button4.FlatStyle = FlatStyle.Flat;
            button4.Location = new Point(149, 48);
            button4.Margin = new Padding(3, 4, 3, 4);
            button4.Name = "button4";
            button4.Size = new Size(135, 51);
            button4.TabIndex = 4;
            button4.Text = "All Invoices";
            button4.TextImageRelation = TextImageRelation.ImageBeforeText;
            button4.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new Font("Segoe UI", 20F, FontStyle.Bold, GraphicsUnit.Pixel);
            label9.Location = new Point(159, 104);
            label9.Name = "label9";
            label9.Size = new Size(118, 28);
            label9.TabIndex = 3;
            label9.Text = "20,000,000";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Font = new Font("Segoe UI", 20F, FontStyle.Bold, GraphicsUnit.Pixel);
            label10.Location = new Point(153, 97);
            label10.Name = "label10";
            label10.Size = new Size(118, 28);
            label10.TabIndex = 1;
            label10.Text = "20,000,000";
            // 
            // lblInProgressInvoices
            // 
            lblInProgressInvoices.AutoSize = true;
            lblInProgressInvoices.Font = new Font("Segoe UI", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            lblInProgressInvoices.Location = new Point(153, 55);
            lblInProgressInvoices.Name = "lblInProgressInvoices";
            lblInProgressInvoices.Size = new Size(88, 21);
            lblInProgressInvoices.TabIndex = 2;
            lblInProgressInvoices.Text = "All Invoices";
            lblInProgressInvoices.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 4;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tableLayoutPanel1.Controls.Add(panel2, 0, 0);
            tableLayoutPanel1.Controls.Add(panel3, 1, 0);
            tableLayoutPanel1.Controls.Add(panel4, 2, 0);
            tableLayoutPanel1.Controls.Add(panel5, 3, 0);
            tableLayoutPanel1.Dock = DockStyle.Top;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Margin = new Padding(3, 4, 3, 4);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(1849, 217);
            tableLayoutPanel1.TabIndex = 3;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Controls.Add(panel7, 0, 1);
            tableLayoutPanel2.Controls.Add(panel6, 0, 0);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(0, 217);
            tableLayoutPanel2.Margin = new Padding(3, 4, 3, 4);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 2;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 60F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
            tableLayoutPanel2.Size = new Size(1849, 838);
            tableLayoutPanel2.TabIndex = 4;
            // 
            // panel7
            // 
            panel7.BackColor = Color.White;
            panel7.Controls.Add(dataGridView2);
            panel7.Controls.Add(label3);
            panel7.Dock = DockStyle.Fill;
            panel7.Location = new Point(3, 506);
            panel7.Margin = new Padding(3, 4, 3, 4);
            panel7.Name = "panel7";
            panel7.Size = new Size(1843, 328);
            panel7.TabIndex = 1;
            // 
            // dataGridView2
            // 
            dataGridView2.AllowUserToAddRows = false;
            dataGridView2.AllowUserToDeleteRows = false;
            dataGridView2.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView2.Columns.AddRange(new DataGridViewColumn[] { colMessage, colInvoiceSyncedStatus });
            dataGridView2.Dock = DockStyle.Bottom;
            dataGridView2.Location = new Point(0, 72);
            dataGridView2.Margin = new Padding(3, 4, 3, 4);
            dataGridView2.Name = "dataGridView2";
            dataGridView2.ReadOnly = true;
            dataGridView2.RowHeadersVisible = false;
            dataGridView2.RowHeadersWidth = 51;
            dataGridView2.Size = new Size(1843, 256);
            dataGridView2.TabIndex = 3;
            // 
            // colMessage
            // 
            colMessage.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colMessage.HeaderText = "Message";
            colMessage.MinimumWidth = 6;
            colMessage.Name = "colMessage";
            colMessage.ReadOnly = true;
            // 
            // colInvoiceSyncedStatus
            // 
            colInvoiceSyncedStatus.HeaderText = "Invoice Synced";
            colInvoiceSyncedStatus.MinimumWidth = 6;
            colInvoiceSyncedStatus.Name = "colInvoiceSyncedStatus";
            colInvoiceSyncedStatus.ReadOnly = true;
            colInvoiceSyncedStatus.Width = 250;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Dock = DockStyle.Top;
            label3.Font = new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Pixel);
            label3.Location = new Point(0, 0);
            label3.Name = "label3";
            label3.Size = new Size(45, 21);
            label3.TabIndex = 2;
            label3.Text = "Logs";
            // 
            // panel6
            // 
            panel6.BackColor = Color.White;
            panel6.Controls.Add(btnNewInvoice);
            panel6.Controls.Add(label2);
            panel6.Controls.Add(dataGridView1);
            panel6.Dock = DockStyle.Fill;
            panel6.Location = new Point(3, 4);
            panel6.Margin = new Padding(3, 4, 3, 4);
            panel6.Name = "panel6";
            panel6.Size = new Size(1843, 494);
            panel6.TabIndex = 2;
            // 
            // btnNewInvoice
            // 
            btnNewInvoice.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnNewInvoice.BackColor = Color.FromArgb(104, 109, 244);
            btnNewInvoice.FlatAppearance.BorderSize = 0;
            btnNewInvoice.FlatStyle = FlatStyle.Flat;
            btnNewInvoice.Font = new Font("Segoe UI", 16F, FontStyle.Regular, GraphicsUnit.Pixel);
            btnNewInvoice.ForeColor = Color.White;
            btnNewInvoice.Location = new Point(1668, 4);
            btnNewInvoice.Margin = new Padding(3, 4, 3, 4);
            btnNewInvoice.Name = "btnNewInvoice";
            btnNewInvoice.Size = new Size(165, 51);
            btnNewInvoice.TabIndex = 2;
            btnNewInvoice.Text = "+ New Invoice";
            btnNewInvoice.UseVisualStyleBackColor = false;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Dock = DockStyle.Top;
            label2.Font = new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Pixel);
            label2.Location = new Point(0, 0);
            label2.Name = "label2";
            label2.Size = new Size(128, 21);
            label2.TabIndex = 1;
            label2.Text = "Invoices Listing";
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Columns.AddRange(new DataGridViewColumn[] { colAll, colSrNo, colInvoiceNo, colPosId, colInvoiceSynced, colDueDate, colTotal, colStatus });
            dataGridView1.Dock = DockStyle.Bottom;
            dataGridView1.Location = new Point(0, 110);
            dataGridView1.Margin = new Padding(3, 4, 3, 4);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.ReadOnly = true;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.RowHeadersWidth = 51;
            dataGridView1.Size = new Size(1843, 384);
            dataGridView1.TabIndex = 0;
            // 
            // colAll
            // 
            colAll.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.NullValue = false;
            colAll.DefaultCellStyle = dataGridViewCellStyle1;
            colAll.HeaderText = "All";
            colAll.MinimumWidth = 6;
            colAll.Name = "colAll";
            colAll.ReadOnly = true;
            colAll.Width = 33;
            // 
            // colSrNo
            // 
            colSrNo.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colSrNo.HeaderText = "Sr. No.";
            colSrNo.MinimumWidth = 6;
            colSrNo.Name = "colSrNo";
            colSrNo.ReadOnly = true;
            // 
            // colInvoiceNo
            // 
            colInvoiceNo.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colInvoiceNo.HeaderText = "Invoice#";
            colInvoiceNo.MinimumWidth = 6;
            colInvoiceNo.Name = "colInvoiceNo";
            colInvoiceNo.ReadOnly = true;
            // 
            // colPosId
            // 
            colPosId.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colPosId.HeaderText = "POS ID";
            colPosId.MinimumWidth = 6;
            colPosId.Name = "colPosId";
            colPosId.ReadOnly = true;
            // 
            // colInvoiceSynced
            // 
            colInvoiceSynced.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colInvoiceSynced.HeaderText = "Invoice Synced";
            colInvoiceSynced.MinimumWidth = 6;
            colInvoiceSynced.Name = "colInvoiceSynced";
            colInvoiceSynced.ReadOnly = true;
            // 
            // colDueDate
            // 
            colDueDate.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colDueDate.HeaderText = "Due Date";
            colDueDate.MinimumWidth = 6;
            colDueDate.Name = "colDueDate";
            colDueDate.ReadOnly = true;
            // 
            // colTotal
            // 
            colTotal.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colTotal.HeaderText = "Total";
            colTotal.MinimumWidth = 6;
            colTotal.Name = "colTotal";
            colTotal.ReadOnly = true;
            // 
            // colStatus
            // 
            colStatus.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colStatus.HeaderText = "Status";
            colStatus.MinimumWidth = 6;
            colStatus.Name = "colStatus";
            colStatus.ReadOnly = true;
            // 
            // DashboardForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1849, 1055);
            Controls.Add(tableLayoutPanel2);
            Controls.Add(tableLayoutPanel1);
            Margin = new Padding(3, 4, 3, 4);
            Name = "DashboardForm";
            Text = "Dashboard";
            Load += DashboardForm_Load;
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            panel4.ResumeLayout(false);
            panel4.PerformLayout();
            panel5.ResumeLayout(false);
            panel5.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            panel7.ResumeLayout(false);
            panel7.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView2).EndInit();
            panel6.ResumeLayout(false);
            panel6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
        }
        #endregion

        public DashboardForm(string arg)
        {

        }
        private ToolStripMenuItem settings;
        private ToolStripMenuItem logout;
        private Panel panel2;
        private Panel panel3;
        private Panel panel4;
        private Panel panel5;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private Panel panel7;
        private Panel panel6;
        private DataGridView dataGridView1;
        private Label label3;
        private Label label2;
        private DataGridView dataGridView2;
        private Button btnNewInvoice;
        private DataGridViewCheckBoxColumn colAll;
        private DataGridViewTextBoxColumn colSrNo;
        private DataGridViewTextBoxColumn colInvoiceNo;
        private DataGridViewTextBoxColumn colPosId;
        private DataGridViewTextBoxColumn colInvoiceSynced;
        private DataGridViewTextBoxColumn colDueDate;
        private DataGridViewTextBoxColumn colTotal;
        private DataGridViewTextBoxColumn colStatus;
        private DataGridViewComboBoxColumn colMessage;
        private DataGridViewTextBoxColumn colInvoiceSyncedStatus;
        private Label label5;
        private Label label6;
        private Label lblPendingInvoices;
        private Label label8;
        private Label lblPaidInvoices;
        private Label label10;
        private Label lblInProgressInvoices;
        private Button button1;
        private Button button2;
        private Label label4;
        private Button button3;
        private Label label7;
        private Button button4;
        private Label label9;
        private Button button5;
        private Label label11;
        private Button button6;
        private Label label12;
        private Button button7;
        private Label label13;
    }
}