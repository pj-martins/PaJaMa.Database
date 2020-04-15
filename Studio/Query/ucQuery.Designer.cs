namespace PaJaMa.Database.Studio.Query
{
	partial class ucQuery
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
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveQueryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openQueryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.tabMain = new PaJaMa.WinControls.TabControl.TabControl();
			this.openHistoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// menuStrip1
			// 
			this.menuStrip1.ImageScalingSize = new System.Drawing.Size(40, 40);
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveToolStripMenuItem,
            this.loadToolStripMenuItem,
            this.copyToolStripMenuItem,
            this.saveQueryToolStripMenuItem,
            this.openQueryToolStripMenuItem,
            this.openHistoryToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(707, 24);
			this.menuStrip1.TabIndex = 1;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// saveToolStripMenuItem
			// 
			this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
			this.saveToolStripMenuItem.Size = new System.Drawing.Size(104, 20);
			this.saveToolStripMenuItem.Text = "Save Workspace";
			this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
			// 
			// loadToolStripMenuItem
			// 
			this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
			this.loadToolStripMenuItem.Size = new System.Drawing.Size(106, 20);
			this.loadToolStripMenuItem.Text = "Load Workspace";
			this.loadToolStripMenuItem.Click += new System.EventHandler(this.loadToolStripMenuItem_Click);
			// 
			// copyToolStripMenuItem
			// 
			this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
			this.copyToolStripMenuItem.Size = new System.Drawing.Size(108, 20);
			this.copyToolStripMenuItem.Text = "Copy Workspace";
			this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
			// 
			// saveQueryToolStripMenuItem
			// 
			this.saveQueryToolStripMenuItem.Name = "saveQueryToolStripMenuItem";
			this.saveQueryToolStripMenuItem.Size = new System.Drawing.Size(78, 20);
			this.saveQueryToolStripMenuItem.Text = "Save Query";
			this.saveQueryToolStripMenuItem.Click += new System.EventHandler(this.SaveQueryToolStripMenuItem_Click);
			// 
			// openQueryToolStripMenuItem
			// 
			this.openQueryToolStripMenuItem.Name = "openQueryToolStripMenuItem";
			this.openQueryToolStripMenuItem.Size = new System.Drawing.Size(83, 20);
			this.openQueryToolStripMenuItem.Text = "Open Query";
			this.openQueryToolStripMenuItem.Click += new System.EventHandler(this.OpenQueryToolStripMenuItem_Click);
			// 
			// tabMain
			// 
			this.tabMain.AllowAdd = true;
			this.tabMain.AllowRemove = true;
			this.tabMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabMain.Location = new System.Drawing.Point(0, 24);
			this.tabMain.Name = "tabMain";
			this.tabMain.SelectedTab = null;
			this.tabMain.Size = new System.Drawing.Size(707, 520);
			this.tabMain.TabIndex = 2;
			this.tabMain.TabClosing += new PaJaMa.WinControls.TabControl.TabEventHandler(this.tabMain_TabClosing);
			this.tabMain.TabAdding += new PaJaMa.WinControls.TabControl.TabEventHandler(this.tabMain_TabAdding);
			// 
			// openHistoryToolStripMenuItem
			// 
			this.openHistoryToolStripMenuItem.Name = "openHistoryToolStripMenuItem";
			this.openHistoryToolStripMenuItem.Size = new System.Drawing.Size(89, 20);
			this.openHistoryToolStripMenuItem.Text = "Open History";
			this.openHistoryToolStripMenuItem.Click += new System.EventHandler(this.OpenHistoryToolStripMenuItem_Click);
			// 
			// ucQuery
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tabMain);
			this.Controls.Add(this.menuStrip1);
			this.Name = "ucQuery";
			this.Size = new System.Drawing.Size(707, 544);
			this.Load += new System.EventHandler(this.frmMain_Load);
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip menuStrip1;
		private PaJaMa.WinControls.TabControl.TabControl tabMain;
		private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem saveQueryToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openQueryToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openHistoryToolStripMenuItem;
	}
}