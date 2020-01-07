using PaJaMa.Common;
using PaJaMa.Database.Studio.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PaJaMa.Database.Studio
{
	public partial class frmMain : Form
	{
		public frmMain()
		{
			InitializeComponent();
			this.Text += " - " + this.GetType().Assembly.AssemblyVersion();
		}
		
		private void frmMain_Load(object sender, EventArgs e)
		{
			WinControls.FormSettings.LoadSettings(this);
		}

		private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (this.WindowState == FormWindowState.Minimized) return;

			WinControls.FormSettings.SaveSettings(this);
		}

		private void uc_QueryDatabase(object sender, QueryEventArgs e)
		{
			ucQuery1.LoadFromIDatabase(e);
			tabMain.SelectedTab = tabQuery;
		}
	}
}
