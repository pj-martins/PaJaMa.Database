using PaJaMa.Database.Library.DatabaseObjects;
using PaJaMa.Database.Library.DataSources;
using PaJaMa.Database.Library.Helpers;
using PaJaMa.Database.Library.Synchronization;
using PaJaMa.Database.Library.Workspaces;
using PaJaMa.Database.Studio.Classes;
using PaJaMa.Database.Studio.Query.QueryBuilder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PaJaMa.Database.Studio.Query
{
	public partial class ucWorkspace : UserControl
	{
		const string NONE = "__NONE__";

		public WinControls.TabControl.TabControl ParentTabControl { get; set; }

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

			var settings = PaJaMa.Common.SettingsHelper.GetUserSettings<DatabaseStudioSettings>();
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
			else if (!string.IsNullOrEmpty(settings.LastQueryConnectionString))
			{
				txtConnectionString.Text = settings.LastQueryConnectionString;
				cboServer.SelectedItem = Type.GetType(settings.LastQueryServerType);
				chkUseDummyDA.Checked = settings.LastQueryUseDummyDA;
			}
		}

		public void Disconnect()
		{
			tabOutputs.Visible = false;
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

			foreach (var page in tabOutputs.TabPages)
			{
				if (page.Controls.Count > 0)
				{
					var uc = page.Controls[0] as ucQueryOutput;
					uc.Disconnect();
				}
			}

			splitMain.Enabled = false;
			treeTables.Nodes.Clear();
		}

		private void btnConnect_Click(object sender, EventArgs e)
		{
			Disconnect();

			if (cboServer.SelectedItem == null)
			{
				MessageBox.Show("Select server type.");
				return;
			}

			try
			{
				_dataSource = Activator.CreateInstance(cboServer.SelectedItem as Type, new object[] { txtConnectionString.Text }) as DataSource;
				_currentConnection = _dataSource.OpenConnection();
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
				if (ex is TargetInvocationException && ex.InnerException != null)
					ex = ex.InnerException;
				MessageBox.Show(ex.Message);
				return;
			}

			tabOutputs.Visible = true;
			if (tabOutputs.TabPages.Count < 1)
			{
				var uc = new ucQueryOutput();
				uc.Dock = DockStyle.Fill;
				if (!uc.Connect(_currentConnection, _dataSource, _currentConnection.Database, chkUseDummyDA.Checked))
					return;

				var tabPage = new WinControls.TabControl.TabPage();
				tabPage.Text = "Query " + (tabOutputs.TabPages.Count + 1).ToString();
				tabPage.Controls.Add(uc);
				tabOutputs.TabPages.Add(tabPage);
				tabOutputs.SelectedTab = tabPage;
			}
			else
			{
				foreach (var page in tabOutputs.TabPages)
				{
					var uc = page.Controls[0] as ucQueryOutput;
					if (!uc.Connect(_currentConnection, _dataSource, _currentConnection.Database, chkUseDummyDA.Checked))
						return;
				}
			}

			var settings = PaJaMa.Common.SettingsHelper.GetUserSettings<DatabaseStudioSettings>();
			List<string> connStrings = settings.ConnectionStrings.Split('|').ToList();
			if (!connStrings.Any(s => s == txtConnectionString.Text))
				connStrings.Add(txtConnectionString.Text);
			settings.ConnectionStrings = string.Join("|", connStrings.ToArray());
			settings.LastQueryConnectionString = txtConnectionString.Text;
			settings.LastQueryServerType = (cboServer.SelectedItem as Type).AssemblyQualifiedName;
			settings.LastQueryUseDummyDA = chkUseDummyDA.Checked;
			PaJaMa.Common.SettingsHelper.SaveUserSettings<DatabaseStudioSettings>(settings);

			lblConnString.Text = txtConnectionString.Text;
			pnlConnect.Visible = false;
			treeTables.Nodes.Clear();

			pnlControls.Visible = true;
			splitMain.Enabled = true;

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

		private ucQueryOutput addQueryOutput(WinControls.TabControl.TabPage tabPage, string initialDatabase)
		{
			var uc = new ucQueryOutput();
			uc.Dock = DockStyle.Fill;
			if (!uc.Connect(_currentConnection, _dataSource, initialDatabase, chkUseDummyDA.Checked))
				return null;

			bool add = false;
			if (tabPage == null)
			{
				tabPage = new WinControls.TabControl.TabPage();
				add = true;

			}
			tabPage.Text = "Query " + (tabOutputs.TabPages.Count + 1).ToString();
			tabPage.Controls.Add(uc);

			if (add)
			{
				tabOutputs.TabPages.Add(tabPage);
				tabOutputs.SelectedTab = tabPage;
			}
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
			var settings = PaJaMa.Common.SettingsHelper.GetUserSettings<DatabaseStudioSettings>();
			var conns = settings.ConnectionStrings.Split('|');
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

		private void refreshTableNodes(Schema schema, TreeNode parentNode)
		{
			foreach (var table in from t in schema.Tables
								  orderby t.TableName
								  select t)
			{
				var node2 = parentNode.Nodes.Add(table.TableName);
				node2.Tag = table;
				var node3 = node2.Nodes.Add("Columns");
				foreach (var column in table.Columns)
				{
					var node4 = node3.Nodes.Add(column.ColumnName + " (" + column.ColumnType.TypeName +
						(column.CharacterMaximumLength.GetValueOrDefault() > 0 ? " (" + column.CharacterMaximumLength.Value.ToString() + ")" : "") +
						", " + (column.IsNullable ? "null" : "not null") + ")");
					node4.Tag = column;
				}

				node3 = node2.Nodes.Add("Keys");
				foreach (var key in table.KeyConstraints)
				{
					var node4 = node3.Nodes.Add(key.ConstraintName);
					node4.Tag = key;
				}

				node3 = node2.Nodes.Add("ForeignKeys");
				foreach (var key in table.ForeignKeys)
				{
					var node4 = node3.Nodes.Add(key.ForeignKeyName);
					node4.Tag = key;
				}

				node3 = node2.Nodes.Add("Constraints");
				foreach (var key in table.DefaultConstraints)
				{
					var node4 = node3.Nodes.Add(key.ConstraintName);
					node4.Tag = key;
				}

				node3 = node2.Nodes.Add("Triggers");
				foreach (var key in table.Triggers)
				{
					var node4 = node3.Nodes.Add(key.TriggerName);
					node4.Tag = key;
				}

				node3 = node2.Nodes.Add("Indexes");
				foreach (var key in table.Indexes)
				{
					var node4 = node3.Nodes.Add(key.IndexName);
					node4.Tag = key;
				}
			}
		}

		private void refreshViewNodes(Schema schema, TreeNode parentNode)
		{
			foreach (var view in from v in schema.Views
								 orderby v.ViewName
								 select v)
			{
				var node2 = parentNode.Nodes.Add(view.ViewName);
				node2.Tag = view;
				var node3 = node2.Nodes.Add("Columns");
				foreach (var column in view.Columns)
				{
					var node4 = node3.Nodes.Add(column.ColumnName);
					node4.Tag = column;
				}
			}
		}

		private void refreshFunctionNodes(Schema schema, TreeNode parentNode)
		{
			foreach (var routineSynonym in from rs in schema.RoutinesSynonyms
										   orderby rs.Name
										   select rs)
			{
				var node2 = parentNode.Nodes.Add(routineSynonym.Name);
				node2.Tag = routineSynonym;
			}
		}

		private void refreshSchemaNodes(TreeNode node)
		{
			var db = node.Tag as Library.DatabaseObjects.Database;
			db.DataSource.PopulateSchemas(db);
			foreach (var schema in db.Schemas.OrderBy(s => s.SchemaName))
			{
				var node2 = string.IsNullOrEmpty(schema.SchemaName) ? node : node.Nodes.Add(schema.SchemaName);
				node2.Tag = schema;
				foreach (var val in Common.EnumHelper.GetEnumValues<SchemaNodeType>())
				{
					var node3 = node2.Nodes.Add(val.ToString());
					node3.Nodes.Add(NONE);
					node3.Tag = new SchemaNode(schema, val);
				}
			}
		}

		private void treeTables_BeforeExpand(object sender, TreeViewCancelEventArgs e)
		{
			try
			{
				var node = e.Node;
				if (node.Nodes.Count == 1 && node.Nodes[0].Text == NONE)
				{
					node.Nodes.Clear();
					if (node.Tag is Library.DatabaseObjects.Database)
					{
						refreshSchemaNodes(node);
					}
					else if (node.Tag is SchemaNode)
					{
						var schemaNode = node.Tag as SchemaNode;
						switch (schemaNode.SchemaNodeType)
						{
							case SchemaNodeType.Tables:
								if (!schemaNode.Schema.Tables.Any())
									_dataSource.PopulateTables(new Schema[] { schemaNode.Schema });
								refreshTableNodes(schemaNode.Schema, node);
								break;
							case SchemaNodeType.Views:
								if (!schemaNode.Schema.Views.Any())
									_dataSource.PopulateViews(schemaNode.Schema);
								refreshViewNodes(schemaNode.Schema, node);
								break;
							case SchemaNodeType.Functions:
								if (!schemaNode.Schema.RoutinesSynonyms.Any())
									_dataSource.PopulateRoutinesSynonyms(schemaNode.Schema);
								refreshFunctionNodes(schemaNode.Schema, node);
								break;
						}
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
				e.Cancel = true;
			}
		}

		private void mnuTree_Opening(object sender, CancelEventArgs e)
		{
			var selectedNode = treeTables.SelectedNode;
			selectTop1000ToolStripMenuItem.Enabled = selectToolStripMenuItem.Enabled = selectedNode != null &&
				selectedNode.Tag != null && (selectedNode.Tag is Table || selectedNode.Tag is Library.DatabaseObjects.View);

			newForeignKeyToolStripMenuItem.Visible = selectedNode.Tag is Table || selectedNode.Tag is Column;
			newColumnToolStripMenuItem.Visible = selectedNode.Tag is Table;
			deleteToolStripMenuItem.Visible = selectedNode.Tag is DatabaseObjectBase;
			newTableToolStripMenuItem.Visible = selectedNode.Tag is Library.DatabaseObjects.Schema ||
				(selectedNode.Tag is SchemaNode && (selectedNode.Tag as SchemaNode).SchemaNodeType == SchemaNodeType.Tables);
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
			var uc = addQueryOutput(null, string.Empty);
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
			var tabMain = this.ParentTabControl;
			uc.ParentTabControl = this.ParentTabControl;
			var tab = new WinControls.TabControl.TabPage("Workspace " + (tabMain.TabPages.Count + 1).ToString());
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

			foreach (var page in tabOutputs.TabPages)
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
				var uc = addQueryOutput(null, qry.Database);
				uc.txtQuery.Text = qry.Query;
			}

		}

		public void LoadFromIDatabase(QueryEventArgs args)
		{
			_queryEventArgs = args;
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

			var uc = ws.addQueryOutput(null, string.Empty);
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
					var output = addQueryOutput(null, string.Empty);
					output.PopulateScript(builder.GetQuery(), treeTables.SelectedNode);
				}
			}
			else
			{
				MessageBox.Show("Only SQL connections supported.");
			}
		}

		private void tabOutputs_TabClosing(object sender, WinControls.TabControl.TabEventArgs e)
		{
			var uc = e.TabPage.Controls[0] as ucQueryOutput;
			uc.Disconnect();
		}

		private void tabOutputs_TabAdding(object sender, WinControls.TabControl.TabEventArgs e)
		{
			addQueryOutput(e.TabPage, _currentConnection.Database);
		}

		private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var tag = treeTables.SelectedNode.Tag;
			var isExpanded = treeTables.SelectedNode.IsExpanded;
			if (tag is SchemaNode)
			{
				var schemaNode = tag as SchemaNode;
				switch (schemaNode.SchemaNodeType)
				{
					case SchemaNodeType.Tables:
						foreach (var s in schemaNode.Schema.Database.Schemas)
						{
							s.Tables.Clear();
						}
						treeTables.SelectedNode.Nodes.Clear();
						_dataSource.PopulateTables(schemaNode.Schema.Database.Schemas.ToArray());
						refreshTableNodes(schemaNode.Schema, treeTables.SelectedNode);
						break;
					case SchemaNodeType.Views:
						schemaNode.Schema.Views.Clear();
						treeTables.SelectedNode.Nodes.Clear();
						_dataSource.PopulateViews(schemaNode.Schema);
						refreshViewNodes(schemaNode.Schema, treeTables.SelectedNode);
						break;
				}
			}
			else if (treeTables.SelectedNode.Tag is Library.DatabaseObjects.Database)
			{
				var db = treeTables.SelectedNode.Tag as Library.DatabaseObjects.Database;
				db.Schemas.Clear();
				treeTables.SelectedNode.Nodes.Clear();
				refreshSchemaNodes(treeTables.SelectedNode);
			}
		}

		private void newForeignKeyToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var tag = treeTables.SelectedNode.Tag;
			if (tag is Table || tag is Column)
			{
				var table = tag is Table ? tag as Table : (tag as Column).Table;
				var col = tag is Column ? tag as Column : null;

				using (var frm = new frmForeignKey())
				{
					frm.Tables = table.Schema.Tables;
					frm.ChildTable = table;
					if (col != null)
						frm.ChildColumn = col;
					if (frm.ShowDialog() == DialogResult.OK)
					{
						var uc = addQueryOutput(null, table.Database.DatabaseName);
						uc.txtQuery.Text = frm.GetScript();
					}
				}
			}
		}

		private void newColumnToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var tag = treeTables.SelectedNode.Tag;
			if (tag is Table)
			{
				var table = tag as Table;
				using (var frm = new frmColumn())
				{
					frm.Tables = table.Schema.Tables;
					frm.Table = table;
					if (frm.ShowDialog() == DialogResult.OK)
					{
						var uc = addQueryOutput(null, table.Database.DatabaseName);
						uc.txtQuery.Text = frm.GetScript();
					}
				}
			}
		}

		private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var tag = treeTables.SelectedNode.Tag as DatabaseObjectBase;
			if (tag != null)
			{
				var syncItem = DatabaseObjectSynchronizationBase.GetSynchronization(tag.Database, tag);
				var uc = addQueryOutput(null, tag.Database.DatabaseName);
				uc.txtQuery.Text = syncItem.GetRawDropText();
			}
		}

		private void treeTables_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Delete)
				deleteToolStripMenuItem_Click(sender, e);
		}

		private void newTableToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using (var frm = new frmNewTable())
			{
				var tag = treeTables.SelectedNode.Tag;
				frm.Schema = tag is Schema ? (tag as Schema) : (tag as SchemaNode).Schema;
				if (frm.ShowDialog() == DialogResult.OK)
				{
					var uc = addQueryOutput(null, frm.Schema.Database.DatabaseName);
					uc.txtQuery.Text = frm.GetScript();
				}
			}
		}
	}
}