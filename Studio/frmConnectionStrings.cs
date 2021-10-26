using PaJaMa.Database.Library;
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
	public partial class frmConnectionStrings : Form
	{
		public frmConnectionStrings()
		{
			InitializeComponent();
		}

		private void frmConnectionStrings_Load(object sender, EventArgs e)
		{
			var settings = PaJaMa.Common.SettingsHelper.GetUserSettings<DatabaseStudioSettings>();
			//if (!string.IsNullOrEmpty(settings.ConnectionStrings))
			//	txtConnStrings.Text = settings.ConnectionStrings.Replace("|", "\r\n");
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			var settings = PaJaMa.Common.SettingsHelper.GetUserSettings<DatabaseStudioSettings>();
			//settings.ConnectionStrings = txtConnStrings.Text.Replace("\r\n", "|");
			//Common.SettingsHelper.SaveUserSettings<DatabaseStudioSettings>(settings);
		}
	}
}
