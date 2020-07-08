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
				var ds = Table.Database.DataSource;
				var whereClause = string.Empty;
				if (tabMain.SelectedIndex == 0)
				{
					var firstIn = true;
					foreach (DataGridViewRow row in gridColumns.Rows)
					{
						var oper = row.Cells["Operator"].Value?.ToString();
						var val = row.Cells["Value"].Value?.ToString();
						if (!string.IsNullOrEmpty(oper) && !string.IsNullOrEmpty(val))
						{
							whereClause += firstIn ? "WHERE " : " AND ";
							whereClause += ds.GetConvertedObjectName(row.Cells["ColumnName"].Value.ToString()) + " "
								+ oper + string.Format(" {0}", oper == "LIKE" ? $"'%{val}%'" : $"'{val}'");
						}
					}
				}
				else
				{
					whereClause = txtFilter.Text;
				}

				var dbName = ds.GetConvertedObjectName(Table.Database.DatabaseName);
				Connection.ChangeDatabase(Table.Database.DatabaseName);
				var objName = Table.GetObjectNameWithSchema(Table.Database.DataSource);
				var columns = Table.Columns.Select(c => c.ColumnName).ToArray();

				Script = string.Format("SELECT *\r\nFROM {1}{0} {2}",
					objName,
					string.IsNullOrEmpty(dbName) ? string.Empty : dbName + ".",
					whereClause
					);

				this.DialogResult = DialogResult.OK;
				this.Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Invalid filter");
			}
		}

		private void frmEditFilter_Load(object sender, EventArgs e)
		{
			if (!Table.Columns.Any())
				Table.Database.DataSource.PopulateChildColumns(Connection, Table);

			gridColumns.AutoGenerateColumns = false;
			gridColumns.DataSource = Table.Columns.Take(100).ToList();
		}
	}
}
