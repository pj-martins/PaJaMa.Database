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
	public partial class frmColumn : Form
	{
		public frmColumn()
		{
			InitializeComponent();
		}

		public List<Table> Tables
		{
			get { return cboTable.Items.OfType<Table>().ToList(); }
			set
			{
				cboTable.Items.Clear();
				cboTable.Items.AddRange(value.OrderBy(v => v.TableName).ToArray());
				cboType.Items.Clear();
				cboType.Items.AddRange(value.First().Database.DataSource.ColumnTypes.OrderBy(c => c.CreateTypeName).ToArray());
				cboType.SelectedIndex = 0;
			}
		}

		public Table Table
		{
			get { return cboTable.SelectedItem as Table; }
			set { cboTable.SelectedItem = value; }
		}

		public string GetScript()
		{
			var table = cboTable.SelectedItem as Table;
			var col = new Column(table.Database);
			if (numMaxLength.Value > 0) col.CharacterMaximumLength = (int)numMaxLength.Value;
			col.ColumnDefault = txtDefault.Text;
			col.ColumnName = txtColumnName.Text;
			col.ColumnType = cboType.SelectedItem as ColumnType;
			col.IsIdentity = chkIdentity.Checked;
			col.IsNullable = chkAllowNull.Checked;
			if (numPrecision.Value > 0) col.NumericPrecision = (int)numPrecision.Value;
			if (numScale.Value > 0) col.NumericScale = (int)numScale.Value;
			col.Schema = table.Schema;
			col.Parent = table;
			return new ColumnSynchronization(table.Database, col).GetRawCreateText(true);
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(txtColumnName.Text) || cboType.SelectedItem == null)
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
