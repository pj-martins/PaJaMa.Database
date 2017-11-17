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

		public override List<SynchronizationItem> GetDropItems()
		{
			return getStandardItems(string.Format("ALTER TABLE {0} DROP COLUMN {1};",
						databaseObject.Table.GetObjectNameWithSchema(targetDatabase.DataSource),
						databaseObject.GetQueryObjectName(targetDatabase.DataSource)),
						level: 1,
						propertyName: Difference.DROP);
		}

		public override List<SynchronizationItem> GetCreateItems()
		{
			return GetAddAlterItems(null);
		}


		public string GetPostScript()
		{
			string part2 = string.Empty;
			if ((databaseObject.DataType == "decimal" || databaseObject.DataType == "numeric") && databaseObject.NumericPrecision != null && databaseObject.NumericScale != null)
			{
				part2 = "(" + databaseObject.NumericPrecision.ToString() + ", " + databaseObject.NumericScale.ToString() + ")";
			}
			else if (databaseObject.CharacterMaximumLength != null && databaseObject.DataType != "text" && databaseObject.DataType != "image"
				&& databaseObject.DataType != "ntext" && databaseObject.DataType != "xml")
			{
				string max = databaseObject.CharacterMaximumLength.ToString();
				if (max == "-1")
					max = "max";
				part2 = "(" + max + ")";
			}

			if (databaseObject.IsIdentity)
				part2 = targetDatabase.DataSource.GetCreateIdentity(databaseObject);

			return part2;
		}

		public string GetDefaultScript()
		{
			string def = string.Empty;
			//if (fromCol["ColumnDefault"] != DBNull.Value && fromCol["ConstraintName"] != DBNull.Value)
			//	def = "CONSTRAINT [" + fromCol["ConstraintName"].ToString() + "] DEFAULT(" + fromCol["ColumnDefault"] + ")";
			//else if (fromCol["COLUMN_DEFAULT"] != DBNull.Value)
			//	def = "DEFAULT(" + fromCol["COLUMN_DEFAULT"] + ")";

			string colDef = targetDatabase.DataSource.GetConvertedColumnDefault(databaseObject.ParentDatabase.DataSource, databaseObject.ColumnDefault);
			if (!string.IsNullOrEmpty(colDef) && colDef.StartsWith("((") && colDef.EndsWith("))"))
				colDef = colDef.Substring(1, colDef.Length - 2);

			if (!string.IsNullOrEmpty(colDef) && !string.IsNullOrEmpty(databaseObject.ConstraintName))
				def = "CONSTRAINT [" + databaseObject.ConstraintName + "] DEFAULT(" + colDef + ")";
			else if (!string.IsNullOrEmpty(colDef))
				def = "DEFAULT(" + colDef + ")";
			return def;
		}

		public List<SynchronizationItem> GetAddAlterItems(Column targetColumn)
		{
			var items = new List<SynchronizationItem>();

			SynchronizationItem item = null;

			var sb = new StringBuilder();

			if (!string.IsNullOrEmpty(databaseObject.Formula))
			{
				if (targetColumn == null || databaseObject.Formula != targetColumn.Formula)
				{
					item = new SynchronizationItem(databaseObject);
					item.Differences.Add(new Difference() { PropertyName = "Formula", SourceValue = databaseObject.Formula, TargetValue = targetColumn == null ? string.Empty : targetColumn.Formula });
					if (targetColumn != null)
						item.AddScript(1, string.Format("ALTER TABLE {0} DROP COLUMN {1};", databaseObject.Table.GetObjectNameWithSchema(targetDatabase.DataSource),
							databaseObject.GetQueryObjectName(targetDatabase.DataSource)));

					item.AddScript(3, string.Format("ALTER TABLE {0} ADD {1} AS {2}",
						databaseObject.Table.GetObjectNameWithSchema(targetDatabase.DataSource),
						databaseObject.GetQueryObjectName(targetDatabase.DataSource),
						databaseObject.Formula));

					items.Add(item);

					return items;
				}
			}

			var differences = targetColumn == null ? new List<Difference>() { new Difference() { PropertyName = Difference.CREATE } }
				: base.GetPropertyDifferences(targetColumn);

			for (int i = differences.Count - 1; i >= 0; i--)
			{
				var diff = differences[i];
				if (diff.PropertyName == "DataType")
				{
					if (diff.TargetValue == targetDatabase.DataSource.GetConvertedColumnType(databaseObject.ParentDatabase.DataSource, diff.SourceValue))
						differences.RemoveAt(i);
				}
				else if (diff.PropertyName == "ColumnDefault")
				{
					if (diff.TargetValue.Replace("(", "").Replace(")", "") ==
						targetDatabase.DataSource.GetConvertedColumnDefault(databaseObject.ParentDatabase.DataSource, diff.SourceValue.Replace("(", "").Replace(")", "")))
						differences.RemoveAt(i);
				}
				// TODO:
				//else if ((targetDatabase.IsSQLite || databaseObject.ParentDatabase.IsSQLite) && (diff.PropertyName == "NumericScale" || diff.PropertyName == "NumericPrecision"))
				//	differences.RemoveAt(i);
			}

			// case mismatch
			if (targetColumn != null && targetColumn.ColumnName != databaseObject.ColumnName)
			{
				item = new SynchronizationItem(databaseObject);
				item.AddScript(2, string.Format("EXEC sp_rename '{0}.{1}.{2}', '{3}', 'COLUMN'",
							targetColumn.Table.Schema.SchemaName,
							targetColumn.Table.TableName,
							targetColumn.ColumnName,
							databaseObject.ColumnName));
				item.Differences.Add(new Difference()
				{
					PropertyName = "ColumnName",
					SourceValue = databaseObject.ColumnName,
					TargetValue = targetColumn.ColumnName
				});
				items.Add(item);
				var diff = differences.First(d => d.PropertyName == "ColumnName");
				differences.Remove(diff);
			}

			if (!differences.Any())
				return items;

			string part2 = GetPostScript();

			string def = string.Empty;

			string tempConstraint = null;

			// default constraints for existing cols need to be created after the fact
			if (targetColumn == null)
			{
				def = GetDefaultScript();

				if (!databaseObject.IsNullable && !databaseObject.IsIdentity && string.IsNullOrEmpty(def) && databaseObject.DataType != "timestamp")
				{
					var clrType = databaseObject.ParentDatabase.DataSource.ColumnTypes.First(c => c.TypeName == databaseObject.DataType).ClrType;

					tempConstraint = "constraint_" + Guid.NewGuid().ToString().Replace("-", "_");

					def = "CONSTRAINT " + tempConstraint + " DEFAULT({0})";

					if (clrType.Equals(typeof(string)))
						def = string.Format(def, "''");
					else if (clrType.Equals(typeof(DateTime)) || clrType.Equals(typeof(DateTimeOffset)))
						def = string.Format(def, "'1/1/1900'");
					else if (clrType.IsNumericType())
						def = string.Format(def, 0);
					else if (clrType.Equals(typeof(byte[])))
						def = string.Format(def, "0x");
					else if (clrType.Equals(typeof(bool)))
						def = string.Format(def, "0");
					else if (clrType.Equals(typeof(Guid)))
						def = string.Format(def, "'" + Guid.Empty.ToString() + "'");
					else
						throw new NotImplementedException();
				}
			}

			sb.AppendLine(targetDatabase.DataSource.GetColumnAddAlterScript(databaseObject, targetColumn == null, part2, def));

			if (!string.IsNullOrEmpty(tempConstraint))
				sb.AppendLine(targetDatabase.DataSource.GetColumnAddAlterScript(databaseObject, false, part2, string.Empty));

			item = new SynchronizationItem(databaseObject);
			item.AddScript(2, sb.ToString());
			item.Differences.AddRange(differences);
			items.Add(item);

			var kcs = databaseObject.Table.KeyConstraints.Where(k => !k.IsPrimaryKey && k.Columns.Any(ic => ic.ColumnName == databaseObject.ColumnName));
			foreach (var kc in kcs)
			{
				var syncItem = new KeyConstraintSynchronization(targetDatabase, kc);
				item.AddScript(0, syncItem.GetRawDropText());
				item.AddScript(10, syncItem.GetRawCreateText());
			}

			if (targetColumn != null && !targetColumn.ParentDatabase.DataSource.BypassConstraints)
			{
				var dcs = databaseObject.Table.DefaultConstraints.Where(dc => dc.Column.ColumnName == databaseObject.ColumnName);
				foreach (var dc in dcs)
				{
					var syncItem = new DefaultConstraintSynchronization(targetDatabase, dc);
					item.AddScript(0, syncItem.GetRawDropText());
					item.AddScript(10, syncItem.GetRawCreateText());
				}
			}

			return items;
		}

		public override List<SynchronizationItem> GetAlterItems(DatabaseObjectBase target)
		{
			return GetAddAlterItems(target as Column);
		}
	}
}
