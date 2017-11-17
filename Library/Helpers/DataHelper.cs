using PaJaMa.Common;
using PaJaMa.Database.Library.Synchronization;
using PaJaMa.Database.Library.Workspaces.Compare;
using PaJaMa.Database.Library.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaJaMa.Database.Library.Helpers
{
	public class DataHelper
	{
		public const string FROM_PREFIX = "< ";
		public const string TO_PREFIX = "> ";

		private DbCommand _currentCommand;
		private DbDataReader _currentReader;

		private bool _cancel = false;

		public void CompareData(TableWorkspace workspace, BackgroundWorker worker = null, bool promptKey = true, string overrideKeyField = null)
		{
			if (workspace.ComparedData != null) return;

			workspace.ComparedData = new DataTableWithSchema();
			DataTable dtFrom = new DataTable();
			DataTable dtTo = new DataTable();

			List<string> primaryKeys = (from kc in workspace.SourceTable.KeyConstraints
										where kc.IsPrimaryKey
										from c in kc.Columns
										select c.ColumnName).ToList();

			if (primaryKeys.Count < 1 && overrideKeyField == null)
			{
				if (promptKey)
					throw new Exception("Cannot compare data: no primary key specified.");
				return;
			}

			_cancel = false;

			using (var conn = workspace.SourceTable.ParentDatabase.DataSource.GetConnection())
			{
				_currentCommand = conn.CreateCommand();
				_currentCommand.CommandText = string.Format("select * from {0}", workspace.SourceTable.GetObjectNameWithSchema(workspace.TargetDatabase.DataSource));
				conn.Open();
				_currentReader = _currentCommand.ExecuteReader();
				if (worker != null) worker.ReportProgress(0, string.Format("Populating {0} source table...", workspace.SourceTable.TableName));
				dtFrom.Load(_currentReader);
				conn.Close();
				if (_cancel) return;
			}

			workspace.ComparedData.PrimaryKeyFields = primaryKeys;

			using (var conn = workspace.TargetTable.ParentDatabase.DataSource.GetConnection())
			{
				_currentCommand = conn.CreateCommand();
				_currentCommand.CommandText = string.Format("select * from {0}", workspace.TargetTable.GetObjectNameWithSchema(workspace.TargetDatabase.DataSource));
				conn.Open();
				_currentReader = _currentCommand.ExecuteReader();
				if (worker != null) worker.ReportProgress(0, string.Format("Populating {0} target table...", workspace.SourceTable.TableName));
				dtTo.Load(_currentReader);
				conn.Close();
				if (_cancel) return;
			}

			_currentCommand.Dispose();
			_currentCommand = null;

			_currentReader.Dispose();
			_currentReader = null;

			foreach (var p in primaryKeys)
			{
				if (!dtTo.Columns.OfType<DataColumn>().Any(dc => dc.ColumnName == p))
				{
					throw new Exception("Target primary key missing: " + p);
				}
			}

			foreach (DataColumn dc in dtFrom.Columns)
			{
				workspace.ComparedData.Columns.Add(FROM_PREFIX + dc.ColumnName, dc.DataType);
				var targetCol = workspace.TargetTable.Columns.Select(c => c.ColumnName).FirstOrDefault(c => c == dc.ColumnName);
				if (string.IsNullOrEmpty(targetCol))
					targetCol = dc.ColumnName;

				var dc2 = dtTo.Columns.OfType<DataColumn>().FirstOrDefault(x => x.ColumnName == targetCol);
				if (dc2 != null)
					workspace.ComparedData.Columns.Add(TO_PREFIX + dc2.ColumnName, dc2.DataType);
			}

			foreach (DataColumn dc in dtTo.Columns.OfType<DataColumn>().Where(x =>
				!dtFrom.Columns.OfType<DataColumn>().Any(y => y.ColumnName == x.ColumnName)
				&& !workspace.SourceTable.Columns.Any(c => c.ColumnName == x.ColumnName)))
			{
				workspace.ComparedData.Columns.Add(TO_PREFIX + dc.ColumnName, dc.DataType);
			}

			int curr = 1;
			int total = dtFrom.Rows.Count;
			foreach (var dr in dtFrom.Rows.OfType<DataRow>())
			{
				if (worker != null) worker.ReportProgress(0, string.Format("Comparing {2}, source row {0} of {1}...", curr++, total, workspace.SourceTable.TableName));
				var drNew = workspace.ComparedData.NewRow();
				IEnumerable<DataRow> toDrs = null;
				if (!string.IsNullOrEmpty(overrideKeyField))
				{
					toDrs = (toDrs ?? dtTo.Rows.OfType<DataRow>()).Where(x => x[overrideKeyField].Equals(dr[overrideKeyField]));
				}
				else
				{
					foreach (var p in primaryKeys)
					{
						toDrs = (toDrs ?? dtTo.Rows.OfType<DataRow>()).Where(x => x[p].Equals(dr[p]));
					}
				}
				DataRow toDr = toDrs.FirstOrDefault();
				foreach (DataColumn dc in workspace.ComparedData.Columns)
				{
					if (dc.ColumnName.StartsWith(FROM_PREFIX))
						drNew[dc] = dr[dc.ColumnName.Substring(2)];
					else if (toDr != null)
						drNew[dc] = toDr[dc.ColumnName.Substring(2)];
				}
				workspace.ComparedData.Rows.Add(drNew);
				if (_cancel) return;
			}

			curr = 1;
			total = dtTo.Rows.Count;
			foreach (var dr in dtTo.Rows.OfType<DataRow>())
			{
				if (worker != null) worker.ReportProgress(0, string.Format("Comparing {2}, target row {0} of {1}...", curr++, total, workspace.TargetTable.TableName));
				IEnumerable<DataRow> fromDrs = null;
				if (!string.IsNullOrEmpty(overrideKeyField))
				{
					fromDrs = (fromDrs ?? dtFrom.Rows.OfType<DataRow>()).Where(x => x[overrideKeyField].Equals(dr[overrideKeyField]));
				}
				else
				{
					foreach (var p in primaryKeys)
					{
						fromDrs = (fromDrs ?? dtFrom.Rows.OfType<DataRow>()).Where(x => x[p].Equals(dr[p]));
					}
				}

				if (fromDrs.Any()) continue;

				var drNew = workspace.ComparedData.NewRow();
				foreach (DataColumn dc in workspace.ComparedData.Columns)
				{
					if (dc.ColumnName.StartsWith(FROM_PREFIX))
						continue;

					drNew[dc] = dr[dc.ColumnName.Substring(2)];
				}
				workspace.ComparedData.Rows.Add(drNew);
				if (_cancel) return;
			}
		}

		public void SynchronizeRows(TableWorkspace workspace, DataRow[] selectedRows)
		{
			if (selectedRows.Length < 1) return;

			List<string> primaryKeys = (from kc in workspace.SourceTable.KeyConstraints
										where kc.IsPrimaryKey
										from c in kc.Columns
										select c.ColumnName).ToList();

			using (var conn = workspace.TargetTable.ParentDatabase.GetConnection())
			{
				conn.Open();

				using (var trans = conn.BeginTransaction())
				{
					_currentCommand = conn.CreateCommand();
					_currentCommand.Transaction = trans;

					try
					{
						StringBuilder sbUpdate = new StringBuilder(string.Format("update {0} set ",
							workspace.TargetTable.GetObjectNameWithSchema(workspace.TargetDatabase.DataSource)));

						StringBuilder sbInsert = new StringBuilder();
						sbInsert.AppendLine(workspace.TargetDatabase.DataSource.GetIdentityInsertOn(workspace.TargetTable));
						sbInsert.AppendLine(string.Format("insert into {0} ({1}) values ({2})",
							workspace.TargetTable.GetObjectNameWithSchema(workspace.TargetDatabase.DataSource),
							string.Join(", ", workspace.SourceTable.Columns.Select(c => c.GetQueryObjectName(workspace.TargetDatabase.DataSource))),
							string.Join(", ", workspace.SourceTable.Columns.Select(c => string.Format("@{0}", c.ColumnName)))
							));

						sbInsert.AppendLine(workspace.TargetDatabase.DataSource.GetIdentityInsertOff(workspace.TargetTable));

						var firstIn = true;
						foreach (var col in workspace.SourceTable.Columns)
						{
							if (primaryKeys.Contains(col.ColumnName)) continue;

							sbUpdate.AppendLine((firstIn ? "" : ", ") + string.Format("{1} = @{0}", col.ColumnName, col.GetQueryObjectName(workspace.TargetDatabase.DataSource)));
							firstIn = false;
						}

						string where = string.Format("where " + string.Join(" and ", primaryKeys.Select(pk => workspace.TargetTable.ParentDatabase.DataSource.GetConvertedObjectName(pk) + " = @" + pk)));
						sbUpdate.AppendLine(where);

						foreach (var dr in selectedRows)
						{
							_currentCommand.Parameters.Clear();

							var toPrimKeyVal = dr[TO_PREFIX + primaryKeys[0]];
							var fromPrimKeyVal = dr[FROM_PREFIX + primaryKeys[0]];
							if (toPrimKeyVal == DBNull.Value)
							{
								_currentCommand.CommandText = sbInsert.ToString();
								foreach (var col in workspace.SourceTable.Columns)
								{
									_currentCommand.AddParameterWithValue("@" + col.ColumnName, dr[FROM_PREFIX + col.ColumnName]);
								}

							}
							else if (fromPrimKeyVal == DBNull.Value)
							{
								foreach (var pk in primaryKeys)
								{
									_currentCommand.AddParameterWithValue("@" + pk, dr[TO_PREFIX + pk]);
								}

								_currentCommand.CommandText = string.Format("delete from {0} {2}",
									workspace.TargetTable.GetObjectNameWithSchema(workspace.TargetDatabase.DataSource),
									where);
							}
							else
							{
								_currentCommand.CommandText = sbUpdate.ToString();
								foreach (var col in workspace.SourceTable.Columns)
								{
									_currentCommand.AddParameterWithValue("@" + col.ColumnName, dr[FROM_PREFIX + col.ColumnName]);
								}
							}

							_currentCommand.ExecuteNonQuery();
						}

						trans.Commit();
					}
					catch
					{
						trans.Rollback();
						throw;
					}
				}
				conn.Close();
			}
		}

		public void Cancel()
		{
			_cancel = true;
			if (_currentCommand != null)
				_currentCommand.Cancel();

			if (_currentReader != null)
				_currentReader.Close();
		}

		public DataDifference GetDataDifferences(TableWorkspace workspace, BackgroundWorker bw)
		{
			workspace.ComparedData = null;
			CompareData(workspace, bw, false);
			var dt = workspace.ComparedData;
			if (dt == null) return null;
			var diff = new DataDifference();
			diff.TableWorkspace = workspace;
			diff.TargetOnly = dt.Rows.OfType<DataRow>().Count(dr => dr[DataHelper.FROM_PREFIX + dt.PrimaryKeyFields[0]].Equals(DBNull.Value));
			diff.SourceOnly = dt.Rows.OfType<DataRow>().Count(dr => dr[DataHelper.TO_PREFIX + dt.PrimaryKeyFields[0]].Equals(DBNull.Value));
			foreach (var dr in dt.Rows.OfType<DataRow>())
			{
				foreach (var col in workspace.SourceTable.Columns)
				{
					if (dr[DataHelper.FROM_PREFIX + dt.PrimaryKeyFields[0]].Equals(DBNull.Value)) continue;
					if (dr[DataHelper.TO_PREFIX + dt.PrimaryKeyFields[0]].Equals(DBNull.Value)) continue;
					if (!dr[DataHelper.FROM_PREFIX + col.ColumnName].Equals(dr[DataHelper.TO_PREFIX + col.ColumnName]))
					{
						diff.Differences++;
						break;
					}
				}
			}
			return diff;
		}
	}
}
