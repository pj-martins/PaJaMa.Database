using PaJaMa.Database.Studio.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PaJaMa.Database.Studio
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			//if (args.Any())
			//{
			//	if (CommandLineHelper.ProcessArguments(args))
			//		return;
			//}

			if (Properties.Settings.Default.UpgradeRequired)
			{
				Properties.Settings.Default.Upgrade();
				Properties.Settings.Default.UpgradeRequired = false;
				Properties.Settings.Default.Save();
			}

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new frmMain());
		}
	}
}
