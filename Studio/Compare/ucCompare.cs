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

namespace PaJaMa.Database.Studio.Compare
{
    public partial class ucCompare : UserControl
    {
        private CompareHelper _compareHelper;

        private List<TabPage> _differencedTabs = new List<TabPage>();
        private List<TableWorkspace> _differentWorkspaces = new List<TableWorkspace>();

        private bool _lockDbChange = false;
        private WorkspaceBase _activeWorkspace;

        public event QueryEventHandler QueryDatabase;

        public ucCompare()
        {
            InitializeComponent();
            MenuHelper.CreateFileMenu(this);
        }

        private void refreshConnStrings()
        {
            var settings = DatabaseStudioSettings.LoadSettings();
            cboSource.Items.Clear();
            cboTarget.Items.Clear();
            var connections = DatabaseConnection.GetConnections();
            cboSource.Items.AddRange(connections.OrderBy(c => c.ConnectionName).ToArray());
            cboTarget.Items.AddRange(connections.OrderBy(c => c.ConnectionName).ToArray());
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            var fromConnection = cboSource.SelectedIndex < 0
                ? cboSource.Items.OfType<DatabaseConnection>().First(x => x.ConnectionName == cboTarget.Text)
                : cboSource.SelectedItem as DatabaseConnection;
            var toConnection = cboTarget.SelectedIndex < 0
                ? cboTarget.Items.OfType<DatabaseConnection>().First(x => x.ConnectionName == cboTarget.Text)
                : cboTarget.SelectedItem as DatabaseConnection;

            Exception exception = null;

            DataSource fromDataSource = null;
            DataSource toDataSource = null;

            var worker = new BackgroundWorker();
            worker.DoWork += delegate (object sender2, DoWorkEventArgs e2)
                {
                    _differencedTabs = new List<TabPage>();
                    try
                    {
                        fromDataSource = Activator.CreateInstance(typeof(DataSource).Assembly.GetType(fromConnection.DataSourceType), new object[] { fromConnection }) as DataSource;
                        fromDataSource.NamedConstraints = chkNamedConstraints.Checked;
                    }
                    catch (Exception ex)
                    {
                        exception = new Exception("Error opening source connection: " +
                            (ex is TargetInvocationException && ex.InnerException != null ? ex.InnerException.Message : ex.Message));
                        return;
                    }

                    try
                    {
                        toDataSource = Activator.CreateInstance(typeof(DataSource).Assembly.GetType(toConnection.DataSourceType), new object[] { toConnection }) as DataSource;
                        toDataSource.NamedConstraints = chkNamedConstraints.Checked;
                    }
                    catch (Exception ex)
                    {
                        exception = new Exception("Error opening target connection: " +
                            (ex is TargetInvocationException && ex.InnerException != null ? ex.InnerException.Message : ex.Message));
                        return;
                    }

                    try
                    {
                        _compareHelper = new CompareHelper(fromDataSource, toDataSource, chkDataOnly.Checked, worker);
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
                            //switch (dlgResult)
                            //{
                            //	case WinControls.YesNoMessageDialogResult.No:
                            //		e3.Result = Common.YesYesToAllNo.No;
                            //		break;
                            //	case WinControls.YesNoMessageDialogResult.Yes:
                            //		e3.Result = Common.YesYesToAllNo.Yes;
                            //		break;
                            //	case WinControls.YesNoMessageDialogResult.YesToAll:
                            //		e3.Result = Common.YesYesToAllNo.YesToAll;
                            //		break;
                            //}
                        };
                    }
                };

            if (WinControls.WinProgressBox.ShowProgress(worker, allowCancel: true, progressBarStyle: ProgressBarStyle.Marquee) != DialogResult.OK) return;

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
                settings.LastCompareSourceConnection = (cboSource.SelectedItem as DatabaseConnection).ConnectionName;
                settings.LastCompareTargetConnection = (cboTarget.SelectedItem as DatabaseConnection).ConnectionName;
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
                cboTarget.Enabled = cboSource.Enabled = chkNamedConstraints.Enabled = false;
                cboSourceDatabase.Enabled = cboTargetDatabase.Enabled = true;
                btnSourceQuery.Enabled = btnTargetQuery.Enabled = true;
                btnGo.Enabled = btnRefresh.Enabled = btnViewMissingDependencies.Enabled = btnSelectAll.Enabled = true;
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

            if (chkDataOnly.Checked)
            {
                refreshTables();
            }
            else
            {
                var dropWorkspaces = new List<DropWorkspace>();
                refreshTables(dropWorkspaces);
                refreshObjects(dropWorkspaces);

                gridDropObjects.AutoGenerateColumns = false;
                gridDropObjects.DataSource = new BindingList<DropWorkspace>(dropWorkspaces.OrderBy(w => w.TargetObject.ToString()).ToList());

                _differencedTabs = new List<TabPage>();
                identifyDifferences();
            }
            setTabText();
        }

        private void refreshTables(List<DropWorkspace> dropWorkspaces = null)
        {
            var lst = TableWorkspaceList.GetTableWorkspaces(_compareHelper, chkDataOnly.Checked, chkCondensed.Checked);

            if (dropWorkspaces != null)
            {
                foreach (var dw in lst.DropWorkspaces)
                {
                    dropWorkspaces.Add(dw);
                }
            }

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

            _differentWorkspaces = lst.Workspaces.OrderBy(w => w.SourceTable.ToString()).ToList();
            populateTables();
        }

        private void populateTables()
        {
            if (_differentWorkspaces == null) return;
            var tables = _differentWorkspaces.ToList();
            if (chkDifferencesOnly.Checked)
            {
                tables = tables.Where(t => t.TargetDatabase == null || t.SynchronizationItems.Any(d => d.Scripts.Any(s => s.Value.Length > 0))).ToList();
            }
            gridTables.DataSource = new BindingList<TableWorkspace>(tables);
        }

        private void refreshObjects(List<DropWorkspace> dropWorkspaces)
        {
            var lst = ObjectWorkspaceList.GetObjectWorkspaces(_compareHelper);
            gridObjects.AutoGenerateColumns = false;
            gridObjects.DataSource = new BindingList<ObjectWorkspace>(lst.Workspaces.OrderBy(w => w.SourceObject.ToString()).ToList());
            foreach (var dw in lst.DropWorkspaces)
            {
                dropWorkspaces.Add(dw);
            }
        }

        private StringBuilder getMissingDependencyString()
        {
            var sb = new StringBuilder();
            var workspaces = gridTables.Rows.OfType<DataGridViewRow>().Select(r => (r.DataBoundItem as TableWorkspace))
                .Where(tw => tw.Select)
                .OfType<WorkspaceBase>()
                .ToList();

            workspaces.AddRange(gridObjects.Rows.OfType<DataGridViewRow>().Select(r => (r.DataBoundItem as ObjectWorkspace))
                .Where(ow => ow.Select));

            workspaces.AddRange(gridDropObjects.Rows.OfType<DataGridViewRow>().Select(r => (r.DataBoundItem as DropWorkspace))
                .Where(ow => ow.Select));

            var missingDependencies = _compareHelper.GetMissingDependencies(workspaces);
            if (missingDependencies.Any())
            {
                foreach (var kvp in missingDependencies)
                {
                    sb.AppendLine(kvp.Key.Description + " is dependent on: " + string.Join(", ", kvp.Value.Select(v => v.Description).ToArray()));
                }
            }

            return sb;
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            var sb = getMissingDependencyString();

            if (sb.Length > 0)
            {
                ScrollableMessageBox.Show(sb.ToString(), "Dependencies");
                return;
            }


            var dropSpaces = gridDropObjects.DataSource == null ? new List<DropWorkspace>() : (gridDropObjects.DataSource as BindingList<DropWorkspace>)
                .Where(t => t.Select).ToList();

            var structureSpaces = (gridTables.DataSource as BindingList<TableWorkspace>)
                .Where(t => t.Select).ToList();

            var dataSpaces = (gridTables.DataSource as BindingList<TableWorkspace>)
                .Where(t => t.SelectTableForData).ToList();

            var truncDelete = (gridTables.DataSource as BindingList<TableWorkspace>)
                .Where(t => t.Truncate || t.Delete).ToList();

            var objSpaces = gridObjects.DataSource == null ? new List<ObjectWorkspace>() : (gridObjects.DataSource as BindingList<ObjectWorkspace>)
                .Where(p => p.Select).ToList();

            var changes = dropSpaces.Select(t => t.TargetObject.Description + " - Drop").Union(
                        structureSpaces.Where(t => t.TargetObject != null).Select(t => t.TargetObject + " - Alter")
                        ).Union(
                        structureSpaces.Where(t => t.TargetObject == null).Select(t => t.SourceObject + " - Create")
                        ).Union(
                        dataSpaces.Select(t => (t.TargetObject == null ? t.SourceObject : t.TargetObject) + " - Data")
                        ).Union(
                        truncDelete.Select(t => (t.TargetObject == null ? t.SourceObject : t.TargetObject) + " - Truncate/Delete")
                        ).Union(
                        objSpaces.Select(t => t.SourceObject + " - " + t.SourceObject.ObjectType)
                    ).ToList();

            if (ScrollableMessageBox.ShowDialog(string.Format("{0} - {1} will be changed, continue?:\r\n \r\n{2}", _compareHelper.ToDataSource.DataSourceName,
                    _compareHelper.ToDataSource.CurrentDatabase.DatabaseName,
                string.Join("\r\n", changes.ToArray())), "Proceed", ScrollableMessageBoxButtons.Yes, ScrollableMessageBoxButtons.No) != Common.PromptResult.Yes)
                return;


            if (!dropSpaces.Any() && !structureSpaces.Any() && !dataSpaces.Any() && !objSpaces.Any() && !truncDelete.Any())
            {
                MessageBox.Show("Nothing to synchronize.");
                return;
            }

            List<WorkspaceBase> workspaces = new List<WorkspaceBase>();
            if (structureSpaces.Any())
                workspaces.AddRange(structureSpaces);

            if (objSpaces.Any())
                workspaces.AddRange(objSpaces);

            if (dropSpaces.Any())
                workspaces.AddRange(dropSpaces);

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
                refreshPage(true);
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
            gridDropObjects.DataSource = null;
            gridObjects.DataSource = null;
            btnDisconnect.Visible = false;
            btnConnect.Visible = true;
            cboSource.Enabled = cboTarget.Enabled = chkNamedConstraints.Enabled = true;
            cboSourceDatabase.Enabled = cboTargetDatabase.Enabled = false;
            btnSourceQuery.Enabled = btnTargetQuery.Enabled = false;
            btnGo.Enabled = btnViewMissingDependencies.Enabled = btnRefresh.Enabled = btnSelectAll.Enabled = false;
            tabTables.Text = "Tables";
            tabObjects.Text = "Objects";
            tabDrop.Text = "Drop";
        }

        private void grid_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if ((sender as DataGridView).CurrentCell != null && (sender as DataGridView).CurrentCell.OwningColumn == TransferBatchSize)
                return;

            setTabText();
        }

        private void gridTables_SelectionChanged(object sender, EventArgs e)
        {
            refreshDifferencesControl();
        }

        private void cboSourceDatabase_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_lockDbChange) return;
            if (MessageBox.Show("Refresh now?", "Refresh", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            _compareHelper.FromDataSource.ChangeDatabase(cboSourceDatabase.Text);
            refreshPage(true);
        }

        private void cboTargetDatabase_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_lockDbChange) return;
            if (MessageBox.Show("Refresh now?", "Refresh", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            _compareHelper.ToDataSource.ChangeDatabase(cboTargetDatabase.Text);
            refreshPage(true);
        }

        private void gridTables_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (gridTables.Columns[e.ColumnIndex] == TransferBatchSize)
                return;

            if (gridTables.Columns[e.ColumnIndex] == TargetTable)
                gridTables_SelectionChanged(sender, e);

            TransferBatchSize.Visible = gridTables.DataSource != null && (gridTables.DataSource as BindingList<TableWorkspace>).Any(tw => tw.SelectTableForData);
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
            }
        }

        private void identifyDifferences()
        {
            if (_differencedTabs.Contains(tabMain.SelectedTab)) return;
            _differencedTabs.Add(tabMain.SelectedTab);

            List<DataGridViewRow> rows = new List<DataGridViewRow>();
            if (tabMain.SelectedTab == tabTables)
                rows = gridTables.Rows.OfType<DataGridViewRow>().ToList();
            else if (tabMain.SelectedTab == tabObjects)
                rows = gridObjects.Rows.OfType<DataGridViewRow>().ToList();

            foreach (DataGridViewRow row in rows)
            {
                var ws = row.DataBoundItem as WorkspaceBase;

                row.DefaultCellStyle.SelectionForeColor = row.DefaultCellStyle.ForeColor = Color.Empty;
                row.DefaultCellStyle.SelectionBackColor = row.DefaultCellStyle.BackColor = Color.Empty;
                if (ws.TargetObject == null)
                {
                    row.DefaultCellStyle.SelectionBackColor = row.DefaultCellStyle.BackColor = Color.LightSkyBlue;
                    row.DefaultCellStyle.SelectionForeColor = row.DefaultCellStyle.ForeColor = Color.Black;
                }
                else
                {
                    var differences = ws.SynchronizationItems;
                    if (differences.Any(d => d.Scripts.Any(s => s.Value.Length > 0)))
                    {
                        row.DefaultCellStyle.SelectionBackColor = row.DefaultCellStyle.BackColor = Color.LightSalmon;
                        row.DefaultCellStyle.SelectionForeColor = row.DefaultCellStyle.ForeColor = Color.Black;
                    }
                }

                if (tabMain.SelectedTab == tabTables)
                {
                    row.Cells[DataDetails.Name].Value = "Data";
                }
            }
        }

        private void setTabText()
        {
            tabTables.Text = "Tables";
            tabObjects.Text = "Objects";
            tabDrop.Text = "Drop";

            if (gridDropObjects.Rows.Count > 0)
            {
                var selected = (gridDropObjects.DataSource as BindingList<DropWorkspace>).Count(d => d.Select);
                tabDrop.Text += " (" + gridDropObjects.Rows.Count.ToString() + (selected > 0 ? ", " + selected.ToString() + " selected" : "") + ")";
            }

            var dict = new Dictionary<DataGridView, TabPage>();
            dict.Add(gridTables, tabTables);
            dict.Add(gridObjects, tabObjects);

            foreach (var kvp in dict)
            {
                int diff = 0;
                int nw = 0;
                int selected = 0;
                foreach (DataGridViewRow row in kvp.Key.Rows.OfType<DataGridViewRow>())
                {
                    var ws = row.DataBoundItem as WorkspaceBase;

                    if (ws.Select)
                        selected++;

                    if (ws.TargetObject == null)
                        nw++;
                    else
                    {
                        var differences = ws.SynchronizationItems;
                        if (differences.Any(d => d.Scripts.Any(s => s.Value.Length > 0)))
                            diff++;
                    }
                }

                string inner = string.Empty;
                if (nw > 0)
                    inner += ", " + nw.ToString() + " new";

                if (diff > 0)
                    inner += ", " + diff.ToString() + " diff";

                if (selected > 0)
                    inner += ", " + selected.ToString() + " selected";

                if (!string.IsNullOrEmpty(inner))
                {
                    kvp.Value.Text += " (" + inner.Substring(2) + ")";
                }
            }
        }

        private void gridObjects_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            //var grid = sender as DataGridView;
            //if (grid.Columns[e.ColumnIndex] == colProgDetails && e.RowIndex >= 0)
            //{
            //	//using (var frm = new frmObjectStructureDetails())
            //	//{
            //	//	var workspace = grid.Rows[e.RowIndex].DataBoundItem as ObjectWorkspace;
            //	//	frm.Workspace = workspace;
            //	//	frm.ShowDialog();
            //	//}
            //}
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

        private void tabMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabMain.SelectedTab == tabDrop)
            {
                refreshDifferencesControl();
                btnViewCreates.Enabled = false;
            }
            else
            {
                refreshDifferencesControl();
                identifyDifferences();
            }
        }

        private void _frmDifferences_Synchronized(object sender, EventArgs e)
        {
            refreshPage(true);
        }

        private void refreshDifferencesControl()
        {
            btnViewCreates.Enabled = false;

            DataGridView grid = null;
            ucDifferences activeDifferences = null;
            if (tabMain.SelectedTab == tabTables)
            {
                grid = gridTables;
                activeDifferences = diffTables;
            }
            else if (tabMain.SelectedTab == tabObjects)
            {
                grid = gridObjects;
                activeDifferences = diffObjects;
            }
            else
            {
                grid = gridDropObjects;
                activeDifferences = diffDrops;
            }

            if (grid == null)
                return;

            activeDifferences.CompareHelper = _compareHelper;

            if (grid.SelectedRows.Count != 1)
                activeDifferences.Workspace = null;
            else
            {
                activeDifferences.Workspace = grid.SelectedRows[0].DataBoundItem as WorkspaceBase;
                _activeWorkspace = activeDifferences.Workspace;
                btnViewCreates.Enabled = _activeWorkspace is WorkspaceWithSourceBase && _activeWorkspace.TargetObject != null && _activeWorkspace.SynchronizationItems.Any();

            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            refreshPage(true);
        }

        private void gridObjects_SelectionChanged(object sender, EventArgs e)
        {
            refreshDifferencesControl();
        }

        private void btnViewMissingDependencies_Click(object sender, EventArgs e)
        {
            var sb = getMissingDependencyString();

            if (sb.Length > 0)
                MessageBox.Show(sb.ToString());
            else
                MessageBox.Show("No missing dependencies!");
        }

        private void btnSwitch_Click(object sender, EventArgs e)
        {
            bool connected = btnDisconnect.Visible;
            if (connected)
                btnDisconnect_Click(sender, e);

            var source = cboSource.SelectedItem as DatabaseConnection;
            var target = cboTarget.SelectedItem as DatabaseConnection;

            if (target != null)
                cboSource.SelectedItem = cboSource.Items.OfType<DatabaseConnection>().First(x => x.ConnectionName == target.ConnectionName);
            if (source != null)
                cboTarget.SelectedItem = cboTarget.Items.OfType<DatabaseConnection>().First(x => x.ConnectionName == source.ConnectionName);

            if (connected)
                btnConnect_Click(sender, e);
        }

        private void gridDropObjects_SelectionChanged(object sender, EventArgs e)
        {
            refreshDifferencesControl();
        }

        private void btnViewCreates_Click(object sender, EventArgs e)
        {
            if (_activeWorkspace is WorkspaceWithSourceBase)
            {
                frmCreates frm = new frmCreates();
                frm.Workspace = _activeWorkspace as WorkspaceWithSourceBase;
                frm.Show();
            }
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            foreach (var ws in (gridDropObjects.DataSource as BindingList<DropWorkspace>))
            {
                ws.Select = true;
            }
            gridDropObjects.Invalidate();

            foreach (var ws in (gridObjects.DataSource as BindingList<ObjectWorkspace>))
            {
                ws.Select = true;
            }
            gridObjects.Invalidate();

            foreach (var ws in (gridTables.DataSource as BindingList<TableWorkspace>))
            {
                ws.Select = true;
            }
            gridTables.Invalidate();

            setTabText();
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
                    dlg.Filter = "DatabaseStudio compare files (*.dbc)|*.dbc";
                    dlg.Title = "Workspaces";
                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        var ws = new CompareWorkspace();
                        ws.FromConnectionString = cboSource.Text;
                        ws.ToConnectionString = cboTarget.Text;
                        ws.FromDatabase = cboSourceDatabase.Text;
                        ws.ToDatabase = cboTargetDatabase.Text;

                        ws.SelectedDropWorkspaces = (gridDropObjects.DataSource as BindingList<DropWorkspace>)
                            .Where(t => t.Select)
                            .Select(dw => SerializableDropWorkspace.GetFromDropWorkspace(dw))
                            .ToList();
                        ws.SelectedTableWorkspaces = (gridTables.DataSource as BindingList<TableWorkspace>)
                            .Where(t => t.Select || t.SelectTableForData || t.Truncate || t.Delete)
                            .Select(tw => SerializableTableWorkspace.GetFromTableWorkspace(tw)).ToList();
                        ws.SelectedObjectWorkspaces = gridObjects.DataSource == null ? new List<SerializableObjectWorkspace>() : (gridObjects.DataSource as BindingList<ObjectWorkspace>)
                            .Where(p => p.Select)
                            .Select(o => SerializableObjectWorkspace.GetFromObjectWorkspace(o))
                            .ToList();

                        PaJaMa.Common.JsonSerialize.SerializeObjectToFile<CompareWorkspace>(ws, dlg.FileName);
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
                    dlg.Filter = "DatabaseStudio compare files (*.dbc)|*.dbc";
                    dlg.Title = "Workspaces";
                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        var workspaces = PaJaMa.Common.JsonSerialize.DeserializeObjectFromFile<CompareWorkspace>(dlg.FileName);
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

                        var ows = gridObjects.DataSource as BindingList<ObjectWorkspace>;
                        foreach (var sow in workspaces.SelectedObjectWorkspaces)
                        {
                            var ow = ows.FirstOrDefault(w => w.SourceObject.ToString() == sow.SourceObjectName && w.SourceObject.ObjectType == sow.ObjectType);
                            if (ow != null)
                            {
                                //if (!string.IsNullOrEmpty(sow.TargetObjectName))
                                //{
                                //	ow.TargetObject = _compareHelper.ToDatabase.Obj
                                //}
                                ow.Select = true;
                            }
                        }

                        var dws = gridDropObjects.DataSource as BindingList<DropWorkspace>;
                        foreach (var sdw in workspaces.SelectedDropWorkspaces)
                        {
                            var dw = dws.FirstOrDefault(w => w.TargetObject.ToString() == sdw.Name && w.TargetObject.ObjectType == sdw.Type);
                            if (dw != null)
                                dw.Select = true;
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

        private void gridTables_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (gridTables.CurrentCell.OwningColumn != TransferBatchSize) return;

            var tws = gridTables.Rows[gridTables.CurrentCell.RowIndex].DataBoundItem as TableWorkspace;
            e.Control.Visible = tws.SelectTableForData;
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
            if (DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;

            refreshConnStrings();
            var settings = PaJaMa.Common.SettingsHelper.GetUserSettings<DatabaseStudioSettings>();

            new GridHelper().DecorateGrid(gridTables);
            new GridHelper().DecorateGrid(gridObjects);
            new GridHelper().DecorateGrid(gridDropObjects);

            if (settings.LastCompareSourceConnection != null)
                cboSource.SelectedItem = cboSource.Items.OfType<DatabaseConnection>().FirstOrDefault(x => x.ConnectionName == settings.LastCompareSourceConnection);

            if (settings.LastCompareTargetConnection != null)
                cboTarget.SelectedItem = cboTarget.Items.OfType<DatabaseConnection>().FirstOrDefault(x => x.ConnectionName == settings.LastCompareTargetConnection);

            this.ParentForm.FormClosing += ParentForm_FormClosing;
            this.ParentForm.Load += ParentForm_Load;

            frmConnections.ConnectionsChanged += (object sender2, EventArgs e2) => refreshConnStrings();
        }

        private void ParentForm_Load(object sender, EventArgs e)
        {
            var formSettings = Common.SettingsHelper.GetUserSettings<CompareFormSettings>();
            if (formSettings.TablesSplitterDistance > 0) splitTables.SplitterDistance = formSettings.TablesSplitterDistance;
            if (formSettings.TableDifferencesSplitterDistance > 0) diffTables.splitMain.SplitterDistance = formSettings.TableDifferencesSplitterDistance;
            if (formSettings.ObjectsSplitterDistance > 0) splitObjects.SplitterDistance = formSettings.ObjectsSplitterDistance;
            if (formSettings.ObjectDifferencesSplitterDistance > 0) diffObjects.splitMain.SplitterDistance = formSettings.ObjectDifferencesSplitterDistance;
            if (formSettings.DropsSplitterDistance > 0) splitDrops.SplitterDistance = formSettings.DropsSplitterDistance;
            if (formSettings.DropDifferencesSplitterDistance > 0) diffDrops.splitMain.SplitterDistance = formSettings.DropDifferencesSplitterDistance;
        }

        private void ParentForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            var formSettings = Common.SettingsHelper.GetUserSettings<CompareFormSettings>();
            formSettings.TablesSplitterDistance = splitTables.SplitterDistance;
            formSettings.TableDifferencesSplitterDistance = diffTables.splitMain.SplitterDistance;
            formSettings.ObjectsSplitterDistance = splitObjects.SplitterDistance;
            formSettings.ObjectDifferencesSplitterDistance = diffObjects.splitMain.SplitterDistance;
            formSettings.DropsSplitterDistance = splitDrops.SplitterDistance;
            formSettings.DropDifferencesSplitterDistance = diffDrops.splitMain.SplitterDistance;
            Common.SettingsHelper.SaveUserSettings<CompareFormSettings>(formSettings);
            if (_compareHelper != null) _compareHelper.Dispose();
        }

        private void cboDriver_Format(object sender, ListControlConvertEventArgs e)
        {
            Type type = e.ListItem as Type;
            e.Value = type.Name;
        }

        private void ChkDifferencesOnly_CheckedChanged(object sender, EventArgs e)
        {
            this.populateTables();
            _differencedTabs.Remove(tabMain.SelectedTab);
            this.identifyDifferences();
        }
    }
}
