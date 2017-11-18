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
	public class ForeignKeySynchronization : DatabaseObjectSynchronizationBase<ForeignKey>
	{
		public ForeignKeySynchronization(DatabaseObjects.Database targetDatabase, ForeignKey foreignKey)
			: base(targetDatabase, foreignKey)
		{
		}

		public override List<SynchronizationItem> GetCreateItems()
		{
			var createString = string.Format(@"
ALTER TABLE {0} {8}{7}{9} ADD CONSTRAINT {1} FOREIGN KEY({2})
REFERENCES {3} ({4})
ON DELETE {5}
ON UPDATE {6}
;
",
	DatabaseObject.ChildTable.GetObjectNameWithSchema(TargetDatabase.DataSource),
	DatabaseObject.GetQueryObjectName(TargetDatabase.DataSource),
	string.Join(",", DatabaseObject.Columns.Select(c => c.ChildColumn.GetQueryObjectName(TargetDatabase.DataSource)).ToArray()),
	DatabaseObject.ParentTable.GetObjectNameWithSchema(TargetDatabase.DataSource),
	string.Join(",", DatabaseObject.Columns.Select(c => c.ParentColumn.GetQueryObjectName(TargetDatabase.DataSource)).ToArray()),
	DatabaseObject.DeleteRule,
	DatabaseObject.UpdateRule,
	TargetDatabase.DataSource.CheckForeignKeys ? DatabaseObject.WithCheck : string.Empty,
	TargetDatabase.DataSource.CheckForeignKeys ? " WITH" : string.Empty,
	TargetDatabase.DataSource.CheckForeignKeys ? " CHECK" : string.Empty
	);
			if (TargetDatabase.DataSource.CheckForeignKeys)
				createString += string.Format(@"
ALTER TABLE {0}
CHECK CONSTRAINT {1}
", DatabaseObject.ChildTable.GetObjectNameWithSchema(TargetDatabase.DataSource),
	DatabaseObject.GetQueryObjectName(TargetDatabase.DataSource));
			return getStandardItems(createString, 7);
		}

		public override List<SynchronizationItem> GetDropItems()
		{
			return getStandardDropItems(string.Format(@"
ALTER TABLE {0} DROP CONSTRAINT {1};
", DatabaseObject.ChildTable.GetObjectNameWithSchema(TargetDatabase.DataSource), DatabaseObject.GetQueryObjectName(TargetDatabase.DataSource)));
		}

		public override List<SynchronizationItem> GetAlterItems(DatabaseObjectBase target, bool ignoreCase)
		{
			var diffs = GetPropertyDifferences(target, ignoreCase);
			if (TargetDatabase.DataSource.BypassForeignKeyRules || DatabaseObject.Database.DataSource.BypassForeignKeyRules)
			{
				for (int i = diffs.Count - 1; i >= 0; i--)
				{
					var d = diffs[i];
					if (d.PropertyName == "ForeignKeyName" ||
						d.PropertyName == "WithCheck" ||
						d.PropertyName == "UpdateRule" ||
						d.PropertyName == "DeleteRule"
						)
						diffs.RemoveAt(i);
				}
			}

			var diff = getColumnDifference(target);
			if (diff != null)
				diffs.Add(diff);

			if (diffs.Any())
			{
				var syncItem = new SynchronizationItem(DatabaseObject);
				syncItem.Differences.AddRange(diffs);
				syncItem.AddScript(0, GetRawDropText());
				syncItem.AddScript(1, GetRawCreateText());
				return new List<SynchronizationItem>() { syncItem };
			}

			return new List<SynchronizationItem>();
		}

		private Difference getColumnDifference(DatabaseObjectBase target)
		{
			var targetFk = target as ForeignKey;
			if (targetFk.Columns.Any(f => !DatabaseObject.Columns.Any(c => c.ParentColumn.ColumnName == f.ParentColumn.ColumnName
				&& c.ChildColumn.ColumnName == f.ChildColumn.ColumnName))
				|| DatabaseObject.Columns.Any(f => !targetFk.Columns.Any(c => c.ParentColumn.ColumnName == f.ParentColumn.ColumnName
				&& c.ChildColumn.ColumnName == f.ChildColumn.ColumnName)))
				return getDifference(DifferenceType.Alter, DatabaseObject, target, "Columns",
					string.Join("\r\n", DatabaseObject.Columns.Select(c => c.ParentColumn.ColumnName + " - " + c.ChildColumn.ColumnName).ToArray()),
					string.Join("\r\n", targetFk.Columns.Select(c => c.ParentColumn.ColumnName + " - " + c.ChildColumn.ColumnName).ToArray()));
			return null;
		}
	}
}
