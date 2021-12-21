﻿using PaJaMa.Database.Library;
using PaJaMa.Database.Library.DatabaseObjects;
using PaJaMa.Database.Library.DataSources;
using PaJaMa.Database.Library.Helpers;
using PaJaMa.Database.Library.Synchronization;
using PaJaMa.Database.Library.Workspaces.Compare;
using PaJaMa.Database.Studio.Classes;
using PaJaMa.WinControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PaJaMa.Database.Studio.DataCompare
{
    public partial class ucDataCompare : UserControl
    {
        private CompareHelper _compareHelper;

        private bool _lockDbChange = false;

        public event QueryEventHandler QueryDatabase;

        public ucDataCompare()
        {
            InitializeComponent();
            MenuHelper.CreateFileMenu(this);
        }

        private void refreshConnStrings()
        {
            var settings = PaJaMa.Common.SettingsHelper.GetUserSettings<DatabaseStudioSettings>();
            if (!settings.Connections.Any()) DatabaseStudioConnection.ConvertFromLegacy(settings);
            cboSource.Items.Clear();
            cboTarget.Items.Clear();
            cboSource.Items.AddRange(settings.Connections.OrderBy(c => c.ConnectionName).ToArray());
            cboTarget.Items.AddRange(settings.Connections.OrderBy(c => c.ConnectionName).ToArray());
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {

            var fromConnection = cboSource.SelectedIndex < 0
                ? cboSource.Items.OfType<DatabaseStudioConnection>().First(x => x.ConnectionName == cboTarget.Text)
                : cboSource.SelectedItem as DatabaseStudioConnection;
            var toConnection = cboTarget.SelectedIndex < 0 
                ? cboTarget.Items.OfType<DatabaseStudioConnection>().First(x => x.ConnectionName == cboTarget.Text)
                : cboTarget.SelectedItem as DatabaseStudioConnection;

            Exception exception = null;

            DataSource fromDataSource = null;
            DataSource toDataSource = null;

            var worker = new BackgroundWorker();
            worker.DoWork += delegate (object sender2, DoWorkEventArgs e2)
                {
                    try
                    {
                        fromDataSource = Activator.CreateInstance(typeof(DataSource).Assembly.GetType(fromConnection.DataSourceType), new object[] { fromConnection.GetConnectionString() }) as DataSource;
                    }
                    catch (Exception ex)
                    {
                        exception = new Exception("Error opening source connection: " +
                            (ex is TargetInvocationException && ex.InnerException != null ? ex.InnerException.Message : ex.Message));
                        return;
                    }

                    try
                    {
                        toDataSource = Activator.CreateInstance(typeof(DataSource).Assembly.GetType(toConnection.DataSourceType), new object[] { toConnection.GetConnectionString() }) as DataSource;
                    }
                    catch (Exception ex)
                    {
                        exception = new Exception("Error opening target connection: " +
                            (ex is TargetInvocationException && ex.InnerException != null ? ex.InnerException.Message : ex.Message));
                        return;
                    }

                    try
                    {
                        _compareHelper = new CompareHelper(fromDataSource, toDataSource, true, worker);
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                    }

                    if (_compareHelper != null)
                    {
                        _compareHelper.Prompt += delegate (object s3, Common.PromptEventArgs e3)
                        {
                            e3.Result = ScrollableMessageBox.ShowDialog(e3.Message, "Error!", ScrollableMessageBoxButtons.YesToAll, ScrollableMessageBoxButtons.Yes, ScrollableMessageBoxButtons.No);
                        };
                    }
                };

            WinControls.WinProgressBox.ShowProgress(worker, progressBarStyle: ProgressBarStyle.Marquee);
            if (exception != null)
                MessageBox.Show(exception.Message);
            else
            {
                var sbUnsupported = new StringBuilder();
                if (fromDataSource.UnsupportedTypes.Count > 0)
                {
                    sbUnsupported.AppendLine("Source does not support types:");
                    fromDataSource.UnsupportedTypes.ForEach(u => sbUnsupported.AppendLine(u));
                    sbUnsupported.AppendLine();
                }

                if (toDataSource.UnsupportedTypes.Count > 0)
                {
                    sbUnsupported.AppendLine("Target does not support types:");
                    toDataSource.UnsupportedTypes.ForEach(u => sbUnsupported.AppendLine(u));
                    sbUnsupported.AppendLine();
                }

                if (sbUnsupported.Length > 0)
                {
                    MessageBox.Show(sbUnsupported.ToString());
                }

                refreshPage(false);

                var settings = PaJaMa.Common.SettingsHelper.GetUserSettings<DatabaseStudioSettings>();
                settings.LastDataCompareSourceConnection = cboSource.SelectedItem as DatabaseStudioConnection;
                settings.LastDataCompareTargetConnection = cboTarget.SelectedItem as DatabaseStudioConnection;
                PaJaMa.Common.SettingsHelper.SaveUserSettings<DatabaseStudioSettings>(settings);

                _lockDbChange = true;
                cboSourceDatabase.Items.Clear();
                cboSourceDatabase.Items.AddRange(fromDataSource.Databases.ToArray());
                cboSourceDatabase.SelectedItem = fromDataSource.CurrentDatabase;

                cboTargetDatabase.Items.Clear();
                cboTargetDatabase.Items.AddRange(toDataSource.Databases.ToArray());
                cboTargetDatabase.SelectedItem = toDataSource.CurrentDatabase;
                _lockDbChange = false;

                btnConnect.Visible = false;
                btnDisconnect.Visible = true;
                cboSource.SelectionLength = 0;
                cboTarget.SelectionLength = 0;
                cboTarget.Enabled = cboSource.Enabled = false;
                cboSourceDatabase.Visible = cboTargetDatabase.Visible = true;
                btnSourceQuery.Enabled = btnTargetQuery.Enabled = true;
                btnGo.Enabled = btnRefresh.Enabled = btnSelectAll.Enabled = true;
            }
        }

        private void refreshPage(bool reinit)
        {
            _compareHelper.IgnoreCase = chkCaseInsensitive.Checked;
            if (reinit)
            {
                var worker = new BackgroundWorker();
                worker.DoWork += delegate (object sender2, DoWorkEventArgs e2)
                {
                    _compareHelper.Init(worker);
                };
                WinControls.WinProgressBox.ShowProgress(worker, progressBarStyle: ProgressBarStyle.Marquee);
            }

            refreshTables();
        }

        private void refreshTables()
        {
            var lst = TableWorkspaceList.GetTableWorkspaces(_compareHelper, true, false);

            TargetTable.Items.Clear();
            TargetTable.Items.Add(string.Empty);
            var toTbls = from s in _compareHelper.ToDataSource.CurrentDatabase.Schemas
                         from t in s.Tables
                         select t;
            TargetTable.Items.AddRange(toTbls.OrderBy(t => t.TableName).ToArray());


            gridTables.AutoGenerateColumns = false;

            if (gridTables.DataSource != null)
            {
                var currws = (gridTables.DataSource as BindingList<TableWorkspace>).ToList();
                foreach (var curr in currws)
                {
                    var newws = lst.Workspaces.FirstOrDefault(ws => ws.SourceObject.ToString() == curr.SourceObject.ToString());
                    if (newws != null)
                    {
                        if (curr.Select) newws.Select = curr.Select;
                        if (curr.SelectTableForData) newws.SelectTableForData = curr.SelectTableForData;
                        if (curr.Delete) newws.Delete = curr.Delete;
                        if (curr.KeepIdentity) newws.KeepIdentity = curr.KeepIdentity;
                        if (curr.RemoveAddIndexes) newws.RemoveAddIndexes = curr.RemoveAddIndexes;
                        if (curr.RemoveAddKeys) newws.RemoveAddKeys = curr.RemoveAddKeys;
                        if (curr.Truncate) newws.Truncate = curr.Truncate;
                    }
                }
            }

            gridTables.DataSource = new BindingList<TableWorkspace>(
                lst.Workspaces
                .Where(ws => ws.TargetTable != null)
                .OrderBy(w => w.SourceTable.ToString()).ToList());
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            var dataSpaces = (gridTables.DataSource as BindingList<TableWorkspace>)
                .Where(t => t.SelectTableForData).ToList();

            var truncDelete = (gridTables.DataSource as BindingList<TableWorkspace>)
                .Where(t => t.Truncate || t.Delete).ToList();

            var changes = dataSpaces.Select(t => (t.TargetObject == null ? t.SourceObject : t.TargetObject) + " - Data").Union(
                        truncDelete.Select(t => (t.TargetObject == null ? t.SourceObject : t.TargetObject) + " - Truncate/Delete")
                    ).ToList();

            if (ScrollableMessageBox.ShowDialog(string.Format("{0} - {1} data will be changed, continue?:\r\n \r\n{2}", _compareHelper.ToDataSource.DataSourceName,
                    _compareHelper.ToDataSource.CurrentDatabase.DatabaseName,
                string.Join("\r\n", changes.ToArray())), "Proceed", ScrollableMessageBoxButtons.Yes, ScrollableMessageBoxButtons.No) != Common.PromptResult.Yes)
                return;


            if (!dataSpaces.Any() && !truncDelete.Any())
            {
                MessageBox.Show("Nothing to synchronize.");
                return;
            }

            List<WorkspaceBase> workspaces = new List<WorkspaceBase>();

            bool success = false;
            var worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += delegate (object sender2, DoWorkEventArgs e2)
            {
                success = synchronize(worker, workspaces, dataSpaces, truncDelete);
            };

            PaJaMa.WinControls.WinProgressBox.ShowProgress(worker, allowCancel: true, progressBarStyle: ProgressBarStyle.Marquee);

            if (success)
            {
                MessageBox.Show("Done");
            }
        }

        private bool synchronize(BackgroundWorker worker, List<WorkspaceBase> workspaces, List<TableWorkspace> dataSpaces, List<TableWorkspace> truncDelete)
        {
            var sync = new SynchronizationHelper();
            sync.DisplayMessage += delegate (object sender, Common.PromptEventArgs e)
            {
                MessageBox.Show("Failed to synchronize: " + e.Message);
            };
            return sync.Synchronize(_compareHelper, workspaces, dataSpaces, truncDelete, worker);
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            gridTables.DataSource = null;
            btnDisconnect.Visible = false;
            btnConnect.Visible = true;
            cboSource.Enabled = cboTarget.Enabled = true;
            cboSourceDatabase.Visible = cboTargetDatabase.Visible = false;
            btnSourceQuery.Enabled = btnTargetQuery.Enabled = false;
            btnGo.Enabled = btnRefresh.Enabled = btnSelectAll.Enabled = false;
        }

        private void cboSourceDatabase_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_lockDbChange) return;
            _compareHelper.FromDataSource.ChangeDatabase(cboSourceDatabase.Text);
            refreshPage(true);
        }

        private void cboTargetDatabase_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_lockDbChange) return;
            _compareHelper.ToDataSource.ChangeDatabase(cboTargetDatabase.Text);
            refreshPage(true);
        }

        private void gridTables_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var grid = sender as DataGridView;
            if (e.RowIndex >= 0)
            {
                if (grid.Columns[e.ColumnIndex] == DataDetails)
                {
                    var workspace = grid.Rows[e.RowIndex].DataBoundItem as TableWorkspace;

                    if (workspace.TargetTable == null)
                    {
                        MessageBox.Show("No target specified.");
                        return;
                    }

                    var frm = new frmDataDetails();
                    frm.SelectedWorkspace = workspace;
                    frm.Show();
                }
                else if (grid.Columns[e.ColumnIndex] == WhereClause)
                {
                    var workspace = grid.Rows[e.RowIndex].DataBoundItem as TableWorkspace;

                    if (workspace.TargetTable == null)
                    {
                        MessageBox.Show("No target specified.");
                        return;
                    }

                    using (var dlg = new frmDataWhere())
                    {
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            workspace.WhereClause = dlg.txtQuery.Text;
                        }
                    }
                }
            }
        }

        private void _frmDifferences_Synchronized(object sender, EventArgs e)
        {
            refreshPage(true);
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            refreshPage(true);
        }

        private void btnSwitch_Click(object sender, EventArgs e)
        {
            bool connected = btnDisconnect.Visible;
            if (connected)
                btnDisconnect_Click(sender, e);

            var source = cboSource.SelectedItem as DatabaseStudioConnection;
            var target = cboTarget.SelectedItem as DatabaseStudioConnection;

            cboSource.SelectedItem = cboSource.Items.OfType<DatabaseStudioConnection>().First(x => x.ConnectionName == target.ConnectionName);
            cboTarget.SelectedItem = cboTarget.Items.OfType<DatabaseStudioConnection>().First(x => x.ConnectionName == source.ConnectionName);

            if (connected)
                btnConnect_Click(sender, e);
        }

        private void btnViewCreates_Click(object sender, EventArgs e)
        {
            //if (_activeWorkspace is WorkspaceWithSourceBase)
            //{
            //	frmCreates frm = new frmCreates();
            //	frm.Workspace = _activeWorkspace as WorkspaceWithSourceBase;
            //	frm.Show();
            //}
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            foreach (var ws in (gridTables.DataSource as BindingList<TableWorkspace>))
            {
                ws.SelectTableForData = true;
            }
            gridTables.Invalidate();
        }

        private void btnSourceQuery_Click(object sender, EventArgs e)
        {
            query(_compareHelper.FromDataSource.CurrentDatabase);
        }

        private void btnTargetQuery_Click(object sender, EventArgs e)
        {
            query(_compareHelper.ToDataSource.CurrentDatabase);
        }

        private void selectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            query(_compareHelper.FromDataSource.CurrentDatabase, 0);
        }

        private void selectTop1000ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            query(_compareHelper.FromDataSource.CurrentDatabase, 1000);
        }

        private void query(Library.DatabaseObjects.Database database, int? topN = null)
        {
            if (database != null)
            {
                var args = new QueryEventArgs();
                args.Database = database;
                if (topN != null)
                {
                    var selectedItem = gridTables.SelectedRows[0].DataBoundItem as TableWorkspace;
                    args.InitialTopN = topN;
                    args.InitialTable = selectedItem.SourceTable.TableName;
                    args.InitialSchema = selectedItem.SourceTable.Schema.SchemaName;
                }
                QueryDatabase(this, args);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (var dlg = new SaveFileDialog())
                {
                    dlg.Filter = "DatabaseStudio compare files (*.dbd)|*.dbd";
                    dlg.Title = "Workspaces";
                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        var ws = new CompareWorkspace();
                        ws.FromConnectionString = cboSource.Text;
                        ws.ToConnectionString = cboTarget.Text;
                        ws.FromDatabase = cboSourceDatabase.Text;
                        ws.ToDatabase = cboTargetDatabase.Text;

                        ws.SelectedTableWorkspaces = (gridTables.DataSource as BindingList<TableWorkspace>)
                            .Where(t => t.Select || t.SelectTableForData || t.Truncate || t.Delete)
                            .Select(tw => SerializableTableWorkspace.GetFromTableWorkspace(tw)).ToList();

                        PaJaMa.Common.XmlSerialize.SerializeObjectToFile<CompareWorkspace>(ws, dlg.FileName);
                        MessageBox.Show("Workspaces saved.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (var dlg = new OpenFileDialog())
                {
                    dlg.Filter = "DatabaseStudio compare files (*.dbd)|*.dbd";
                    dlg.Title = "Workspaces";
                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        var workspaces = PaJaMa.Common.XmlSerialize.DeserializeObjectFromFile<CompareWorkspace>(dlg.FileName);
                        if (btnDisconnect.Visible)
                            btnDisconnect_Click(sender, e);

                        cboSource.Text = workspaces.FromConnectionString;
                        cboTarget.Text = workspaces.ToConnectionString;
                        cboSourceDatabase.Text = workspaces.FromDatabase;
                        cboTargetDatabase.Text = workspaces.ToDatabase;
                        btnConnect_Click(sender, e);

                        var tws = gridTables.DataSource as BindingList<TableWorkspace>;
                        foreach (var stw in workspaces.SelectedTableWorkspaces)
                        {
                            var tw = tws.FirstOrDefault(w => w.SourceTable.ToString() == stw.SourceSchemaTableName);
                            if (tw != null)
                            {
                                if (!string.IsNullOrEmpty(stw.TargetSchemaTableName))
                                {
                                    tw.TargetObject = (from s in _compareHelper.ToDataSource.CurrentDatabase.Schemas
                                                       from t in s.Tables
                                                       where t.ToString() == stw.TargetSchemaTableName
                                                       select t).FirstOrDefault();
                                }
                                tw.SelectTableForData = stw.SelectTableForData;
                                tw.Select = stw.SelectTableForStructure;
                                tw.Delete = stw.Delete;
                                tw.Truncate = stw.Truncate;
                                tw.KeepIdentity = stw.KeepIdentity;
                                tw.RemoveAddKeys = stw.RemoveAddKeys;
                                tw.RemoveAddIndexes = stw.RemoveAddIndexes;
                                tw.TransferBatchSize = stw.TransferBatchSize;
                            }
                        }

                        TransferBatchSize.Visible = workspaces.SelectedTableWorkspaces.Any(tw => tw.SelectTableForData);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnDataDifferences_Click(object sender, EventArgs e)
        {
            var bw = new BackgroundWorker();
            var tws = (gridTables.DataSource as BindingList<TableWorkspace>).Where(tw => tw.SelectTableForData);
            var differences = new List<DataDifference>();
            var prog = new PaJaMa.WinControls.WinProgressBox();
            var dataHelper = new DataHelper();
            prog.Cancel += delegate (object sender2, EventArgs e2)
            {
                dataHelper.Cancel();
            };
            bw.DoWork += delegate (object sender2, DoWorkEventArgs e2)
            {
                foreach (var tw in tws)
                {
                    if (bw.CancellationPending)
                        return;
                    try
                    {
                        var diffs = dataHelper.GetDataDifferences(tw, bw);
                        if (diffs != null)
                            differences.Add(diffs);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            };

            prog.Show(bw, allowCancel: true);

            foreach (var diff in differences)
            {
                var row = gridTables.Rows.OfType<DataGridViewRow>().First(r => r.DataBoundItem.Equals(diff.TableWorkspace));
                row.Cells[DataDetails.Name].Value = string.Format("{0}/{1}/{2}", diff.Differences, diff.SourceOnly, diff.TargetOnly);
            }
        }

        private void mnuMain_Opening(object sender, CancelEventArgs e)
        {
            var allRowsDataSelected = gridTables.SelectedRows.OfType<DataGridViewRow>().Any()
                && gridTables.SelectedRows.OfType<DataGridViewRow>().All(dgv => (dgv.DataBoundItem as TableWorkspace).SelectTableForData);

            setBatchSizeToolStripMenuItem.Visible = allRowsDataSelected;
        }

        private void setBatchSizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var result = PaJaMa.WinControls.NumericInputBox.Show("Enter batch size", "Batch Size", TableWorkspace.DEFAULT_BATCH_SIZE);
            if (result.Result == DialogResult.OK)
            {
                foreach (DataGridViewRow row in gridTables.SelectedRows)
                {
                    var tw = row.DataBoundItem as TableWorkspace;
                    tw.TransferBatchSize = (int)result.Value;
                }
                gridTables.Refresh();
            }
        }

        private void ucCompare_Load(object sender, EventArgs e)
        {
            var settings = PaJaMa.Common.SettingsHelper.GetUserSettings<DatabaseStudioSettings>();

            refreshConnStrings();

            new GridHelper().DecorateGrid(gridTables);

            if (settings.LastDataCompareSourceConnection != null)
                cboSource.SelectedItem = cboSource.Items.OfType<DatabaseStudioConnection>().First(x => x.ConnectionName == settings.LastDataCompareSourceConnection.ConnectionName);

            if (settings.LastDataCompareTargetConnection != null)
                cboTarget.SelectedItem = cboTarget.Items.OfType<DatabaseStudioConnection>().First(x => x.ConnectionName == settings.LastDataCompareTargetConnection.ConnectionName);

            this.ParentForm.FormClosing += ParentForm_FormClosing;
            this.ParentForm.Load += ParentForm_Load;

            frmConnections.ConnectionsChanged += (object sender2, EventArgs e2) => refreshConnStrings();
        }

        private void ParentForm_Load(object sender, EventArgs e)
        {
            //var formSettings = Common.SettingsHelper.GetUserSettings<CompareFormSettings>();
            //if (formSettings.TablesSplitterDistance > 0) splitTables.SplitterDistance = formSettings.TablesSplitterDistance;
            //if (formSettings.TableDifferencesSplitterDistance > 0) diffTables.splitMain.SplitterDistance = formSettings.TableDifferencesSplitterDistance;
            //if (formSettings.ObjectsSplitterDistance > 0) splitObjects.SplitterDistance = formSettings.ObjectsSplitterDistance;
            //if (formSettings.ObjectDifferencesSplitterDistance > 0) diffObjects.splitMain.SplitterDistance = formSettings.ObjectDifferencesSplitterDistance;
            //if (formSettings.DropsSplitterDistance > 0) splitDrops.SplitterDistance = formSettings.DropsSplitterDistance;
            //if (formSettings.DropDifferencesSplitterDistance > 0) diffDrops.splitMain.SplitterDistance = formSettings.DropDifferencesSplitterDistance;
        }

        private void ParentForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //var formSettings = Common.SettingsHelper.GetUserSettings<CompareFormSettings>();
            //formSettings.TablesSplitterDistance = splitTables.SplitterDistance;
            //formSettings.TableDifferencesSplitterDistance = diffTables.splitMain.SplitterDistance;
            //formSettings.ObjectsSplitterDistance = splitObjects.SplitterDistance;
            //formSettings.ObjectDifferencesSplitterDistance = diffObjects.splitMain.SplitterDistance;
            //formSettings.DropsSplitterDistance = splitDrops.SplitterDistance;
            //formSettings.DropDifferencesSplitterDistance = diffDrops.splitMain.SplitterDistance;
            //Common.SettingsHelper.SaveUserSettings<CompareFormSettings>(formSettings);
        }

        private void cboDriver_Format(object sender, ListControlConvertEventArgs e)
        {
            Type type = e.ListItem as Type;
            e.Value = type.Name;
        }
    }
}
