using PaJaMa.Database.Library.DataSources;
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
		public DataSource DataSource { get; set; }
		public SearchHelper(DataSource dataSource, BackgroundWorker worker)
		{
			DataSource = dataSource;
			DataSource.PopulateChildren(null, true, worker);
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
							   group c by c.Column.Parent into g
							   select g;

			using (var conn = DataSource.OpenConnection())
			{
				using (var cmd = conn.CreateCommand())
				{
					foreach (var tbl in tblsToSearch)
					{
						cmd.Parameters.Clear();

						var dt = new DataTable(tbl.Key.ObjectName);
						var sb = new StringBuilder(string.Format("select * from {0}", tbl.Key.GetObjectNameWithSchema(DataSource)));
						var firstIn = true;
						foreach (var col in tbl)
						{
							sb.AppendLine(firstIn ? "where " : "or ");
							var param = cmd.CreateParameter();
							param.DbType = col.Column.ColumnType.DbType;
							param.ParameterName = string.Format("@{0}", col.Column.ColumnName);
							param.Value = searchFor;
							if (param.DbType == DbType.String)
								sb.AppendLine(string.Format("{0} like @{1}", DataSource.GetConvertedObjectName(col.Column.ColumnName), col.Column.ColumnName));
							else
								sb.AppendLine(string.Format("{0} = @{1}", DataSource.GetConvertedObjectName(col.Column.ColumnName), col.Column.ColumnName));
							cmd.Parameters.Add(param);
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
