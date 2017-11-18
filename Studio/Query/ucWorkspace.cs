using PaJaMa.Database.Library.DatabaseObjects;
using PaJaMa.Database.Library.Helpers;
using PaJaMa.Database.Library.Synchronization;
using PaJaMa.Database.Library.Workspaces;
using PaJaMa.Database.Studio.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PaJaMa.Database.Studio.Query
{
	public partial class ucWorkspace : UserControl
	{
		const string NONE = "__NONE__";

		private DbConnection _currentConnection;
		private string _initialConnString;
		private Type _initialDbType;
		private DataSource _dataSource;

		private QueryEventArgs _queryEventArgs;

		public ucWorkspace()
		{
			InitializeComponent();
		}

		private void ucWorkspace_Load(object sender, EventArgs e)
		{
			cboServer.DataSource = DataSource.GetDataSourceTypes();

			var settings = Properties.Settings.Default;

			if (settings.ConnectionStrings == null)
				settings.ConnectionStrings = string.Empty;

			refreshConnStrings();

			this.ParentForm.FormClosing += ucWorkspace_FormClosing;

			if (!string.IsNullOrEmpty(_initialConnString))
			{
				txtConnectionString.Text = _initialConnString;
				cboServer.SelectedItem = _initialDbType;
				btnConnect_Click(sender, e);
				//if (cboDatabases.Items.Count > 0)
				//	cboDatabases.SelectedItem = copiedFrom.cboDatabases.SelectedItem;

				//var treeNode = treeTables.Nodes.OfType<TreeNode>().First(n => n.Text == copiedFrom.cboDatabases.SelectedItem.ToString());
				//treeNode.Expand();

				//copiedFrom = null;
			}
			else if (_queryEventArgs != null)
			{
				txtConnectionString.Text = _queryEventArgs.Database.DataSource.ConnectionString;
				cboServer.SelectedItem = typeof(SqlConnection);
				btnConnect_Click(this, new EventArgs());
				//if (cboDatabases.Items.Count > 0)
				//	cboDatabases.SelectedItem = _queryEventArgs.Database.DatabaseName;

				var treeNode = treeTables.Nodes.OfType<TreeNode>().First(n => n.Text == _queryEventArgs.Database.DatabaseName);
				treeNode.Expand();

				if (_queryEventArgs.InitialTable != null)
				{
					var childNode = (from n in treeNode.Nodes.OfType<TreeNode>()
									 from n2 in n.Nodes.OfType<TreeNode>()
									 let tbl = n2.Tag as Table
									 where tbl != null && tbl.TableName == _queryEventArgs.InitialTable
										&& tbl.Schema.SchemaName == _queryEventArgs.InitialSchema
									 select n2).First();

					treeTables.SelectedNode = childNode;

					if (_queryEventArgs.InitialTopN != null)
						select(_queryEventArgs.InitialTopN.Value);
				}

				_queryEventArgs = null;
			}
			else if (!string.IsNullOrEmpty(Properties.Settings.Default.LastQueryConnectionString))
			{
				txtConnectionString.Text = Properties.Settings.Default.LastQueryConnectionString;
				cboServer.SelectedItem = Type.GetType(Properties.Settings.Default.LastQueryServerType);
				chkUseDummyDA.Checked = Properties.Settings.Default.LastQueryUseDummyDA;
			}
		}

		public void Disconnect()
		{
			if (_currentConnection != null)
			{
				if (_currentConnection.State == ConnectionState.Open)
					_currentConnection.Close();
				_currentConnection.Dispose();
				_currentConnection = null;
			}
			//lblResults.Text = "";

			pnlControls.Visible = false;
			pnlConnect.Visible = true;

			foreach (TabPage page in tabOutputs.TabPages)
			{
				var uc = page.Controls[0] as ucQueryOutput;
				uc.Disconnect();
			}

			tabOutputs.TabPages.Clear();
			splitMain.Panel1Collapsed = true;
		}

		private void btnConnect_Click(object sender, EventArgs e)
		{
			Disconnect();

			if (cboServer.SelectedItem == null)
			{
				MessageBox.Show("Select server type.");
				return;
			}

			_dataSource = Activator.CreateInstance(cboServer.SelectedItem as Type, new object[] { txtConnectionString.Text }) as DataSource;
			_currentConnection = _dataSource.OpenConnection();

			try
			{
				if (chkUseDummyDA.Checked)
				{
					DbDataAdapter dummy;
					if (_currentConnection.GetType().Equals(typeof(System.Data.Odbc.OdbcConnection)))
						dummy = new System.Data.Odbc.OdbcDataAdapter("dummy", (System.Data.Odbc.OdbcConnection)_currentConnection);
					else
						throw new NotImplementedException();
				}
			}
			catch (Exception ex)
			{
				Disconnect();
				MessageBox.Show(ex.Message);
				return;
			}

			var uc = new ucQueryOutput();
			uc.Dock = DockStyle.Fill;
			if (!uc.Connect(_currentConnection, _dataSource, _currentConnection.Database, chkUseDummyDA.Checked))
				return;

			List<string> connStrings = Properties.Settings.Default.ConnectionStrings.Split('|').ToList();
			if (!connStrings.Any(s => s == txtConnectionString.Text))
				connStrings.Add(txtConnectionString.Text);
			Properties.Settings.Default.ConnectionStrings = string.Join("|", connStrings.ToArray());
			Properties.Settings.Default.LastQueryConnectionString = txtConnectionString.Text;
			Properties.Settings.Default.LastQueryServerType = (cboServer.SelectedItem as Type).AssemblyQualifiedName;
			Properties.Settings.Default.LastQueryUseDummyDA = chkUseDummyDA.Checked;
			Properties.Settings.Default.Save();

			var tabPage = new TabPage();
			tabPage.Text = "Query " + (tabOutputs.TabPages.Count + 1).ToString();
			tabPage.Controls.Add(uc);
			tabOutputs.TabPages.Add(tabPage);
			tabOutputs.SelectedTab = tabPage;

			lblConnString.Text = txtConnectionString.Text;
			pnlConnect.Visible = false;
			treeTables.Nodes.Clear();

			pnlControls.Visible = true;

			splitMain.Panel1Collapsed = false;

			foreach (var db in _dataSource.Databases)
			{
				var node = treeTables.Nodes.Add(db.DatabaseName);
				node.Nodes.Add(NONE);
				node.Tag = db;
			}

			if (!string.IsNullOrEmpty(_currentConnection.Database))
			{
				var treeNode = treeTables.Nodes.OfType<TreeNode>().First(n => n.Text == _currentConnection.Database);
				treeNode.Expand();
			}
		}

		private ucQueryOutput addQueryOutput(string initialDatabase)
		{
			var uc = new ucQueryOutput();
			uc.Dock = DockStyle.Fill;
			if (!uc.Connect(_currentConnection, _dataSource, initialDatabase, chkUseDummyDA.Checked))
				return null;
			var tabPage = new TabPage();
			tabPage.Text = "Query " + (tabOutputs.TabPages.Count + 1).ToString();
			tabPage.Controls.Add(uc);
			tabOutputs.TabPages.Add(tabPage);
			tabOutputs.SelectedTab = tabPage;
			return uc;
		}

		private void btnDisconnect_Click(object sender, EventArgs e)
		{
			Disconnect();
		}

		//private DataTable FixBinaryColumnsForDisplay(DataTable t)
		//{
		//	List<string> binaryColumnNames = t.Columns.Cast<DataColumn>().Where(col => col.DataType.Equals(typeof(byte[]))).Select(col => col.ColumnName).ToList();
		//	foreach (string binaryColumnName in binaryColumnNames)
		//	{
		//		// Create temporary column to copy over data
		//		string tempColumnName = "C" + Guid.NewGuid().ToString();
		//		t.Columns.Add(new DataColumn(tempColumnName, typeof(string)));
		//		t.Columns[tempColumnName].SetOrdinal(t.Columns[binaryColumnName].Ordinal);

		//		// Replace values in every row
		//		StringBuilder hexBuilder = new StringBuilder(8000 * 2 + 2);
		//		foreach (DataRow r in t.Rows)
		//		{
		//			r[tempColumnName] = BinaryDataColumnToString(hexBuilder, r[binaryColumnName]);
		//		}

		//		t.Columns.Remove(binaryColumnName);
		//		t.Columns[tempColumnName].ColumnName = binaryColumnName;
		//	}
		//	return t;
		//}

		//private string BinaryDataColumnToString(StringBuilder hexBuilder, object columnValue)
		//{
		//	const string hexChars = "0123456789ABCDEF";
		//	if (columnValue == DBNull.Value)
		//	{
		//		// Return special "(null)" value here for null column values
		//		return "(null)";
		//	}
		//	else
		//	{
		//		// Otherwise return hex representation
		//		byte[] byteArray = (byte[])columnValue;
		//		int displayLength = (byteArray.Length > maxBinaryDisplayString) ? maxBinaryDisplayString : byteArray.Length;
		//		hexBuilder.Length = 0;
		//		hexBuilder.Append("0x");
		//		for (int i = 0; i < displayLength; i++)
		//		{
		//			hexBuilder.Append(hexChars[(int)byteArray[i] >> 4]);
		//			hexBuilder.Append(hexChars[(int)byteArray[i] % 0x10]);
		//		}
		//		return hexBuilder.ToString();
		//	}
		//}

		private void refreshConnStrings()
		{
			var conns = Properties.Settings.Default.ConnectionStrings.Split('|');
			txtConnectionString.Items.Clear();
			txtConnectionString.Items.AddRange(conns.OrderBy(c => c).ToArray());
		}

		private void ucWorkspace_FormClosing(object sender, FormClosingEventArgs e)
		{
			Disconnect();
		}

		private void cboServer_Format(object sender, ListControlConvertEventArgs e)
		{
			Type type = e.ListItem as Type;
			e.Value = type.Name;
		}


		private void btnShowHideTables_Click(object sender, EventArgs e)
		{
			splitMain.Panel1Collapsed = !splitMain.Panel1Collapsed;
		}

		private void treeTables_BeforeExpand(object sender, TreeViewCancelEventArgs e)
		{
			var node = e.Node;
			if (node.Nodes.Count == 1 && node.Nodes[0].Text == NONE)
			{
				node.Nodes.Clear();
				if (node.Tag is Library.DatabaseObjects.Database)
				{
					var db = node.Tag as Library.DatabaseObjects.Database;
					db.PopulateSchemas(true);
					foreach (var schema in db.Schemas.OrderBy(s => s.SchemaName))
					{
						var node2 = string.IsNullOrEmpty(schema.SchemaName) ? node : node.Nodes.Add(schema.SchemaName);

						var node3 = node2.Nodes.Add("Tables");
						node3.Nodes.Add(NONE);
						node3.Tag = schema;

						node3 = node2.Nodes.Add("Views");
						node3.Nodes.Add(NONE);
						node3.Tag = schema;
					}
				}
				else if (node.Tag is Schema)
				{
					var schema = node.Tag as Schema;
					if (node.Text == "Tables")
					{
						schema.Database.PopulateTables(schema);
						foreach (var table in from t in schema.Tables
											  orderby t.TableName
											  select t)
						{
							var node4 = node.Nodes.Add(table.TableName);
							foreach (var column in table.Columns)
							{
								node4.Nodes.Add(column.ColumnName + " (" + column.DataType + ", "
											+ (column.IsNullable ? "null" : "not null") + ")");
							}
							node4.Tag = table;
						}
					}
					else if (node.Text == "Views")
					{
						schema.Database.PopulateViews(schema);
						foreach (var view in from v in schema.Views
											 orderby v.ViewName
											 select v)
						{
							var node4 = node.Nodes.Add(view.ViewName);
							foreach (var column in view.Columns)
							{
								node4.Nodes.Add(column.ColumnName + " (" + column.DataType + ", "
											+ (column.IsNullable ? "null" : "not null") + ")");
							}
							node4.Tag = view;
						}
					}
				}
			}
		}

		private void mnuTree_Opening(object sender, CancelEventArgs e)
		{
			var selectedNode = treeTables.SelectedNode;
			selectTop1000ToolStripMenuItem.Enabled = selectToolStripMenuItem.Enabled = selectedNode != null &&
				selectedNode.Tag != null && (selectedNode.Tag is Table || selectedNode.Tag is Library.DatabaseObjects.View);
		}


		private void selectToolStripMenuItem_Click(object sender, EventArgs e)
		{
			selectToNew(0);
		}

		private void selectTop1000ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			selectToNew(1000);
		}

		private void select(int topN)
		{
			var uc = tabOutputs.SelectedTab.Controls[0] as ucQueryOutput;
			uc.SelectTopN(topN, treeTables.SelectedNode);
		}


		private void selectToNew(int topN)
		{
			var uc = addQueryOutput(string.Empty);
			if (uc == null) return;
			uc.SelectTopN(topN, treeTables.SelectedNode);
		}

		public ucWorkspace CopyWorkspace(bool andText, string initialConnString = null)
		{
			var uc = new ucWorkspace();
			if (string.IsNullOrEmpty(initialConnString))
				initialConnString = txtConnectionString.Text;

			uc._initialConnString = initialConnString;
			uc._initialDbType = cboServer.SelectedItem as Type;
			//if (andText)
			//	uc.txtQuery.Text = txtQuery.Text;
			var tabMain = this.Parent.Parent as TabControl;
			var tab = new TabPage("Workspace " + (tabMain.TabPages.Count + 1).ToString());
			uc.Dock = DockStyle.Fill;
			tab.Controls.Add(uc);
			tabMain.TabPages.Add(tab);
			tabMain.SelectedTab = tab;
			return uc;
		}

		private void treeTables_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			if (e.Button == MouseButtons.Right) treeTables.SelectedNode = e.Node;
		}

		public QueryWorkspace GetWorkspace()
		{
			var ws = new QueryWorkspace()
			{
				ConnectionString = txtConnectionString.Text,
				ConnectionType = (cboServer.SelectedItem as Type).FullName,
				//Database = cboDatabases.Text
			};

			foreach (TabPage page in tabOutputs.TabPages)
			{
				var uc = page.Controls[0] as ucQueryOutput;
				ws.Queries.Add(new QueryOutput() { Query = uc.txtQuery.Text, Database = uc.cboDatabases.Text });
			}

			return ws;
		}

		public void LoadWorkspace(QueryWorkspace workspace)
		{
			txtConnectionString.Text = workspace.ConnectionString;
			cboServer.SelectedItem = cboServer.Items.OfType<Type>().First(t => t.FullName == workspace.ConnectionType);
			btnConnect_Click(this, new EventArgs());

			tabOutputs.TabPages.Clear();
			foreach (var qry in workspace.Queries)
			{
				var uc = addQueryOutput(qry.Database);
				uc.txtQuery.Text = qry.Query;
			}

		}

		public void LoadFromIDatabase(QueryEventArgs args)
		{
			_queryEventArgs = args;
		}

		private void btnClose_Click(object sender, EventArgs e)
		{
			if (tabOutputs.TabPages.Count < 1) return;
			var uc = tabOutputs.SelectedTab.Controls[0] as ucQueryOutput;
			uc.Disconnect();
			tabOutputs.TabPages.Remove(tabOutputs.SelectedTab);
		}

		private void btnAdd_Click(object sender, EventArgs e)
		{
			addQueryOutput(_currentConnection.Database);
		}

		private void scriptCreateToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var connString = txtConnectionString.Text;
			var ws = this;
			var inputBox = PaJaMa.WinControls.InputBox.Show("Enter Target Connection String", "Target", connString);
			if (inputBox.Result == DialogResult.OK && connString != inputBox.Text)
			{
				connString = inputBox.Text;
				ws = CopyWorkspace(false, connString);
			}
			else if (inputBox.Result == DialogResult.Cancel)
				return;

			var uc = ws.addQueryOutput(string.Empty);
			if (uc == null) return;

			if (treeTables.SelectedNode.Tag is Library.DatabaseObjects.Database)
			{
				var db = treeTables.SelectedNode.Tag as Library.DatabaseObjects.Database;
				inputBox = PaJaMa.WinControls.InputBox.Show("Enter Target Database", "Target", db.DatabaseName);
				if (inputBox.Result == DialogResult.OK)
				{
					uc.PopulateScript(new DatabaseSynchronization(db).GetCreateScript(inputBox.Text), treeTables.SelectedNode);
				}
			}
			else
			{
				var obj = treeTables.SelectedNode.Tag as DatabaseObjectBase;
				if (obj != null)
				{
					uc.PopulateScript(DatabaseObjectSynchronizationBase.GetSynchronization(obj.Database, obj).GetRawCreateText(), treeTables.SelectedNode);
				}
			}
		}

		private void buildQueryToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (_currentConnection is SqlConnection)
			{
				string databaseName = _currentConnection.Database;
				if (treeTables.SelectedNode.Tag is Library.DatabaseObjects.Database)
					databaseName = (treeTables.SelectedNode.Tag as Library.DatabaseObjects.Database).DatabaseName;
				var builder = new QueryBuilder.frmQueryBuilder(new QueryBuilderHelper(_currentConnection.ConnectionString, databaseName));
				var dlgResult = builder.ShowDialog();
				if (dlgResult == DialogResult.OK)
				{
					var output = addQueryOutput(string.Empty);
					output.PopulateScript(builder.GetQuery(), treeTables.SelectedNode);
				}
			}
			else
			{
				MessageBox.Show("Only SQL connections supported.");
			}
		}
	}
}
