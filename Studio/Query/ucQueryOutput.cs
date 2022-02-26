using Newtonsoft.Json.Linq;
using PaJaMa.Database.Library;
using PaJaMa.Database.Library.DatabaseObjects;
using PaJaMa.Database.Library.DataSources;
using PaJaMa.Database.Library.Workspaces;
using PaJaMa.Database.Studio.Classes;
using ScintillaNET;
using ScintillaNET_FindReplaceDialog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace PaJaMa.Database.Studio.Query
{
	public partial class ucQueryOutput : UserControl
	{
		[DllImport("user32.dll")]
		public static extern bool GetCaretPos(out System.Drawing.Point lpPoint);

		private List<SplitContainer> _splitContainers = new List<SplitContainer>();
		private bool _lock = false;
		private bool _stopRequested = false;
		private bool _kaying = false;
		private DateTime _start;
		private DbCommand _currentCommand;
		private DataSource _dataSource;
		private DatabaseConnection _databaseConnection;
		private string _query;
		private bool _flagIntellisense;
		private ListBox _intelliBox;
		private IntellisenseHelper _intellisenseHelper;
		private Dictionary<int, Dictionary<int, string>> _errorDict;

		private FindReplace _findReplace;

		public DbConnection CurrentConnection;
		public ucWorkspace Workspace { get; set; }
		public QueryOutput QueryOutput { get; set; }
		public EventHandler<QueryExecutedEventArgs> QueryExecuted;

		public const string PATTERN = "(.* |^|\n|.)([A-Za-z]+)$";

		public ucQueryOutput()
		{
			InitializeComponent();
			_intelliBox = new ListBox();
			_intelliBox.Visible = false;
			_intelliBox.Parent = this;
			_intelliBox.KeyDown += _intelliBox_KeyDown;
		}

		private void btnGo_Click(object sender, EventArgs e)
		{
			string query = txtQuery.Text;
			if (txtQuery.SelectedText.Length > 0)
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

			SaveOutput(true);
		}

		public bool Connect(DbConnection connection, DatabaseConnection databaseConnection, DataSource dataSource, QueryOutput queryOutput, bool useDummyDA)
		{
			try
			{
				_dataSource = dataSource;
				_databaseConnection = databaseConnection;
				_intellisenseHelper = new IntellisenseHelper(_dataSource);
				if (useDummyDA)
					CurrentConnection = connection;
				else
				{
					CurrentConnection = dataSource.OpenConnection();
					// TODO: generic
					if (CurrentConnection is SqlConnection)
						(CurrentConnection as SqlConnection).InfoMessage += ucQueryOutput_InfoMessage;
				}

				pnlButtons.Enabled = splitQuery.Enabled = true;


				lblDatabase.Visible = cboDatabases.Visible = dataSource.Databases.Count > 1;


				cboDatabases.Items.Clear();
				cboDatabases.Items.AddRange(dataSource.Databases.ToArray());

				if (!string.IsNullOrEmpty(queryOutput.Database) && queryOutput.Database != CurrentConnection.Database && dataSource.Databases.Any(d => d.DatabaseName == queryOutput.Database))
					CurrentConnection.ChangeDatabase(queryOutput.Database);

				_lock = true;
				cboDatabases.Text = CurrentConnection.Database;
				txtQuery.Text = queryOutput.Query;
				txtQuery.EmptyUndoBuffer();

				txtQuery.Margins[0].Type = MarginType.Number;

				// remove conflicting hotkeys from scintilla
				txtQuery.ClearCmdKey(Keys.Control | Keys.F);
				txtQuery.ClearCmdKey(Keys.Control | Keys.R);
				txtQuery.ClearCmdKey(Keys.Control | Keys.H);
				txtQuery.ClearCmdKey(Keys.Control | Keys.L);
				txtQuery.ClearCmdKey(Keys.Control | Keys.U);

				// Configure the default style
				txtQuery.StyleResetDefault();
				txtQuery.Styles[Style.Default].Font = "Consolas";
				txtQuery.Styles[Style.Default].Size = 10;
				txtQuery.StyleClearAll();

				// Configure the CPP (C#) lexer styles
				txtQuery.Styles[Style.Sql.Default].ForeColor = Color.Silver;
				txtQuery.Styles[Style.Sql.Comment].ForeColor = Color.FromArgb(0, 128, 0); // Green
				txtQuery.Styles[Style.Sql.CommentLine].ForeColor = Color.FromArgb(0, 128, 0); // Green
				txtQuery.Styles[Style.Sql.CommentLineDoc].ForeColor = Color.FromArgb(128, 128, 128); // Gray
				txtQuery.Styles[Style.Sql.Number].ForeColor = Color.Olive;
				txtQuery.Styles[Style.Sql.Word].ForeColor = Color.Blue;
				txtQuery.Styles[Style.Sql.Word2].ForeColor = Color.Blue;
				txtQuery.Styles[Style.Sql.String].ForeColor = Color.FromArgb(163, 21, 21); // Red
				txtQuery.Styles[Style.Sql.Character].ForeColor = Color.FromArgb(163, 21, 21); // Red
				txtQuery.Styles[Style.Sql.Operator].ForeColor = Color.Purple;

				var keywords = new List<string>();
				keywords.AddRange(_dataSource.GetReservedKeywords().Select(k => k.ToUpper()));
				keywords.AddRange(_dataSource.GetReservedKeywords().Select(k => k.ToLower()));
				txtQuery.SetKeywords(0, string.Join(" ", keywords.ToArray()));

				keywords = new List<string>();
				keywords.AddRange(_dataSource.ColumnTypes.Select(c => c.TypeName.ToUpper()));
				keywords.AddRange(_dataSource.ColumnTypes.Select(c => c.TypeName.ToLower()));
				txtQuery.SetKeywords(1, string.Join(" ", keywords.ToArray()));

				_findReplace = new FindReplace(txtQuery);
				_findReplace.KeyPressed += _findReplace_KeyPressed;

				_lock = false;
				return true;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
				return false;
			}
		}

		private void _findReplace_KeyPressed(object sender, KeyEventArgs e)
		{

		}

		private void ucQueryOutput_InfoMessage(object sender, SqlInfoMessageEventArgs e)
		{
			if (this.IsHandleCreated)
			{
				this.Invoke(new Action(() =>
					{
						txtMessages.Text += e.Message + "\r\n\r\n";
					}));
			}
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
			var parts = _query.Replace("\r\r", "\r").Split(new string[] { "\r\ngo\r\n", "\r\nGO\r\n", "\r\nGo\r\n", "\r\ngO\r\n",
					"\ngo\n", "\nGO\n", "\nGo\n", "\ngO\n"
				}, StringSplitOptions.RemoveEmptyEntries);

			var sbErrors = new StringBuilder();
			int tries = 2;
			bool hasTable = false;
			var createdGrids = new List<DataGridView>();
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

								hasTable = true;

								var grid = new DataGridView();
								createdGrids.Add(grid);
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
									// grid.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
									grid.Dock = DockStyle.Fill;
									grid.AllowUserToAddRows = grid.AllowUserToDeleteRows = false;
									grid.ReadOnly = true;
									// grid.VirtualMode = true;
									// grid.RowCount = 0;
									// grid.CellValueNeeded += grid_CellValueNeeded;
									grid.CellFormatting += grid_CellFormatting;
									grid.DataError += Grid_DataError;
									grid.RowPostPaint += Grid_RowPostPaint;
									grid.ContextMenuStrip = mnuGrid;

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
				if ((!hasTable && totalResults == 0) || sbErrors.Length > 0)
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
				this.QueryExecuted?.Invoke(this, new QueryExecutedEventArgs() { Grids = createdGrids });
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
				if (!tbl.Columns.Any())
					tbl.Database.DataSource.PopulateChildColumns(CurrentConnection, tbl);
				dbName = tbl.Database.DataSource.GetConvertedObjectName(tbl.Database.DatabaseName);
				CurrentConnection.ChangeDatabase(tbl.Database.DatabaseName);
				objName = tbl.GetObjectNameWithSchema(tbl.Database.DataSource);
				columns = tbl.Columns.Select(c => c.ColumnName).ToArray();
			}



			txtQuery.AppendText(string.Format("SELECT {0}\r\n\t{1}\r\nFROM {4}{2}\r\n{3}",
				topN != null ? _dataSource.GetPreTopN(topN.Value) : string.Empty,
				_dataSource.GetColumnSelectList(columns),
				objName,
				topN != null ? _dataSource.GetPostTopN(topN.Value) : string.Empty,
				string.IsNullOrEmpty(dbName) ? string.Empty : dbName + "."
				));

			SaveOutput(false);
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
				_dataSource.ChangeDatabase(cboDatabases.Text);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
				_lock = true;
				cboDatabases.Text = CurrentConnection.Database;
				_lock = false;
			}
		}

		private void formatSQL()
		{
			txtQuery.Text = FormatHelper.GetFormattedSQL(_dataSource, txtQuery.Text);
			txtQuery.Focus();
		}

		public void CommentSelected()
		{
			txtQuery.BeginUndoAction();
			int lineStart = txtQuery.SelectionStart;
			while ((lineStart > 0) && (txtQuery.Text[lineStart - 1] != '\n'))
				lineStart--;
			int selectionEnd = txtQuery.SelectionEnd;
			for (int i = lineStart; i <= selectionEnd; i++)
			{
				if (i == 0 || (i < txtQuery.Text.Length && txtQuery.Text[i - 1] == '\n'))
				{
					txtQuery.InsertText(i, "-- ");
				}
			}
			txtQuery.EndUndoAction();
		}

		public void UnCommentSelected()
		{
			txtQuery.BeginUndoAction();
			int lineStart = txtQuery.SelectionStart;
			while ((lineStart > 0) && (txtQuery.Text[lineStart - 1] != '\n'))
				lineStart--;
			int selectionEnd = txtQuery.SelectionEnd;
			for (int i = lineStart; i <= selectionEnd; i++)
			{
				if (i == 0 || (i < txtQuery.Text.Length && txtQuery.Text[i - 1] == '\n'))
				{
					if (txtQuery.Text.Substring(i, 2) == "--")
					{
						txtQuery.DeleteRange(i, 2);
					}
					if (txtQuery.Text.Substring(i, 1) == " ")
					{
						txtQuery.DeleteRange(i, 1);
					}
				}
			}
			txtQuery.EndUndoAction();
		}

		private void txtQuery_KeyDown(object sender, KeyEventArgs e)
		{
			if (_kaying)
			{
				if (e.KeyCode == Keys.C && e.Modifiers == Keys.Control)
				{
					this.CommentSelected();
				}
				else if (e.KeyCode == Keys.U && e.Modifiers == Keys.Control)
				{
					this.UnCommentSelected();
				}
				else if (e.KeyCode == Keys.D && e.Modifiers == Keys.Control)
				{
					this.formatSQL();
					e.Handled = true;
				}
				_kaying = false;
			}
			else if (e.KeyCode == Keys.F && e.Modifiers == (Keys.Shift | Keys.Alt))
			{
				this.formatSQL();
				e.Handled = true;
				// remove focus from toolstrip
				SendKeys.Send("%");
			}
			else if ((e.KeyCode == Keys.E && e.Modifiers == Keys.Control) || e.KeyCode == Keys.F5)
			{
				btnGo_Click(sender, e);
				e.Handled = true;
			}
			else if (e.KeyCode == Keys.K && e.Modifiers == Keys.Control)
			{
				_kaying = true;
				e.SuppressKeyPress = true;
				e.Handled = true;
			}
			else if (e.KeyCode == Keys.S && e.Control)
			{
				SaveCurrentQuery();
			}
			else if (e.KeyCode == Keys.Space && e.Modifiers == Keys.Control)
			{
				_flagIntellisense = true;
			}
			else if (e.KeyCode == Keys.Down && _intelliBox.Visible)
			{
				e.Handled = true;
				_intelliBox.Focus();
				_intelliBox.SelectedIndex++;
			}
			else if (((int)(char)e.KeyCode < 65 || (int)(char)e.KeyCode > 90) && e.KeyCode != Keys.Back && e.KeyCode != Keys.Shift && e.KeyCode != Keys.ShiftKey && _intelliBox.Visible)
			{
				_intelliBox.Hide();
			}
			else if (_intelliBox.Visible)
			{
				_flagIntellisense = true;
			}

			if (e.Control && e.KeyCode == Keys.F)
			{
				_findReplace.ShowFind();
				e.SuppressKeyPress = true;
			}
			else if (e.Shift && e.KeyCode == Keys.F3)
			{
				_findReplace.Window.FindPrevious();
				e.SuppressKeyPress = true;
			}
			else if (e.KeyCode == Keys.F3)
			{
				_findReplace.Window.FindNext();
				e.SuppressKeyPress = true;
			}
			else if (e.Control && e.KeyCode == Keys.H)
			{
				_findReplace.ShowReplace();
				e.SuppressKeyPress = true;
			}
			else if (e.Control && e.KeyCode == Keys.I)
			{
				_findReplace.ShowIncrementalSearch();
				e.SuppressKeyPress = true;
			}
			else if (e.Control && e.KeyCode == Keys.G)
			{
				GoTo MyGoTo = new GoTo((Scintilla)sender);
				MyGoTo.ShowGoToDialog();
				e.SuppressKeyPress = true;
			}
			else if (e.Control && e.KeyCode == Keys.W)
			{
				txtQuery.WrapMode = txtQuery.WrapMode == WrapMode.None ? WrapMode.Word : WrapMode.None;
				e.SuppressKeyPress = true;
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

		public void SaveOutput(bool andSave)
		{
			QueryOutput.Query = txtQuery.Text.Length > 50000 ? "" : txtQuery.Text;
			if (andSave)
			{
				_databaseConnection.SaveQueryOutputs();
			}

			var queryHistoryPath = new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
				"DatabaseStudio", "QueryHistory", "QueryHistory_" + DateTime.Now.ToString("yyyyMMdd") + ".sql"));
			if (!queryHistoryPath.Directory.Exists) queryHistoryPath.Directory.Create();
			File.AppendAllText(queryHistoryPath.FullName, "\r\n\r\n\r\n" + 
				_databaseConnection.ConnectionName + "\r\n\r\n" +
				txtQuery.Text);
		}

		private void selectIntellisenseItem()
		{
			if (_intelliBox.SelectedItem != null)
			{
				var regex = Regex.Match(txtQuery.Text.Substring(0, txtQuery.SelectionStart), IntellisenseHelper.PATTERN);
				if (regex.Success)
				{
					var replace = regex.Groups[2].Value.Split('.').Last();
					txtQuery.SelectionStart = Math.Max(0, txtQuery.SelectionStart - replace.Length);
					txtQuery.SelectionEnd = txtQuery.SelectionStart + replace.Length;
					if (txtQuery.Text.Length > txtQuery.SelectionEnd &&
						_dataSource.SurroundingCharacters.Contains(txtQuery.Text.Substring(txtQuery.SelectionEnd, 1)))
					{
						txtQuery.SelectionEnd++;
					}

					txtQuery.ReplaceSelection(string.Empty);
				}
				txtQuery.ReplaceSelection((_intelliBox.SelectedItem as IntellisenseMatch).ShortName);
			}
			_intelliBox.Hide();
			txtQuery.Focus();
		}

		private void showIntellisense()
		{
			int tries = 2;
			if (CurrentConnection.State != ConnectionState.Open)
			{
				while (tries > 0)
				{
					try
					{
						CurrentConnection.Open();
					}
					catch (Exception ex)
					{
						tries--;
						if (tries > 0 && ex.Message == "Fatal error encountered during command execution.")
							continue;
						return;
					}
				}
			}

			try
			{
				var matches = _intellisenseHelper.GetIntellisenseMatches(txtQuery.Text, txtQuery.SelectionStart, CurrentConnection);
				_intelliBox.Items.Clear();
				string maxString = string.Empty;
				foreach (var m in matches)
				{
					if (m.ShortName.Length > maxString.Length) maxString = m.ShortName;
					_intelliBox.Items.Add(m);
				}
				if (_intelliBox.Items.Count > 0)
					_intelliBox.SelectedIndex = 0;
				Point p = new Point();
				GetCaretPos(out p);
				var measure = txtQuery.CreateGraphics().MeasureString(maxString, txtQuery.Font);
				_intelliBox.SetBounds(p.X, p.Y + 20, 500, Math.Min(300, Math.Max(32, (int)(measure.Height * _intelliBox.Items.Count))));
				_intelliBox.Show();
				_intelliBox.BringToFront();
			}
			catch (Exception e)
			{
				// TODO:
			}
		}



		private void TxtQuery_KeyPress(object sender, KeyPressEventArgs e)
		{
			e.Handled = e.KeyChar == 32 && _flagIntellisense;
		}

		private void _intelliBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
			{
				_intelliBox.Hide();
				txtQuery.Focus();
			}
			else if ((e.KeyCode == Keys.Enter || e.KeyCode == Keys.Tab) && _intelliBox.SelectedItem != null)
			{
				this.selectIntellisenseItem();
			}
		}

		private void TxtQuery_KeyUp(object sender, KeyEventArgs e)
		{
			if (_flagIntellisense)
			{
				showIntellisense();
			}
			if (e.KeyCode == Keys.Enter)
			{
				if (txtQuery.SelectionStart > 0 && txtQuery.SelectedText.Length == 0)
				{
					int lineStart = txtQuery.SelectionStart - 1;
					while ((lineStart > 0) && (txtQuery.Text[lineStart - 1] != '\n'))
					{
						lineStart--;
					}
					var indentMatch = Regex.Match(txtQuery.Text.Substring(lineStart, (txtQuery.SelectionStart - lineStart)),
						"^([ \t]+)");
					if (indentMatch.Success)
					{
						txtQuery.InsertText(txtQuery.SelectionStart, indentMatch.Groups[1].Value);
						txtQuery.SelectionStart += indentMatch.Groups[1].Value.Length;
					}
				}

			}

			_flagIntellisense = false;
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (_intelliBox.Visible && keyData == Keys.Tab)
			{
				this.selectIntellisenseItem();
				return true;
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}

		public void SaveCurrentQuery()
		{
			var dlg = new SaveFileDialog();
			dlg.Filter = "(*.sql)|*.sql|All files (*.*)|*.*";
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				File.WriteAllText(dlg.FileName, txtQuery.Text);
			}
		}

		private int maxLineNumberCharLength;
		private void TxtQuery_TextChanged(object sender, EventArgs e)
		{
			// Did the number of characters in the line number display change?
			// i.e. nnn VS nn, or nnnn VS nn, etc...
			var maxLineNumberCharLength = txtQuery.Lines.Count.ToString().Length;
			if (maxLineNumberCharLength == this.maxLineNumberCharLength)
				return;

			// Calculate the width required to display the last line number
			// and include some padding for good measure.
			const int padding = 2;
			txtQuery.Margins[0].Width = txtQuery.TextWidth(Style.LineNumber, new string('9', maxLineNumberCharLength + 1)) + padding;
			this.maxLineNumberCharLength = maxLineNumberCharLength;
		}

		private void TxtQuery_MouseUp(object sender, MouseEventArgs e)
		{
			// Indicators 0-7 could be in use by a lexer
			// so we'll use indicator 8 to highlight words.
			const int NUM = 8;

			// Remove all uses of our indicator
			txtQuery.IndicatorCurrent = NUM;
			txtQuery.IndicatorClearRange(0, txtQuery.TextLength);

			var text = txtQuery.SelectedText;
			if (string.IsNullOrEmpty(text)) return;

			// Update indicator appearance
			txtQuery.Indicators[NUM].Style = IndicatorStyle.StraightBox;
			txtQuery.Indicators[NUM].Under = true;
			txtQuery.Indicators[NUM].ForeColor = Color.Green;
			txtQuery.Indicators[NUM].OutlineAlpha = 50;
			txtQuery.Indicators[NUM].Alpha = 30;

			// Search the document
			txtQuery.TargetStart = 0;
			txtQuery.TargetEnd = txtQuery.TextLength;
			txtQuery.SearchFlags = SearchFlags.None;
			while (txtQuery.SearchInTarget(text) != -1)
			{
				if (txtQuery.TargetStart != txtQuery.SelectionStart)
				{
					// Mark the search results with the current indicator
					txtQuery.IndicatorFillRange(txtQuery.TargetStart, txtQuery.TargetEnd - txtQuery.TargetStart);
				}

				// Search the remainder of the document
				txtQuery.TargetStart = txtQuery.TargetEnd;
				txtQuery.TargetEnd = txtQuery.TextLength;
			}
		}

		private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
		{
			copyGridContent(false);
		}

		private void CopyWithheadersToolStripMenuItem_Click(object sender, EventArgs e)
		{
			copyGridContent(true);
		}

		private void copyToJSONToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var grid = mnuGrid.SourceControl as DataGridView;
			var array = new JArray();
			foreach (DataGridViewRow row in grid.Rows)
			{
				var jobj = new JObject();
				foreach (DataGridViewColumn col in grid.Columns)
				{
					jobj.Add(new JProperty(col.HeaderText, row.Cells[col.Index].Value == DBNull.Value ? null : row.Cells[col.Index].Value));
				}
				array.Add(jobj);
			}
			Clipboard.SetDataObject(array.ToString());
		}

		private void copyGridContent(bool withHeader)
		{
			var grid = mnuGrid.SourceControl as DataGridView;
			grid.ClipboardCopyMode = withHeader
				? DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText
				: DataGridViewClipboardCopyMode.EnableWithoutHeaderText;

			Clipboard.SetDataObject(grid.GetClipboardContent());

			grid.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
		}

		private void WrapTextToolStripMenuItem_Click(object sender, EventArgs e)
		{
			txtQuery.WrapMode = WrapMode.Word;
		}

		private void UnwrapTextToolStripMenuItem_Click(object sender, EventArgs e)
		{
			txtQuery.WrapMode = WrapMode.None;
		}

		public void ExecuteQuery()
		{
			btnGo_Click(btnGo, new EventArgs());
		}

		private Table getDraggedTable(DragEventArgs e)
        {
			var nodes = e.Data.GetData(typeof(List<TreeNode>));
			if (nodes != null)
			{
				return ((List<TreeNode>)nodes)
					.Where(x => x.Tag is Table || (x.Parent != null && x.Parent.Tag is Table))
					.Select(x => x.Tag as Table ?? x.Parent.Tag as Table).FirstOrDefault();
			}
			return null;
		}

        private void txtQuery_DragEnter(object sender, DragEventArgs e)
        {
			if (getDraggedTable(e) != null) e.Effect = DragDropEffects.Move;
		}

        private void txtQuery_DragDrop(object sender, DragEventArgs e)
        {
			var table = getDraggedTable(e);
			if (table != null)
            {
				// var charPos = txtQuery.CharPositionFromPoint(e.X, e.Y);
				if (!table.Columns.Any()) table.Database.DataSource.PopulateChildColumns(this.CurrentConnection, table);
				txtQuery.InsertText(txtQuery.CurrentPosition, string.Join(",\r\n", table.Columns.Select(x => table.Database.DataSource.GetConvertedObjectName(x.ColumnName))));
            }
        }
    }
}
