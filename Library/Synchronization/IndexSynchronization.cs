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

		public override List<SynchronizationItem> GetDropItems(DatabaseObjectBase sourceParent)
		{
			return getStandardDropItems(TargetDatabase.DataSource.GetIndexDropScript(DatabaseObject), sourceParent);
		}

		public override List<SynchronizationItem> GetCreateItems()
		{
			return getStandardItems(TargetDatabase.DataSource.GetIndexCreateScript(DatabaseObject));
		}
	}
}
