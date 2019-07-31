using PaJaMa.Database.Library;
using PaJaMa.Database.Library.DatabaseObjects;
using PaJaMa.Database.Library.DataSources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PaJaMa.Database.Studio.Query.QueryBuilder
{
	public partial class frmRename : Form
	{
		private DatabaseObjectBase _dbObj;
		public DatabaseObjectBase DatabaseObject
		{
			get { return _dbObj; }
			set
			{
				_dbObj = value;
				txtNewName.Text = _dbObj == null ? string.Empty : _dbObj.ObjectName;
			}
		}

		public frmRename()
		{
			InitializeComponent();
		}

		public string GetScript()
		{
			return _dbObj.Database.DataSource.GetRenameScript(this.DatabaseObject, txtNewName.Text);
		}

		private void BtnConnect_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void TxtNewName_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				BtnConnect_Click(sender, new EventArgs());
			}
		}
	}
}
