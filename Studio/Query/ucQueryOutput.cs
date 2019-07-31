using PaJaMa.Database.Library.DatabaseObjects;
using PaJaMa.Database.Library.DataSources;
using PaJaMa.Database.Library.Workspaces;
using PaJaMa.Database.Studio.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace PaJaMa.Database.Studio.Query
{
	public partial class ucQueryOutput : UserControl
	{
		private List<SplitContainer> _splitContainers = new List<SplitContainer>();
		private bool _lock = false;
		private bool _stopRequested = false;
		private DateTime _start;
		private DbCommand _currentCommand;
		private DataSource _server;
		private string _query;
		private Dictionary<int, Dictionary<int, string>> _errorDict;

		public DbConnection CurrentConnection;
		public ucWorkspace Workspace { get; set; }
		public QueryOutput QueryOutput { get; set; }

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
			_currentCommand = CurrentConnection.CreateCommand();
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

			if (CurrentConnection.GetType().Equals(typeof(System.Data.OleDb.OleDbConnection)))
				execute();
			else
				new Thread(new ThreadStart(execute)).Start();

			saveOutput();
		}

		public bool Connect(DbConnection connection, DataSource server, QueryOutput queryOutput, bool useDummyDA)
		{
			try
			{
				_server = server;
				if (useDummyDA)
					CurrentConnection = connection;
				else
				{
					CurrentConnection = server.OpenConnection(string.Empty);
					// TODO: generic
					if (CurrentConnection is SqlConnection)
						(CurrentConnection as SqlConnection).InfoMessage += ucQueryOutput_InfoMessage;
				}

				pnlButtons.Enabled = splitQuery.Enabled = true;


				lblDatabase.Visible = cboDatabases.Visible = server.Databases.Count > 1;


				cboDatabases.Items.Clear();
				cboDatabases.Items.AddRange(server.Databases.ToArray());

				if (!string.IsNullOrEmpty(queryOutput.Database) && queryOutput.Database != CurrentConnection.Database)
					CurrentConnection.ChangeDatabase(queryOutput.Database);

				_lock = true;
				cboDatabases.Text = CurrentConnection.Database;
				txtQuery.Text = queryOutput.Query;
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
			this.Invoke(new Action(() =>
				{
					txtMessages.Text += e.Message + "\r\n\r\n";
				}));
		}

		public void Disconnect()
		{
			if (CurrentConnection != null)
			{
				if (CurrentConnection.State == ConnectionState.Open)
					CurrentConnection.Close();
				CurrentConnection.Dispose();
				CurrentConnection = null;
			}
		}

		private void setDatabaseText()
		{
			if (cboDatabases.Items.Count > 0 && cboDatabases.Visible)
			{
				_lock = true;
				cboDatabases.Text = CurrentConnection.Database;
				_lock = false;
			}
		}


		private void execute()
		{
			_errorDict = new Dictionary<int, Dictionary<int, string>>();
			int totalResults = 0;
			int recordsAffected = 0;
			var parts = _query.Split(new string[] { "\r\ngo\r\n", "\r\nGO\r\n", "\r\nGo\r\n", "\r\ngO\r\n",
					"\ngo\n", "\nGO\n", "\nGo\n", "\ngO\n"
				}, StringSplitOptions.RemoveEmptyEntries);

			var sbErrors = new StringBuilder();
			int tries = 2;
			foreach (var part in parts)
			{
				while (tries > 0)
				{
					try
					{
						if (CurrentConnection.State != ConnectionState.Open)
							CurrentConnection.Open();

						_currentCommand.CommandText = part;
						_currentCommand.CommandTimeout = 600000;
						using (var dr = _currentCommand.ExecuteReader())
						{
							// this.Invoke(new Action(() =>
							// {
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
								//var timer = new System.Threading.Timer((object stateInfo) =>
								//{
								//	this.Invoke(new Action(() =>
								//	{
								//		dt.EndLoadData();
								//		dt.BeginLoadData();
								//		Application.DoEvents();
								//	}));
								//}, null, 3000, 3000);

								var schema = dr.GetSchemaTable();
								if (schema == null || !hasNext)
								{
									if (dr.RecordsAffected > 0)
										recordsAffected += dr.RecordsAffected;
									break;
								}

								var grid = new DataGridView();
								this.Invoke(new Action(() =>
								{
									foreach (var row in schema.Rows.OfType<DataRow>())
									{
									// int existingCount = dt.Columns.OfType<DataColumn>().Count(c => c.ColumnName == row["ColumnName"].ToString());
									var colType = Type.GetType(row["DataType"].ToString());
										if (colType == null || colType.Equals(typeof(byte[])) || colType == typeof(Array))
											colType = typeof(string);
										string colName = row["ColumnName"].ToString();
										int curr = 1;
										while (dt.Columns.OfType<DataColumn>().Any(c => c.ColumnName == colName))
										{
											colName = row["ColumnName"].ToString() + curr.ToString();
											curr++;
										}
										dt.Columns.Add(colName, colType);
									// grid.Columns.Add(colName, row["ColumnName"].ToString());
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
								// grid.VirtualMode = true;
								// grid.RowCount = 0;
								// grid.CellValueNeeded += grid_CellValueNeeded;
								grid.CellFormatting += grid_CellFormatting;
									grid.DataError += Grid_DataError;
									grid.RowPostPaint += Grid_RowPostPaint;

									var splitDetails = new SplitContainer();
									splitDetails.Dock = DockStyle.Fill;
									splitDetails.Panel2Collapsed = true;
									splitDetails.Panel2MinSize = 0;
									var pnlDetail = new Panel();
									pnlDetail.Dock = DockStyle.Fill;
									pnlDetail.BorderStyle = BorderStyle.Fixed3D;
									pnlDetail.AutoScroll = true;
									splitDetails.Panel2.Controls.Add(pnlDetail);
									splitDetails.Panel1.Controls.Add(grid);
									grid.SelectionChanged += (object s, EventArgs e) => setDetailControls(splitDetails);

									splitContainer.Panel1.Controls.Add(splitDetails);
									if (lastSplit != null)
										lastSplit.Panel2.Controls.Add(splitContainer);
									else
										tabResults.Controls.Add(splitContainer);
									_splitContainers.Add(splitContainer);
									foreach (var split in _splitContainers)
									{
										split.SplitterDistance = splitQuery.Panel2.Height / _splitContainers.Count;
									}
									splitDetails.SplitterDistance = (int)((double)splitDetails.Width * 0.7);
									grid.DataSource = dt;
									grid.AutoGenerateColumns = true;
								// grid.Tag = new List<DataTable>() { dt };

								foreach (DataGridViewColumn col in grid.Columns)
									{
										if (!string.IsNullOrEmpty(col.Name))
										{
											var dtCol = dt.Columns[col.Name];
											col.ToolTipText = dtCol.DataType.Name;
										}
									}
								}));

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
											if (dr[j] is byte[])
											{
												row[j] = Convert.ToBase64String(dr[j] as byte[]);
											}
											else if (dr[j].GetType().IsArray)
											{
												List<string> stringVals = new List<string>();
												row[j] = string.Join(",", ((Array)dr[j]).OfType<object>().Select(o => o.ToString()));
											}
											else
											{
												row[j] = dr[j];
											}
										}
										catch (Exception ex)
										{
											// TODO: log
											if (!_errorDict.ContainsKey(i))
												_errorDict.Add(i, new Dictionary<int, string>());
											_errorDict[i].Add(j, "ERR! " + ex.Message);
										}
									}

									dt.Rows.Add(row);

									// if (i % 1000 == 0)
									//if ((DateTime.Now - lastRefresh).TotalSeconds > 3)
									//{
									//	lastRefresh = DateTime.Now;
									//	dt.EndLoadData();
									//	// grid.RowCount += dt.Rows.Count;
									//	dt = dt.Clone();
									//	// (grid.Tag as List<DataTable>).Add(dt);
									//	dt.BeginLoadData();
									//	//dt.EndLoadData();
									//	//dt.BeginLoadData();
									//	//Application.DoEvents();

									//	Application.DoEvents();
									//}

									// if (i % 1000 == 0)
									if ((DateTime.Now - lastRefresh).TotalSeconds > 2)
									{
										lastRefresh = DateTime.Now;
										this.Invoke(new Action(() =>
										{
											dt.EndLoadData();
											Application.DoEvents();
											dt.BeginLoadData();

										}));
									}
								}

								this.Invoke(new Action(() =>
								{
									dt.EndLoadData();
									Application.DoEvents();

								}));

								// var sum = (grid.Tag as List<DataTable>).Sum(t => t.Rows.Count);
								// grid.RowCount = sum;
								// totalResults += sum;
								totalResults = dt.Rows.Count;

								//grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

								if (!_stopRequested)
									hasNext = dr.NextResult();
							}
							// }));
						}
						tries = 0;
					}
					catch (Exception ex)
					{
						tries--;
						if (tries > 0 && ex.Message == "Fatal error encountered during command execution.")
							continue;
						sbErrors.AppendLine(ex.Message);
						tries = 0;
					}
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
				populateDetailPanels();
			}));
		}

		private void Grid_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
		{
			var grid = sender as DataGridView;
			var rowIdx = (e.RowIndex + 1).ToString();

			var centerFormat = new StringFormat()
			{
				// right alignment might actually make more sense for numbers
				Alignment = StringAlignment.Center,
				LineAlignment = StringAlignment.Center
			};

			var headerBounds = new Rectangle(e.RowBounds.Left, e.RowBounds.Top, grid.RowHeadersWidth, e.RowBounds.Height);
			e.Graphics.DrawString(rowIdx, this.Font, SystemBrushes.ControlText, headerBounds, centerFormat);
		}

		private void Grid_DataError(object sender, DataGridViewDataErrorEventArgs e)
		{
		}

		//private object getDataTableObject(DataGridView grid, int rowIndex, int colIndex)
		//{
		//	var dts = grid.Tag as List<DataTable>;
		//	if (dts.Count <= 0) return null;

		//	int dtRowIndex = rowIndex;
		//	int dtTableIndex = 0;
		//	var dt = dts[dtTableIndex];

		//	while (dtRowIndex >= dt.Rows.Count)
		//	{
		//		dtRowIndex -= dt.Rows.Count;
		//		dtTableIndex++;
		//		dt = dts[dtTableIndex];
		//	}

		//	return dt.Rows[dtRowIndex][colIndex];
		//}

		private void grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
		{
			var grid = sender as DataGridView;
			var val = grid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
			if (val == DBNull.Value)
			{
				e.CellStyle.BackColor = Color.LightYellow;
				e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Italic);
				e.Value = "NULL";
			}
			if (_errorDict.ContainsKey(e.RowIndex) && _errorDict[e.RowIndex].ContainsKey(e.ColumnIndex))
			{
				e.CellStyle.BackColor = Color.Red;
				e.Value = _errorDict[e.RowIndex][e.ColumnIndex];
			}
		}

		//private void grid_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
		//{
		//	var grid = sender as DataGridView;
		//	e.Value = getDataTableObject(grid, e.RowIndex, e.ColumnIndex);

		//	var cell = grid.Rows[e.RowIndex].Cells[e.ColumnIndex];
		//	if (e.Value == DBNull.Value)
		//	{
		//		cell.Style.BackColor = Color.LightYellow;
		//		cell.Style.Font = new Font(grid.Font, FontStyle.Italic);
		//	}

		//	if (_errorDict.ContainsKey(cell.RowIndex) && _errorDict[cell.RowIndex].ContainsKey(cell.ColumnIndex))
		//	{
		//		cell.Style.BackColor = Color.Red;
		//		e.Value = _errorDict[cell.RowIndex][cell.ColumnIndex];
		//	}

		//	if (e.Value == DBNull.Value)
		//		e.Value = "NULL";

		//	grid.Rows[e.RowIndex].HeaderCell.Value = (e.RowIndex + 1).ToString();

		//	//int currPage = 0;
		//	//int currRow = 0;
		//	//var currDt = dts[currPage];
		//	//while (e.RowIndex > currDt.Rows.Count + currRow)
		//	//{
		//	//	currPage++;
		//	//	currRow += currDt.Rows.Count;
		//	//	currDt = dts[currPage];
		//	//}
		//}

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

				
			string objName = string.Empty;
			var dbName = string.Empty;
			string[] columns = null;

			if (selectedNode.Tag is Library.DatabaseObjects.View)
			{
				var view = selectedNode.Tag as Library.DatabaseObjects.View;
                CurrentConnection.ChangeDatabase(view.Database.DatabaseName);
                dbName = view.Database.DataSource.GetConvertedObjectName(view.Database.DatabaseName);
				objName = view.GetObjectNameWithSchema(view.Database.DataSource);
				columns = view.Columns.Select(c => c.ColumnName).ToArray();
			}
			else
			{
				var tbl = selectedNode.Tag as Table;
				dbName = tbl.Database.DataSource.GetConvertedObjectName(tbl.Database.DatabaseName);
                CurrentConnection.ChangeDatabase(tbl.Database.DatabaseName);
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

			saveOutput();
		}

		public void PopulateScript(string script, TreeNode selectedNode)
		{
			var dbName = string.Empty;
			if (selectedNode.Tag is DatabaseObjectBase)
			{
				dbName = (selectedNode.Tag as DatabaseObjectBase).Database.DatabaseName;
			}
			else if (selectedNode.Parent != null && CurrentConnection.Database != selectedNode.Parent.Parent.Text)
			{
				dbName = selectedNode.Parent.Parent.Text;
			}

			if (dbName != CurrentConnection.Database)
				CurrentConnection.ChangeDatabase(dbName);

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
				CurrentConnection.ChangeDatabase(cboDatabases.Text);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
				_lock = true;
				cboDatabases.Text = CurrentConnection.Database;
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

		private void chkDetail_CheckedChanged(object sender, EventArgs e)
		{
			populateDetailPanels();
		}

		private void populateDetailPanels()
		{
			foreach (var spc in _splitContainers)
			{
				var splitDetails = spc.Panel1.Controls[0] as SplitContainer;
				splitDetails.Panel2Collapsed = !chkDetail.Checked;
				setDetailControls(splitDetails);
			}
		}

		private void setDetailControls(SplitContainer splitDetails)
		{
			var pnlDetail = splitDetails.Panel2.Controls[0] as Panel;
			if (splitDetails.Panel2Collapsed) return;
			var grid = splitDetails.Panel1.Controls[0] as DataGridView;
			if (pnlDetail.Controls.Count < 1)
			{
				pnlDetail.SuspendLayout();
				int top = 5;
				int maxLabelWidth = 0;
				var labels = new List<Label>();
				foreach (DataGridViewColumn col in grid.Columns)
				{
					var lbl = new Label();
					lbl.Text = col.HeaderText;
					lbl.Location = new Point(1, top);
					lbl.AutoSize = false;
					lbl.TextAlign = ContentAlignment.MiddleRight;
					if (lbl.Width > maxLabelWidth) maxLabelWidth = lbl.Width;
					labels.Add(lbl);
					pnlDetail.Controls.Add(lbl);
					top += 25;
				}
				foreach (var lbl in labels)
				{
					lbl.Width = maxLabelWidth - 8;
				}
				top = 5;
				foreach (DataGridViewColumn col in grid.Columns)
				{
					var txt = new TextBox();
					txt.Location = new Point(maxLabelWidth, top);
					txt.Width = pnlDetail.Width - maxLabelWidth - 7;
					txt.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
					txt.Name = col.Name;
					txt.ReadOnly = true;
					pnlDetail.Controls.Add(txt);
					top += 25;
				}
				pnlDetail.ResumeLayout();
			}

			foreach (var txt in pnlDetail.Controls.OfType<TextBox>())
			{
				txt.Text = string.Empty;
				var rows = grid.SelectedCells.OfType<DataGridViewCell>().Select(c => c.OwningRow).Distinct();
				if (rows.Count() != 1) continue;
				var row = rows.First();
				var col = grid.Columns[txt.Name];
				txt.Text = row.Cells[col.Index].Value == null ? string.Empty : row.Cells[col.Index].Value.ToString();

				// TODO: is it really null or just text?
				txt.BackColor = Color.Empty;
				txt.Font = new Font(txt.Font, FontStyle.Regular);
				if (txt.Text == "NULL")
				{
					txt.BackColor = Color.LightYellow;
					txt.Font = new Font(txt.Font, FontStyle.Italic);
				}
			}
		}

		private void saveOutput()
		{
			QueryOutput.Query = txtQuery.Text;
			PaJaMa.Common.SettingsHelper.SaveUserSettings<DatabaseStudioSettings>(Workspace.Settings);
		}
	}
}
