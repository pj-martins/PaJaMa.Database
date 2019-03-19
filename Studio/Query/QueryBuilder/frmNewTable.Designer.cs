namespace PaJaMa.Database.Studio.Query.QueryBuilder
{
	partial class frmNewTable
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
			this.gridMain = new System.Windows.Forms.DataGridView();
			this.ColumnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Nullable = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.ColumnType = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.MaxLength = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ColumnDefault = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Identity = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.PrimaryKey = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.NumericPrecision = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.NumericScale = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.panel1 = new System.Windows.Forms.Panel();
			this.btnOK = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.panel2 = new System.Windows.Forms.Panel();
			this.label1 = new System.Windows.Forms.Label();
			this.txtTableName = new System.Windows.Forms.TextBox();
			((System.ComponentModel.ISupportInitialize)(this.gridMain)).BeginInit();
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// gridMain
			// 
			this.gridMain.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.gridMain.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnName,
            this.Nullable,
            this.ColumnType,
            this.MaxLength,
            this.ColumnDefault,
            this.Identity,
            this.PrimaryKey,
            this.NumericPrecision,
            this.NumericScale});
			this.gridMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridMain.Location = new System.Drawing.Point(0, 27);
			this.gridMain.Name = "gridMain";
			this.gridMain.Size = new System.Drawing.Size(1031, 567);
			this.gridMain.TabIndex = 0;
			this.gridMain.CellParsing += new System.Windows.Forms.DataGridViewCellParsingEventHandler(this.gridMain_CellParsing);
			// 
			// ColumnName
			// 
			this.ColumnName.DataPropertyName = "ColumnName";
			this.ColumnName.HeaderText = "Name";
			this.ColumnName.Name = "ColumnName";
			// 
			// Nullable
			// 
			this.Nullable.DataPropertyName = "IsNullable";
			this.Nullable.HeaderText = "Nullable";
			this.Nullable.Name = "Nullable";
			// 
			// ColumnType
			// 
			this.ColumnType.DataPropertyName = "ColumnType";
			this.ColumnType.HeaderText = "Type";
			this.ColumnType.Name = "ColumnType";
			// 
			// MaxLength
			// 
			this.MaxLength.DataPropertyName = "CharacterMaximumLength";
			this.MaxLength.HeaderText = "Max";
			this.MaxLength.Name = "MaxLength";
			// 
			// ColumnDefault
			// 
			this.ColumnDefault.DataPropertyName = "ColumnDefault";
			this.ColumnDefault.HeaderText = "Default";
			this.ColumnDefault.Name = "ColumnDefault";
			// 
			// Identity
			// 
			this.Identity.DataPropertyName = "IsIdentity";
			this.Identity.HeaderText = "Identity";
			this.Identity.Name = "Identity";
			// 
			// PrimaryKey
			// 
			this.PrimaryKey.HeaderText = "Primary Key";
			this.PrimaryKey.Name = "PrimaryKey";
			// 
			// NumericPrecision
			// 
			this.NumericPrecision.DataPropertyName = "NumericPrecision";
			this.NumericPrecision.HeaderText = "Precision";
			this.NumericPrecision.Name = "NumericPrecision";
			// 
			// NumericScale
			// 
			this.NumericScale.DataPropertyName = "NumericScale";
			this.NumericScale.HeaderText = "Scale";
			this.NumericScale.Name = "NumericScale";
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.btnOK);
			this.panel1.Controls.Add(this.btnCancel);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(0, 594);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(1031, 28);
			this.panel1.TabIndex = 1;
			// 
			// btnOK
			// 
			this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOK.Location = new System.Drawing.Point(872, 3);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(75, 23);
			this.btnOK.TabIndex = 1;
			this.btnOK.Text = "OK";
			this.btnOK.UseVisualStyleBackColor = true;
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.Location = new System.Drawing.Point(953, 3);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 0;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.label1);
			this.panel2.Controls.Add(this.txtTableName);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel2.Location = new System.Drawing.Point(0, 0);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(1031, 27);
			this.panel2.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 6);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(65, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Table Name";
			// 
			// txtTableName
			// 
			this.txtTableName.Location = new System.Drawing.Point(83, 3);
			this.txtTableName.Name = "txtTableName";
			this.txtTableName.Size = new System.Drawing.Size(481, 20);
			this.txtTableName.TabIndex = 0;
			// 
			// frmNewTable
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1031, 622);
			this.Controls.Add(this.gridMain);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.panel1);
			this.Name = "frmNewTable";
			this.ShowIcon = false;
			this.Text = "Create Table";
			this.Load += new System.EventHandler(this.frmNewTable_Load);
			((System.ComponentModel.ISupportInitialize)(this.gridMain)).EndInit();
			this.panel1.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.DataGridView gridMain;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtTableName;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.DataGridViewTextBoxColumn ColumnName;
		private System.Windows.Forms.DataGridViewCheckBoxColumn Nullable;
		private System.Windows.Forms.DataGridViewComboBoxColumn ColumnType;
		private System.Windows.Forms.DataGridViewTextBoxColumn MaxLength;
		private System.Windows.Forms.DataGridViewTextBoxColumn ColumnDefault;
		private System.Windows.Forms.DataGridViewCheckBoxColumn Identity;
		private System.Windows.Forms.DataGridViewCheckBoxColumn PrimaryKey;
		private System.Windows.Forms.DataGridViewTextBoxColumn NumericPrecision;
		private System.Windows.Forms.DataGridViewTextBoxColumn NumericScale;
	}
}