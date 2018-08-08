using PaJaMa.Database.Library.Workspaces;
using PaJaMa.Database.Studio.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PaJaMa.Database.Studio.Query
{
	public partial class ucQuery : UserControl
	{
		public const string TEMP_PATH = "DatabaseStudio\\Query";
		public ucQuery()
		{
			InitializeComponent();
		}

		private void frmMain_Load(object sender, EventArgs e)
		{
			var tmp = Path.Combine(Path.GetTempPath(), TEMP_PATH);
			if (Directory.Exists(tmp))
			{
				var tempFiles = new DirectoryInfo(tmp).GetFiles();
				if (tempFiles.Any())
				{
					if (MessageBox.Show("Open previous workspaces?", "Open Workspaces", MessageBoxButtons.YesNo) == DialogResult.Yes)
					{
						foreach (var finf in tempFiles)
						{
							try
							{
								var workspace = PaJaMa.Common.XmlSerialize.DeserializeObjectFromFile<QueryWorkspace>(finf.FullName);
								var uc = addWorkspace(null);
								uc.LoadWorkspace(workspace);
							}
							catch
							{
								// TODO:
							}
						}
					}
					foreach (var finf in tempFiles)
					{
						finf.Delete();
					}
				}
			}

			if (tabMain.TabPages.Count < 1)
				addWorkspace(null);
		}

		private void saveToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				if (tabMain.SelectedTab != null)
				{
					var workSpace = (tabMain.SelectedTab.Controls[0] as ucWorkspace).GetWorkspace();
					if (string.IsNullOrEmpty(workSpace.ConnectionString) || workSpace.ConnectionType == null)
						MessageBox.Show("Incomplete workspace.");
					else
					{
						using (var dlg = new SaveFileDialog())
						{
							dlg.Filter = "DatabaseStudio files (*.dbs)|*.dbs";
							dlg.Title = "Workspace";
							if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
							{
								tabMain.SelectedTab.Text = new FileInfo(dlg.FileName).Name;
								PaJaMa.Common.XmlSerialize.SerializeObjectToFile<QueryWorkspace>(workSpace, dlg.FileName);
								MessageBox.Show("Workspace saved.");
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void loadToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				using (var dlg = new OpenFileDialog())
				{
					dlg.Filter = "DatabaseStudio files (*.dbs)|*.dbs";
					dlg.Title = "Workspace";
					if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
					{
						var workspace = PaJaMa.Common.XmlSerialize.DeserializeObjectFromFile<QueryWorkspace>(dlg.FileName);
						addWorkspace(null);

						tabMain.SelectedTab.Text = new FileInfo(dlg.FileName).Name;
						(tabMain.SelectedTab.Controls[0] as ucWorkspace).LoadWorkspace(workspace);
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		public void LoadFromIDatabase(QueryEventArgs args)
		{
			var uc = new ucWorkspace();
			var tab = new WinControls.TabControl.TabPage("Workspace " + (tabMain.TabPages.Count + 1).ToString());
			uc.ParentTabControl = tabMain;
			uc.Dock = DockStyle.Fill;
			tab.Controls.Add(uc);
			tabMain.TabPages.Add(tab);
			tabMain.SelectedTab = tab;
			uc.LoadFromIDatabase(args);
		}

		private ucWorkspace addWorkspace(WinControls.TabControl.TabPage tabPage)
		{
			var uc = new ucWorkspace();
			bool add = false;
			if (tabPage == null)
			{
				tabPage = new WinControls.TabControl.TabPage();
				add = true;
			}
			tabPage.Text = "Workspace " + (tabMain.TabPages.Count + 1).ToString();
			uc.ParentTabControl = tabMain;
			uc.Dock = DockStyle.Fill;
			tabPage.Controls.Add(uc);
			if (add)
			{
				tabMain.TabPages.Add(tabPage);
				tabMain.SelectedTab = tabPage;
			}
			return uc;
		}

		private void copyToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (tabMain.SelectedTab != null)
			{
				var workSpace = tabMain.SelectedTab.Controls[0] as ucWorkspace;
				workSpace.CopyWorkspace(true);
			}
		}

		private void tabMain_TabClosing(object sender, WinControls.TabControl.TabEventArgs e)
		{
			var uc = e.TabPage.Controls[0] as ucWorkspace;
			uc.DeleteTemp();
			uc.Disconnect();
		}

		private void tabMain_TabAdding(object sender, WinControls.TabControl.TabEventArgs e)
		{
			addWorkspace(e.TabPage);
		}
	}
}
