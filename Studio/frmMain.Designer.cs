﻿namespace PaJaMa.Database.Studio
{
	partial class frmMain
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
			this.tabMain = new System.Windows.Forms.TabControl();
			this.tabCompare = new System.Windows.Forms.TabPage();
			this.ucCompare1 = new PaJaMa.Database.Studio.Compare.ucCompare();
			this.tabQuery = new System.Windows.Forms.TabPage();
			this.ucQuery1 = new PaJaMa.Database.Studio.Query.ucQuery();
			this.tabDataGenerate = new System.Windows.Forms.TabPage();
			this.ucDataGenerate1 = new PaJaMa.Database.Studio.DataGenerate.ucDataGenerate();
			this.tabSearch = new System.Windows.Forms.TabPage();
			this.ucSearch1 = new PaJaMa.Database.Studio.Search.ucSearch();
			this.tabMonitor = new System.Windows.Forms.TabPage();
			this.ucMonitor = new PaJaMa.Database.Studio.Monitor.ucMonitor();
			this.tabDataCompare = new System.Windows.Forms.TabPage();
			this.ucDataCompare = new PaJaMa.Database.Studio.DataCompare.ucDataCompare();
			this.tabMain.SuspendLayout();
			this.tabCompare.SuspendLayout();
			this.tabQuery.SuspendLayout();
			this.tabDataGenerate.SuspendLayout();
			this.tabSearch.SuspendLayout();
			this.tabMonitor.SuspendLayout();
			this.tabDataCompare.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabMain
			// 
			this.tabMain.Alignment = System.Windows.Forms.TabAlignment.Bottom;
			this.tabMain.Controls.Add(this.tabCompare);
			this.tabMain.Controls.Add(this.tabQuery);
			this.tabMain.Controls.Add(this.tabDataCompare);
			this.tabMain.Controls.Add(this.tabDataGenerate);
			this.tabMain.Controls.Add(this.tabSearch);
			this.tabMain.Controls.Add(this.tabMonitor);
			this.tabMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabMain.Location = new System.Drawing.Point(0, 0);
			this.tabMain.Name = "tabMain";
			this.tabMain.SelectedIndex = 0;
			this.tabMain.Size = new System.Drawing.Size(1117, 605);
			this.tabMain.TabIndex = 0;
			// 
			// tabCompare
			// 
			this.tabCompare.Controls.Add(this.ucCompare1);
			this.tabCompare.Location = new System.Drawing.Point(4, 4);
			this.tabCompare.Name = "tabCompare";
			this.tabCompare.Padding = new System.Windows.Forms.Padding(3);
			this.tabCompare.Size = new System.Drawing.Size(1109, 579);
			this.tabCompare.TabIndex = 0;
			this.tabCompare.Text = "Compare";
			this.tabCompare.UseVisualStyleBackColor = true;
			// 
			// ucCompare1
			// 
			this.ucCompare1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ucCompare1.Location = new System.Drawing.Point(3, 3);
			this.ucCompare1.Name = "ucCompare1";
			this.ucCompare1.Size = new System.Drawing.Size(1103, 573);
			this.ucCompare1.TabIndex = 0;
			this.ucCompare1.QueryDatabase += new PaJaMa.Database.Studio.Classes.QueryEventHandler(this.uc_QueryDatabase);
			// 
			// tabQuery
			// 
			this.tabQuery.Controls.Add(this.ucQuery1);
			this.tabQuery.Location = new System.Drawing.Point(4, 4);
			this.tabQuery.Name = "tabQuery";
			this.tabQuery.Padding = new System.Windows.Forms.Padding(3);
			this.tabQuery.Size = new System.Drawing.Size(1109, 579);
			this.tabQuery.TabIndex = 1;
			this.tabQuery.Text = "Query";
			this.tabQuery.UseVisualStyleBackColor = true;
			// 
			// ucQuery1
			// 
			this.ucQuery1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ucQuery1.Location = new System.Drawing.Point(3, 3);
			this.ucQuery1.Name = "ucQuery1";
			this.ucQuery1.Size = new System.Drawing.Size(881, 524);
			this.ucQuery1.TabIndex = 0;
			// 
			// tabDataGenerate
			// 
			this.tabDataGenerate.Controls.Add(this.ucDataGenerate1);
			this.tabDataGenerate.Location = new System.Drawing.Point(4, 4);
			this.tabDataGenerate.Name = "tabDataGenerate";
			this.tabDataGenerate.Padding = new System.Windows.Forms.Padding(3);
			this.tabDataGenerate.Size = new System.Drawing.Size(1109, 579);
			this.tabDataGenerate.TabIndex = 2;
			this.tabDataGenerate.Text = "Data Generate";
			this.tabDataGenerate.UseVisualStyleBackColor = true;
			// 
			// ucDataGenerate1
			// 
			this.ucDataGenerate1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ucDataGenerate1.Location = new System.Drawing.Point(3, 3);
			this.ucDataGenerate1.Name = "ucDataGenerate1";
			this.ucDataGenerate1.Size = new System.Drawing.Size(881, 524);
			this.ucDataGenerate1.TabIndex = 0;
			this.ucDataGenerate1.QueryDatabase += new PaJaMa.Database.Studio.Classes.QueryEventHandler(this.uc_QueryDatabase);
			// 
			// tabSearch
			// 
			this.tabSearch.Controls.Add(this.ucSearch1);
			this.tabSearch.Location = new System.Drawing.Point(4, 4);
			this.tabSearch.Name = "tabSearch";
			this.tabSearch.Padding = new System.Windows.Forms.Padding(3);
			this.tabSearch.Size = new System.Drawing.Size(1109, 579);
			this.tabSearch.TabIndex = 3;
			this.tabSearch.Text = "Search";
			this.tabSearch.UseVisualStyleBackColor = true;
			// 
			// ucSearch1
			// 
			this.ucSearch1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ucSearch1.Location = new System.Drawing.Point(3, 3);
			this.ucSearch1.Name = "ucSearch1";
			this.ucSearch1.Size = new System.Drawing.Size(881, 524);
			this.ucSearch1.TabIndex = 0;
			// 
			// tabMonitor
			// 
			this.tabMonitor.Controls.Add(this.ucMonitor);
			this.tabMonitor.Location = new System.Drawing.Point(4, 4);
			this.tabMonitor.Name = "tabMonitor";
			this.tabMonitor.Padding = new System.Windows.Forms.Padding(3);
			this.tabMonitor.Size = new System.Drawing.Size(1109, 579);
			this.tabMonitor.TabIndex = 4;
			this.tabMonitor.Text = "Monitor";
			this.tabMonitor.UseVisualStyleBackColor = true;
			// 
			// ucMonitor
			// 
			this.ucMonitor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ucMonitor.Location = new System.Drawing.Point(3, 3);
			this.ucMonitor.Name = "ucMonitor";
			this.ucMonitor.Size = new System.Drawing.Size(881, 524);
			this.ucMonitor.TabIndex = 0;
			// 
			// tabDataCompare
			// 
			this.tabDataCompare.Controls.Add(this.ucDataCompare);
			this.tabDataCompare.Location = new System.Drawing.Point(4, 4);
			this.tabDataCompare.Name = "tabDataCompare";
			this.tabDataCompare.Padding = new System.Windows.Forms.Padding(3);
			this.tabDataCompare.Size = new System.Drawing.Size(1109, 579);
			this.tabDataCompare.TabIndex = 5;
			this.tabDataCompare.Text = "Data Compare";
			this.tabDataCompare.UseVisualStyleBackColor = true;
			// 
			// ucCompare2
			// 
			this.ucDataCompare.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ucDataCompare.Location = new System.Drawing.Point(3, 3);
			this.ucDataCompare.Name = "ucCompare2";
			this.ucDataCompare.Size = new System.Drawing.Size(1103, 573);
			this.ucDataCompare.TabIndex = 1;
			// 
			// frmMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1117, 605);
			this.Controls.Add(this.tabMain);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "frmMain";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Database Studio";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
			this.Load += new System.EventHandler(this.frmMain_Load);
			this.tabMain.ResumeLayout(false);
			this.tabCompare.ResumeLayout(false);
			this.tabQuery.ResumeLayout(false);
			this.tabDataGenerate.ResumeLayout(false);
			this.tabSearch.ResumeLayout(false);
			this.tabMonitor.ResumeLayout(false);
			this.tabDataCompare.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TabControl tabMain;
		private System.Windows.Forms.TabPage tabCompare;
		private System.Windows.Forms.TabPage tabQuery;
		private System.Windows.Forms.TabPage tabDataGenerate;
		private Compare.ucCompare ucCompare1;
		private Query.ucQuery ucQuery1;
		private DataGenerate.ucDataGenerate ucDataGenerate1;
		private System.Windows.Forms.TabPage tabSearch;
		private Search.ucSearch ucSearch1;
		private System.Windows.Forms.TabPage tabMonitor;
		private Monitor.ucMonitor ucMonitor;
		private System.Windows.Forms.TabPage tabDataCompare;
		private DataCompare.ucDataCompare ucDataCompare;
	}
}