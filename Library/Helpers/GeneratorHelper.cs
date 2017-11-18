using PaJaMa.Common;
using PaJaMa.Database.DataGenerate.Content;
using PaJaMa.Database.Library.DatabaseObjects;
using PaJaMa.Database.Library.Workspaces.Generate;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaJaMa.Database.Library.Helpers
{
	public class GeneratorHelper
	{
		public event DialogEventHandler Prompt;

		public DatabaseObjects.DataSource DataSource { get; set; }
		public GeneratorHelper(Type dataSourceType, string connectionString, BackgroundWorker worker)
		{
			DataSource = Activator.CreateInstance(dataSourceType, new object[] { connectionString }) as DatabaseObjects.DataSource;
            DataSource.CurrentDatabase.PopulateChildren(false, false, worker);
		}

		public Dictionary<Table, List<Table>> GetMissingDependencies(List<TableWorkspace> selectedWorkspaces)
		{
			var missing = new Dictionary<Table, List<Table>>();
			populateMissingDependencies(selectedWorkspaces, missing);
			return missing;
		}

		public List<Table> GetMissingDependencies(Table sourceTable, List<Table> selectedTables)
		{
			List<Table> missing = new List<Table>();
			foreach (var fk in sourceTable.ForeignKeys)
			{
				var tbl = selectedTables.FirstOrDefault(t => t.TableName == fk.ParentTable.TableName);
				if (tbl != null)
					continue;

				missing.Add(fk.ParentTable);
			}

			return missing;
		}


		private void populateMissingDependencies(List<TableWorkspace> selectedWorkspaces, Dictionary<Table, List<Table>> missing)
		{
			foreach (var ws in selectedWorkspaces)
			{
				recursivelyPopulateMissingDependencies(ws.Table, selectedWorkspaces, missing, new List<Table>());
			}
		}

		private void recursivelyPopulateMissingDependencies(Table parent, List<TableWorkspace> selectedWorkspaces, Dictionary<Table, List<Table>> missing,
			List<Table> checkedObjects)
		{
			if (checkedObjects.Contains(parent)) return;

			checkedObjects.Add(parent);

			var missingDendencies = GetMissingDependencies(parent, selectedWorkspaces.Select(ws => ws.Table).ToList());
			foreach (var child in missingDendencies)
			{
				if (checkedObjects.Contains(child))
					continue;

				checkedObjects.Add(child);

				if (!missing.ContainsKey(parent))
					missing.Add(parent, new List<Table>());

				if (!missing[parent].Contains(child))
					missing[parent].Add(child);

				recursivelyPopulateMissingDependencies(child, selectedWorkspaces, missing, checkedObjects);
			}
		}

		public bool Generate(BackgroundWorker worker, List<TableWorkspace> workspaces)
		{
			using (var conn = new SqlConnection(DataSource.ConnectionString))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					int totalCount = workspaces.Select(w => w.AddRowCount).Sum();
					int curr = 0;

					var keys = workspaces.Where(t => t.RemoveAddKeys).ToList();
					var truncDelete = workspaces.Where(t => t.Truncate || t.Delete).ToList();

					using (var cmd = conn.CreateCommand())
					{
						cmd.Transaction = trans;
						foreach (var table in keys)
						{
							worker.ReportProgress(100 * curr / keys.Count, "Removing foreign keys for " + table.Table.TableName);
							table.Table.RemoveForeignKeys(cmd);
							curr++;
						}

						curr = 0;
						foreach (var table in truncDelete)
						{
							worker.ReportProgress(100 * curr / truncDelete.Count, "Truncating/Deleting " + table.Table.TableName);
							table.Table.TruncateDelete(table.Table.Database, cmd, table.Truncate);
							curr++;
						}
					}

					curr = 0;
					var ds = new DataSet();
					foreach (var ws in workspaces.Where(x => x.AddRowCount > 0))
					{
						try
						{
							List<ColumnWorkspace> addedCols = new List<ColumnWorkspace>();
							foreach (var col in ws.ColumnWorkspaces)
							{
								if (col.Content != null && !(col.Content is NoContent))
								{
									addedCols.Add(col);
								}
							}


							if (worker.CancellationPending)
							{
								trans.Rollback();
								return false;
							}

							var da = new SqlDataAdapter(string.Format("select {2} from [{0}].[{1}] where 1 = 2", ws.Table.Schema.SchemaName, ws.Table.TableName,
								string.Join(", ", addedCols.Select(c => string.Format("[{0}]", c.Column.ColumnName)).ToArray())),
								conn);
							var dt = new DataTable();
							da.SelectCommand.Transaction = trans;
							da.Fill(dt);

							string lbl = "Inserting into " + ws.Table.ToString();

							for (int i = 0; i < ws.AddRowCount; i++)
							{
								worker.ReportProgress(100 * curr++ / totalCount, lbl);
								var dr = dt.NewRow();
								foreach (var col in addedCols)
								{
									var colObj = col.Content.GetContent(trans);
									dr[col.Column.ColumnName] = colObj == null ? DBNull.Value : colObj;
								}
								dt.Rows.Add(dr);

								if (worker.CancellationPending)
								{
									trans.Rollback();
									return false;
								}
							}

							da.InsertCommand = new SqlCommand(string.Format("insert into [{0}].[{1}] ({2}) values ({3})",
								ws.Table.Schema.SchemaName, ws.Table.TableName,
								string.Join(", ", addedCols.Select(c => string.Format("[{0}]", c.Column.ColumnName)).ToArray()),
								string.Join(", ", addedCols.Select(c => string.Format("@{0}", c.Column.ColumnName)).ToArray())), conn);
							da.InsertCommand.Transaction = trans;

							foreach (var col in addedCols)
							{
								da.InsertCommand.Parameters.Add("@" + col.Column.ColumnName, col.Content.DbType, col.Column.CharacterMaximumLength.GetValueOrDefault(), col.Column.ColumnName);
							}

							da.Update(dt);
						}
						catch (Exception ex)
						{
							var args = new DialogEventArgs("Failed to generate data for " + ws.Table.TableName + ": " + ex.Message + ". Continue?");
							Prompt(this, args);
							if (args.Result == YesYesToAllNo.No)
							{
								trans.Rollback();
								return false;
							}
						}
					}

					curr = 0;
					using (var cmd = conn.CreateCommand())
					{
						cmd.Transaction = trans;
						foreach (var table in keys)
						{
							curr++;
							worker.ReportProgress(100 * curr / keys.Count, "Adding foreign keys for " + table.Table.TableName);
							table.Table.AddForeignKeys(cmd);
						}
					}

					trans.Commit();
				}
				conn.Close();
				SqlConnection.ClearPool(conn);
			}
			return true;
		}
	}
}
