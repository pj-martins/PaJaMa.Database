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
	public partial class frmEditFilter : Form
	{
		public Table Table { get; set; }
		public DbConnection Connection { get; set; }

		public string Script { get; private set; }
		public frmEditFilter()
		{
			InitializeComponent();
		}

		private void btnGenerate_Click(object sender, EventArgs e)
		{
			try
			{
				if (!Table.Columns.Any())
					Table.Database.DataSource.PopulateChildColumns(Connection, Table);
				var dbName = Table.Database.DataSource.GetConvertedObjectName(Table.Database.DatabaseName);
				Connection.ChangeDatabase(Table.Database.DatabaseName);
				var objName = Table.GetObjectNameWithSchema(Table.Database.DataSource);
				var columns = Table.Columns.Select(c => c.ColumnName).ToArray();

				Script = string.Format("SELECT *\r\nFROM {1}{0} {2}",
					objName,
					string.IsNullOrEmpty(dbName) ? string.Empty : dbName + ".",
					txtFilter.Text
					);

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
