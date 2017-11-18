﻿using PaJaMa.Common;
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
				var createAlter = databaseObject.Definition;
				createAlter = Regex.Replace(createAlter, "CREATE VIEW", "ALTER VIEW", RegexOptions.IgnoreCase);
				items.AddRange(getStandardItems(createAlter, propertyName: Difference.ALTER));
			}

			return items;
		}

		public override List<SynchronizationItem> GetCreateItems()
		{
			return getStandardItems(databaseObject.Definition);
		}

		public override string ToString()
		{
			return databaseObject.Schema.SchemaName + "." + databaseObject.ViewName;
		}

		public override List<SynchronizationItem> GetDropItems()
		{
			return getStandardDropItems(string.Format("DROP {0} [{1}].[{2}]", databaseObject.ObjectType.ToString().ToUpper(),
				databaseObject.Schema.SchemaName, databaseObject.ObjectName));
		}
	}
}
