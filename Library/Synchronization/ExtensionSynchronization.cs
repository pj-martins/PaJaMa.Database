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
	public class ExtensionSynchronization : DatabaseObjectSynchronizationBase<Extension>
	{
		public ExtensionSynchronization(DatabaseObjects.Database targetDatabase, Extension extension)
			: base(targetDatabase, extension)
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
            item.AddScript(0, string.Format("CREATE EXTENSION IF NOT EXISTS \"{0}\"", databaseObject.Name));
            return items;
        }

		public override string ToString()
		{
			return databaseObject.Database.DatabaseName + "." + databaseObject.Name;
		}

		public override List<SynchronizationItem> GetDropItems()
		{
            var items = new List<SynchronizationItem>();
            var item = new SynchronizationItem(databaseObject);
            items.Add(item);
            item.Differences.Add(new Difference() { PropertyName = Difference.CREATE });
            item.AddScript(0, string.Format("DROP EXTENSION \"{0}\"", databaseObject.Name));
            return items;
        }
	}
}
