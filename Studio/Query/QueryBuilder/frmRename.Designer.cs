namespace PaJaMa.Database.Studio.Query.QueryBuilder
{
	partial class frmRename
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
			this.txtNewName = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.btnConnect = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// txtNewName
			// 
			this.txtNewName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtNewName.Location = new System.Drawing.Point(79, 12);
			this.txtNewName.Name = "txtNewName";
			this.txtNewName.Size = new System.Drawing.Size(286, 20);
			this.txtNewName.TabIndex = 1;
			this.txtNewName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TxtNewName_KeyDown);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(13, 15);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(60, 13);
			this.label2.TabIndex = 6;
			this.label2.Text = "New Name";
			// 
			// btnConnect
			// 
			this.btnConnect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnConnect.Location = new System.Drawing.Point(263, 38);
			this.btnConnect.Name = "btnConnect";
			this.btnConnect.Size = new System.Drawing.Size(102, 23);
			this.btnConnect.TabIndex = 7;
			this.btnConnect.Text = "Rename";
			this.btnConnect.UseVisualStyleBackColor = true;
			this.btnConnect.Click += new System.EventHandler(this.BtnConnect_Click);
			// 
			// frmRename
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(377, 73);
			this.Controls.Add(this.btnConnect);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.txtNewName);
			this.Name = "frmRename";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Rename";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox txtNewName;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button btnConnect;
	}
}