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
    databaseObject.ChildTable.GetObjectNameWithSchema(targetDatabase.DataSource),
    databaseObject.GetQueryObjectName(targetDatabase.DataSource),
    string.Join(",", databaseObject.Columns.Select(c => c.ChildColumn.GetQueryObjectName(targetDatabase.DataSource)).ToArray()),
    databaseObject.ParentTable.GetObjectNameWithSchema(targetDatabase.DataSource),
    string.Join(",", databaseObject.Columns.Select(c => c.ParentColumn.GetQueryObjectName(targetDatabase.DataSource)).ToArray()),
    databaseObject.DeleteRule,
    databaseObject.UpdateRule,
    databaseObject.Database.DataSource.CheckForeignKeys ? databaseObject.WithCheck : string.Empty,
    databaseObject.Database.DataSource.CheckForeignKeys ? " WITH" : string.Empty,
    databaseObject.Database.DataSource.CheckForeignKeys ? "CHECK" : string.Empty
    );
            if (databaseObject.Database.DataSource.CheckForeignKeys)
                createString += string.Format(@"
ALTER TABLE {0}
CHECK CONSTRAINT {1}
", databaseObject.ChildTable.GetObjectNameWithSchema(targetDatabase.DataSource),
    databaseObject.GetQueryObjectName(targetDatabase.DataSource));
            return getStandardItems(createString, 7);
        }

        public override List<SynchronizationItem> GetDropItems()
        {
            return getStandardDropItems(string.Format(@"
ALTER TABLE {0} DROP CONSTRAINT {1};
", databaseObject.ChildTable.GetObjectNameWithSchema(targetDatabase.DataSource), databaseObject.GetQueryObjectName(targetDatabase.DataSource)));
        }

        public override List<SynchronizationItem> GetAlterItems(DatabaseObjectBase target, bool ignoreCase)
        {
            var diffs = GetPropertyDifferences(target, ignoreCase);
			if (targetDatabase.DataSource.BypassForeignKeyRules || databaseObject.Database.DataSource.BypassForeignKeyRules)
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
                var syncItem = new SynchronizationItem(databaseObject);
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
            if (targetFk.Columns.Any(f => !databaseObject.Columns.Any(c => c.ParentColumn.ColumnName == f.ParentColumn.ColumnName
                && c.ChildColumn.ColumnName == f.ChildColumn.ColumnName))
                || databaseObject.Columns.Any(f => !targetFk.Columns.Any(c => c.ParentColumn.ColumnName == f.ParentColumn.ColumnName
                && c.ChildColumn.ColumnName == f.ChildColumn.ColumnName)))
                return new Difference()
                {
                    PropertyName = "Columns",
                    SourceValue = string.Join("\r\n", databaseObject.Columns.Select(c => c.ParentColumn.ColumnName + " - " + c.ChildColumn.ColumnName).ToArray()),
                    TargetValue = string.Join("\r\n", targetFk.Columns.Select(c => c.ParentColumn.ColumnName + " - " + c.ChildColumn.ColumnName).ToArray()),
                };
            return null;
        }
    }
}
