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
				DatabaseObject.Table.GetObjectNameWithSchema(TargetDatabase.DataSource),
				DatabaseObject.GetQueryObjectName(TargetDatabase.DataSource)));
		}

		public override List<SynchronizationItem> GetCreateItems()
		{
			return getStandardItems(TargetDatabase.DataSource.GetIndexCreateScript(DatabaseObject));
		}
	}
}
