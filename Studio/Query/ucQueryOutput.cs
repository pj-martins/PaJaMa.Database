using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading;
using PaJaMa.Database.Library.DatabaseObjects;
using PaJaMa.Database.Library.Helpers;
using PaJaMa.Database.Library.DataSources;

namespace PaJaMa.Database.Studio.Query
{
	public partial class ucQueryOutput : UserControl
	{
		private List<SplitContainer> _splitContainers = new List<SplitContainer>();
		private bool _lock = false;
		private bool _stopRequested = false;
		private DateTime _start;
		public DbConnection _currentConnection;
		private DbCommand _currentCommand;
		private DataSource _server;
		private string _query;
		private bool _somethingPrinted = false;

		public ucQueryOutput()
		{
			InitializeComponent();
		}

		private void btnGo_Click(object sender, EventArgs e)
		{
			string query = txtQuery.Text;
			if (txtQuery.SelectionLength > 0)
				query = txtQuery.SelectedText;

			// rich text replaces newlines
			query = query.Replace("\n", "\r\n");

			lblResults.Visible = false;
			_stopRequested = false;
			_currentCommand = _currentConnection.CreateCommand();
			_query = query;
			btnGo.Visible = false;
			btnStop.Visible = true;
			btnStop.Enabled = true;
			cboDatabases.Enabled = false;
			_start = DateTime.Now;
			lblTime.Text = string.Empty;
			lblTime.Visible = true;
			timDuration.Enabled = true;
			progMain.Visible = true;
			txtQuery.ReadOnly = true;

			var currControls = tabResults.Controls.OfType<Control>();
			foreach (var ctrl in currControls)
			{
				tabResults.Controls.Remove(ctrl);
				ctrl.Dispose();
			}
			txtMessages.Text = string.Empty;
			tabControl1.SelectedTab = tabResults;
			currControls = null;
			_splitContainers = new List<SplitContainer>();

			this.Parent.Text += " (Executing)";

			if (_currentConnection.GetType().Equals(typeof(System.Data.OleDb.OleDbConnection)))
				execute();
			else
				new Thread(new ThreadStart(execute)).Start();
		}

		public bool Connect(DbConnection connection, DataSource server, string initialDatabase, bool useDummyDA)
		{
			try
			{
                _server = server;
				if (useDummyDA)
					_currentConnection = connection;
				else
				{
					_currentConnection = server.OpenConnection();
					// TODO: generic
					if (_currentConnection is SqlConnection)
						(_currentConnection as SqlConnection).InfoMessage += ucQueryOutput_InfoMessage;
				}

				pnlButtons.Enabled = splitQuery.Enabled = true;


				lblDatabase.Visible = cboDatabases.Visible = server.Databases.Count > 1;


				cboDatabases.Items.Clear();
				cboDatabases.Items.AddRange(server.Databases.ToArray());

				if (!string.IsNullOrEmpty(initialDatabase) && initialDatabase != _currentConnection.Database)
					_currentConnection.ChangeDatabase(initialDatabase);

				_lock = true;
				cboDatabases.Text = _currentConnection.Database;
				_lock = false;
				return true;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
				return false;
			}
		}

		private void ucQueryOutput_InfoMessage(object sender, SqlInfoMessageEventArgs e)
		{
			_somethingPrinted = true;
			this.Invoke(new Action(() =>
				{
					txtMessages.Text += e.Message + "\r\n\r\n";
				}));
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
		}

		private void setDatabaseText()
		{
			if (cboDatabases.Items.Count > 0 && cboDatabases.Visible)
			{
				_lock = true;
				cboDatabases.Text = _currentConnection.Database;
				_lock = false;
			}
		}


		private void execute()
		{
			int totalResults = 0;
			int recordsAffected = 0;
			var parts = _query.Split(new string[] { "\r\ngo\r\n", "\r\nGO\r\n", "\r\nGo\r\n", "\r\ngO\r\n",
					"\ngo\n", "\nGO\n", "\nGo\n", "\ngO\n"
				}, StringSplitOptions.RemoveEmptyEntries);

			var sbErrors = new StringBuilder();
			foreach (var part in parts)
			{
				try
				{
					_currentCommand.CommandText = part;
					_currentCommand.CommandTimeout = 600000;
					using (var dr = _currentCommand.ExecuteReader())
					{
						this.Invoke(new Action(() =>
						{
							//if (!_stopRequested && !dr.HasRows)
							//{
							//	lblResults.Text = "Complete.";
							//	lblResults.Visible = true;
							//	setDatabaseText();

							//	return;
							//}
							bool hasNext = true;
							while (!_stopRequested)
							{
								DataTable dt = new DataTable();
								var schema = dr.GetSchemaTable();
								if (schema == null || !hasNext)
								{
									if (dr.RecordsAffected > 0)
										recordsAffected += dr.RecordsAffected;
									break;
								}

								var grid = new DataGridView();
								foreach (var row in schema.Rows.OfType<DataRow>())
								{
									// int existingCount = dt.Columns.OfType<DataColumn>().Count(c => c.ColumnName == row["ColumnName"].ToString());
									var colType = Type.GetType(row["DataType"].ToString());
									if (colType == null || colType.Equals(typeof(byte[])))
										colType = typeof(string);
									string colName = row["ColumnName"].ToString();
									int curr = 1;
									while (dt.Columns.OfType<DataColumn>().Any(c => c.ColumnName == colName))
									{
										colName = row["ColumnName"].ToString() + curr.ToString();
										curr++;
									}
									dt.Columns.Add(colName, colType);
									grid.Columns.Add(colName, row["ColumnName"].ToString());
								}

								var lastSplit = _splitContainers.LastOrDefault();
								if (lastSplit != null)
									lastSplit.Panel2Collapsed = false;
								var splitContainer = new SplitContainer();
								splitContainer.Dock = DockStyle.Fill;
								splitContainer.Orientation = Orientation.Horizontal;
								splitContainer.Panel2Collapsed = true;
								splitContainer.Panel2MinSize = 0;
								grid.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
								grid.Dock = DockStyle.Fill;
								grid.AllowUserToAddRows = grid.AllowUserToDeleteRows = false;
								grid.ReadOnly = true;
								grid.VirtualMode = true;
								grid.RowCount = 0;
								grid.CellValueNeeded += grid_CellValueNeeded;
								grid.CellFormatting += grid_CellFormatting;
								splitContainer.Panel1.Controls.Add(grid);
								if (lastSplit != null)
									lastSplit.Panel2.Controls.Add(splitContainer);
								else
									tabResults.Controls.Add(splitContainer);
								_splitContainers.Add(splitContainer);
								foreach (var split in _splitContainers)
								{
									split.SplitterDistance = splitQuery.Panel2.Height / _splitContainers.Count;
								}
								//grid.DataSource = dt;
								grid.AutoGenerateColumns = true;
								grid.Tag = new List<DataTable>() { dt };

								foreach (DataGridViewColumn col in grid.Columns)
								{
									if (!string.IsNullOrEmpty(col.Name))
									{
										var dtCol = dt.Columns[col.Name];
										col.ToolTipText = dtCol.DataType.Name;
									}
								}

								int i = 0;
								var lastRefresh = DateTime.Now;
								dt.BeginLoadData();
								while (dr.Read())
								{
									i++;
									if (_stopRequested) break;
									var row = dt.NewRow();
									var cols = dt.Columns.OfType<DataColumn>();
									for (int j = 0; j < cols.Count(); j++)
									{
										try
										{
											row[j] = dr[j] is byte[] ? Convert.ToBase64String(dr[j] as byte[]) : dr[j];
										}
										catch (Exception ex)
										{
											row[j] = "ERR! " + ex.Message;
										}
									}

									dt.Rows.Add(row);

									// if (i % 1000 == 0)
									if ((DateTime.Now - lastRefresh).TotalSeconds > 3)
									{
										lastRefresh = DateTime.Now;
										dt.EndLoadData();
										grid.RowCount += dt.Rows.Count;
										dt = dt.Clone();
										(grid.Tag as List<DataTable>).Add(dt);
										dt.BeginLoadData();
										//dt.EndLoadData();
										//dt.BeginLoadData();
										//Application.DoEvents();

										Application.DoEvents();
									}

									if (i % 100 == 0)
										Application.DoEvents();
								}
								dt.EndLoadData();

								var sum = (grid.Tag as List<DataTable>).Sum(t => t.Rows.Count);
								grid.RowCount = sum;
								totalResults += sum;

								//grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

								if (!_stopRequested)
									hasNext = dr.NextResult();
							}
						}));
					}
				}
				catch (Exception ex)
				{
					sbErrors.AppendLine(ex.Message);
				}
			}
			_currentCommand.Dispose();
			_currentCommand = null;
			this.Invoke(new Action(() =>
			{
				lblResults.Text = totalResults == 0 ? (recordsAffected < 0 ? "Complete." : recordsAffected.ToString() + " record(s) affected") : (totalResults.ToString() + " Records.");
				lblResults.Visible = true;
				if (totalResults == 0 || sbErrors.Length > 0)
				{
					txtMessages.Text += sbErrors.Length > 0 ? sbErrors.ToString() : lblResults.Text;
					tabControl1.SelectedTab = tabMessages;
				}

				setDatabaseText();

				timDuration.Enabled = false;
				progMain.Visible = false;
				btnGo.Visible = true;
				cboDatabases.Enabled = true;
				btnStop.Visible = false;
				txtQuery.ReadOnly = false;
				txtQuery.Focus();
				this.Parent.Text = this.Parent.Text.Replace(" (Executing)", "");
			}));
		}

		private object getDataTableObject(DataGridView grid, int rowIndex, int colIndex)
		{
			var dts = grid.Tag as List<DataTable>;
			if (dts.Count <= 0) return null;

			int dtRowIndex = rowIndex;
			int dtTableIndex = 0;
			var dt = dts[dtTableIndex];

			while (dtRowIndex >= dt.Rows.Count)
			{
				dtRowIndex -= dt.Rows.Count;
				dtTableIndex++;
				dt = dts[dtTableIndex];
			}

			return dt.Rows[dtRowIndex][colIndex];
		}

		private void grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
		{
			var val = getDataTableObject(sender as DataGridView, e.RowIndex, e.ColumnIndex);

			if (val == DBNull.Value)
			{
				e.CellStyle.BackColor = Color.LightYellow;
				e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Italic);
			}
		}

		private void grid_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
		{
			e.Value = getDataTableObject(sender as DataGridView, e.RowIndex, e.ColumnIndex);

			if (e.Value == DBNull.Value)
				e.Value = "NULL";

			(sender as DataGridView).Rows[e.RowIndex].HeaderCell.Value = (e.RowIndex + 1).ToString();

			//int currPage = 0;
			//int currRow = 0;
			//var currDt = dts[currPage];
			//while (e.RowIndex > currDt.Rows.Count + currRow)
			//{
			//	currPage++;
			//	currRow += currDt.Rows.Count;
			//	currDt = dts[currPage];
			//}
		}

		public void SelectTopN(int? topN, TreeNode selectedNode)
		{
			if (!string.IsNullOrEmpty(txtQuery.Text))
			{
				var dlgResult = MessageBox.Show("Yes to overwrite, No to append.", "Append", MessageBoxButtons.YesNoCancel);
				switch (dlgResult)
				{
					case DialogResult.Cancel:
						return;
					case DialogResult.Yes:
						txtQuery.Text = string.Empty;
						break;
					case DialogResult.No:
						txtQuery.AppendText(";\r\n");
						break;
				}
			}

			if (selectedNode.Parent != null && _currentConnection.Database != selectedNode.Parent.Parent.Text)
			{
				var node = selectedNode.Parent;
				while (!(node.Tag is Database.Library.DatabaseObjects.Database))
				{
					node = node.Parent;
				}
				_currentConnection.ChangeDatabase(node.Text);
			}

			string objName = string.Empty;
            var dbName = string.Empty;
			string[] columns = null;

			if (selectedNode.Tag is Library.DatabaseObjects.View)
			{
				var view = selectedNode.Tag as Library.DatabaseObjects.View;
                dbName = view.Database.DataSource.GetConvertedObjectName(view.Database.DatabaseName);
                objName = view.GetObjectNameWithSchema(view.Database.DataSource);
				columns = view.Columns.Select(c => c.ColumnName).ToArray();
			}
			else
			{
				var tbl = selectedNode.Tag as Table;
                dbName = tbl.Database.DataSource.GetConvertedObjectName(tbl.Database.DatabaseName);
                objName = tbl.GetObjectNameWithSchema(tbl.Database.DataSource);
				columns = tbl.Columns.Select(c => c.ColumnName).ToArray();
			}

			txtQuery.AppendText(string.Format("select {0}\r\n\t{1}\r\nfrom {4}{2}\r\n{3}",
				topN != null ? _server.GetPreTopN(topN.Value) : string.Empty,
				_server.GetColumnSelectList(columns),
				objName,
				topN != null ? _server.GetPostTopN(topN.Value) : string.Empty,
                string.IsNullOrEmpty(dbName) ? string.Empty : dbName + "."
				));
		}

		public void PopulateScript(string script, TreeNode selectedNode)
		{
			if (selectedNode.Parent != null && _currentConnection.Database != selectedNode.Parent.Parent.Text)
				_currentConnection.ChangeDatabase(selectedNode.Parent.Parent.Text);

			if (_currentConnection.GetType().Equals(typeof(SqlConnection)))
				txtQuery.Text = script;
		}

		private void btnStop_Click(object sender, EventArgs e)
		{
			btnStop.Enabled = false;
			_stopRequested = true;
			_currentCommand.Cancel();
		}

		private void timDuration_Tick(object sender, EventArgs e)
		{
			lblTime.Text = (DateTime.Now - _start).TotalHours.ToString("00") + ":" + (DateTime.Now - _start).Minutes.ToString("00") + ":" +
				(DateTime.Now - _start).Seconds.ToString("00");
		}

		private void cboDatabases_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (_lock) return;
			try
			{
				_currentConnection.ChangeDatabase(cboDatabases.Text);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
				_lock = true;
				cboDatabases.Text = _currentConnection.Database;
				_lock = false;
			}
		}

		private void txtQuery_KeyDown(object sender, KeyEventArgs e)
		{
			if ((e.KeyCode == Keys.E && e.Modifiers == Keys.Control) || e.KeyCode == Keys.F5)
			{
				btnGo_Click(sender, e);
				e.Handled = true;
			}
			//else if (e.KeyCode == Keys.A && e.Modifiers == Keys.Control)
			//	(sender as TextBox).SelectAll();
		}
	}
}
