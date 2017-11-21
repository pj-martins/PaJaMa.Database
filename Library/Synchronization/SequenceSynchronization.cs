using PaJaMa.Common;
using PaJaMa.Database.Library.DatabaseObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PaJaMa.Database.Library.Synchronization
{
	public class SequenceSynchronization : DatabaseObjectSynchronizationBase<Sequence>
	{
		public SequenceSynchronization(DatabaseObjects.Database targetDatabase, Sequence sequence)
			: base(targetDatabase, sequence)
		{
		}

		public override List<SynchronizationItem> GetAlterItems(DatabaseObjectBase target, bool ignoreCase)
		{
            return new List<SynchronizationItem>();
		}

		public override List<SynchronizationItem> GetCreateItems()
		{
            var items = new List<SynchronizationItem>();
			var diff = getDifference(DifferenceType.Create, DatabaseObject);
			if (diff != null)
			{
				var item = new SynchronizationItem(DatabaseObject);
				items.Add(item);
				item.Differences.Add(diff);

				// TODO: cache
				item.AddScript(0, string.Format(@"CREATE SEQUENCE IF NOT EXISTS {0}
                INCREMENT {1}
                MINVALUE {2}
                MAXVALUE {3}
                START {4}
                CACHE 1
                {5} CYCLE",
					DatabaseObject.GetObjectNameWithSchema(TargetDatabase.DataSource),
					DatabaseObject.Increment,
					DatabaseObject.MinValue,
					DatabaseObject.MaxValue,
					DatabaseObject.Start,
					DatabaseObject.Cycle
					));
			}
            return items;
        }

		public override string ToString()
		{
			return DatabaseObject.Schema.SchemaName + "." + DatabaseObject.SequenceName;
		}

		public override List<SynchronizationItem> GetDropItems(DatabaseObjectBase sourceParent)
		{
            var items = new List<SynchronizationItem>();
			var diff = getDifference(DifferenceType.Drop, DatabaseObject);
			if (diff != null)
			{
				var item = new SynchronizationItem(DatabaseObject);
				items.Add(item);
				item.Differences.Add(diff);
				item.AddScript(0, string.Format("DROP SEQUENCE {0}.\"{1}\"", DatabaseObject.Schema.SchemaName,
					DatabaseObject.SequenceName));
			}
            return items;
        }
	}
}
