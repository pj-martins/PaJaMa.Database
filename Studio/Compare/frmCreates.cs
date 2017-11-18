using PaJaMa.Database.Library.Synchronization;
using PaJaMa.Database.Library.Workspaces.Compare;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PaJaMa.Database.Studio.Compare
{
	public partial class frmCreates : Form
	{
		public WorkspaceWithSourceBase Workspace { get; set; }

		public frmCreates()
		{
			InitializeComponent();
		}

		private void frmStructureDetails_Load(object sender, EventArgs e)
		{
			var sync = DatabaseObjectSynchronizationBase.GetSynchronization(Workspace.SourceObject.Database, Workspace.SourceObject);

			txtFromScript.Text = string.Join("\r\n\r\n", sync.GetRawCreateText());
			if (Workspace.TargetObject == null)
				txtToScript.Text = string.Empty;
			else
			{
				sync = DatabaseObjectSynchronizationBase.GetSynchronization(Workspace.TargetObject.Database, Workspace.TargetObject);
				txtToScript.Text = sync.GetRawCreateText();
			}
			splitMain.Panel2Collapsed = Workspace.TargetObject == null;
		}

		private void txt_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Control && e.KeyCode == Keys.A)
			{
				var txt = sender as TextBox;
				txt.SelectAll();
			}
		}
	}
}
