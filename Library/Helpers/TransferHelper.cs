using PaJaMa.Database.Library.Workspaces.Compare;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PaJaMa.Database.Library.Helpers
{
	public class TransferHelper
	{
		private BackgroundWorker _worker;

		public TransferHelper(BackgroundWorker worker)
		{
			_worker = worker;
		}

		public bool Transfer(List<TableWorkspace> workspaces, DbTransaction trans, DatabaseObjects.Database fromDatabase)
		{
			string tableName = string.Empty;
			using (var cmd = trans.Connection.CreateCommand())
			{
				cmd.Transaction = trans;

				var sort = !workspaces.All(ws => ws.RemoveAddKeys);

				int i = 0;
				var selected = workspaces.Where(t => t.SelectTableForData).ToList();
				var datas = sort ? getSortedWorkspaces(selected) : selected;
				var counts = datas.Count();
				foreach (var table in datas)
				{
					i++;
					tableName = table.TargetTable.ToString();

					if (table.RemoveAddIndexes)
					{
						_worker.ReportProgress(100 * i / counts, "Removing indexes for " + tableName);
						table.TargetTable.RemoveIndexes(cmd);
					}
				}

				i = 0;

				using (var conn = fromDatabase.OpenConnection())
				{
					using (var cmdSrc = conn.CreateCommand())
					{
						foreach (var table in datas)
						{
							i++;

							_worker.ReportProgress(100 * i / counts, string.Format("Copying: {0}",
											table.SourceTable.GetObjectNameWithSchema(table.TargetDatabase.DataSource)));

							int rowCount = 0;
							cmdSrc.CommandText = string.Format("select count(*) from {0}", table.SourceTable.GetObjectNameWithSchema(table.SourceTable.Database.DataSource));
							rowCount = Convert.ToInt32(cmdSrc.ExecuteScalar());

							cmdSrc.CommandText = string.Format("select * from {0}", table.SourceTable.GetObjectNameWithSchema(table.SourceTable.Database.DataSource));
							using (var rdr = cmdSrc.ExecuteReader())
							{
								if (trans.Connection is SqlConnection)
								{
									using (var copy = new SqlBulkCopy((SqlConnection)trans.Connection,
										(table.KeepIdentity ? SqlBulkCopyOptions.KeepIdentity : SqlBulkCopyOptions.Default) | SqlBulkCopyOptions.CheckConstraints,
										(SqlTransaction)trans))
									{
										foreach (var col in table.SourceTable.Columns)
										{
											if (!string.IsNullOrEmpty(col.Formula))
												continue;

											if (!table.TargetTable.Columns.Any(c => c.ColumnName == col.ColumnName))
												continue;

											copy.ColumnMappings.Add(col.ObjectName, col.ObjectName);
										}

										copy.BulkCopyTimeout = 600;
										copy.BatchSize = table.TransferBatchSize == 0 ? TableWorkspace.DEFAULT_BATCH_SIZE : table.TransferBatchSize;
										copy.NotifyAfter = 500;
										copy.SqlRowsCopied += delegate (object sender, SqlRowsCopiedEventArgs e)
										{
											if (_worker.CancellationPending)
											{
												e.Abort = true;
												return;
											}
											_worker.ReportProgress(100 * i / counts, string.Format("Copying: [{0}].[{1}] {2} of {3}",
												table.SourceTable.Schema.SchemaName, table.SourceTable.TableName, e.RowsCopied, rowCount));
										};
										copy.DestinationTableName = string.Format("[{0}].[{1}]", table.TargetTable.Schema.SchemaName, table.TargetTable.TableName);
										copy.WriteToServer(rdr);
									}
								}
								else
								{
									if (rdr.HasRows)
									{
										List<string> columns = new List<string>();
										for (int j = 0; j < rdr.FieldCount; j++)
										{
											columns.Add(rdr.GetName(j));
										}
										var batchSize = table.TransferBatchSize == 0 ? TableWorkspace.DEFAULT_BATCH_SIZE : table.TransferBatchSize;
										var insertQry = $@"insert into {table.TargetTable.GetObjectNameWithSchema(table.TargetDatabase.DataSource)} 
            ({string.Join(", ", columns.Select(dc => table.TargetObject.Database.DataSource.GetConvertedObjectName(dc)).ToArray())}) values ";
										cmd.CommandText = insertQry;
										bool firstIn = true;
										int counter = 0;
										int rowsCopied = 0;
										while (rdr.Read())
										{
											rowsCopied++;
											if (_worker.CancellationPending)
												return false;
											_worker.ReportProgress(100 * i / counts, string.Format("Copying: {0} {1} of {2}",
												table.SourceTable.GetObjectNameWithSchema(table.TargetDatabase.DataSource), rowsCopied, rowCount));

											cmd.CommandText += (firstIn ? "" : ",\r\n") + "(" + string.Join(", ",
												columns.Select(dc => getReaderValue(rdr, dc) == DBNull.Value ? "NULL" : "'" + getReaderValue(rdr, dc).ToString().Replace("'", "''") + "'").ToArray()) + ")";
											counter++;
											firstIn = false;
											if (counter >= batchSize)
											{
												cmd.ExecuteNonQuery();
												cmd.CommandText = insertQry;
												counter = 0;
												firstIn = true;
											}
										}

										if (!firstIn)
											cmd.ExecuteNonQuery();
									}
								}
							}
						}
					}

					conn.Close();
				}

				i = 0;
				foreach (var table in datas)
				{
					i++;
					tableName = table.TargetTable.TableName;

					if (table.RemoveAddIndexes)
					{
						_worker.ReportProgress(100 * i / datas.Count(), "Adding indexes for " + tableName);
						table.TargetTable.AddIndexes(cmd);
					}
				}
			}

			return true;
		}


		private object getReaderValue(DbDataReader rdr, string columnName)
		{
			try
			{
				var value = rdr[columnName];
				if (value is Array)
				{
					// only postgres I think
					return "{" + string.Join(", ", ((Array)value).OfType<object>()) + "}";
				}
				return value;
			}
			catch (Exception ex)
			{
				// TODO: log
				for (int i = 0; i < rdr.FieldCount; i++)
				{
					if (rdr.GetName(i) == columnName)
						return Activator.CreateInstance(rdr.GetFieldType(i));
				}

				// shouldn't get here
				return DBNull.Value;
			}
		}

		private List<TableWorkspace> getSortedWorkspaces(List<TableWorkspace> workspaces)
		{
			List<TableWorkspace> sortedWorkspaces = new List<TableWorkspace>();
			List<TableWorkspace> currentWorkspaces = workspaces.ToList();
			List<DatabaseObjects.ForeignKey> circularKeys = new List<DatabaseObjects.ForeignKey>();
			bool isInInfinite = false;
			while (currentWorkspaces.Count > 0)
			{
				int currentCount = currentWorkspaces.Count;
				foreach (var ws in currentWorkspaces)
				{
					bool goodToGo = true;
					foreach (var fk in ws.TargetTable.ForeignKeys)
					{
						if (currentWorkspaces.Any(w => w.TargetTable.TableName == fk.ParentTable.TableName))
						{
							if (isInInfinite && fk.Columns.All(ck => ck.ChildColumn.IsNullable))
							{
								circularKeys.Add(fk);
								break;
							}

							goodToGo = false;
							break;
						}
					}
					if (goodToGo)
					{
						sortedWorkspaces.Add(ws);
						currentWorkspaces.Remove(ws);
						break;
					}
				}
				isInInfinite = currentCount == currentWorkspaces.Count;
			}

			return sortedWorkspaces;
		}
	}
}
