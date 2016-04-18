namespace XenAdmin.ConsoleView
{
    partial class VNCTabView
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
            Program.AssertOnEventThread();

            UnregisterEventListeners();

            if (disposing && (components != null))
            {
                components.Dispose();
            }

            if (disposing && vncScreen != null && !vncScreen.IsDisposed)
            {
                vncScreen.Dispose();
            }

            if (this.fullscreenForm != null)
            {
                fullscreenForm.Hide();
                fullscreenForm.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VNCTabView));
            this.buttonPanel = new System.Windows.Forms.Panel();
            this.gradientPanel1 = new XenAdmin.Controls.GradientPanel.GradientPanel();
            this.HostLabel = new System.Windows.Forms.Label();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonSSH = new System.Windows.Forms.Button();
            this.toggleConsoleButton = new System.Windows.Forms.Button();
            this.multipleDvdIsoList1 = new XenAdmin.Controls.MultipleDvdIsoList();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.sendCAD = new System.Windows.Forms.Button();
            this.contentPanel = new System.Windows.Forms.Panel();
            this.bottomPanel = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox1 = new XenAdmin.Controls.DecentGroupBox();
            this.scaleCheckBox = new System.Windows.Forms.CheckBox();
            this.fullscreenButton = new System.Windows.Forms.Button();
            this.dockButton = new System.Windows.Forms.Button();
            this.tip = new System.Windows.Forms.ToolTip(this.components);
            this.LifeCycleMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.panel2 = new System.Windows.Forms.Panel();
            this.powerStateLabel = new System.Windows.Forms.Label();
            this.dedicatedGpuWarning = new System.Windows.Forms.Label();
            this.buttonPanel.SuspendLayout();
            this.gradientPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.bottomPanel.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonPanel
            // 
            this.buttonPanel.Controls.Add(this.gradientPanel1);
            resources.ApplyResources(this.buttonPanel, "buttonPanel");
            this.buttonPanel.Name = "buttonPanel";
            // 
            // gradientPanel1
            // 
            resources.ApplyResources(this.gradientPanel1, "gradientPanel1");
            this.gradientPanel1.Controls.Add(this.tableLayoutPanel2);
            this.gradientPanel1.Name = "gradientPanel1";
            this.gradientPanel1.Scheme = XenAdmin.Controls.GradientPanel.GradientPanel.Schemes.Tab;
            // 
            // HostLabel
            // 
            this.HostLabel.AutoEllipsis = true;
            resources.ApplyResources(this.HostLabel, "HostLabel");
            this.HostLabel.ForeColor = System.Drawing.Color.White;
            this.HostLabel.Name = "HostLabel";
            // 
            // tableLayoutPanel2
            // 
            resources.ApplyResources(this.tableLayoutPanel2, "tableLayoutPanel2");
            this.tableLayoutPanel2.Controls.Add(this.HostLabel, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.buttonSSH, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.toggleConsoleButton, 5, 0);
            this.tableLayoutPanel2.Controls.Add(this.multipleDvdIsoList1, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.pictureBox1, 1, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            // 
            // buttonSSH
            // 
            resources.ApplyResources(this.buttonSSH, "buttonSSH");
            this.buttonSSH.Name = "buttonSSH";
            this.buttonSSH.UseVisualStyleBackColor = true;
            this.buttonSSH.Click += new System.EventHandler(this.buttonSSH_Click);
            // 
            // toggleConsoleButton
            // 
            resources.ApplyResources(this.toggleConsoleButton, "toggleConsoleButton");
            this.toggleConsoleButton.Name = "toggleConsoleButton";
            this.tip.SetToolTip(this.toggleConsoleButton, resources.GetString("toggleConsoleButton.ToolTip"));
            this.toggleConsoleButton.UseVisualStyleBackColor = true;
            this.toggleConsoleButton.Click += new System.EventHandler(this.toggleConsoleButton_Click);
            // 
            // multipleDvdIsoList1
            // 
            resources.ApplyResources(this.multipleDvdIsoList1, "multipleDvdIsoList1");
            this.multipleDvdIsoList1.Name = "multipleDvdIsoList1";
            this.multipleDvdIsoList1.VM = null;
            // 
            // pictureBox1
            // 
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Image = global::XenAdmin.Properties.Resources._001_LifeCycle_h32bit_24;
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.LifeCycleButton_MouseClick);
            this.pictureBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseDown);
            this.pictureBox1.MouseEnter += new System.EventHandler(this.pictureBox1_MouseEnter);
            this.pictureBox1.MouseLeave += new System.EventHandler(this.pictureBox1_MouseLeave);
            this.pictureBox1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseUp);
            // 
            // sendCAD
            // 
            resources.ApplyResources(this.sendCAD, "sendCAD");
            this.sendCAD.Name = "sendCAD";
            this.sendCAD.UseVisualStyleBackColor = true;
            this.sendCAD.Click += new System.EventHandler(this.sendCAD_Click);
            // 
            // contentPanel
            // 
            resources.ApplyResources(this.contentPanel, "contentPanel");
            this.contentPanel.Name = "contentPanel";
            // 
            // bottomPanel
            // 
            this.bottomPanel.Controls.Add(this.tableLayoutPanel1);
            resources.ApplyResources(this.bottomPanel, "bottomPanel");
            this.bottomPanel.Name = "bottomPanel";
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.sendCAD, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.scaleCheckBox, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.fullscreenButton, 5, 0);
            this.tableLayoutPanel1.Controls.Add(this.dockButton, 3, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // groupBox1
            // 
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // scaleCheckBox
            // 
            resources.ApplyResources(this.scaleCheckBox, "scaleCheckBox");
            this.scaleCheckBox.Name = "scaleCheckBox";
            this.scaleCheckBox.UseVisualStyleBackColor = true;
            this.scaleCheckBox.CheckedChanged += new System.EventHandler(this.scaleCheckBox_CheckedChanged);
            // 
            // fullscreenButton
            // 
            resources.ApplyResources(this.fullscreenButton, "fullscreenButton");
            this.fullscreenButton.Name = "fullscreenButton";
            this.fullscreenButton.UseVisualStyleBackColor = true;
            this.fullscreenButton.Click += new System.EventHandler(this.fullscreenButton_Click);
            // 
            // dockButton
            // 
            resources.ApplyResources(this.dockButton, "dockButton");
            this.dockButton.Image = global::XenAdmin.Properties.Resources.detach_24;
            this.dockButton.Name = "dockButton";
            this.dockButton.UseVisualStyleBackColor = true;
            this.dockButton.Click += new System.EventHandler(this.dockButton_Click);
            // 
            // tip
            // 
            this.tip.ShowAlways = true;
            // 
            // LifeCycleMenuStrip
            // 
            this.LifeCycleMenuStrip.Name = "LifeCycleMenuStrip";
            resources.ApplyResources(this.LifeCycleMenuStrip, "LifeCycleMenuStrip");
            this.LifeCycleMenuStrip.Closing += new System.Windows.Forms.ToolStripDropDownClosingEventHandler(this.LifeCycleMenuStrip_Closing);
            this.LifeCycleMenuStrip.Opened += new System.EventHandler(this.LifeCycleMenuStrip_Opened);
            // 
            // panel2
            // 
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.BackColor = System.Drawing.Color.Transparent;
            this.panel2.Controls.Add(this.buttonPanel);
            this.panel2.Name = "panel2";
            // 
            // powerStateLabel
            // 
            this.powerStateLabel.AutoEllipsis = true;
            this.powerStateLabel.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.powerStateLabel, "powerStateLabel");
            this.powerStateLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.powerStateLabel.Name = "powerStateLabel";
            this.powerStateLabel.Click += new System.EventHandler(this.powerStateLabel_Click);
            // 
            // dedicatedGpuWarning
            // 
            this.dedicatedGpuWarning.AutoEllipsis = true;
            this.dedicatedGpuWarning.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.dedicatedGpuWarning, "dedicatedGpuWarning");
            this.dedicatedGpuWarning.ForeColor = System.Drawing.SystemColors.ControlText;
            this.dedicatedGpuWarning.Name = "dedicatedGpuWarning";
            // 
            // VNCTabView
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.contentPanel);
            this.Controls.Add(this.dedicatedGpuWarning);
            this.Controls.Add(this.powerStateLabel);
            this.Controls.Add(this.bottomPanel);
            this.Controls.Add(this.panel2);
            this.Name = "VNCTabView";
            this.buttonPanel.ResumeLayout(false);
            this.gradientPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.bottomPanel.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel buttonPanel;
        public System.Windows.Forms.Button dockButton;
        private System.Windows.Forms.Button sendCAD;
        private System.Windows.Forms.Panel contentPanel;
        private System.Windows.Forms.Panel bottomPanel;
        public System.Windows.Forms.CheckBox scaleCheckBox;
        private System.Windows.Forms.Button fullscreenButton;
        private XenAdmin.Controls.DecentGroupBox groupBox1;
        private System.Windows.Forms.ToolTip tip;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ContextMenuStrip LifeCycleMenuStrip;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Panel panel2;
        private XenAdmin.Controls.GradientPanel.GradientPanel gradientPanel1;
        private System.Windows.Forms.Label HostLabel;
        public System.Windows.Forms.Button toggleConsoleButton;
        private XenAdmin.Controls.MultipleDvdIsoList multipleDvdIsoList1;
        private System.Windows.Forms.Label powerStateLabel;
        private System.Windows.Forms.Label dedicatedGpuWarning;
        public System.Windows.Forms.Button buttonSSH;
    }
}
