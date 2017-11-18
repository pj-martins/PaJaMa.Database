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
            var item = new SynchronizationItem(databaseObject);
            items.Add(item);
            item.Differences.Add(new Difference() { PropertyName = Difference.CREATE });

			// TODO: cache
			item.AddScript(0, string.Format(@"CREATE SEQUENCE IF NOT EXISTS {0}
                INCREMENT {1}
                MINVALUE {2}
                MAXVALUE {3}
                START {4}
                CACHE 1
                {5} CYCLE",
				databaseObject.GetObjectNameWithSchema(targetDatabase.DataSource),
                databaseObject.Increment,
                databaseObject.MinValue,
                databaseObject.MaxValue,
                databaseObject.Start,
                databaseObject.Cycle
                ));
            return items;
        }

		public override string ToString()
		{
			return databaseObject.Schema.SchemaName + "." + databaseObject.SequenceName;
		}

		public override List<SynchronizationItem> GetDropItems()
		{
            var items = new List<SynchronizationItem>();
            var item = new SynchronizationItem(databaseObject);
            items.Add(item);
            item.Differences.Add(new Difference() { PropertyName = Difference.CREATE });
            item.AddScript(0, string.Format("DROP SEQUENCE {0}.\"{1}\"", databaseObject.Schema.SchemaName,
                databaseObject.SequenceName));
            return items;
        }
	}
}
