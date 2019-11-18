using PaJaMa.Common;
using PaJaMa.Database.Library.DatabaseObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PaJaMa.Database.Studio.Query
{
	public partial class frmSearchResults : Form
	{
		public List<TreeNode> FoundNodes { get; set; }
		public TreeView TreeView { get; set; }
		private TreeNode _lastSelectedNode;
		public frmSearchResults()
		{
			InitializeComponent();
		}

		private void LstResults_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (TreeView.SelectedNode != null)
			{
				TreeView.SelectedNode.BackColor = Color.White;
			}
			if (lstResults.SelectedIndices.Count < 1) return;
			TreeView.SelectedNode = FoundNodes[lstResults.SelectedIndices[0]];
			TreeView.SelectedNode.BackColor = Color.Yellow;
			_lastSelectedNode = TreeView.SelectedNode;
		}

		private void FrmSearchResults_Load(object sender, EventArgs e)
		{
			FormSettings.LoadSettings(this);

			lstResults.Items.Clear();
			foreach (var n in FoundNodes)
			{
				var txt = (n.Tag as DatabaseObjectBase).Database.DatabaseName + "." +
						(n.Tag is Column col ? col.Parent.ObjectName + "." + col.ColumnName : (n.Tag as Table).TableName);
				lstResults.Items.Add(txt);
			}
		}

		private void FrmSearchResults_FormClosed(object sender, FormClosedEventArgs e)
		{
			if (_lastSelectedNode != null)
			{
				_lastSelectedNode.BackColor = Color.White;
			}
		}

		private void FrmSearchResults_FormClosing(object sender, FormClosingEventArgs e)
		{
			FormSettings.SaveSettings(this);
		}
	}
}
