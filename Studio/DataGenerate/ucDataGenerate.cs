using PaJaMa.Common;
using PaJaMa.Database.Library;
using PaJaMa.Database.Library.Helpers;
using PaJaMa.Database.Library.Workspaces.Generate;
using PaJaMa.Database.Studio.Classes;
using PaJaMa.Database.Studio.DataGenerate.Content;
using PaJaMa.WinControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PaJaMa.Database.Studio.DataGenerate
{
	public partial class ucDataGenerate : UserControl
	{
		private GeneratorHelper _generatorHelper;

		private bool _lockDbChange = false;

		public event QueryEventHandler QueryDatabase;

		public ucDataGenerate()
		{
			InitializeComponent();
		}

		private void UcDataGenerate_Load(object sender, EventArgs e)
		{
			var settings = PaJaMa.Common.SettingsHelper.GetUserSettings<DatabaseStudioSettings>();
			if (!settings.Connections.Any()) DatabaseStudioConnection.ConvertFromLegacy(settings);

			refreshConnStrings();

			//if (!string.IsNullOrEmpty(settings.LastQueryConnectionString))
			//	cboConnection.Text = settings.LastQueryConnectionString;

			new GridHelper().DecorateGrid(gridTables);
		}

		private void refreshConnStrings()
		{
			var settings = PaJaMa.Common.SettingsHelper.GetUserSettings<DatabaseStudioSettings>();
			if (!settings.Connections.Any()) DatabaseStudioConnection.ConvertFromLegacy(settings);
				cboConnection.Items.Clear();
				cboConnection.Items.AddRange(settings.Connections.OrderBy(c => c.ConnectionName).ToArray());
		}

		private void btnConnect_Click(object sender, EventArgs e)
		{

			string connString = cboConnection.Text;

			Exception exception = null;

			DataTable schema = null;
			string database = string.Empty;

			var worker = new BackgroundWorker();
			worker.DoWork += delegate(object sender2, DoWorkEventArgs e2)
			{
				try
				{
					using (var conn = new SqlConnection(connString))
					{
						conn.Open();
						schema = conn.GetSchema("Databases");
						database = conn.Database;
						conn.Close();
						SqlConnection.ClearPool(conn);
					}
				}
				catch (Exception ex)
				{
					exception = new Exception("Error opening source connection: " + ex.Message);
					return;
				}

                // TODO:
				_generatorHelper = new GeneratorHelper(typeof(SqlConnection), connString, worker);
				_generatorHelper.Prompt += delegate (object s3, PromptEventArgs e3)
				{
					e3.Result = ScrollableMessageBox.ShowDialog(e3.Message, "Error", ScrollableMessageBoxButtons.Yes, ScrollableMessageBoxButtons.No);
				};
			};

			WinControls.WinProgressBox.ShowProgress(worker, progressBarStyle: ProgressBarStyle.Marquee);
			if (exception != null)
				MessageBox.Show(exception.Message);
			else
			{
				refreshPage(false);

				var settings = PaJaMa.Common.SettingsHelper.GetUserSettings<DatabaseStudioSettings>();
				PaJaMa.Common.SettingsHelper.SaveUserSettings<DatabaseStudioSettings>(settings);

				_lockDbChange = true;
				cboDatabase.Items.Clear();
				foreach (var dr in schema.Rows.OfType<DataRow>())
				{
					cboDatabase.Items.Add(dr["database_name"].ToString());
				}
				cboDatabase.Text = database;

				_lockDbChange = false;

				btnConnect.Visible = false;
				btnDisconnect.Visible = true;
				cboConnection.SelectionLength = 0;
				cboConnection.Enabled = false;
				cboDatabase.Visible = true;
				btnGo.Enabled = btnRefresh.Enabled = btnViewMissingDependencies.Enabled = btnAdd10.Enabled = btnAddNRows.Enabled = btnQuery.Enabled = true;
			}
		}

		private void refreshPage(bool reinit)
		{
			if (reinit)
			{
				var worker = new BackgroundWorker();
				worker.DoWork += delegate(object sender2, DoWorkEventArgs e2)
				{
					_generatorHelper.DataSource.PopulateChildren(null, true, worker);
				};
				WinControls.WinProgressBox.ShowProgress(worker, progressBarStyle: ProgressBarStyle.Marquee);
			}

			refreshTables();
		}

		private void refreshTables()
		{
			var lst = TableWorkspaceList.GetTableWorkspaces(_generatorHelper);
			gridTables.AutoGenerateColumns = false;
			gridTables.DataSource = new BindingList<TableWorkspace>(lst.Workspaces.OrderBy(w => w.Table.ToString()).ToList());
		}

		private StringBuilder getMissingDependencyString()
		{
			var sb = new StringBuilder();
			var workspaces = gridTables.Rows.OfType<DataGridViewRow>().Select(r => (r.DataBoundItem as TableWorkspace))
				.Where(tw => tw.AddRowCount > 0 || tw.CurrentRowCount > 0)
				.ToList();

			var missingDependencies = _generatorHelper.GetMissingDependencies(workspaces);
			if (missingDependencies.Any())
			{
				foreach (var kvp in missingDependencies)
				{
					sb.AppendLine(kvp.Key.TableName + " is dependent on: " + string.Join(", ", kvp.Value.Select(v => v.TableName).ToArray()));
				}
			}

			return sb;
		}

		private void btnGo_Click(object sender, EventArgs e)
		{
			var sb = getMissingDependencyString();

			if (sb.Length > 0)
			{
				MessageBox.Show(sb.ToString(), "Dependencies");
				return;
			}


			var workspaces = getSortedWorkspaces((gridTables.DataSource as BindingList<TableWorkspace>)
				.Where(t => t.AddRowCount > 0 || t.Truncate || t.Delete).ToList());

			var changes = workspaces.Select(t => t.Table.TableName).ToList();

			if (changes.Count() > 15)
			{
				changes = changes.Take(15).ToList();
				changes.Add("...");
			}

			if (ScrollableMessageBox.ShowDialog(string.Format("{0} - {1} will be populated:\r\n\r\n{2}\r\n\r\nContinue?", _generatorHelper.DataSource.DataSourceName,
					_generatorHelper.DataSource.CurrentDatabase.DatabaseName,
				string.Join("\r\n", changes.ToArray())), "Proceed", ScrollableMessageBoxButtons.Yes, ScrollableMessageBoxButtons.No) != Common.PromptResult.Yes)
				return;


			if (!workspaces.Any())
			{
				MessageBox.Show("Nothing to populate.");
				return;
			}

			bool success = false;
			var worker = new BackgroundWorker();
			worker.WorkerReportsProgress = true;
			worker.WorkerSupportsCancellation = true;
			worker.DoWork += delegate(object sender2, DoWorkEventArgs e2)
			{
				if (workspaces.Any() && !_generatorHelper.Generate(worker, workspaces))
					return;

				success = true;
			};

			PaJaMa.WinControls.WinProgressBox.ShowProgress(worker, allowCancel: true);

			if (success)
			{
				MessageBox.Show("Done");
				refreshPage(true);
			}
		}

		private List<TableWorkspace> getSortedWorkspaces(List<TableWorkspace> workspaces)
		{
			var sortedWorkspaces = new List<TableWorkspace>();

			// make copy
			var currentWorkspaces = workspaces.ToList();

			while (currentWorkspaces.Count > 0)
			{
				foreach (var ws in currentWorkspaces)
				{
					var tempSelected = sortedWorkspaces.ToList();
					tempSelected.Add(ws);

					var missing = _generatorHelper.GetMissingDependencies(tempSelected);
					if (!missing.Any(n => n.Value.Any(v => currentWorkspaces.Any(cw => cw.Table.TableName == v.TableName))))
					{
						sortedWorkspaces.Add(ws);
						currentWorkspaces.Remove(ws);
						break;
					}
				}
			}

			return sortedWorkspaces;
		}

		private void btnDisconnect_Click(object sender, EventArgs e)
		{
			gridTables.DataSource = null;
			btnDisconnect.Visible = false;
			btnConnect.Visible = true;
			cboConnection.Enabled = true;
			cboDatabase.Visible = false;
			btnGo.Enabled = btnViewMissingDependencies.Enabled = btnRefresh.Enabled = btnAdd10.Enabled = btnAddNRows.Enabled = btnQuery.Enabled = false;
		}

		private void cboDatabase_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (_lockDbChange) return;
			_generatorHelper.DataSource.ChangeDatabase(cboDatabase.Text);
			refreshPage(true);
		}

		private void gridTables_MouseClick(object sender, MouseEventArgs e)
		{
			//if (e.Button == System.Windows.Forms.MouseButtons.Right)
			//	_tablesMenu.Show(gridTables, new Point(e.X, e.Y));
		}

		private void gridObjects_MouseClick(object sender, MouseEventArgs e)
		{
			//if (e.Button == System.Windows.Forms.MouseButtons.Right)
			//	_objectsMenu.Show(gridObjects, new Point(e.X, e.Y));
		}

		private void gridTables_MouseDown(object sender, MouseEventArgs e)
		{

		}

		private void gridTables_DataError(object sender, DataGridViewDataErrorEventArgs e)
		{

		}

		private void gridDataColumns_DataError(object sender, DataGridViewDataErrorEventArgs e)
		{

		}

		private void btnRefresh_Click(object sender, EventArgs e)
		{
			refreshPage(true);
		}

		private void btnViewMissingDependencies_Click(object sender, EventArgs e)
		{
			var sb = getMissingDependencyString();

			if (sb.Length > 0)
				MessageBox.Show(sb.ToString());
			else
				MessageBox.Show("No missing dependencies!");
		}

		private void btnSelectAll_Click(object sender, EventArgs e)
		{
			foreach (var row in gridTables.SelectedRows.OfType<DataGridViewRow>())
			{
				var tableWorkspace = row.DataBoundItem as TableWorkspace;
				tableWorkspace.AddRowCount += 10;
			}
			gridTables.Invalidate();
		}

		private void gridTables_SelectionChanged(object sender, EventArgs e)
		{
			gridColumns.DataSource = null;
			if (gridTables.SelectedRows.Count != 1) return;

			var tableWorkspace = gridTables.SelectedRows[0].DataBoundItem as TableWorkspace;
			gridColumns.AutoGenerateColumns = false;
			gridColumns.DataSource = tableWorkspace.ColumnWorkspaces;
		}

		private void gridColumns_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.ColumnIndex == Content.Index)
			{
				var row = gridColumns.Rows[e.RowIndex].DataBoundItem as ColumnWorkspace;
				if (row.Content == null) return;
				ContentControl.ShowPropertiesControl(row.Content);
			}
		}

		private void btnQuery_Click(object sender, EventArgs e)
		{
			query(null);
		}

		private void selectToolStripMenuItem_Click(object sender, EventArgs e)
		{
			query(0);
		}

		private void selectTop1000ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			query(1000);
		}

		private void query(int? topN = null)
		{
			if (_generatorHelper.DataSource.CurrentDatabase != null)
			{
				var args = new QueryEventArgs();
				args.Database = _generatorHelper.DataSource.CurrentDatabase;
				if (topN != null)
				{
					var selectedItem = gridTables.SelectedRows[0].DataBoundItem as TableWorkspace;
					args.InitialTopN = topN;
					args.InitialTable = selectedItem.Table.TableName;
					args.InitialSchema = selectedItem.Table.Schema.SchemaName;
				}
				QueryDatabase(this, args);
			}
		}

		private void btnAddNRows_Click(object sender, EventArgs e)
		{
			using (var dlg = new dlgAddNRows())
			{
				if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					foreach (var row in gridTables.SelectedRows.OfType<DataGridViewRow>())
					{
						var tableWorkspace = row.DataBoundItem as TableWorkspace;
						tableWorkspace.AddRowCount += (int)dlg.NumberRows.Value;
					}
					gridTables.Invalidate();
				}
			}
		}
	}
}