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
	public class ColumnSynchronization : DatabaseObjectSynchronizationBase<Column>
	{
		public ColumnSynchronization(DatabaseObjects.Database targetDatabase, Column column)
			: base(targetDatabase, column)
		{
		}

		public override List<SynchronizationItem> GetDropItems(DatabaseObjectBase sourceParent)
		{
			return getStandardDropItems(string.Format("ALTER TABLE {0} DROP COLUMN {1};",
						DatabaseObject.Table.GetObjectNameWithSchema(TargetDatabase.DataSource),
						DatabaseObject.GetQueryObjectName(TargetDatabase.DataSource)),
						sourceParent, 1);
		}

		public override List<SynchronizationItem> GetCreateItems()
		{
			return GetAddAlterItems(null, false);
		}


		public string GetDefaultScript()
		{
			string def = string.Empty;
			string colDef = TargetDatabase.DataSource.GetColumnDefault(DatabaseObject, DatabaseObject.ColumnDefault);
			if (!string.IsNullOrEmpty(colDef) && colDef.StartsWith("((") && colDef.EndsWith("))"))
				colDef = colDef.Substring(1, colDef.Length - 2);

			if (!string.IsNullOrEmpty(colDef) && !string.IsNullOrEmpty(DatabaseObject.ConstraintName))
				def = "CONSTRAINT " + TargetDatabase.DataSource.GetConvertedObjectName(DatabaseObject.ConstraintName) + " DEFAULT(" + colDef + ")";
			else if (!string.IsNullOrEmpty(colDef))
				def = "DEFAULT(" + colDef + ")";
			return def;
		}

		public List<SynchronizationItem> GetAddAlterItems(Column targetColumn, bool ignoreCase)
		{
			var items = new List<SynchronizationItem>();

			SynchronizationItem item = null;

			var sb = new StringBuilder();

			if (!string.IsNullOrEmpty(DatabaseObject.Formula))
			{
				if (targetColumn == null || DatabaseObject.Formula != targetColumn.Formula)
				{
					var diff = getDifference(DifferenceType.Alter, DatabaseObject, targetColumn, "Formula",
						DatabaseObject.Formula, targetColumn == null ? string.Empty : targetColumn.Formula);
					if (diff == null) return new List<SynchronizationItem>();
					item = new SynchronizationItem(DatabaseObject);
					item.Differences.Add(diff);
					if (targetColumn != null)
						item.AddScript(1, string.Format("ALTER TABLE {0} DROP COLUMN {1};", DatabaseObject.Table.GetObjectNameWithSchema(TargetDatabase.DataSource),
							DatabaseObject.GetQueryObjectName(TargetDatabase.DataSource)));

					item.AddScript(3, string.Format("ALTER TABLE {0} ADD {1} AS {2}",
						DatabaseObject.Table.GetObjectNameWithSchema(TargetDatabase.DataSource),
						DatabaseObject.GetQueryObjectName(TargetDatabase.DataSource),
						DatabaseObject.Formula));

					items.Add(item);

					return items;
				}
			}

			var differences = new List<Difference>();
			if (targetColumn == null)
			{
				var difference = getDifference(DifferenceType.Create, DatabaseObject);
				if (difference != null)
					differences.Add(difference);
			}
			else
				differences = base.GetPropertyDifferences(targetColumn, ignoreCase);

			// case mismatch
			if (!ignoreCase && targetColumn != null && targetColumn.ColumnName != DatabaseObject.ColumnName)
			{
				item = new SynchronizationItem(DatabaseObject);
				item.AddScript(2, string.Format("EXEC sp_rename '{0}.{1}.{2}', '{3}', 'COLUMN'",
							targetColumn.Table.Schema.SchemaName,
							targetColumn.Table.TableName,
							targetColumn.ColumnName,
							DatabaseObject.ColumnName));
				var diff = getDifference(DifferenceType.Alter, DatabaseObject, targetColumn,
					"Column", DatabaseObject.ColumnName, targetColumn.ColumnName);
				if (diff != null)
					item.Differences.Add(diff);
				if (item.Differences.Count > 0)
					items.Add(item);

				diff = differences.FirstOrDefault(d => d.PropertyName == "ColumnName");
				if (diff != null)
					differences.Remove(diff);
			}

			if (!differences.Any())
				return items;

			string part2 = TargetDatabase.DataSource.GetColumnPostPart(DatabaseObject);

			string def = string.Empty;

			var tempConstraint = "";

			// default constraints for existing cols need to be created after the fact
			if (targetColumn == null)
			{
				def = GetDefaultScript();

				if (!DatabaseObject.IsNullable && !DatabaseObject.IsIdentity && string.IsNullOrEmpty(def) && DatabaseObject.ColumnType.DataType != DataType.RowVersion)
				{
					var colType = TargetDatabase.DataSource.ColumnTypes.First(c => c.DataType == DatabaseObject.ColumnType.DataType);
					tempConstraint = "constraint_" + Guid.NewGuid().ToString().Replace("-", "_");
					DatabaseObject.ColumnDefault = colType.DefaultValue;
					DatabaseObject.ConstraintName = tempConstraint;
					def = GetDefaultScript();
				}
			}

			sb.AppendLine(TargetDatabase.DataSource.GetColumnAddAlterScript(DatabaseObject, targetColumn, part2, def));

			if (!string.IsNullOrEmpty(tempConstraint))
			{
				// sb.AppendLine(TargetDatabase.DataSource.GetColumnAddAlterScript(DatabaseObject, targetColumn, part2, string.Empty));
				var defConstraint = new DefaultConstraint(TargetDatabase);
				defConstraint.ConstraintName = tempConstraint;
				defConstraint.Table = DatabaseObject.Table;
				var defSync = new DefaultConstraintSynchronization(TargetDatabase, defConstraint);
				sb.AppendLine(defSync.GetRawDropText());
			}

			item = new SynchronizationItem(DatabaseObject);
			item.AddScript(2, sb.ToString());
			item.Differences.AddRange(differences);
			items.Add(item);

			var kcs = DatabaseObject.Table.KeyConstraints.Where(k => !k.IsPrimaryKey && k.Columns.Any(ic => ic.ColumnName == DatabaseObject.ColumnName));
			foreach (var kc in kcs)
			{
				var syncItem = new KeyConstraintSynchronization(TargetDatabase, kc);
				item.AddScript(0, syncItem.GetRawDropText());
				item.AddScript(10, syncItem.GetRawCreateText());
			}

			if (targetColumn != null)
			{
				var dcs = DatabaseObject.Table.DefaultConstraints.Where(dc => dc.Column.ColumnName == DatabaseObject.ColumnName);
				foreach (var dc in dcs)
				{
					var syncItem = new DefaultConstraintSynchronization(TargetDatabase, dc);
					item.AddScript(0, syncItem.GetRawDropText());
					item.AddScript(10, syncItem.GetRawCreateText());
				}
			}

			return items;
		}

		public override List<SynchronizationItem> GetAlterItems(DatabaseObjectBase target, bool ignoreCase)
		{
			return GetAddAlterItems(target as Column, ignoreCase);
		}
	}
}
