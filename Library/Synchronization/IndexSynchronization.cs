using PaJaMa.Common;
using PaJaMa.Database.Library.DatabaseObjects;
using PaJaMa.Database.Library.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PaJaMa.Database.Library.Synchronization
{
	public class IndexSynchronization : DatabaseObjectSynchronizationBase<Index>
	{
		public IndexSynchronization(DatabaseObjects.Database targetDatabase, Index index)
			: base(targetDatabase, index)
		{
		}

		public override List<SynchronizationItem> GetDropItems()
		{
			return getStandardDropItems(string.Format("DROP INDEX {0}.{1}",
				databaseObject.Table.GetObjectNameWithSchema(targetDatabase.DataSource),
				databaseObject.GetQueryObjectName(targetDatabase.DataSource)));
		}

		public override List<SynchronizationItem> GetCreateItems()
		{
			return getStandardItems(GetCreateScript(false).ToString());
		}

		public StringBuilder GetCreateScript(bool hasTarget)
		{
			var indexCols = databaseObject.IndexColumns.Where(i => i.Ordinal != 0);
			var includeCols = databaseObject.IndexColumns.Where(i => i.Ordinal == 0);
			var sb = new StringBuilder();

			sb.AppendLineFormat(@"CREATE {0} {1} INDEX {2} ON {3}
(
	{4}
){5};", 
(bool)databaseObject.IsUnique ? "UNIQUE" : "",
targetDatabase.DataSource.BypassClusteredNonClustered ? string.Empty : databaseObject.IndexType,
targetDatabase.DataSource.GetConvertedObjectName(databaseObject.IndexName),
databaseObject.Table.GetObjectNameWithSchema(targetDatabase.DataSource),
string.Join(",\r\n\t",
indexCols.OrderBy(c => c.Ordinal).Select(c =>
	string.Format("{0} {1}", targetDatabase.DataSource.GetConvertedObjectName(c.ColumnName), c.Descending ? "DESC" : "ASC")).ToArray()),
	!includeCols.Any() ? string.Empty : string.Format(@"
INCLUDE (
	{0}
)", string.Join(",\r\n\t",
includeCols.Select(c =>
	string.Format("{0}", targetDatabase.DataSource.GetConvertedObjectName(c.ColumnName)).ToString()))
	));

			return sb;
		}
	}
}
