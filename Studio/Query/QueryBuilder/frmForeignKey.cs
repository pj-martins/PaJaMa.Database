using PaJaMa.Database.Library.DatabaseObjects;
using PaJaMa.Database.Library.Synchronization;
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
	public partial class frmForeignKey : Form
	{
		public List<Table> Tables
		{
			get { return cboParentTable.Items.OfType<Table>().ToList(); }
			set
			{
				cboParentTable.Items.Clear();
				cboChildTable.Items.Clear();
				cboParentTable.Items.AddRange(value.OrderBy(v => v.TableName).ToArray());
				cboChildTable.Items.AddRange(value.OrderBy(v => v.TableName).ToArray());
			}
		}

		public DbConnection Connection { get; set; }

		public Table ChildTable
		{
			get { return cboChildTable.SelectedItem as Table; }
			set { cboChildTable.SelectedItem = value; }
		}

		public Column ChildColumn
		{
			get { return cboChildColumn.SelectedItem as Column; }
			set { cboChildColumn.SelectedItem = value; }
		}

		public frmForeignKey()
		{
			InitializeComponent();
			cboDeleteRule.SelectedIndex = 0;
		}

		public string GetScript()
		{
			var parentTable = cboParentTable.SelectedItem as Table;
			var parentColumn = cboParentColumn.SelectedItem as Column;
			var fk = new ForeignKey(parentTable.Database);
			fk.ChildTable = ChildTable;
			fk.ParentTable = parentTable;
			fk.ForeignKeyName = $"FK_{fk.ChildTable.TableName}_{fk.ParentTable.TableName}";
			fk.Columns.Add(new ForeignKeyColumn() { ChildColumn = ChildColumn, ParentColumn = parentColumn });
			// TODO: rules
			fk.DeleteRule = cboDeleteRule.Text;
			fk.UpdateRule = "NO ACTION";
			return new ForeignKeySynchronization(parentTable.Database, fk).GetRawCreateText(true);
		}

		private void cboParentTable_SelectedIndexChanged(object sender, EventArgs e)
		{
			cboParentColumn.Items.Clear();
			var parentTable = cboParentTable.SelectedItem as Table;
			if (parentTable != null)
			{
				if (!parentTable.Columns.Any())
				{
					parentTable.Database.DataSource.PopulateColumnsForTable(Connection, parentTable);
				}
				cboParentColumn.Items.AddRange(parentTable.Columns.OrderBy(c => c.ColumnName).ToArray());
				if (ChildColumn != null)
					cboParentColumn.SelectedItem = cboParentColumn.Items.OfType<Column>().FirstOrDefault(c => c.ColumnName == ChildColumn.ColumnName);
			}
		}

		private void cboChildTable_SelectedIndexChanged(object sender, EventArgs e)
		{
			cboChildColumn.Items.Clear();
			if (ChildTable != null)
				cboChildColumn.Items.AddRange(ChildTable.Columns.OrderBy(c => c.ColumnName).ToArray());
		}


		private void btnOK_Click(object sender, EventArgs e)
		{
			var parentTable = cboParentTable.SelectedItem as Table;
			var parentColumn = cboParentColumn.SelectedItem as Column;
			if (parentTable == null || ChildTable == null || parentColumn == null || ChildColumn == null)
			{
				MessageBox.Show("Please fill in all fields!");
				return;
			}
			this.DialogResult = DialogResult.OK;
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}


	}
}
