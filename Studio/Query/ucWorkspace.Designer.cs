namespace PaJaMa.Database.Studio.Query
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
			this.btnSearch = new System.Windows.Forms.Button();
			this.lblConnString = new System.Windows.Forms.Label();
			this.treeTables = new System.Windows.Forms.TreeView();
			this.mnuTree = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.selectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.selectTop1000ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.scriptCreateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.renameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.buildQueryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.newForeignKeyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.newColumnToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.newTableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.scriptInsertToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.splitMain = new System.Windows.Forms.SplitContainer();
			this.pnlSearch = new System.Windows.Forms.Panel();
			this.btnListResults = new System.Windows.Forms.Button();
			this.txtSearchColumn = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.txtSearchTable = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.btnDoSearch = new System.Windows.Forms.Button();
			this.tabOutputs = new PaJaMa.WinControls.TabControl.TabControl();
			this.pnlConnect.SuspendLayout();
			this.pnlControls.SuspendLayout();
			this.mnuTree.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
			this.splitMain.Panel1.SuspendLayout();
			this.splitMain.Panel2.SuspendLayout();
			this.splitMain.SuspendLayout();
			this.pnlSearch.SuspendLayout();
			this.SuspendLayout();
			// 
			// txtConnectionString
			// 
			this.txtConnectionString.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtConnectionString.Location = new System.Drawing.Point(79, 12);
			this.txtConnectionString.Name = "txtConnectionString";
			this.txtConnectionString.Size = new System.Drawing.Size(538, 21);
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
			this.pnlConnect.Size = new System.Drawing.Size(997, 44);
			this.pnlConnect.TabIndex = 5;
			// 
			// chkUseDummyDA
			// 
			this.chkUseDummyDA.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.chkUseDummyDA.AutoSize = true;
			this.chkUseDummyDA.Location = new System.Drawing.Point(623, 13);
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
			this.btnConnect.Location = new System.Drawing.Point(883, 10);
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
			this.cboServer.Location = new System.Drawing.Point(735, 11);
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
			this.pnlControls.Controls.Add(this.btnSearch);
			this.pnlControls.Controls.Add(this.lblConnString);
			this.pnlControls.Controls.Add(this.btnDisconnect);
			this.pnlControls.Dock = System.Windows.Forms.DockStyle.Top;
			this.pnlControls.Location = new System.Drawing.Point(0, 0);
			this.pnlControls.Name = "pnlControls";
			this.pnlControls.Size = new System.Drawing.Size(997, 40);
			this.pnlControls.TabIndex = 4;
			this.pnlControls.Visible = false;
			// 
			// btnSearch
			// 
			this.btnSearch.Location = new System.Drawing.Point(123, 8);
			this.btnSearch.Name = "btnSearch";
			this.btnSearch.Size = new System.Drawing.Size(102, 23);
			this.btnSearch.TabIndex = 11;
			this.btnSearch.Text = "Search";
			this.btnSearch.UseVisualStyleBackColor = true;
			this.btnSearch.Click += new System.EventHandler(this.BtnSearch_Click);
			// 
			// lblConnString
			// 
			this.lblConnString.AutoSize = true;
			this.lblConnString.Location = new System.Drawing.Point(231, 13);
			this.lblConnString.Name = "lblConnString";
			this.lblConnString.Size = new System.Drawing.Size(35, 13);
			this.lblConnString.TabIndex = 10;
			this.lblConnString.Text = "label2";
			// 
			// treeTables
			// 
			this.treeTables.ContextMenuStrip = this.mnuTree;
			this.treeTables.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeTables.Location = new System.Drawing.Point(0, 82);
			this.treeTables.Name = "treeTables";
			this.treeTables.Size = new System.Drawing.Size(213, 545);
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
            this.renameToolStripMenuItem,
            this.buildQueryToolStripMenuItem,
            this.refreshToolStripMenuItem,
            this.newForeignKeyToolStripMenuItem,
            this.newColumnToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.newTableToolStripMenuItem,
            this.scriptInsertToolStripMenuItem});
			this.mnuTree.Name = "mnuTree";
			this.mnuTree.Size = new System.Drawing.Size(164, 246);
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
			// renameToolStripMenuItem
			// 
			this.renameToolStripMenuItem.Name = "renameToolStripMenuItem";
			this.renameToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
			this.renameToolStripMenuItem.Text = "Re&name";
			this.renameToolStripMenuItem.Click += new System.EventHandler(this.RenameToolStripMenuItem_Click);
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
			// scriptInsertToolStripMenuItem
			// 
			this.scriptInsertToolStripMenuItem.Name = "scriptInsertToolStripMenuItem";
			this.scriptInsertToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
			this.scriptInsertToolStripMenuItem.Text = "Script &Insert";
			this.scriptInsertToolStripMenuItem.Click += new System.EventHandler(this.ScriptInsertToolStripMenuItem_Click);
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
			this.splitMain.Panel1.Controls.Add(this.pnlSearch);
			// 
			// splitMain.Panel2
			// 
			this.splitMain.Panel2.AutoScroll = true;
			this.splitMain.Panel2.Controls.Add(this.tabOutputs);
			this.splitMain.Size = new System.Drawing.Size(997, 627);
			this.splitMain.SplitterDistance = 213;
			this.splitMain.TabIndex = 9;
			// 
			// pnlSearch
			// 
			this.pnlSearch.Controls.Add(this.btnListResults);
			this.pnlSearch.Controls.Add(this.txtSearchColumn);
			this.pnlSearch.Controls.Add(this.label4);
			this.pnlSearch.Controls.Add(this.txtSearchTable);
			this.pnlSearch.Controls.Add(this.label3);
			this.pnlSearch.Controls.Add(this.btnDoSearch);
			this.pnlSearch.Dock = System.Windows.Forms.DockStyle.Top;
			this.pnlSearch.Location = new System.Drawing.Point(0, 0);
			this.pnlSearch.Name = "pnlSearch";
			this.pnlSearch.Size = new System.Drawing.Size(213, 82);
			this.pnlSearch.TabIndex = 9;
			this.pnlSearch.Visible = false;
			// 
			// btnListResults
			// 
			this.btnListResults.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnListResults.Location = new System.Drawing.Point(69, 56);
			this.btnListResults.Name = "btnListResults";
			this.btnListResults.Size = new System.Drawing.Size(85, 23);
			this.btnListResults.TabIndex = 7;
			this.btnListResults.Text = "List Results";
			this.btnListResults.UseVisualStyleBackColor = true;
			this.btnListResults.Visible = false;
			this.btnListResults.Click += new System.EventHandler(this.btnListResults_Click);
			// 
			// txtSearchColumn
			// 
			this.txtSearchColumn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtSearchColumn.Location = new System.Drawing.Point(69, 32);
			this.txtSearchColumn.Name = "txtSearchColumn";
			this.txtSearchColumn.Size = new System.Drawing.Size(141, 20);
			this.txtSearchColumn.TabIndex = 6;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(10, 35);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(42, 13);
			this.label4.TabIndex = 5;
			this.label4.Text = "Column";
			// 
			// txtSearchTable
			// 
			this.txtSearchTable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtSearchTable.Location = new System.Drawing.Point(69, 6);
			this.txtSearchTable.Name = "txtSearchTable";
			this.txtSearchTable.Size = new System.Drawing.Size(141, 20);
			this.txtSearchTable.TabIndex = 4;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(10, 9);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(34, 13);
			this.label3.TabIndex = 3;
			this.label3.Text = "Table";
			// 
			// btnDoSearch
			// 
			this.btnDoSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnDoSearch.Location = new System.Drawing.Point(161, 56);
			this.btnDoSearch.Name = "btnDoSearch";
			this.btnDoSearch.Size = new System.Drawing.Size(49, 23);
			this.btnDoSearch.TabIndex = 0;
			this.btnDoSearch.Text = "Go";
			this.btnDoSearch.UseVisualStyleBackColor = true;
			this.btnDoSearch.Click += new System.EventHandler(this.BtnDoSearch_Click);
			// 
			// tabOutputs
			// 
			this.tabOutputs.AllowAdd = true;
			this.tabOutputs.AllowRemove = true;
			this.tabOutputs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabOutputs.Location = new System.Drawing.Point(0, 0);
			this.tabOutputs.Name = "tabOutputs";
			this.tabOutputs.SelectedTab = null;
			this.tabOutputs.Size = new System.Drawing.Size(780, 627);
			this.tabOutputs.TabIndex = 0;
			this.tabOutputs.Visible = false;
			this.tabOutputs.TabClosing += new PaJaMa.WinControls.TabControl.TabEventHandler(this.tabOutputs_TabClosing);
			this.tabOutputs.TabAdding += new PaJaMa.WinControls.TabControl.TabEventHandler(this.tabOutputs_TabAdding);
			// 
			// ucWorkspace
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.splitMain);
			this.Controls.Add(this.pnlConnect);
			this.Controls.Add(this.pnlControls);
			this.Name = "ucWorkspace";
			this.Size = new System.Drawing.Size(997, 711);
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
			this.pnlSearch.ResumeLayout(false);
			this.pnlSearch.PerformLayout();
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
		private System.Windows.Forms.ToolStripMenuItem renameToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem scriptInsertToolStripMenuItem;
		private System.Windows.Forms.Button btnSearch;
		private System.Windows.Forms.Panel pnlSearch;
		private System.Windows.Forms.TextBox txtSearchColumn;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox txtSearchTable;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button btnDoSearch;
		private System.Windows.Forms.Button btnListResults;
	}
}

