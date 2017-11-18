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
	public class KeyConstraintSynchronization : DatabaseObjectSynchronizationBase<KeyConstraint>
	{
		public KeyConstraintSynchronization(DatabaseObjects.Database targetDatabase, KeyConstraint constraint)
			: base(targetDatabase, constraint)
		{
		}


		public string GetInnerCreateText()
		{
			return string.Format(@"CONSTRAINT {0}
{1}
{2}
({3})", databaseObject.GetQueryObjectName(targetDatabase.DataSource), databaseObject.IsPrimaryKey ? "PRIMARY KEY" : "UNIQUE", targetDatabase.DataSource.BypassClusteredNonClustered ? string.Empty : databaseObject.ClusteredNonClustered, string.Join(", ",
	  databaseObject.Columns.OrderBy(c => c.Ordinal).Select(c => string.Format("{0} {1}", targetDatabase.DataSource.GetConvertedObjectName(c.ColumnName), (targetDatabase.DataSource.BypassKeyConstraints ? string.Empty : (c.Descending ? "DESC" : "ASC"))))));
		}

		public override List<SynchronizationItem> GetCreateItems()
		{
			var items = getStandardItems(string.Format("ALTER TABLE {0} ADD {1};", databaseObject.Table.GetObjectNameWithSchema(targetDatabase.DataSource), GetInnerCreateText()));

			if (targetDatabase.DataSource.BypassKeyConstraints)
			{
				foreach (var col in databaseObject.Columns)
				{
					var tableCol = databaseObject.Table.Columns.FirstOrDefault(c => c.ColumnName == col.ColumnName);
					if (tableCol != null && !string.IsNullOrEmpty(tableCol.ColumnDefault))
					{
						string def = tableCol.ColumnDefault;
						if (!string.IsNullOrEmpty(def) && def.StartsWith("((") && def.EndsWith("))"))
							def = def.Substring(1, def.Length - 2);

						items.AddRange(getStandardItems(string.Format("ALTER TABLE {0} ALTER COLUMN \"{1}\" SET DEFAULT {2};",
							databaseObject.Table.GetObjectNameWithSchema(targetDatabase.DataSource), tableCol.ColumnName, def)));
					}
				}
			}

			return items;
		}

		public override List<SynchronizationItem> GetDropItems()
		{
			return getStandardDropItems(string.Format("ALTER TABLE {0} DROP CONSTRAINT {1};",
				databaseObject.Table.GetObjectNameWithSchema(targetDatabase.DataSource),
				databaseObject.ConstraintName));
		}

		public override List<SynchronizationItem> GetAlterItems(DatabaseObjectBase target, bool ignoreCase)
		{
			var items = base.GetAlterItems(target, ignoreCase);
			if (target != null)
			{
				var targetKey = target as KeyConstraint;
				var childKeys = from t in databaseObject.Table.Schema.Tables
								from fk in t.ForeignKeys
								where fk.ParentTable.TableName == targetKey.Table.TableName
								select fk;
				foreach (var childKey in childKeys)
				{
					var childSync = new ForeignKeySynchronization(targetDatabase, childKey);
					var item = new SynchronizationItem(childKey);
					foreach (var dropItem in childSync.GetDropItems())
					{
						item.Differences.AddRange(dropItem.Differences);
						foreach (var script in dropItem.Scripts)
						{
							item.AddScript(-1, script.Value.ToString());
						}
					}
					foreach (var createItem in childSync.GetCreateItems())
					{
						item.Differences.AddRange(createItem.Differences);
						foreach (var script in createItem.Scripts)
						{
							item.AddScript(100, script.Value.ToString());
						}
					}
					items.Add(item);
				}
			}
			return items;
		}

		//public override List<Difference> GetDifferences(DatabaseObjectBase target)
		//{
		//	var diffs = base.GetDifferences(target);
		//	if (target == null)
		//		return diffs;

		//	var targetConstraint = target as KeyConstraint;
		//	foreach (var col in databaseObject.Columns)
		//	{
		//		var targetCol = targetConstraint.Columns.FirstOrDefault(c => c.ColumnName == col.ColumnName);
		//		if (targetCol == null)
		//			diffs.Add(new Difference() { PropertyName = Difference.CREATE });
		//		else 
		//		{if (targetCol.Ordinal != col.Ordinal)
		//			diffs.Add(new Difference)
		//		}
		//	}

		//	return diffs;
		//}
	}
}
