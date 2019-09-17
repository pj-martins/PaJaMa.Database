using PaJaMa.Database.Library.DatabaseObjects;
using PaJaMa.Database.Library.Synchronization;
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
	public partial class frmNewTable : Form
	{
		private Schema _schema;
		public Schema Schema
		{
			get { return _schema; }
			set
			{
				_schema = value;
				ColumnType.Items.AddRange(value.Database.DataSource.ColumnTypes.OrderBy(ct => ct.CreateTypeName).ToArray());
			}
		}
		public frmNewTable()
		{
			InitializeComponent();
		}

		private void frmNewTable_Load(object sender, EventArgs e)
		{
			var bindingList = new BindingList<Column>();
			bindingList.AllowNew = true;
			bindingList.AllowEdit = true;
			bindingList.AllowRemove = true;
			bindingList.AddingNew += BindingList_AddingNew;
			gridMain.AutoGenerateColumns = false;
			gridMain.DataSource = bindingList;
		}

		private void BindingList_AddingNew(object sender, AddingNewEventArgs e)
		{
			e.NewObject = new Column(_schema.Database);
		}

		private void gridMain_CellParsing(object sender, DataGridViewCellParsingEventArgs e)
		{
			if (this.gridMain.Columns[e.ColumnIndex] == ColumnType)
			{
				if (e != null && e.Value != null)
				{
					try
					{
						e.Value = ColumnType.Items.OfType<ColumnType>().First(ct => ct.CreateTypeName == e.Value.ToString());
						e.ParsingApplied = true;

					}
					catch (FormatException)
					{
						e.ParsingApplied = false;
					}
				}
			}
		}

		public string GetScript()
		{
			var tbl = new Table(_schema.Database);
			tbl.Schema = _schema;
			tbl.TableName = txtTableName.Text;
			tbl.Columns.AddRange((gridMain.DataSource as BindingList<Column>).ToList());
			foreach (var c in tbl.Columns)
			{
				c.Parent = tbl;
			}
			var keyConstraints = new List<KeyConstraint>();
			foreach (var row in gridMain.Rows.OfType<DataGridViewRow>())
			{
				if (row.Cells[PrimaryKey.Index].Value != null && (bool)row.Cells[PrimaryKey.Index].Value)
				{
					var kc = new KeyConstraint(_schema.Database);
					var colName = row.Cells[ColumnName.Index].Value.ToString();
					kc.ConstraintName = "PK_" + colName;
					kc.IsPrimaryKey = true;
					kc.Columns.Add(new IndexColumn() { ColumnName = colName });
					keyConstraints.Add(kc);
				}
			}
			tbl.KeyConstraints.AddRange(keyConstraints);
			return new TableSynchronization(_schema.Database, tbl).GetRawCreateText();
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			this.Close();
		}
	}
}
