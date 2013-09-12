namespace VirtualizedListSample
{
    partial class frmMain
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
            this.lstItems = new System.Windows.Forms.ListView();
            this.lblInfo = new System.Windows.Forms.Label();
            this.colName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colDetails = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.statusBar = new System.Windows.Forms.StatusStrip();
            this.tsStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.pnlListErrorIndicator = new System.Windows.Forms.Panel();
            this.lblListErrorNotification = new System.Windows.Forms.Label();
            this.numDelay = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.btnToggleListMode = new System.Windows.Forms.Button();
            this.statusBar.SuspendLayout();
            this.pnlListErrorIndicator.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numDelay)).BeginInit();
            this.SuspendLayout();
            // 
            // lstItems
            // 
            this.lstItems.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstItems.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colName,
            this.colDetails});
            this.lstItems.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstItems.Location = new System.Drawing.Point(12, 62);
            this.lstItems.Name = "lstItems";
            this.lstItems.Size = new System.Drawing.Size(631, 359);
            this.lstItems.TabIndex = 0;
            this.lstItems.UseCompatibleStateImageBehavior = false;
            this.lstItems.View = System.Windows.Forms.View.Details;
            this.lstItems.VirtualMode = true;
            // 
            // lblInfo
            // 
            this.lblInfo.AutoSize = true;
            this.lblInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInfo.Location = new System.Drawing.Point(12, 9);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(529, 20);
            this.lblInfo.TabIndex = 1;
            this.lblInfo.Text = "Demonstrates the use of VirtualizedItemsSource with virtualized list views.";
            // 
            // colName
            // 
            this.colName.Text = "Name";
            this.colName.Width = 150;
            // 
            // colDetails
            // 
            this.colDetails.Text = "Details";
            this.colDetails.Width = 250;
            // 
            // statusBar
            // 
            this.statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsStatusLabel});
            this.statusBar.Location = new System.Drawing.Point(0, 439);
            this.statusBar.Name = "statusBar";
            this.statusBar.Size = new System.Drawing.Size(655, 22);
            this.statusBar.TabIndex = 2;
            this.statusBar.Text = "statusStrip1";
            // 
            // tsStatusLabel
            // 
            this.tsStatusLabel.Name = "tsStatusLabel";
            this.tsStatusLabel.Size = new System.Drawing.Size(30, 17);
            this.tsStatusLabel.Text = "IDLE";
            // 
            // pnlListErrorIndicator
            // 
            this.pnlListErrorIndicator.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlListErrorIndicator.BackColor = System.Drawing.Color.Maroon;
            this.pnlListErrorIndicator.Controls.Add(this.lblListErrorNotification);
            this.pnlListErrorIndicator.Location = new System.Drawing.Point(13, 357);
            this.pnlListErrorIndicator.Name = "pnlListErrorIndicator";
            this.pnlListErrorIndicator.Size = new System.Drawing.Size(599, 64);
            this.pnlListErrorIndicator.TabIndex = 11;
            this.pnlListErrorIndicator.Visible = false;
            // 
            // lblListErrorNotification
            // 
            this.lblListErrorNotification.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblListErrorNotification.BackColor = System.Drawing.Color.White;
            this.lblListErrorNotification.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblListErrorNotification.ForeColor = System.Drawing.Color.DarkRed;
            this.lblListErrorNotification.Location = new System.Drawing.Point(3, 3);
            this.lblListErrorNotification.Name = "lblListErrorNotification";
            this.lblListErrorNotification.Size = new System.Drawing.Size(593, 58);
            this.lblListErrorNotification.TabIndex = 0;
            this.lblListErrorNotification.Text = "label1";
            this.lblListErrorNotification.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblListErrorNotification.Click += new System.EventHandler(this.lblListErrorNotification_Click);
            // 
            // numDelay
            // 
            this.numDelay.Location = new System.Drawing.Point(131, 34);
            this.numDelay.Maximum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.numDelay.Name = "numDelay";
            this.numDelay.Size = new System.Drawing.Size(61, 20);
            this.numDelay.TabIndex = 12;
            this.numDelay.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numDelay.ValueChanged += new System.EventHandler(this.numDelay_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(13, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(114, 13);
            this.label1.TabIndex = 13;
            this.label1.Text = "Simultated Avg. Delay:";
            // 
            // btnToggleListMode
            // 
            this.btnToggleListMode.Location = new System.Drawing.Point(508, 31);
            this.btnToggleListMode.Name = "btnToggleListMode";
            this.btnToggleListMode.Size = new System.Drawing.Size(135, 23);
            this.btnToggleListMode.TabIndex = 14;
            this.btnToggleListMode.Text = "Toggle List Mode";
            this.btnToggleListMode.UseVisualStyleBackColor = true;
            this.btnToggleListMode.Click += new System.EventHandler(this.btnToggleListMode_Click);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(655, 461);
            this.Controls.Add(this.btnToggleListMode);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numDelay);
            this.Controls.Add(this.pnlListErrorIndicator);
            this.Controls.Add(this.statusBar);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.lstItems);
            this.Name = "frmMain";
            this.Text = "Virtualized Items Source Demo";
            this.statusBar.ResumeLayout(false);
            this.statusBar.PerformLayout();
            this.pnlListErrorIndicator.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numDelay)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView lstItems;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.ColumnHeader colName;
        private System.Windows.Forms.ColumnHeader colDetails;
        private System.Windows.Forms.StatusStrip statusBar;
        private System.Windows.Forms.ToolStripStatusLabel tsStatusLabel;
        private System.Windows.Forms.Panel pnlListErrorIndicator;
        private System.Windows.Forms.Label lblListErrorNotification;
        private System.Windows.Forms.NumericUpDown numDelay;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnToggleListMode;
    }
}

