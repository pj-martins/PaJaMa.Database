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
	public class ViewSynchronization : DatabaseObjectSynchronizationBase<View>
	{
		public ViewSynchronization(DatabaseObjects.Database targetDatabase, View view)
			: base(targetDatabase, view)
		{
		}

		public override List<SynchronizationItem> GetAlterItems(DatabaseObjectBase target, bool ignoreCase)
		{
			var items = new List<SynchronizationItem>();
			var diffs = GetPropertyDifferences(target, ignoreCase);
			if (diffs.Any())
			{
				var createAlter = DatabaseObject.Definition;
				createAlter = Regex.Replace(createAlter, "CREATE VIEW", "ALTER VIEW", RegexOptions.IgnoreCase);
				items.AddRange(getStandardItems(createAlter, difference: getDifference(DifferenceType.Alter, DatabaseObject, target)));
			}

			return items;
		}

		public override List<SynchronizationItem> GetCreateItems()
		{
			return getStandardItems(DatabaseObject.Definition);
		}

		public override string ToString()
		{
			return DatabaseObject.Schema.SchemaName + "." + DatabaseObject.ViewName;
		}

		public override List<SynchronizationItem> GetDropItems()
		{
			return getStandardDropItems(string.Format("DROP {0} [{1}].[{2}]", DatabaseObject.ObjectType.ToString().ToUpper(),
				DatabaseObject.Schema.SchemaName, DatabaseObject.ObjectName));
		}
	}
}
