namespace VirtualizedListSample
{
    partial class frmTestWeakCache
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
            this.btnToggleThread1 = new System.Windows.Forms.Button();
            this.btnToggleThread2 = new System.Windows.Forms.Button();
            this.lblCacheInfo = new System.Windows.Forms.Label();
            this.btnPurge = new System.Windows.Forms.Button();
            this.btnForceGC = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnToggleThread1
            // 
            this.btnToggleThread1.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnToggleThread1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnToggleThread1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnToggleThread1.Location = new System.Drawing.Point(28, 89);
            this.btnToggleThread1.Name = "btnToggleThread1";
            this.btnToggleThread1.Size = new System.Drawing.Size(172, 34);
            this.btnToggleThread1.TabIndex = 0;
            this.btnToggleThread1.Text = "Start Thread 1";
            this.btnToggleThread1.UseVisualStyleBackColor = false;
            this.btnToggleThread1.Click += new System.EventHandler(this.btnToggleThread1_Click);
            // 
            // btnToggleThread2
            // 
            this.btnToggleThread2.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnToggleThread2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnToggleThread2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnToggleThread2.Location = new System.Drawing.Point(28, 131);
            this.btnToggleThread2.Name = "btnToggleThread2";
            this.btnToggleThread2.Size = new System.Drawing.Size(172, 34);
            this.btnToggleThread2.TabIndex = 1;
            this.btnToggleThread2.Text = "Start Thread 2";
            this.btnToggleThread2.UseVisualStyleBackColor = false;
            this.btnToggleThread2.Click += new System.EventHandler(this.btnToggleThread2_Click);
            // 
            // lblCacheInfo
            // 
            this.lblCacheInfo.AutoSize = true;
            this.lblCacheInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCacheInfo.Location = new System.Drawing.Point(25, 35);
            this.lblCacheInfo.Name = "lblCacheInfo";
            this.lblCacheInfo.Size = new System.Drawing.Size(103, 13);
            this.lblCacheInfo.TabIndex = 2;
            this.lblCacheInfo.Text = "Cache Is EMPTY";
            // 
            // btnPurge
            // 
            this.btnPurge.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnPurge.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnPurge.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPurge.Location = new System.Drawing.Point(28, 216);
            this.btnPurge.Name = "btnPurge";
            this.btnPurge.Size = new System.Drawing.Size(172, 34);
            this.btnPurge.TabIndex = 4;
            this.btnPurge.Text = "Purge Cache";
            this.btnPurge.UseVisualStyleBackColor = false;
            this.btnPurge.Click += new System.EventHandler(this.btnPurge_Click);
            // 
            // btnForceGC
            // 
            this.btnForceGC.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnForceGC.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnForceGC.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnForceGC.Location = new System.Drawing.Point(28, 174);
            this.btnForceGC.Name = "btnForceGC";
            this.btnForceGC.Size = new System.Drawing.Size(172, 34);
            this.btnForceGC.TabIndex = 5;
            this.btnForceGC.Text = "Force GC";
            this.btnForceGC.UseVisualStyleBackColor = false;
            this.btnForceGC.Click += new System.EventHandler(this.btnForceGC_Click);
            // 
            // frmTestWeakCache
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(225, 262);
            this.Controls.Add(this.btnForceGC);
            this.Controls.Add(this.btnPurge);
            this.Controls.Add(this.lblCacheInfo);
            this.Controls.Add(this.btnToggleThread2);
            this.Controls.Add(this.btnToggleThread1);
            this.ForeColor = System.Drawing.SystemColors.WindowText;
            this.Name = "frmTestWeakCache";
            this.Text = "Test Weak Cache";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnToggleThread1;
        private System.Windows.Forms.Button btnToggleThread2;
        private System.Windows.Forms.Label lblCacheInfo;
        private System.Windows.Forms.Button btnPurge;
        private System.Windows.Forms.Button btnForceGC;
    }
}