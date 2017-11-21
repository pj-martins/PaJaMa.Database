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
	public class RoutineSynonymSynchronization : DatabaseObjectSynchronizationBase<RoutineSynonym>
	{
		public RoutineSynonymSynchronization(DatabaseObjects.Database targetDatabase, RoutineSynonym routineSynonym)
			: base(targetDatabase, routineSynonym)
		{
		}

		public override List<SynchronizationItem> GetAlterItems(DatabaseObjectBase target, bool ignoreCase)
		{
			var items = new List<SynchronizationItem>();
			var propDifferences = GetPropertyDifferences(target, ignoreCase);
			if (propDifferences.Any())
			{
				var createAlter = DatabaseObject.Definition;
				if (DatabaseObject.Type == PaJaMa.Database.Library.DatabaseObjects.RoutineSynonym.RoutineSynonymType.Synonym)
				{
					createAlter = createAlter.Insert(0, new RoutineSynonymSynchronization(TargetDatabase, target as RoutineSynonym).GetRawDropText() + "\r\n");
				}
				else
				{
					createAlter = Regex.Replace(createAlter, "CREATE PROCEDURE", "ALTER PROCEDURE", RegexOptions.IgnoreCase);
					createAlter = Regex.Replace(createAlter, "CREATE FUNCTION", "ALTER FUNCTION", RegexOptions.IgnoreCase);
					createAlter = Regex.Replace(createAlter, "CREATE VIEW", "ALTER VIEW", RegexOptions.IgnoreCase);
				}
				var diff = getDifference(DifferenceType.Alter, DatabaseObject, target);
				if (diff != null)
					items.AddRange(getStandardItems(createAlter, difference: diff));
			}

			return items;
		}

		public override List<SynchronizationItem> GetCreateItems()
		{
			return getStandardItems(DatabaseObject.Definition);
		}

		public override string ToString()
		{
			return DatabaseObject.Schema.SchemaName + "." + DatabaseObject.Name;
		}

		public override List<SynchronizationItem> GetDropItems(DatabaseObjectBase sourceParent)
		{
			return getStandardDropItems(string.Format("DROP {0} [{1}].[{2}]", DatabaseObject.ObjectType.ToString().ToUpper(),
				DatabaseObject.Schema.SchemaName, DatabaseObject.ObjectName), sourceParent);
		}
	}
}
