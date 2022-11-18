namespace PaJaMa.Database.Studio
{
    partial class frmConnections
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
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.gridMain = new System.Windows.Forms.DataGridView();
			this.ConnectionName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Server = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.txtTunnelUser = new System.Windows.Forms.TextBox();
			this.label11 = new System.Windows.Forms.Label();
			this.numForward = new System.Windows.Forms.NumericUpDown();
			this.label10 = new System.Windows.Forms.Label();
			this.txtTunnel = new System.Windows.Forms.TextBox();
			this.btnBrowseKey = new System.Windows.Forms.Button();
			this.numTunnelPort = new System.Windows.Forms.NumericUpDown();
			this.labelkey = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.txtTunnelKey = new System.Windows.Forms.TextBox();
			this.btnCopy = new System.Windows.Forms.Button();
			this.btnSave = new System.Windows.Forms.Button();
			this.chkIntegratedSecurity = new System.Windows.Forms.CheckBox();
			this.label8 = new System.Windows.Forms.Label();
			this.cboDataSource = new System.Windows.Forms.ComboBox();
			this.label7 = new System.Windows.Forms.Label();
			this.txtDatabase = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.txtConnectionName = new System.Windows.Forms.TextBox();
			this.btnAdd = new System.Windows.Forms.Button();
			this.btnRemove = new System.Windows.Forms.Button();
			this.label5 = new System.Windows.Forms.Label();
			this.numPort = new System.Windows.Forms.NumericUpDown();
			this.label4 = new System.Windows.Forms.Label();
			this.txtPassword = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.txtUser = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.txtAppend = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.txtServer = new System.Windows.Forms.TextBox();
			this.btnShow = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridMain)).BeginInit();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numForward)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numTunnelPort)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numPort)).BeginInit();
			this.SuspendLayout();
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.gridMain);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.btnShow);
			this.splitContainer1.Panel2.Controls.Add(this.groupBox1);
			this.splitContainer1.Panel2.Controls.Add(this.btnCopy);
			this.splitContainer1.Panel2.Controls.Add(this.btnSave);
			this.splitContainer1.Panel2.Controls.Add(this.chkIntegratedSecurity);
			this.splitContainer1.Panel2.Controls.Add(this.label8);
			this.splitContainer1.Panel2.Controls.Add(this.cboDataSource);
			this.splitContainer1.Panel2.Controls.Add(this.label7);
			this.splitContainer1.Panel2.Controls.Add(this.txtDatabase);
			this.splitContainer1.Panel2.Controls.Add(this.label6);
			this.splitContainer1.Panel2.Controls.Add(this.txtConnectionName);
			this.splitContainer1.Panel2.Controls.Add(this.btnAdd);
			this.splitContainer1.Panel2.Controls.Add(this.btnRemove);
			this.splitContainer1.Panel2.Controls.Add(this.label5);
			this.splitContainer1.Panel2.Controls.Add(this.numPort);
			this.splitContainer1.Panel2.Controls.Add(this.label4);
			this.splitContainer1.Panel2.Controls.Add(this.txtPassword);
			this.splitContainer1.Panel2.Controls.Add(this.label3);
			this.splitContainer1.Panel2.Controls.Add(this.txtUser);
			this.splitContainer1.Panel2.Controls.Add(this.label2);
			this.splitContainer1.Panel2.Controls.Add(this.txtAppend);
			this.splitContainer1.Panel2.Controls.Add(this.label1);
			this.splitContainer1.Panel2.Controls.Add(this.txtServer);
			this.splitContainer1.Size = new System.Drawing.Size(1052, 560);
			this.splitContainer1.SplitterDistance = 681;
			this.splitContainer1.TabIndex = 0;
			// 
			// gridMain
			// 
			this.gridMain.AllowUserToAddRows = false;
			this.gridMain.AllowUserToDeleteRows = false;
			this.gridMain.AllowUserToOrderColumns = true;
			this.gridMain.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.gridMain.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ConnectionName,
            this.Server});
			this.gridMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridMain.Location = new System.Drawing.Point(0, 0);
			this.gridMain.MultiSelect = false;
			this.gridMain.Name = "gridMain";
			this.gridMain.ReadOnly = true;
			this.gridMain.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.gridMain.Size = new System.Drawing.Size(681, 560);
			this.gridMain.TabIndex = 0;
			this.gridMain.SelectionChanged += new System.EventHandler(this.gridMain_SelectionChanged);
			// 
			// ConnectionName
			// 
			this.ConnectionName.DataPropertyName = "ConnectionName";
			this.ConnectionName.HeaderText = "Name";
			this.ConnectionName.Name = "ConnectionName";
			this.ConnectionName.ReadOnly = true;
			this.ConnectionName.Width = 200;
			// 
			// Server
			// 
			this.Server.DataPropertyName = "Server";
			this.Server.HeaderText = "Server";
			this.Server.Name = "Server";
			this.Server.ReadOnly = true;
			this.Server.Width = 400;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.txtTunnelUser);
			this.groupBox1.Controls.Add(this.label11);
			this.groupBox1.Controls.Add(this.numForward);
			this.groupBox1.Controls.Add(this.label10);
			this.groupBox1.Controls.Add(this.txtTunnel);
			this.groupBox1.Controls.Add(this.btnBrowseKey);
			this.groupBox1.Controls.Add(this.numTunnelPort);
			this.groupBox1.Controls.Add(this.labelkey);
			this.groupBox1.Controls.Add(this.label9);
			this.groupBox1.Controls.Add(this.txtTunnelKey);
			this.groupBox1.Location = new System.Drawing.Point(15, 252);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(340, 138);
			this.groupBox1.TabIndex = 24;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Tunnel";
			// 
			// txtTunnelUser
			// 
			this.txtTunnelUser.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtTunnelUser.Location = new System.Drawing.Point(63, 45);
			this.txtTunnelUser.Name = "txtTunnelUser";
			this.txtTunnelUser.Size = new System.Drawing.Size(271, 20);
			this.txtTunnelUser.TabIndex = 27;
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(9, 48);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(29, 13);
			this.label11.TabIndex = 26;
			this.label11.Text = "User";
			// 
			// numForward
			// 
			this.numForward.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.numForward.Location = new System.Drawing.Point(63, 71);
			this.numForward.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.numForward.Name = "numForward";
			this.numForward.Size = new System.Drawing.Size(115, 20);
			this.numForward.TabIndex = 25;
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(10, 73);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(45, 13);
			this.label10.TabIndex = 24;
			this.label10.Text = "Forward";
			// 
			// txtTunnel
			// 
			this.txtTunnel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtTunnel.Location = new System.Drawing.Point(63, 19);
			this.txtTunnel.Name = "txtTunnel";
			this.txtTunnel.Size = new System.Drawing.Size(208, 20);
			this.txtTunnel.TabIndex = 19;
			// 
			// btnBrowseKey
			// 
			this.btnBrowseKey.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnBrowseKey.Location = new System.Drawing.Point(309, 96);
			this.btnBrowseKey.Name = "btnBrowseKey";
			this.btnBrowseKey.Size = new System.Drawing.Size(24, 22);
			this.btnBrowseKey.TabIndex = 22;
			this.btnBrowseKey.Text = "...";
			this.btnBrowseKey.UseVisualStyleBackColor = true;
			this.btnBrowseKey.Click += new System.EventHandler(this.btnBrowseKey_Click);
			// 
			// numTunnelPort
			// 
			this.numTunnelPort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.numTunnelPort.Location = new System.Drawing.Point(277, 19);
			this.numTunnelPort.Name = "numTunnelPort";
			this.numTunnelPort.Size = new System.Drawing.Size(57, 20);
			this.numTunnelPort.TabIndex = 23;
			// 
			// labelkey
			// 
			this.labelkey.AutoSize = true;
			this.labelkey.Location = new System.Drawing.Point(10, 100);
			this.labelkey.Name = "labelkey";
			this.labelkey.Size = new System.Drawing.Size(25, 13);
			this.labelkey.TabIndex = 20;
			this.labelkey.Text = "Key";
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(9, 22);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(40, 13);
			this.label9.TabIndex = 18;
			this.label9.Text = "Tunnel";
			// 
			// txtTunnelKey
			// 
			this.txtTunnelKey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtTunnelKey.Location = new System.Drawing.Point(63, 97);
			this.txtTunnelKey.Name = "txtTunnelKey";
			this.txtTunnelKey.Size = new System.Drawing.Size(241, 20);
			this.txtTunnelKey.TabIndex = 21;
			// 
			// btnCopy
			// 
			this.btnCopy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCopy.Location = new System.Drawing.Point(118, 525);
			this.btnCopy.Name = "btnCopy";
			this.btnCopy.Size = new System.Drawing.Size(75, 23);
			this.btnCopy.TabIndex = 11;
			this.btnCopy.Text = "Copy";
			this.btnCopy.UseVisualStyleBackColor = true;
			this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
			// 
			// btnSave
			// 
			this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnSave.Location = new System.Drawing.Point(199, 525);
			this.btnSave.Name = "btnSave";
			this.btnSave.Size = new System.Drawing.Size(75, 23);
			this.btnSave.TabIndex = 12;
			this.btnSave.Text = "Save";
			this.btnSave.UseVisualStyleBackColor = true;
			this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
			// 
			// chkIntegratedSecurity
			// 
			this.chkIntegratedSecurity.AutoSize = true;
			this.chkIntegratedSecurity.Location = new System.Drawing.Point(15, 229);
			this.chkIntegratedSecurity.Name = "chkIntegratedSecurity";
			this.chkIntegratedSecurity.Size = new System.Drawing.Size(115, 17);
			this.chkIntegratedSecurity.TabIndex = 9;
			this.chkIntegratedSecurity.Text = "Integrated Security";
			this.chkIntegratedSecurity.UseVisualStyleBackColor = true;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(12, 41);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(31, 13);
			this.label8.TabIndex = 17;
			this.label8.Text = "Type";
			// 
			// cboDataSource
			// 
			this.cboDataSource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cboDataSource.DisplayMember = "ShortName";
			this.cboDataSource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboDataSource.FormattingEnabled = true;
			this.cboDataSource.Location = new System.Drawing.Point(71, 38);
			this.cboDataSource.Name = "cboDataSource";
			this.cboDataSource.Size = new System.Drawing.Size(284, 21);
			this.cboDataSource.TabIndex = 2;
			this.cboDataSource.ValueMember = "Type";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(12, 120);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(53, 13);
			this.label7.TabIndex = 15;
			this.label7.Text = "Database";
			// 
			// txtDatabase
			// 
			this.txtDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtDatabase.Location = new System.Drawing.Point(71, 117);
			this.txtDatabase.Name = "txtDatabase";
			this.txtDatabase.Size = new System.Drawing.Size(284, 20);
			this.txtDatabase.TabIndex = 5;
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(12, 15);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(35, 13);
			this.label6.TabIndex = 13;
			this.label6.Text = "Name";
			// 
			// txtConnectionName
			// 
			this.txtConnectionName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtConnectionName.Location = new System.Drawing.Point(71, 12);
			this.txtConnectionName.Name = "txtConnectionName";
			this.txtConnectionName.Size = new System.Drawing.Size(284, 20);
			this.txtConnectionName.TabIndex = 1;
			this.txtConnectionName.TextChanged += new System.EventHandler(this.txtServer_TextChanged);
			// 
			// btnAdd
			// 
			this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnAdd.Location = new System.Drawing.Point(37, 525);
			this.btnAdd.Name = "btnAdd";
			this.btnAdd.Size = new System.Drawing.Size(75, 23);
			this.btnAdd.TabIndex = 10;
			this.btnAdd.Text = "Add";
			this.btnAdd.UseVisualStyleBackColor = true;
			this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
			// 
			// btnRemove
			// 
			this.btnRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnRemove.Location = new System.Drawing.Point(280, 525);
			this.btnRemove.Name = "btnRemove";
			this.btnRemove.Size = new System.Drawing.Size(75, 23);
			this.btnRemove.TabIndex = 13;
			this.btnRemove.Text = "Remove";
			this.btnRemove.UseVisualStyleBackColor = true;
			this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(12, 93);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(26, 13);
			this.label5.TabIndex = 9;
			this.label5.Text = "Port";
			// 
			// numPort
			// 
			this.numPort.Location = new System.Drawing.Point(71, 91);
			this.numPort.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.numPort.Name = "numPort";
			this.numPort.Size = new System.Drawing.Size(120, 20);
			this.numPort.TabIndex = 4;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(12, 177);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(53, 13);
			this.label4.TabIndex = 7;
			this.label4.Text = "Password";
			// 
			// txtPassword
			// 
			this.txtPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtPassword.Location = new System.Drawing.Point(71, 174);
			this.txtPassword.Name = "txtPassword";
			this.txtPassword.PasswordChar = '*';
			this.txtPassword.Size = new System.Drawing.Size(215, 20);
			this.txtPassword.TabIndex = 7;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(12, 151);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(29, 13);
			this.label3.TabIndex = 5;
			this.label3.Text = "User";
			// 
			// txtUser
			// 
			this.txtUser.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtUser.Location = new System.Drawing.Point(71, 148);
			this.txtUser.Name = "txtUser";
			this.txtUser.Size = new System.Drawing.Size(284, 20);
			this.txtUser.TabIndex = 6;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 203);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(44, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Append";
			// 
			// txtAppend
			// 
			this.txtAppend.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtAppend.Location = new System.Drawing.Point(71, 200);
			this.txtAppend.Name = "txtAppend";
			this.txtAppend.Size = new System.Drawing.Size(284, 20);
			this.txtAppend.TabIndex = 8;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 68);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(38, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Server";
			// 
			// txtServer
			// 
			this.txtServer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtServer.Location = new System.Drawing.Point(71, 65);
			this.txtServer.Name = "txtServer";
			this.txtServer.Size = new System.Drawing.Size(284, 20);
			this.txtServer.TabIndex = 3;
			this.txtServer.TextChanged += new System.EventHandler(this.txtServer_TextChanged);
			// 
			// btnShow
			// 
			this.btnShow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnShow.Location = new System.Drawing.Point(292, 172);
			this.btnShow.Name = "btnShow";
			this.btnShow.Size = new System.Drawing.Size(63, 23);
			this.btnShow.TabIndex = 25;
			this.btnShow.Text = "Show";
			this.btnShow.UseVisualStyleBackColor = true;
			this.btnShow.Click += new System.EventHandler(this.btnShow_Click);
			// 
			// frmConnections
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1052, 560);
			this.Controls.Add(this.splitContainer1);
			this.Name = "frmConnections";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Connections";
			this.Load += new System.EventHandler(this.frmConnections_Load);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.gridMain)).EndInit();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numForward)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numTunnelPort)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numPort)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtServer;
        private System.Windows.Forms.NumericUpDown numPort;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtUser;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtAppend;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.DataGridView gridMain;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtConnectionName;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtDatabase;
        private System.Windows.Forms.ComboBox cboDataSource;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox chkIntegratedSecurity;
        private System.Windows.Forms.DataGridViewTextBoxColumn ConnectionName;
        private System.Windows.Forms.DataGridViewTextBoxColumn Server;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCopy;
        private System.Windows.Forms.Label labelkey;
        private System.Windows.Forms.TextBox txtTunnelKey;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtTunnel;
        private System.Windows.Forms.Button btnBrowseKey;
        private System.Windows.Forms.NumericUpDown numTunnelPort;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.NumericUpDown numForward;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtTunnelUser;
        private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Button btnShow;
	}
}