using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using PaJaMa.Database.Studio.Classes;
using PaJaMa.Common;
using PaJaMa.Database.Library.Helpers;
using PaJaMa.Database.Library.Workspaces.Search;

namespace PaJaMa.Database.Studio.Search
{
	public partial class ucSearch : UserControl
	{
		private SearchHelper _searchHelper;
		private bool _lockDbChange = false;

		public ucSearch()
		{
			InitializeComponent();

			if (DesignMode) return;

			var settings = PaJaMa.Common.SettingsHelper.GetUserSettings<DatabaseStudioSettings>();
			if (settings.SearchConnectionStrings == null)
				settings.SearchConnectionStrings = string.Empty;

			refreshConnStrings();

			if (!string.IsNullOrEmpty(settings.LastSearchConnectionString))
				cboConnectionString.Text = settings.LastSearchConnectionString;

			new GridHelper().DecorateGrid(gridTables);
			new GridHelper().DecorateGrid(gridColumns);
		}

		private void refreshConnStrings()
		{
			var settings = PaJaMa.Common.SettingsHelper.GetUserSettings<DatabaseStudioSettings>();
			if (!string.IsNullOrEmpty(settings.SearchConnectionStrings))
			{
				var conns = settings.SearchConnectionStrings.Split('|');
				cboConnectionString.Items.Clear();
				cboConnectionString.Items.AddRange(conns.OrderBy(c => c).ToArray());
			}
		}

		private void btnConnect_Click(object sender, EventArgs e)
		{

			string connString = cboConnectionString.Text;

			Exception exception = null;

			DataTable schema = null;
			string database = string.Empty;

			var worker = new BackgroundWorker();
			worker.DoWork += delegate (object sender2, DoWorkEventArgs e2)
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

				_searchHelper = new SearchHelper(typeof(SqlConnection), connString, worker);
			};

			WinControls.WinProgressBox.ShowProgress(worker, progressBarStyle: ProgressBarStyle.Marquee);
			if (exception != null)
				MessageBox.Show(exception.Message);
			else
			{
				refreshPage(false);

				var settings = PaJaMa.Common.SettingsHelper.GetUserSettings<DatabaseStudioSettings>();
				List<string> connStrings = settings.SearchConnectionStrings.Split('|').ToList();
				if (!connStrings.Any(s => s == cboConnectionString.Text))
					connStrings.Add(cboConnectionString.Text);

				settings.SearchConnectionStrings = string.Join("|", connStrings.ToArray());
				settings.LastSearchConnectionString = cboConnectionString.Text;
				PaJaMa.Common.SettingsHelper.SaveUserSettings<DatabaseStudioSettings>(settings);

				_lockDbChange = true;
				cboDatabase.Items.Clear();
				foreach (var dr in schema.Rows.OfType<DataRow>())
				{
					cboDatabase.Items.Add(dr["database_name"].ToString());
				}
				cboDatabase.Text = database;

				_lockDbChange = false;

				btnConnect.Visible = btnRemoveConnString.Visible = false;
				btnDisconnect.Visible = true;
				cboConnectionString.SelectionLength = 0;
				cboConnectionString.Enabled = false;
				cboDatabase.Visible = true;
				btnSearch.Enabled = btnRefresh.Enabled = true;
			}
		}

		private void btnDisconnect_Click(object sender, EventArgs e)
		{
			gridTables.DataSource = null;
			btnDisconnect.Visible = false;
			btnConnect.Visible = true;
			cboConnectionString.Enabled = true;
			btnRemoveConnString.Visible = true;
			cboDatabase.Visible = false;
			btnSearch.Enabled = btnRefresh.Enabled = false;
		}

		private void btnRemoveConnString_Click(object sender, EventArgs e)
		{
			removeConnString(cboConnectionString.Text, true);
		}

		private void btnRefresh_Click(object sender, EventArgs e)
		{
			refreshPage(true);
		}

		private void btnSearch_Click(object sender, EventArgs e)
		{
			tabResults.TabPages.Clear();
			var dts = new BindingList<DataTable>();
			dts.ListChanged += dts_ListChanged;

			var cols = from ws in gridTables.DataSource as BindingList<TableWorkspace>
					   from c in ws.ColumnWorkspaces
					   where c.Select
					   select c;

			var worker = new BackgroundWorker();
			string searchFor = txtSearch.Text;
			worker.DoWork += delegate (object sender2, DoWorkEventArgs e2)
			{
				try
				{
					_searchHelper.Search(searchFor, cols.ToList(), dts);
				}
				catch (Exception ex)
				{
					this.Invoke(new Action(() =>
					{
						MessageBox.Show(ex.Message);
					}));
				}
			};
			WinControls.WinProgressBox.ShowProgress(worker, progressBarStyle: ProgressBarStyle.Marquee);
		}

		private void dts_ListChanged(object sender, ListChangedEventArgs e)
		{
			if (e.ListChangedType == ListChangedType.ItemAdded)
			{
				this.Invoke(new Action(() =>
				{
					var tbl = (sender as BindingList<DataTable>)[e.NewIndex];
					var tab = new TabPage(tbl.TableName);
					var grid = new DataGridView();
					grid.AllowUserToAddRows = false;
					grid.AllowUserToDeleteRows = true;
					grid.DataSource = tbl;
					grid.Dock = DockStyle.Fill;
					tab.Controls.Add(grid);
					tabResults.TabPages.Add(tab);
				}));
			}
		}

		private void cboDatabase_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (_lockDbChange) return;
			_searchHelper.DataSource.ChangeDatabase(cboDatabase.Text);
			refreshPage(true);
		}

		private void cboConnectionString_SelectedIndexChanged(object sender, EventArgs e)
		{
			btnRemoveConnString.Enabled = !string.IsNullOrEmpty(cboConnectionString.Text);
		}


		private void removeConnString(string connString, bool source)
		{
			var settings = PaJaMa.Common.SettingsHelper.GetUserSettings<DatabaseStudioSettings>();
			List<string> connStrings = settings.SearchConnectionStrings.Split('|').ToList();
			connStrings.Remove(connString);
			settings.SearchConnectionStrings = string.Join("|", connStrings.ToArray());
			settings.LastSearchConnectionString = string.Empty;
			PaJaMa.Common.SettingsHelper.SaveUserSettings<DatabaseStudioSettings>(settings);
			refreshConnStrings();
			cboConnectionString.Text = string.Empty;
		}

		private void refreshPage(bool reinit)
		{
			if (reinit)
			{
				var worker = new BackgroundWorker();
				worker.DoWork += delegate (object sender2, DoWorkEventArgs e2)
				{
					_searchHelper.DataSource.CurrentDatabase.PopulateChildren(true, worker);
				};
				WinControls.WinProgressBox.ShowProgress(worker, progressBarStyle: ProgressBarStyle.Marquee);
			}

			refreshTables();
		}

		private void refreshTables()
		{
			var lst = TableWorkspaceList.GetTableWorkspaces(_searchHelper);
			gridTables.AutoGenerateColumns = false;
			gridTables.DataSource = new BindingList<TableWorkspace>(lst.Workspaces.OrderBy(w => w.Table.ToString()).ToList());
		}

		private void gridTables_SelectionChanged(object sender, EventArgs e)
		{
			gridColumns.DataSource = null;

			var tableWorkspaces = gridTables.SelectedRows.OfType<DataGridViewRow>().Select(r => r.DataBoundItem as TableWorkspace);
			gridColumns.AutoGenerateColumns = false;
			gridColumns.DataSource = new SortableBindingList<ColumnWorkspace>((from t in tableWorkspaces
																			   from c in t.ColumnWorkspaces
																			   select c).ToList());
		}
	}
}
