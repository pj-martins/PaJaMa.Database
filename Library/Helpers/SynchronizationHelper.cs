using PaJaMa.Common;
using PaJaMa.Database.Library.Workspaces.Compare;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaJaMa.Database.Library.Helpers
{
	public class SynchronizationHelper
	{
		public event PromptEventHandler DisplayMessage;

		public bool Synchronize(CompareHelper compareHelper, List<WorkspaceBase> workspaces, List<TableWorkspace> dataSpaces, 
			List<TableWorkspace> truncDelete, BackgroundWorker worker)
		{
			bool success = true;
			TransferHelper transferHelper = null;
			if (dataSpaces.Any())
				transferHelper = new TransferHelper(compareHelper.ToDataSource, worker);
            using (var conn = compareHelper.ToDataSource.OpenConnection(string.Empty))
 			{
				using (var trans = conn.BeginTransaction())
				{
					int i = 0;
					
					try
					{
						//if (!truncDelete.All(x => x.SelectTableForData && !x.Select))
						{
							using (var cmd = conn.CreateCommand())
							{
								cmd.Transaction = trans;

								foreach (var table in truncDelete)
								{
									table.TargetTable.ResetForeignKeys();
								}

								i = 0;
								var sbAll = new StringBuilder();
								foreach (var table in truncDelete)
								{
									if (table.RemoveAddKeys)
									{
										worker.ReportProgress(100 * i / truncDelete.Count, "Removing foreign keys for " + table.TargetTable.TableName);
										sbAll.AppendLine(table.TargetTable.RemoveForeignKeys(cmd));
									}
								}

								i = 0;
								foreach (var table in truncDelete)
								{
									worker.ReportProgress(100 * i / truncDelete.Count, "Truncating/Deleting " + table.TargetTable.TableName);
									sbAll.AppendLine(table.TargetTable.TruncateDelete(table.TargetDatabase, cmd, table.Truncate));
									i++;
								}

								i = 0;
								foreach (var table in truncDelete.Where(td => td.Select && !td.SelectTableForData))
								{
									i++;
									if (table.RemoveAddKeys)
									{
										worker.ReportProgress(100 * i / truncDelete.Count, "Adding foreign keys for " + table.TargetTable.TableName);
										sbAll.AppendLine(table.TargetTable.AddForeignKeys(cmd));
									}
								}
							}
						}
					}
					catch (Exception ex)
					{
						trans.Rollback();

						foreach (var table in truncDelete)
						{
							table.TargetTable.ResetForeignKeys();
						}

						DisplayMessage(this, new PromptEventArgs(ex.Message));
						return false;
					}

					if (workspaces.Any())
					{
						try
						{
							compareHelper.Synchronize(worker, workspaces, trans);
						}
						catch (Exception ex)
						{
							DisplayMessage(this, new PromptEventArgs(ex.Message));
							trans.Rollback();
							return false;
						}
					}

					if (dataSpaces.Any())
					{
						try
						{
							success = transferHelper.Transfer(dataSpaces, trans, compareHelper.FromDataSource.CurrentDatabase);
						}
						catch (Exception ex)
						{
							success = false;
							DisplayMessage(this, new PromptEventArgs(ex.Message));
						}
					}

					var adds = truncDelete.Where(td => !td.Select);
					if (adds.Any())
					{
						try
						{
							using (var cmd = conn.CreateCommand())
							{
								cmd.Transaction = trans;

								i = 0;
								var sb = new StringBuilder();
								foreach (var table in adds)
								{
									i++;
									if (table.RemoveAddKeys)
									{
										worker.ReportProgress(100 * i / adds.Count(), "Adding foreign keys for " + table.TargetTable.TableName);
										sb.AppendLine(table.TargetTable.AddForeignKeys(cmd));
									}
								}
							}
						}
						catch (Exception ex)
						{
							trans.Rollback();

							foreach (var table in truncDelete)
							{
								table.TargetTable.ResetForeignKeys();
							}

							DisplayMessage(this, new PromptEventArgs(ex.Message));
							return false;
						}
					}

					if (!success)
					{
						trans.Rollback();
					}
					else
					{
						trans.Commit();
					}
				}
				conn.Close();
			}

			return success;
		}
	}
}
