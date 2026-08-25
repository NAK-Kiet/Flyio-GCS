namespace MissionPlanner.GCSViews
{
    partial class Help
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.helpScrollPanel = new System.Windows.Forms.Panel();
            this.contentLayout = new System.Windows.Forms.TableLayoutPanel();
            this.CHK_showconsole = new System.Windows.Forms.CheckBox();
            this.BUT_updatecheck = new MissionPlanner.Controls.MyButton();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.BUT_betaupdate = new MissionPlanner.Controls.MyButton();
            this.helpScrollPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // helpScrollPanel
            //
            this.helpScrollPanel.AutoScroll = true;
            this.helpScrollPanel.Controls.Add(this.contentLayout);
            this.helpScrollPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.helpScrollPanel.Location = new System.Drawing.Point(0, 0);
            this.helpScrollPanel.Name = "helpScrollPanel";
            this.helpScrollPanel.Size = new System.Drawing.Size(1000, 700);
            this.helpScrollPanel.TabIndex = 0;
            // 
            // contentLayout
            // 
            this.contentLayout.AutoSize = true;
            this.contentLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.contentLayout.ColumnCount = 1;
            this.contentLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.contentLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.contentLayout.Location = new System.Drawing.Point(0, 0);
            this.contentLayout.Name = "contentLayout";
            this.contentLayout.Padding = new System.Windows.Forms.Padding(32, 24, 32, 28);
            this.contentLayout.RowCount = 0;
            this.contentLayout.Size = new System.Drawing.Size(983, 68);
            this.contentLayout.TabIndex = 0;
            // 
            // CHK_showconsole
            // 
            this.CHK_showconsole.AutoSize = true;
            this.CHK_showconsole.Name = "CHK_showconsole";
            this.CHK_showconsole.Text = "Show console window";
            this.CHK_showconsole.UseVisualStyleBackColor = true;
            this.CHK_showconsole.CheckedChanged += new System.EventHandler(this.CHK_showconsole_CheckedChanged);
            // 
            // BUT_updatecheck
            // 
            this.BUT_updatecheck.Size = new System.Drawing.Size(170, 38);
            this.BUT_updatecheck.Name = "BUT_updatecheck";
            this.BUT_updatecheck.Text = "Check for Updates";
            this.BUT_updatecheck.UseVisualStyleBackColor = false;
            this.BUT_updatecheck.Click += new System.EventHandler(this.BUT_updatecheck_Click);
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "View Mission Planner changelog";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // BUT_betaupdate
            // 
            this.BUT_betaupdate.Size = new System.Drawing.Size(190, 38);
            this.BUT_betaupdate.Name = "BUT_betaupdate";
            this.BUT_betaupdate.Text = "Check for BETA Updates";
            this.BUT_betaupdate.UseVisualStyleBackColor = false;
            this.BUT_betaupdate.Click += new System.EventHandler(this.BUT_betaupdate_Click);
            // 
            // Help
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.helpScrollPanel);
            this.Name = "Help";
            this.Size = new System.Drawing.Size(1000, 700);
            this.Load += new System.EventHandler(this.Help_Load);
            this.helpScrollPanel.ResumeLayout(false);
            this.helpScrollPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel helpScrollPanel;
        private System.Windows.Forms.TableLayoutPanel contentLayout;
        private Controls.MyButton BUT_updatecheck;
        private System.Windows.Forms.CheckBox CHK_showconsole;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private Controls.MyButton BUT_betaupdate;

    }
}
