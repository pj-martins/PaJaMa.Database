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
			var script = TargetDatabase.DataSource.GetForeignKeyCreateScript(DatabaseObject);
			return string.IsNullOrEmpty(script) ? new List<SynchronizationItem>() : getStandardItems(script, 7);
		}

		public override List<SynchronizationItem> GetDropItems(DatabaseObjectBase sourceParent)
		{
            var script = TargetDatabase.DataSource.GetForeignKeyDropScript(DatabaseObject);
            return getStandardDropItems(string.Format(script, 
                DatabaseObject.ChildTable.GetObjectNameWithSchema(TargetDatabase.DataSource), DatabaseObject.GetQueryObjectName(TargetDatabase.DataSource)),
			sourceParent);
		}

		public override List<SynchronizationItem> GetAlterItems(DatabaseObjectBase target, bool ignoreCase, bool condensed)
		{
			var diffs = GetPropertyDifferences(target, ignoreCase);
			var diff = getColumnDifference(target);
			if (diff != null)
				diffs.Add(diff);

			if (diffs.Any())
			{
				var syncItem = new SynchronizationItem(DatabaseObject);
				syncItem.Differences.AddRange(diffs);
				syncItem.AddScript(0, GetRawDropText());
				syncItem.AddScript(100, GetRawCreateText());
				return new List<SynchronizationItem>() { syncItem };
			}

			return new List<SynchronizationItem>();
		}

		protected override Difference getDifference(DifferenceType differenceType, DatabaseObjectBase fromObject, DatabaseObjectBase toObject = null, string propertyName = null, string sourceValue = null, string targetValue = null)
		{
			if (propertyName != null && propertyName == "ForeignKeyName" && differenceType == DifferenceType.Alter && !fromObject.Database.DataSource.NamedConstraints)
			{
				return null;
			}
			return base.getDifference(differenceType, fromObject, toObject, propertyName, sourceValue, targetValue);
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
