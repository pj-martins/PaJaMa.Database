using PaJaMa.Database.Library.Workspaces.Search;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace PaJaMa.Database.Library.Helpers
{
	public class SearchHelper
	{
		public DatabaseObjects.DataSource DataSource { get; set; }
		public SearchHelper(Type dataSourceType, string connectionString, BackgroundWorker worker)
		{
			DataSource = Activator.CreateInstance(dataSourceType, new object[] { connectionString }) as DatabaseObjects.DataSource;
			DataSource.CurrentDatabase.PopulateChildren(true, false, worker);
		}

		//public void Init(BackgroundWorker worker)
		//{
		//	Database = new DatabaseObjects.Database(Database.ConnectionType, Database.ConnectionString);
		//	Database.PopulateChildren(true, worker);
		//}

		public void Search(string searchFor, List<ColumnWorkspace> columns, BindingList<DataTable> outputTables)
		{
			if (string.IsNullOrEmpty(searchFor)) return;
			if (!columns.Any()) return;

			var tblsToSearch = from c in columns
							   group c by c.Column.Table into g
							   select g;

			using (var conn = DataSource.OpenConnection())
			{
				using (var cmd = conn.CreateCommand())
				{
					foreach (var tbl in tblsToSearch)
					{
						cmd.Parameters.Clear();

						var dt = new DataTable(tbl.Key.TableName);
						var sb = new StringBuilder(string.Format("select * from [{0}].[{1}]", tbl.Key.Schema.SchemaName, tbl.Key.TableName));
						var firstIn = true;
						foreach (var col in tbl)
						{
							sb.AppendLine(firstIn ? "where " : "and ");
							sb.AppendLine(string.Format("[{0}] = @{0}", col.Column.ColumnName));
                            // TODO:
							(cmd as System.Data.SqlClient.SqlCommand).Parameters.AddWithValue(string.Format("@{0}", col.Column.ColumnName), searchFor);

							firstIn = false;
						}
						cmd.CommandText = sb.ToString();
						using (var rdr = cmd.ExecuteReader())
						{
							dt.Load(rdr);
						}
						outputTables.Add(dt);
					}
				}
				conn.Close();
			}
		}
	}
}
