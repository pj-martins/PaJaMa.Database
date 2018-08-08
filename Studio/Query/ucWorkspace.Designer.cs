﻿namespace PaJaMa.Database.Studio.Query
{
	partial class ucWorkspace
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
			this.components = new System.ComponentModel.Container();
			this.txtConnectionString = new System.Windows.Forms.ComboBox();
			this.pnlConnect = new System.Windows.Forms.Panel();
			this.chkUseDummyDA = new System.Windows.Forms.CheckBox();
			this.label1 = new System.Windows.Forms.Label();
			this.btnConnect = new System.Windows.Forms.Button();
			this.cboServer = new System.Windows.Forms.ComboBox();
			this.btnDisconnect = new System.Windows.Forms.Button();
			this.pnlControls = new System.Windows.Forms.Panel();
			this.lblConnString = new System.Windows.Forms.Label();
			this.btnShowHideTables = new System.Windows.Forms.Button();
			this.treeTables = new System.Windows.Forms.TreeView();
			this.mnuTree = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.selectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.selectTop1000ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.scriptCreateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.buildQueryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.newForeignKeyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.newColumnToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.newTableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.splitMain = new System.Windows.Forms.SplitContainer();
			this.tabOutputs = new PaJaMa.WinControls.TabControl.TabControl();
			this.timSaveTemp = new System.Windows.Forms.Timer(this.components);
			this.pnlConnect.SuspendLayout();
			this.pnlControls.SuspendLayout();
			this.mnuTree.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
			this.splitMain.Panel1.SuspendLayout();
			this.splitMain.Panel2.SuspendLayout();
			this.splitMain.SuspendLayout();
			this.SuspendLayout();
			// 
			// txtConnectionString
			// 
			this.txtConnectionString.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtConnectionString.Location = new System.Drawing.Point(79, 12);
			this.txtConnectionString.Name = "txtConnectionString";
			this.txtConnectionString.Size = new System.Drawing.Size(296, 21);
			this.txtConnectionString.TabIndex = 0;
			// 
			// pnlConnect
			// 
			this.pnlConnect.Controls.Add(this.chkUseDummyDA);
			this.pnlConnect.Controls.Add(this.label1);
			this.pnlConnect.Controls.Add(this.btnConnect);
			this.pnlConnect.Controls.Add(this.cboServer);
			this.pnlConnect.Controls.Add(this.txtConnectionString);
			this.pnlConnect.Dock = System.Windows.Forms.DockStyle.Top;
			this.pnlConnect.Location = new System.Drawing.Point(0, 40);
			this.pnlConnect.Name = "pnlConnect";
			this.pnlConnect.Size = new System.Drawing.Size(755, 44);
			this.pnlConnect.TabIndex = 5;
			// 
			// chkUseDummyDA
			// 
			this.chkUseDummyDA.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.chkUseDummyDA.AutoSize = true;
			this.chkUseDummyDA.Location = new System.Drawing.Point(381, 13);
			this.chkUseDummyDA.Name = "chkUseDummyDA";
			this.chkUseDummyDA.Size = new System.Drawing.Size(106, 17);
			this.chkUseDummyDA.TabIndex = 5;
			this.chkUseDummyDA.Text = "Init Data Adapter";
			this.chkUseDummyDA.UseVisualStyleBackColor = true;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 15);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(61, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "Connection";
			// 
			// btnConnect
			// 
			this.btnConnect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnConnect.Location = new System.Drawing.Point(641, 10);
			this.btnConnect.Name = "btnConnect";
			this.btnConnect.Size = new System.Drawing.Size(102, 23);
			this.btnConnect.TabIndex = 3;
			this.btnConnect.Text = "Connect";
			this.btnConnect.UseVisualStyleBackColor = true;
			this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
			// 
			// cboServer
			// 
			this.cboServer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cboServer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboServer.FormattingEnabled = true;
			this.cboServer.Location = new System.Drawing.Point(493, 11);
			this.cboServer.Name = "cboServer";
			this.cboServer.Size = new System.Drawing.Size(142, 21);
			this.cboServer.TabIndex = 1;
			this.cboServer.Format += new System.Windows.Forms.ListControlConvertEventHandler(this.cboServer_Format);
			// 
			// btnDisconnect
			// 
			this.btnDisconnect.Location = new System.Drawing.Point(15, 8);
			this.btnDisconnect.Name = "btnDisconnect";
			this.btnDisconnect.Size = new System.Drawing.Size(102, 23);
			this.btnDisconnect.TabIndex = 5;
			this.btnDisconnect.Text = "Disconnect";
			this.btnDisconnect.UseVisualStyleBackColor = true;
			this.btnDisconnect.Click += new System.EventHandler(this.btnDisconnect_Click);
			// 
			// pnlControls
			// 
			this.pnlControls.Controls.Add(this.lblConnString);
			this.pnlControls.Controls.Add(this.btnShowHideTables);
			this.pnlControls.Controls.Add(this.btnDisconnect);
			this.pnlControls.Dock = System.Windows.Forms.DockStyle.Top;
			this.pnlControls.Location = new System.Drawing.Point(0, 0);
			this.pnlControls.Name = "pnlControls";
			this.pnlControls.Size = new System.Drawing.Size(755, 40);
			this.pnlControls.TabIndex = 4;
			this.pnlControls.Visible = false;
			// 
			// lblConnString
			// 
			this.lblConnString.AutoSize = true;
			this.lblConnString.Location = new System.Drawing.Point(278, 13);
			this.lblConnString.Name = "lblConnString";
			this.lblConnString.Size = new System.Drawing.Size(35, 13);
			this.lblConnString.TabIndex = 10;
			this.lblConnString.Text = "label2";
			// 
			// btnShowHideTables
			// 
			this.btnShowHideTables.Location = new System.Drawing.Point(122, 8);
			this.btnShowHideTables.Name = "btnShowHideTables";
			this.btnShowHideTables.Size = new System.Drawing.Size(150, 23);
			this.btnShowHideTables.TabIndex = 8;
			this.btnShowHideTables.Text = "Show/Hide Tables";
			this.btnShowHideTables.UseVisualStyleBackColor = true;
			this.btnShowHideTables.Click += new System.EventHandler(this.btnShowHideTables_Click);
			// 
			// treeTables
			// 
			this.treeTables.ContextMenuStrip = this.mnuTree;
			this.treeTables.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeTables.Location = new System.Drawing.Point(0, 0);
			this.treeTables.Name = "treeTables";
			this.treeTables.Size = new System.Drawing.Size(162, 533);
			this.treeTables.TabIndex = 8;
			this.treeTables.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeTables_BeforeExpand);
			this.treeTables.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeTables_NodeMouseClick);
			this.treeTables.KeyUp += new System.Windows.Forms.KeyEventHandler(this.treeTables_KeyUp);
			// 
			// mnuTree
			// 
			this.mnuTree.ImageScalingSize = new System.Drawing.Size(40, 40);
			this.mnuTree.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectToolStripMenuItem,
            this.selectTop1000ToolStripMenuItem,
            this.scriptCreateToolStripMenuItem,
            this.buildQueryToolStripMenuItem,
            this.refreshToolStripMenuItem,
            this.newForeignKeyToolStripMenuItem,
            this.newColumnToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.newTableToolStripMenuItem});
			this.mnuTree.Name = "mnuTree";
			this.mnuTree.Size = new System.Drawing.Size(164, 202);
			this.mnuTree.Opening += new System.ComponentModel.CancelEventHandler(this.mnuTree_Opening);
			// 
			// selectToolStripMenuItem
			// 
			this.selectToolStripMenuItem.Name = "selectToolStripMenuItem";
			this.selectToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
			this.selectToolStripMenuItem.Text = "Select";
			this.selectToolStripMenuItem.Click += new System.EventHandler(this.selectToolStripMenuItem_Click);
			// 
			// selectTop1000ToolStripMenuItem
			// 
			this.selectTop1000ToolStripMenuItem.Name = "selectTop1000ToolStripMenuItem";
			this.selectTop1000ToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
			this.selectTop1000ToolStripMenuItem.Text = "Select Top 1000";
			this.selectTop1000ToolStripMenuItem.Click += new System.EventHandler(this.selectTop1000ToolStripMenuItem_Click);
			// 
			// scriptCreateToolStripMenuItem
			// 
			this.scriptCreateToolStripMenuItem.Name = "scriptCreateToolStripMenuItem";
			this.scriptCreateToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
			this.scriptCreateToolStripMenuItem.Text = "Script &Create";
			this.scriptCreateToolStripMenuItem.Click += new System.EventHandler(this.scriptCreateToolStripMenuItem_Click);
			// 
			// buildQueryToolStripMenuItem
			// 
			this.buildQueryToolStripMenuItem.Name = "buildQueryToolStripMenuItem";
			this.buildQueryToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
			this.buildQueryToolStripMenuItem.Text = "&Build Query";
			this.buildQueryToolStripMenuItem.Click += new System.EventHandler(this.buildQueryToolStripMenuItem_Click);
			// 
			// refreshToolStripMenuItem
			// 
			this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
			this.refreshToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
			this.refreshToolStripMenuItem.Text = "&Refresh";
			this.refreshToolStripMenuItem.Click += new System.EventHandler(this.refreshToolStripMenuItem_Click);
			// 
			// newForeignKeyToolStripMenuItem
			// 
			this.newForeignKeyToolStripMenuItem.Name = "newForeignKeyToolStripMenuItem";
			this.newForeignKeyToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
			this.newForeignKeyToolStripMenuItem.Text = "New &Foreign Key";
			this.newForeignKeyToolStripMenuItem.Click += new System.EventHandler(this.newForeignKeyToolStripMenuItem_Click);
			// 
			// newColumnToolStripMenuItem
			// 
			this.newColumnToolStripMenuItem.Name = "newColumnToolStripMenuItem";
			this.newColumnToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
			this.newColumnToolStripMenuItem.Text = "New C&olumn";
			this.newColumnToolStripMenuItem.Click += new System.EventHandler(this.newColumnToolStripMenuItem_Click);
			// 
			// deleteToolStripMenuItem
			// 
			this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
			this.deleteToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
			this.deleteToolStripMenuItem.Text = "&Delete";
			this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
			// 
			// newTableToolStripMenuItem
			// 
			this.newTableToolStripMenuItem.Name = "newTableToolStripMenuItem";
			this.newTableToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
			this.newTableToolStripMenuItem.Text = "New &Table";
			this.newTableToolStripMenuItem.Click += new System.EventHandler(this.newTableToolStripMenuItem_Click);
			// 
			// splitMain
			// 
			this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitMain.Location = new System.Drawing.Point(0, 84);
			this.splitMain.Name = "splitMain";
			// 
			// splitMain.Panel1
			// 
			this.splitMain.Panel1.Controls.Add(this.treeTables);
			// 
			// splitMain.Panel2
			// 
			this.splitMain.Panel2.AutoScroll = true;
			this.splitMain.Panel2.Controls.Add(this.tabOutputs);
			this.splitMain.Size = new System.Drawing.Size(755, 533);
			this.splitMain.SplitterDistance = 162;
			this.splitMain.TabIndex = 9;
			// 
			// tabOutputs
			// 
			this.tabOutputs.AllowAdd = true;
			this.tabOutputs.AllowRemove = true;
			this.tabOutputs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabOutputs.Location = new System.Drawing.Point(0, 0);
			this.tabOutputs.Name = "tabOutputs";
			this.tabOutputs.SelectedTab = null;
			this.tabOutputs.Size = new System.Drawing.Size(589, 533);
			this.tabOutputs.TabIndex = 0;
			this.tabOutputs.Visible = false;
			this.tabOutputs.TabClosing += new PaJaMa.WinControls.TabControl.TabEventHandler(this.tabOutputs_TabClosing);
			this.tabOutputs.TabAdding += new PaJaMa.WinControls.TabControl.TabEventHandler(this.tabOutputs_TabAdding);
			// 
			// timSaveTemp
			// 
			this.timSaveTemp.Enabled = true;
			this.timSaveTemp.Interval = 10000;
			this.timSaveTemp.Tick += new System.EventHandler(this.timSaveTemp_Tick);
			// 
			// ucWorkspace
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.splitMain);
			this.Controls.Add(this.pnlConnect);
			this.Controls.Add(this.pnlControls);
			this.Name = "ucWorkspace";
			this.Size = new System.Drawing.Size(755, 617);
			this.Load += new System.EventHandler(this.ucWorkspace_Load);
			this.pnlConnect.ResumeLayout(false);
			this.pnlConnect.PerformLayout();
			this.pnlControls.ResumeLayout(false);
			this.pnlControls.PerformLayout();
			this.mnuTree.ResumeLayout(false);
			this.splitMain.Panel1.ResumeLayout(false);
			this.splitMain.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
			this.splitMain.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ComboBox txtConnectionString;
		private System.Windows.Forms.Panel pnlConnect;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button btnConnect;
		private System.Windows.Forms.Button btnDisconnect;
		private System.Windows.Forms.Panel pnlControls;
		private System.Windows.Forms.TreeView treeTables;
		private System.Windows.Forms.SplitContainer splitMain;
		private System.Windows.Forms.Button btnShowHideTables;
		private System.Windows.Forms.ContextMenuStrip mnuTree;
		private System.Windows.Forms.ToolStripMenuItem selectTop1000ToolStripMenuItem;
		private System.Windows.Forms.Label lblConnString;
		private System.Windows.Forms.ToolStripMenuItem selectToolStripMenuItem;
		private PaJaMa.WinControls.TabControl.TabControl tabOutputs;
		private System.Windows.Forms.ComboBox cboServer;
		private System.Windows.Forms.ToolStripMenuItem scriptCreateToolStripMenuItem;
		private System.Windows.Forms.CheckBox chkUseDummyDA;
		private System.Windows.Forms.ToolStripMenuItem buildQueryToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem refreshToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem newForeignKeyToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem newColumnToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem newTableToolStripMenuItem;
		private System.Windows.Forms.Timer timSaveTemp;
	}
}

