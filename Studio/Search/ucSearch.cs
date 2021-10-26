using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PaJaMa.Database.Studio.Classes;
using PaJaMa.Common;
using PaJaMa.Database.Library.Helpers;
using PaJaMa.Database.Library.Workspaces.Search;
using PaJaMa.Database.Library.DataSources;
using PaJaMa.Database.Library;

namespace PaJaMa.Database.Studio.Search
{
    public partial class ucSearch : UserControl
    {
        private SearchHelper _searchHelper;
        private bool _lockDbChange = false;

        public ucSearch()
        {
            InitializeComponent();
        }


        private void UcSearch_Load(object sender, EventArgs e)
        {
            var settings = PaJaMa.Common.SettingsHelper.GetUserSettings<DatabaseStudioSettings>();
            if (!settings.Connections.Any()) DatabaseStudioConnection.ConvertFromLegacy(settings);

            refreshConnStrings();

            if (settings.LastSearchConnection != null)
                cboConnection.SelectedItem = cboConnection.Items.OfType<DatabaseStudioConnection>().First(x => x.ConnectionName == settings.LastSearchConnection.ConnectionName);

            var types = DataSource.GetDataSourceTypes();

            new GridHelper().DecorateGrid(gridTables);
            new GridHelper().DecorateGrid(gridColumns);
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
            var connection = cboConnection.SelectedItem as DatabaseStudioConnection;

            Exception exception = null;

            DataTable schema = null;
            string database = string.Empty;

            var worker = new BackgroundWorker();
            worker.DoWork += delegate (object sender2, DoWorkEventArgs e2)
            {
                try
                {
                    var dataSource = Activator.CreateInstance(typeof(DataSource).Assembly.GetType(connection.DataSourceType), new object[] { connection.GetConnectionString() }) as DataSource;
                    using (var conn = dataSource.OpenConnection(string.Empty))
                    {
                        schema = conn.GetSchema("Databases");
                        database = conn.Database;
                        conn.Close();
                    }
                }
                catch (Exception ex)
                {
                    exception = new Exception("Error opening source connection: " + ex.Message);
                    return;
                }

                _searchHelper = new SearchHelper(typeof(DataSource).Assembly.GetType(connection.DataSourceType), connection.GetConnectionString(), worker);
            };

            WinControls.WinProgressBox.ShowProgress(worker, progressBarStyle: ProgressBarStyle.Marquee);
            if (exception != null)
                MessageBox.Show(exception.Message);
            else
            {
                refreshPage(false);

                var settings = PaJaMa.Common.SettingsHelper.GetUserSettings<DatabaseStudioSettings>();
                settings.LastSearchConnection = cboConnection.SelectedItem as DatabaseStudioConnection;
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
                btnSearch.Enabled = btnRefresh.Enabled = true;
            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            gridTables.DataSource = null;
            btnDisconnect.Visible = false;
            btnConnect.Visible = true;
            cboConnection.Enabled = true;
            cboDatabase.Visible = false;
            btnSearch.Enabled = btnRefresh.Enabled = false;
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
                    grid.AllowUserToDeleteRows = false;
                    grid.ReadOnly = true;
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

        private void refreshPage(bool reinit)
        {
            if (reinit)
            {
                var worker = new BackgroundWorker();
                worker.DoWork += delegate (object sender2, DoWorkEventArgs e2)
                {
                    _searchHelper.DataSource.PopulateChildren(null, true, worker);
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

        private void cboServer_Format(object sender, ListControlConvertEventArgs e)
        {
            Type type = e.ListItem as Type;
            e.Value = type.Name;
        }

        private void gridColumns_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            gridTables.Invalidate();
        }
    }
}
