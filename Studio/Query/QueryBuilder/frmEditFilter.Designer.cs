namespace PaJaMa.Database.Studio.Query.QueryBuilder
{
	partial class frmEditFilter
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
			this.txtFilter = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.btnGenerate = new System.Windows.Forms.Button();
			this.tabMain = new System.Windows.Forms.TabControl();
			this.pageFilter = new System.Windows.Forms.TabPage();
			this.gridColumns = new System.Windows.Forms.DataGridView();
			this.pageCustom = new System.Windows.Forms.TabPage();
			this.Key = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.ColumnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Operator = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.Value = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.tabMain.SuspendLayout();
			this.pageFilter.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridColumns)).BeginInit();
			this.pageCustom.SuspendLayout();
			this.SuspendLayout();
			// 
			// txtFilter
			// 
			this.txtFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtFilter.Location = new System.Drawing.Point(6, 19);
			this.txtFilter.Multiline = true;
			this.txtFilter.Name = "txtFilter";
			this.txtFilter.Size = new System.Drawing.Size(389, 246);
			this.txtFilter.TabIndex = 1;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(6, 3);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(73, 13);
			this.label2.TabIndex = 6;
			this.label2.Text = "Where clause";
			// 
			// btnGenerate
			// 
			this.btnGenerate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnGenerate.Location = new System.Drawing.Point(376, 350);
			this.btnGenerate.Name = "btnGenerate";
			this.btnGenerate.Size = new System.Drawing.Size(102, 23);
			this.btnGenerate.TabIndex = 7;
			this.btnGenerate.Text = "Edit";
			this.btnGenerate.UseVisualStyleBackColor = true;
			this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
			// 
			// tabMain
			// 
			this.tabMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tabMain.Controls.Add(this.pageFilter);
			this.tabMain.Controls.Add(this.pageCustom);
			this.tabMain.Location = new System.Drawing.Point(12, 12);
			this.tabMain.Name = "tabMain";
			this.tabMain.SelectedIndex = 0;
			this.tabMain.Size = new System.Drawing.Size(466, 332);
			this.tabMain.TabIndex = 8;
			// 
			// pageFilter
			// 
			this.pageFilter.Controls.Add(this.gridColumns);
			this.pageFilter.Location = new System.Drawing.Point(4, 22);
			this.pageFilter.Name = "pageFilter";
			this.pageFilter.Padding = new System.Windows.Forms.Padding(3);
			this.pageFilter.Size = new System.Drawing.Size(458, 306);
			this.pageFilter.TabIndex = 0;
			this.pageFilter.Text = "Filter";
			this.pageFilter.UseVisualStyleBackColor = true;
			// 
			// gridColumns
			// 
			this.gridColumns.AllowUserToAddRows = false;
			this.gridColumns.AllowUserToDeleteRows = false;
			this.gridColumns.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.gridColumns.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Key,
            this.ColumnName,
            this.Operator,
            this.Value});
			this.gridColumns.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridColumns.Location = new System.Drawing.Point(3, 3);
			this.gridColumns.Name = "gridColumns";
			this.gridColumns.Size = new System.Drawing.Size(452, 300);
			this.gridColumns.TabIndex = 0;
			// 
			// pageCustom
			// 
			this.pageCustom.Controls.Add(this.txtFilter);
			this.pageCustom.Controls.Add(this.label2);
			this.pageCustom.Location = new System.Drawing.Point(4, 22);
			this.pageCustom.Name = "pageCustom";
			this.pageCustom.Padding = new System.Windows.Forms.Padding(3);
			this.pageCustom.Size = new System.Drawing.Size(401, 271);
			this.pageCustom.TabIndex = 1;
			this.pageCustom.Text = "Custom";
			this.pageCustom.UseVisualStyleBackColor = true;
			// 
			// Key
			// 
			this.Key.HeaderText = "Key";
			this.Key.Name = "Key";
			this.Key.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.Key.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			// 
			// ColumnName
			// 
			this.ColumnName.DataPropertyName = "ColumnName";
			this.ColumnName.HeaderText = "Column";
			this.ColumnName.Name = "ColumnName";
			this.ColumnName.ReadOnly = true;
			// 
			// Operator
			// 
			this.Operator.HeaderText = "Operator";
			this.Operator.Items.AddRange(new object[] {
            "",
            "LIKE",
            "=",
            "<>"});
			this.Operator.Name = "Operator";
			// 
			// Value
			// 
			this.Value.HeaderText = "Value";
			this.Value.Name = "Value";
			// 
			// frmEditFilter
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(490, 385);
			this.Controls.Add(this.tabMain);
			this.Controls.Add(this.btnGenerate);
			this.Name = "frmEditFilter";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Edit Rows";
			this.Load += new System.EventHandler(this.frmEditFilter_Load);
			this.tabMain.ResumeLayout(false);
			this.pageFilter.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.gridColumns)).EndInit();
			this.pageCustom.ResumeLayout(false);
			this.pageCustom.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button btnGenerate;
		private System.Windows.Forms.TextBox txtFilter;
		private System.Windows.Forms.TabControl tabMain;
		private System.Windows.Forms.TabPage pageFilter;
		private System.Windows.Forms.TabPage pageCustom;
		private System.Windows.Forms.DataGridView gridColumns;
		private System.Windows.Forms.DataGridViewCheckBoxColumn Key;
		private System.Windows.Forms.DataGridViewTextBoxColumn ColumnName;
		private System.Windows.Forms.DataGridViewComboBoxColumn Operator;
		private System.Windows.Forms.DataGridViewTextBoxColumn Value;
	}
}