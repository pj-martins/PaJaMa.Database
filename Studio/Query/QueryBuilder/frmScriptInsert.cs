using PaJaMa.Database.Library;
using PaJaMa.Database.Library.DatabaseObjects;
using PaJaMa.Database.Library.DataSources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PaJaMa.Database.Studio.Query.QueryBuilder
{
	public partial class frmScriptInsert : Form
	{
		public Table Table { get; set; }
		public DbConnection Connection { get; set; }

		public string Script { get; private set; }
		public frmScriptInsert()
		{
			InitializeComponent();
		}

		private void btnGenerate_Click(object sender, EventArgs e)
		{
			try
			{
				Script = Table.Database.DataSource.GetInsertScript(this.Connection, this.Table, txtFilter.Text);
				this.DialogResult = DialogResult.OK;
				this.Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Invalid filter");
			}
		}
	}
}
