namespace PaJaMa.Database.Studio.Query.QueryBuilder
{
	partial class frmColumn
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
			this.cboTable = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.cboType = new System.Windows.Forms.ComboBox();
			this.label5 = new System.Windows.Forms.Label();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnOK = new System.Windows.Forms.Button();
			this.txtColumnName = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.numScale = new System.Windows.Forms.NumericUpDown();
			this.label2 = new System.Windows.Forms.Label();
			this.chkAllowNull = new System.Windows.Forms.CheckBox();
			this.label4 = new System.Windows.Forms.Label();
			this.txtDefault = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.numMaxLength = new System.Windows.Forms.NumericUpDown();
			this.label7 = new System.Windows.Forms.Label();
			this.numPrecision = new System.Windows.Forms.NumericUpDown();
			this.chkIdentity = new System.Windows.Forms.CheckBox();
			((System.ComponentModel.ISupportInitialize)(this.numScale)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numMaxLength)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numPrecision)).BeginInit();
			this.SuspendLayout();
			// 
			// cboTable
			// 
			this.cboTable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cboTable.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboTable.FormattingEnabled = true;
			this.cboTable.Location = new System.Drawing.Point(97, 12);
			this.cboTable.Name = "cboTable";
			this.cboTable.Size = new System.Drawing.Size(233, 21);
			this.cboTable.TabIndex = 0;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(12, 15);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(34, 13);
			this.label3.TabIndex = 4;
			this.label3.Text = "Table";
			// 
			// cboType
			// 
			this.cboType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cboType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboType.FormattingEnabled = true;
			this.cboType.Items.AddRange(new object[] {
            "NO ACTION",
            "CASCADE"});
			this.cboType.Location = new System.Drawing.Point(97, 65);
			this.cboType.Name = "cboType";
			this.cboType.Size = new System.Drawing.Size(233, 21);
			this.cboType.TabIndex = 2;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(12, 68);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(31, 13);
			this.label5.TabIndex = 9;
			this.label5.Text = "Type";
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.Location = new System.Drawing.Point(255, 246);
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
			this.btnOK.Location = new System.Drawing.Point(174, 246);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(75, 23);
			this.btnOK.TabIndex = 11;
			this.btnOK.Text = "OK";
			this.btnOK.UseVisualStyleBackColor = true;
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			// 
			// txtColumnName
			// 
			this.txtColumnName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtColumnName.Location = new System.Drawing.Point(97, 39);
			this.txtColumnName.Name = "txtColumnName";
			this.txtColumnName.Size = new System.Drawing.Size(233, 20);
			this.txtColumnName.TabIndex = 1;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 42);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(42, 13);
			this.label1.TabIndex = 13;
			this.label1.Text = "Column";
			// 
			// numScale
			// 
			this.numScale.Location = new System.Drawing.Point(97, 144);
			this.numScale.Name = "numScale";
			this.numScale.Size = new System.Drawing.Size(120, 20);
			this.numScale.TabIndex = 5;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 144);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(34, 13);
			this.label2.TabIndex = 15;
			this.label2.Text = "Scale";
			// 
			// chkAllowNull
			// 
			this.chkAllowNull.AutoSize = true;
			this.chkAllowNull.Checked = true;
			this.chkAllowNull.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkAllowNull.Location = new System.Drawing.Point(15, 223);
			this.chkAllowNull.Name = "chkAllowNull";
			this.chkAllowNull.Size = new System.Drawing.Size(72, 17);
			this.chkAllowNull.TabIndex = 8;
			this.chkAllowNull.Text = "Allow Null";
			this.chkAllowNull.UseVisualStyleBackColor = true;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(12, 170);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(41, 13);
			this.label4.TabIndex = 18;
			this.label4.Text = "Default";
			// 
			// txtDefault
			// 
			this.txtDefault.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtDefault.Location = new System.Drawing.Point(97, 170);
			this.txtDefault.Name = "txtDefault";
			this.txtDefault.Size = new System.Drawing.Size(233, 20);
			this.txtDefault.TabIndex = 6;
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(12, 92);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(63, 13);
			this.label6.TabIndex = 20;
			this.label6.Text = "Max Length";
			// 
			// numMaxLength
			// 
			this.numMaxLength.Location = new System.Drawing.Point(97, 92);
			this.numMaxLength.Name = "numMaxLength";
			this.numMaxLength.Size = new System.Drawing.Size(120, 20);
			this.numMaxLength.TabIndex = 3;
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(12, 118);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(50, 13);
			this.label7.TabIndex = 22;
			this.label7.Text = "Precision";
			// 
			// numPrecision
			// 
			this.numPrecision.Location = new System.Drawing.Point(97, 118);
			this.numPrecision.Name = "numPrecision";
			this.numPrecision.Size = new System.Drawing.Size(120, 20);
			this.numPrecision.TabIndex = 4;
			// 
			// chkIdentity
			// 
			this.chkIdentity.AutoSize = true;
			this.chkIdentity.Location = new System.Drawing.Point(15, 200);
			this.chkIdentity.Name = "chkIdentity";
			this.chkIdentity.Size = new System.Drawing.Size(60, 17);
			this.chkIdentity.TabIndex = 7;
			this.chkIdentity.Text = "Identity";
			this.chkIdentity.UseVisualStyleBackColor = true;
			// 
			// frmColumn
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(342, 281);
			this.Controls.Add(this.chkIdentity);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.numPrecision);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.numMaxLength);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.txtDefault);
			this.Controls.Add(this.chkAllowNull);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.numScale);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.txtColumnName);
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.cboType);
			this.Controls.Add(this.cboTable);
			this.Controls.Add(this.label3);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "frmColumn";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "New Column";
			((System.ComponentModel.ISupportInitialize)(this.numScale)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numMaxLength)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numPrecision)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.ComboBox cboTable;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ComboBox cboType;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.TextBox txtColumnName;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.NumericUpDown numScale;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.CheckBox chkAllowNull;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox txtDefault;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.NumericUpDown numMaxLength;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.NumericUpDown numPrecision;
		private System.Windows.Forms.CheckBox chkIdentity;
	}
}