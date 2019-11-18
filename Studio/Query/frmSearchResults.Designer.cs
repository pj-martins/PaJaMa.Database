namespace PaJaMa.Database.Studio.Query
{
	partial class frmSearchResults
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
			this.lstResults = new System.Windows.Forms.ListBox();
			this.SuspendLayout();
			// 
			// lstResults
			// 
			this.lstResults.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lstResults.FormattingEnabled = true;
			this.lstResults.Location = new System.Drawing.Point(0, 0);
			this.lstResults.Name = "lstResults";
			this.lstResults.Size = new System.Drawing.Size(491, 309);
			this.lstResults.TabIndex = 0;
			this.lstResults.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.LstResults_MouseDoubleClick);
			// 
			// frmSearchResults
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(491, 309);
			this.Controls.Add(this.lstResults);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Name = "frmSearchResults";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Search Results";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmSearchResults_FormClosing);
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FrmSearchResults_FormClosed);
			this.Load += new System.EventHandler(this.FrmSearchResults_Load);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListBox lstResults;
	}
}