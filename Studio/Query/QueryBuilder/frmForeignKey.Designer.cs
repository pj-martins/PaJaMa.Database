namespace PaJaMa.Database.Studio.Query.QueryBuilder
{
	partial class frmForeignKey
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
			this.label1 = new System.Windows.Forms.Label();
			this.cboParentTable = new System.Windows.Forms.ComboBox();
			this.cboParentColumn = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.cboChildTable = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.cboChildColumn = new System.Windows.Forms.ComboBox();
			this.label4 = new System.Windows.Forms.Label();
			this.cboDeleteRule = new System.Windows.Forms.ComboBox();
			this.label5 = new System.Windows.Forms.Label();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnOK = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 69);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(68, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Parent Table";
			// 
			// cboParentTable
			// 
			this.cboParentTable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cboParentTable.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboParentTable.FormattingEnabled = true;
			this.cboParentTable.Location = new System.Drawing.Point(97, 66);
			this.cboParentTable.Name = "cboParentTable";
			this.cboParentTable.Size = new System.Drawing.Size(233, 21);
			this.cboParentTable.TabIndex = 1;
			this.cboParentTable.SelectedIndexChanged += new System.EventHandler(this.cboParentTable_SelectedIndexChanged);
			// 
			// cboParentColumn
			// 
			this.cboParentColumn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cboParentColumn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboParentColumn.FormattingEnabled = true;
			this.cboParentColumn.Location = new System.Drawing.Point(97, 93);
			this.cboParentColumn.Name = "cboParentColumn";
			this.cboParentColumn.Size = new System.Drawing.Size(233, 21);
			this.cboParentColumn.TabIndex = 3;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 96);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(76, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "Parent Column";
			// 
			// cboChildTable
			// 
			this.cboChildTable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cboChildTable.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboChildTable.FormattingEnabled = true;
			this.cboChildTable.Location = new System.Drawing.Point(97, 12);
			this.cboChildTable.Name = "cboChildTable";
			this.cboChildTable.Size = new System.Drawing.Size(233, 21);
			this.cboChildTable.TabIndex = 5;
			this.cboChildTable.SelectedIndexChanged += new System.EventHandler(this.cboChildTable_SelectedIndexChanged);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(12, 15);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(60, 13);
			this.label3.TabIndex = 4;
			this.label3.Text = "Child Table";
			// 
			// cboChildColumn
			// 
			this.cboChildColumn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cboChildColumn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboChildColumn.FormattingEnabled = true;
			this.cboChildColumn.Location = new System.Drawing.Point(97, 39);
			this.cboChildColumn.Name = "cboChildColumn";
			this.cboChildColumn.Size = new System.Drawing.Size(233, 21);
			this.cboChildColumn.TabIndex = 7;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(12, 42);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(68, 13);
			this.label4.TabIndex = 6;
			this.label4.Text = "Child Column";
			// 
			// cboDeleteRule
			// 
			this.cboDeleteRule.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cboDeleteRule.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboDeleteRule.FormattingEnabled = true;
			this.cboDeleteRule.Items.AddRange(new object[] {
            "NO ACTION",
            "CASCADE"});
			this.cboDeleteRule.Location = new System.Drawing.Point(97, 120);
			this.cboDeleteRule.Name = "cboDeleteRule";
			this.cboDeleteRule.Size = new System.Drawing.Size(233, 21);
			this.cboDeleteRule.TabIndex = 8;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(12, 123);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(63, 13);
			this.label5.TabIndex = 9;
			this.label5.Text = "Delete Rule";
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.Location = new System.Drawing.Point(255, 153);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 10;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// btnOK
			// 
			this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOK.Location = new System.Drawing.Point(174, 153);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(75, 23);
			this.btnOK.TabIndex = 11;
			this.btnOK.Text = "OK";
			this.btnOK.UseVisualStyleBackColor = true;
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			// 
			// frmForeignKey
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(342, 188);
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.cboDeleteRule);
			this.Controls.Add(this.cboChildColumn);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.cboChildTable);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.cboParentColumn);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.cboParentTable);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "frmForeignKey";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "New Foreign Key";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox cboParentTable;
		private System.Windows.Forms.ComboBox cboParentColumn;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox cboChildTable;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ComboBox cboChildColumn;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.ComboBox cboDeleteRule;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnOK;
	}
}