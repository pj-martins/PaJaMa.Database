using PaJaMa.Common;
using PaJaMa.Database.Library.DatabaseObjects;
using PaJaMa.Database.Library.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaJaMa.Database.Library.Synchronization
{
	public class TableSynchronization : DatabaseObjectSynchronizationBase<Table>
	{
		private List<string> _createdColumns;
		private List<string> _alteredColumns;

		public TableSynchronization(DatabaseObjects.Database targetDatabase, Table table) : base(targetDatabase, table)
		{
		}

		private StringBuilder getColumnCreates()
		{
			_createdColumns = new List<string>();

			var sb = new StringBuilder();
			bool firstIn = true;
			foreach (var col in DatabaseObject.Columns.OrderBy(c => c.OrdinalPosition))
			{
				_createdColumns.Add(col.ColumnName);
				if (!string.IsNullOrEmpty(col.Formula))
				{
					sb.AppendLine("\t" + (firstIn ? string.Empty : ",") + string.Format("[{0}] AS {1}",
						col.ColumnName,
						col.Formula));

					firstIn = false;
					continue;
				}

				string part2 = TargetDatabase.DataSource.GetColumnPostPart(col);
				string def = new ColumnSynchronization(TargetDatabase, col).GetDefaultScript();

				sb.AppendLine("\t" + (firstIn ? string.Empty : ",") + string.Format("{0} {1}{2} {3} {4}",
					TargetDatabase.DataSource.GetConvertedObjectName(col.ColumnName),
					TargetDatabase.DataSource.GetConvertedColumnType(col, true),
					part2,
					col.IsNullable ? "NULL" : "NOT NULL",
					def));
				firstIn = false;
			}
			return sb;
		}

		private string getCreateScript()
		{
			var sb = new StringBuilder();
			sb.AppendLine(TargetDatabase.DataSource.GetPreTableCreateScript(DatabaseObject));
			var tbl = TargetDatabase.DataSource.GetConvertedObjectName(DatabaseObject.TableName);
			sb.AppendLineFormat("CREATE TABLE {0}(", DatabaseObject.GetObjectNameWithSchema(TargetDatabase.DataSource));

			sb.AppendLine(getColumnCreates().ToString());
			foreach (var kc in DatabaseObject.KeyConstraints)
			{
				sb.AppendLine(", " + TargetDatabase.DataSource.GetKeyConstraintCreateScript(kc));
			}
			sb.AppendLine(");");


			return sb.ToString();
		}

		public override List<SynchronizationItem> GetCreateItems()
		{
			var items = new List<SynchronizationItem>();
			var diff = getDifference(DifferenceType.Create, DatabaseObject);
			if (diff != null)
			{
				var item = new SynchronizationItem(DatabaseObject);
				items.Add(item);
				item.Differences.Add(diff);
				item.AddScript(2, getCreateScript());
			}

			foreach (var fk in DatabaseObject.ForeignKeys)
			{
				items.AddRange(new ForeignKeySynchronization(TargetDatabase, fk).GetCreateItems());
			}

			items.AddRange(getIndexCreateUpdateItems(null, false));
			items.AddRange(getDefaultConstraintCreateUpdateItems(null, false));
			items.AddRange(getTriggerUpdateItems(null, false));
			foreach (var column in DatabaseObject.Columns)
			{
				items.AddRange(ExtendedPropertySynchronization.GetExtendedProperties(TargetDatabase, column, null));
			}

			return items;
		}

		public override List<SynchronizationItem> GetAlterItems(DatabaseObjectBase target, bool ignoreCase)
		{
			var targetTable = target as Table;

			var items = new List<SynchronizationItem>();


			bool recreate = false;

			foreach (var fc in DatabaseObject.Columns)
			{
				var tc = targetTable.Columns.FirstOrDefault(c => string.Compare(c.ColumnName, fc.ColumnName, ignoreCase) == 0);
				if (tc != null && tc.IsIdentity != fc.IsIdentity)
				{
					var diff = getDifference(DifferenceType.Create, fc, tc, "IsIdentity", fc.IsIdentity.ToString(), tc.IsIdentity.ToString());
					if (diff != null)
					{
						var item = getRecreateTableItem(targetTable, ignoreCase);
						item.Differences.Add(diff);
						items.Add(item);
						recreate = true;
						targetTable = null;
						break;
					}
				}

				if (tc != null && tc.ColumnType.DataType == DataType.RowVersion)
				{
					var diff = new ColumnSynchronization(TargetDatabase, fc).GetPropertyDifferences(tc, ignoreCase);
					if (diff.Any())
					{
						var item = getRecreateTableItem(targetTable, ignoreCase);
						item.Differences.AddRange(diff);
						items.Add(item);
						recreate = true;
						targetTable = null;
						break;
					}
				}
			}

			if (!recreate)
			{
				foreach (var tk in targetTable.ForeignKeys)
				{
					if (DataSourcesAreDifferent && (DatabaseObject.Database.DataSource.MatchConstraintsByColumns || targetTable.Database.DataSource.MatchConstraintsByColumns))
					{
						if (!DatabaseObject.ForeignKeys.Any(k =>
							string.Compare(k.ChildTable.ObjectName, tk.ChildTable.ObjectName, ignoreCase) == 0
							&& string.Compare(k.ParentTable.ObjectName, tk.ParentTable.ObjectName, ignoreCase) == 0
							&& k.Columns.All(x => tk.Columns.Any(c => string.Compare(c.ChildColumn.ObjectName, x.ChildColumn.ObjectName, ignoreCase) == 0
							&& string.Compare(c.ParentColumn.ObjectName, x.ParentColumn.ObjectName, ignoreCase) == 0))))
							items.AddRange(new ForeignKeySynchronization(TargetDatabase, tk).GetDropItems(DatabaseObject));
					}
					else if (!DatabaseObject.ForeignKeys.Any(k => string.Compare(k.ForeignKeyName, tk.ForeignKeyName, ignoreCase) == 0))
					{
						if (targetTable.Database.DataSource.ForeignKeyDropsWithColumns)
						{
							// column being dropped
							if (tk.Columns.Any(c => !DatabaseObject.Columns.Any(x => string.Compare(x.ColumnName, c.ChildColumn.ColumnName, ignoreCase) == 0)))
								continue;
						}

						items.AddRange(new ForeignKeySynchronization(TargetDatabase, tk).GetDropItems(DatabaseObject));
					}
				}

				foreach (var fk in DatabaseObject.ForeignKeys)
				{
					var tk = targetTable.ForeignKeys.FirstOrDefault(x => string.Compare(x.ObjectName, fk.ObjectName, ignoreCase) == 0);
					if (tk == null)
					{
						if (DataSourcesAreDifferent && (TargetDatabase.DataSource.MatchConstraintsByColumns || DatabaseObject.Database.DataSource.MatchConstraintsByColumns))
						{
							tk = targetTable.ForeignKeys.FirstOrDefault(x =>
								string.Compare(x.ParentTable.TableName, fk.ParentTable.TableName, ignoreCase) == 0
								&& string.Compare(x.ChildTable.TableName, fk.ChildTable.TableName, ignoreCase) == 0
								&& x.Columns.All(y =>
									fk.Columns.Any(z =>
										string.Compare(z.ChildColumn.ColumnName, y.ChildColumn.ColumnName, ignoreCase) == 0
										&& string.Compare(z.ParentColumn.ColumnName, y.ParentColumn.ColumnName, ignoreCase) == 0)));
						}
					}
					if (tk != null)
					{
						items.AddRange(new ForeignKeySynchronization(TargetDatabase, fk).GetSynchronizationItems(tk, ignoreCase));
					}
					else
					{
						items.AddRange(new ForeignKeySynchronization(TargetDatabase, fk).GetCreateItems());
					}
				}

				foreach (var tc in targetTable.Columns)
				{
					if (!DatabaseObject.Columns.Any(c => string.Compare(c.ColumnName, tc.ColumnName, ignoreCase) == 0))
					{
						items.AddRange(new ColumnSynchronization(TargetDatabase, tc).GetDropItems(DatabaseObject));
					}
				}

				_createdColumns = new List<string>();
				_alteredColumns = new List<string>();

				foreach (var fc in DatabaseObject.Columns)
				{
					var tc = targetTable.Columns.FirstOrDefault(c => string.Compare(c.ColumnName, fc.ColumnName, ignoreCase) == 0);
					if (tc == null)
					{
						items.AddRange(new ColumnSynchronization(TargetDatabase, fc).GetAddAlterItems(null, ignoreCase));
						_createdColumns.Add(fc.ColumnName);
					}
					else
					{
						var alteredItems = new ColumnSynchronization(TargetDatabase, fc).GetSynchronizationItems(tc, ignoreCase);
						if (alteredItems.Any())
						{
							items.AddRange(alteredItems);
							_alteredColumns.Add(fc.ColumnName);
						}
					}
				}
			}

			if (!recreate)
				items.AddRange(getKeyConstraintUpdateItems(targetTable, ignoreCase));

			items.AddRange(getIndexCreateUpdateItems(targetTable, ignoreCase));
			items.AddRange(getDefaultConstraintCreateUpdateItems(targetTable, ignoreCase));
			items.AddRange(getTriggerUpdateItems(targetTable, ignoreCase));
			foreach (var column in DatabaseObject.Columns)
			{
				items.AddRange(ExtendedPropertySynchronization.GetExtendedProperties(TargetDatabase, column, targetTable == null ? null : targetTable.Columns.FirstOrDefault(c => c.ColumnName == column.ColumnName)));
			}
			return items;
		}

		//public override List<Difference> GetDifferences(DatabaseObjectBase target)
		//{
		//	return (from si in GetSynchronizationItems(target)
		//			from d in si.Differences
		//			select d).ToList();
		//}

		//public override List<Difference> GetDifferences(DatabaseObjectBase target)
		//{
		//	var diffs = base.GetDifferences(target);
		//	foreach (var col in databaseObject.Columns)
		//	{
		//		diffs.AddRange(new ColumnSynchronization(col).GetDifferences(target == null ? null : (target as Table).Columns.FirstOrDefault(x => x.ColumnName == col.ColumnName)));
		//	}

		//	foreach (var i in databaseObject.Indexes)
		//	{
		//		diffs.AddRange(new IndexSynchronization(i).GetDifferences(target == null ? null : (target as Table).Indexes.FirstOrDefault(x => x.IndexName == i.IndexName)));
		//	}

		//	foreach (var kc in databaseObject.KeyConstraints)
		//	{
		//		diffs.AddRange(new KeyConstraintSynchronization(kc).GetDifferences(target == null ? null : (target as Table).KeyConstraints.FirstOrDefault(x => x.ConstraintName == kc.ConstraintName)));
		//	}

		//	foreach (var fk in databaseObject.ForeignKeys)
		//	{
		//		diffs.AddRange(new ForeignKeySynchronization(fk).GetDifferences(target == null ? null : (target as Table).ForeignKeys.FirstOrDefault(x => x.ForeignKeyName == fk.ForeignKeyName)));
		//	}

		//	foreach (var dc in databaseObject.DefaultConstraints)
		//	{
		//		diffs.AddRange(new DefaultConstraintSynchronization(dc).GetDifferences(target == null ? null : (target as Table).DefaultConstraints.FirstOrDefault(x => x.ConstraintName == dc.ConstraintName)));
		//	}

		//	foreach (var t in databaseObject.Triggers)
		//	{
		//		diffs.AddRange(new TriggerSynchronization(t).GetDifferences(target == null ? null : (target as Table).Triggers.FirstOrDefault(x => x.TriggerName == t.TriggerName)));
		//	}
		//	return diffs;
		//}

		private SynchronizationItem getRecreateTableItem(Table targetTable, bool ignoreCase)
		{
			var item = new SynchronizationItem(DatabaseObject);
			var tmpTable = "#tmp" + Guid.NewGuid().ToString().Replace("-", "_");
			item.AddScript(0, string.Format("SELECT * INTO {0} FROM [{1}].[{2}]",
				tmpTable,
				DatabaseObject.Schema.SchemaName,
				DatabaseObject.TableName
			));

			var foreignKeys = from s in targetTable.Schema.Database.Schemas
							  from t in s.Tables
							  from fk in t.ForeignKeys
							  where string.Compare(fk.ParentTable.ObjectName, DatabaseObject.ObjectName, ignoreCase) == 0
							  || string.Compare(fk.ChildTable.ObjectName, DatabaseObject.ObjectName, ignoreCase) == 0
							  select fk;

			foreach (var fk in foreignKeys)
			{
				var dropItems = new ForeignKeySynchronization(TargetDatabase, fk).GetDropItems(DatabaseObject);
				foreach (var di in dropItems)
				{
					foreach (var s in di.Scripts)
					{
						item.AddScript(4, s.Value.ToString());
					}
				}
			}

			item.AddScript(4, string.Format("DROP TABLE [{0}].[{1}]", DatabaseObject.Schema.SchemaName,
				DatabaseObject.TableName));
			item.AddScript(5, getCreateScript());

			if (DatabaseObject.Columns.Any(c => c.IsIdentity))
				item.AddScript(6, string.Format("SET IDENTITY_INSERT [{0}].[{1}] ON", DatabaseObject.Schema.SchemaName, DatabaseObject.TableName));

			item.AddScript(6, string.Format("INSERT INTO [{0}].[{1}] ({2}) SELECT {2} FROM [{3}]",
				DatabaseObject.Schema.SchemaName,
				DatabaseObject.TableName,
				string.Join(",", DatabaseObject.Columns.Where(c => c.ColumnType.DataType != DataType.RowVersion).Select(c => "[" + c.ColumnName + "]").ToArray()),
				tmpTable));

			if (DatabaseObject.Columns.Any(c => c.IsIdentity))
				item.AddScript(6, string.Format("SET IDENTITY_INSERT [{0}].[{1}] OFF", DatabaseObject.Schema.SchemaName, DatabaseObject.TableName));

			foreach (var fk in foreignKeys)
			{
				foreach (var i in new ForeignKeySynchronization(TargetDatabase, fk).GetCreateItems())
				{
					foreach (var kvp in i.Scripts.Where(s => s.Value.Length > 0))
					{
						item.AddScript(7, kvp.Value.ToString());
					}
				}
			}

			item.AddScript(7, string.Format("DROP TABLE [{0}]", tmpTable));


			return item;
		}

		private List<SynchronizationItem> getKeyConstraintUpdateItems(Table targetTable, bool ignoreCase)
		{
			var items = new List<SynchronizationItem>();

			var skips = new List<string>();

			if (targetTable != null)
			{
				foreach (var toConstraint in targetTable.KeyConstraints)
				{
					bool drop = false;
					Difference diff = null;
					var fromConstraint = DatabaseObject.KeyConstraints.FirstOrDefault(c => string.Compare(c.ConstraintName, toConstraint.ConstraintName, ignoreCase) == 0);
					if (fromConstraint == null)
						drop = true;
					else if (DataSourcesAreDifferent && fromConstraint.Columns.Any(c => !toConstraint.Columns.Any(t => string.Compare(t.ColumnName, c.ColumnName, ignoreCase) == 0
						&& t.Ordinal == c.Ordinal && t.Descending == c.Descending)) ||
							toConstraint.Columns.Any(c => !fromConstraint.Columns.Any(t => string.Compare(t.ColumnName, c.ColumnName, ignoreCase) == 0
						&& t.Ordinal == c.Ordinal && t.Descending == c.Descending)))
						diff = getDifference(DifferenceType.Alter, fromConstraint, toConstraint, "Columns",
							string.Join(", ", fromConstraint.Columns.OrderBy(ic => ic.Ordinal).Select(ic => ic.ColumnName + " (" + (ic.Descending ? "DESC" : "ASC") + ")").ToArray()),
							string.Join(", ", toConstraint.Columns.OrderBy(ic => ic.Ordinal).Select(ic => ic.ColumnName + " (" + (ic.Descending ? "DESC" : "ASC") + ")").ToArray())
						);
					else if (toConstraint.Columns.Any(t => _alteredColumns.Contains(t.ColumnName)))
						drop = true;

					if (diff != null && toConstraint.IsPrimaryKey)
					{
						var item = new SynchronizationItem(toConstraint);
						items.Add(item);
						item.Differences.Add(diff);
						foreach (var constraintItem in new KeyConstraintSynchronization(TargetDatabase, fromConstraint).GetAlterItems(toConstraint, ignoreCase))
						{
							foreach (var script in constraintItem.Scripts)
							{
								item.AddScript(script.Key, script.Value.ToString());
							}
						}
						//item.AddScript(1, toConstraint.GetRawDropText());
						//item.AddScript(7, fromConstraint.GetRawCreateText());
					}

					if (drop)
						items.AddRange(new KeyConstraintSynchronization(TargetDatabase, toConstraint).GetDropItems(DatabaseObject));
					else
						skips.Add(toConstraint.ConstraintName);
				}
			}

			var target = targetTable == null ? DatabaseObject : targetTable;
			foreach (var fromConstraint in DatabaseObject.KeyConstraints)
			{
				if (skips.Any(s => string.Compare(s, fromConstraint.ConstraintName, ignoreCase) == 0))
					continue;

				var diff = getDifference(DifferenceType.Create, fromConstraint);
				if (diff != null)
				{
					var item = new SynchronizationItem(fromConstraint);
					item.Differences.Add(diff);
					item.AddScript(7, new KeyConstraintSynchronization(TargetDatabase, fromConstraint).GetRawCreateText());
					items.Add(item);
				}
			}

			return items;
		}

		private List<SynchronizationItem> getIndexCreateUpdateItems(Table targetTable, bool ignoreCase)
		{
			var items = new List<SynchronizationItem>();

			var skips = new List<string>();

			if (targetTable != null)
			{
				foreach (var toIndex in targetTable.Indexes)
				{
					bool drop = false;
					Difference diff = null;
					var fromIndex = DatabaseObject.Indexes.FirstOrDefault(c => string.Compare(c.IndexName, toIndex.IndexName, ignoreCase) == 0);
					if (fromIndex == null)
						drop = true;
					else if (DataSourcesAreDifferent && fromIndex.IndexColumns.Any(c => !toIndex.IndexColumns.Any(t => string.Compare(t.ColumnName, c.ColumnName, ignoreCase) == 0
						&& t.Ordinal == c.Ordinal && t.Descending == c.Descending)) ||
							toIndex.IndexColumns.Any(c => !fromIndex.IndexColumns.Any(t => string.Compare(t.ColumnName, c.ColumnName, ignoreCase) == 0
						&& t.Ordinal == c.Ordinal && t.Descending == c.Descending)))
						diff = getDifference(DifferenceType.Alter, fromIndex, toIndex, "Columns",
							string.Join(", ", fromIndex.IndexColumns.OrderBy(ic => ic.Ordinal).Select(ic => ic.ColumnName).ToArray()),
							string.Join(", ", toIndex.IndexColumns.OrderBy(ic => ic.Ordinal).Select(ic => ic.ColumnName).ToArray())
						);
					else if (toIndex.IndexColumns.Any(t => _alteredColumns.Contains(t.ColumnName)))
						drop = true;

					if (diff != null)
					{
						var item = new SynchronizationItem(toIndex);
						items.Add(item);
						item.Differences.Add(diff);
						item.AddScript(1, new IndexSynchronization(TargetDatabase, toIndex).GetRawDropText());
						item.AddScript(7, TargetDatabase.DataSource.GetIndexCreateScript(fromIndex));
					}

					if (drop)
						items.AddRange(new IndexSynchronization(TargetDatabase, toIndex).GetDropItems(DatabaseObject));
					else
						skips.Add(toIndex.IndexName);
				}
			}

			var target = targetTable == null ? DatabaseObject : targetTable;
			foreach (var fromIndex in DatabaseObject.Indexes)
			{
				if (skips.Contains(fromIndex.IndexName))
					continue;

				var diff = getDifference(DifferenceType.Create, fromIndex);
				if (diff != null)
				{
					var item = new SynchronizationItem(fromIndex);
					item.Differences.Add(diff);
					item.AddScript(7, TargetDatabase.DataSource.GetIndexCreateScript(fromIndex));
					items.Add(item);
				}
			}

			return items;
		}

		private List<SynchronizationItem> getDefaultConstraintCreateUpdateItems(Table targetTable, bool ignoreCase)
		{
			var skips = new List<DefaultConstraint>();
			var items = new List<SynchronizationItem>();

			if (targetTable != null)
			{
				foreach (var toConstraint in targetTable.DefaultConstraints)
				{
					Difference diff = null;
					bool drop = false;
					DefaultConstraint fromConstraint = null;
					if (DataSourcesAreDifferent && (TargetDatabase.DataSource.MatchConstraintsByColumns || DatabaseObject.Database.DataSource.MatchConstraintsByColumns))
					{
						fromConstraint = DatabaseObject.DefaultConstraints.FirstOrDefault(c => string.Compare(c.Table.TableName, DatabaseObject.TableName, ignoreCase) == 0 &&
							string.Compare(c.Column.ColumnName, toConstraint.Column.ColumnName, ignoreCase) == 0);
					}
					else
					{
						fromConstraint = DatabaseObject.DefaultConstraints.FirstOrDefault(c => string.Compare(c.Table.TableName, DatabaseObject.TableName, ignoreCase) == 0 &&
						string.Compare(c.ConstraintName, toConstraint.ConstraintName, ignoreCase) == 0);
					}

					if (fromConstraint == null)
						drop = true;
					else if (fromConstraint.Column.ColumnName != toConstraint.Column.ColumnName ||
							TargetDatabase.DataSource.GetColumnDefault(fromConstraint.Column, fromConstraint.ColumnDefault).Replace("(", "").Replace(")", "")
								!= TargetDatabase.DataSource.GetColumnDefault(toConstraint.Column, toConstraint.ColumnDefault).Replace("(", "").Replace(")", ""))
					{
						diff = getDifference(DifferenceType.Alter, fromConstraint, toConstraint, "ColumnDefault",
							TargetDatabase.DataSource.GetColumnDefault(fromConstraint.Column, fromConstraint.ColumnDefault),
							TargetDatabase.DataSource.GetColumnDefault(toConstraint.Column, toConstraint.ColumnDefault)
						);

						var creates = new DefaultConstraintSynchronization(TargetDatabase, fromConstraint).GetCreateItems();
						var item = creates.First();
						item.Differences.Add(diff);

						// handled on column add
						if (_createdColumns.Contains(fromConstraint.Column.ColumnName))
							item.Scripts.Clear();

						item.AddScript(0, new DefaultConstraintSynchronization(TargetDatabase, toConstraint).GetRawDropText());
						items.Add(item);
					}

					if (drop)
					{
						// table was dropped
						items.AddRange(new DefaultConstraintSynchronization(TargetDatabase, toConstraint).GetDropItems(DatabaseObject));
					}
					else
						skips.Add(toConstraint);
				}
			}

			foreach (var fromConstraint in DatabaseObject.DefaultConstraints)
			{
				var to = skips.FirstOrDefault(s => s.ConstraintName == fromConstraint.ConstraintName);
				if (to == null && (DataSourcesAreDifferent && (TargetDatabase.DataSource.MatchConstraintsByColumns || DatabaseObject.Database.DataSource.MatchConstraintsByColumns)))
					to = skips.FirstOrDefault(s => s.Column.ColumnName == fromConstraint.Column.ColumnName);
				if (to != null)
					continue;

				if (_createdColumns.Contains(fromConstraint.Column.ObjectName))
					continue;

				items.AddRange(new DefaultConstraintSynchronization(TargetDatabase, fromConstraint).GetCreateItems());
			}

			return items;
		}

		private List<SynchronizationItem> getTriggerUpdateItems(Table targetTable, bool ignoreCase)
		{
			if (TargetDatabase.DataSource.GetType().FullName != DatabaseObject.Database.GetType().FullName) return new List<SynchronizationItem>();

			var skips = new List<string>();
			var items = new List<SynchronizationItem>();

			if (targetTable != null)
			{
				foreach (var toTrigger in targetTable.Triggers)
				{
					bool drop = false;
					var fromTrigger = DatabaseObject.Triggers.FirstOrDefault(c => string.Compare(c.TriggerName, toTrigger.TriggerName, ignoreCase) == 0);
					if (fromTrigger == null)
						drop = true;
					else
						items.AddRange(new TriggerSynchronization(TargetDatabase, fromTrigger).GetSynchronizationItems(toTrigger, ignoreCase));

					if (drop)
						items.AddRange(new TriggerSynchronization(TargetDatabase, toTrigger).GetDropItems(DatabaseObject));
					else
						skips.Add(toTrigger.TriggerName);
				}
			}

			foreach (var fromTrigger in DatabaseObject.Triggers)
			{
				if (skips.Contains(fromTrigger.TriggerName))
					continue;

				items.AddRange(new TriggerSynchronization(TargetDatabase, fromTrigger).GetCreateItems());
			}

			return items;
		}


		public override List<SynchronizationItem> GetDropItems(DatabaseObjectBase sourceParent)
		{
			return getStandardDropItems(string.Format("DROP TABLE {0}", DatabaseObject.GetObjectNameWithSchema(TargetDatabase.DataSource)),
				sourceParent);
		}

		public override List<DatabaseObjectBase> GetMissingDependencies(List<DatabaseObjectBase> existingTargetObjects, List<SynchronizationItem> selectedItems,
			bool isForDrop, bool ignoreCase)
		{
			if (isForDrop)
				return base.GetMissingDependencies(existingTargetObjects, selectedItems, isForDrop, ignoreCase);

			var toTbls = existingTargetObjects.OfType<Table>();

			List<DatabaseObjectBase> missing = new List<DatabaseObjectBase>();
			foreach (var fk in DatabaseObject.ForeignKeys)
			{
				var toTbl = toTbls.FirstOrDefault(t => string.Compare(t.TableName, fk.ParentTable.TableName, ignoreCase) == 0);
				if (toTbl != null)
				{
					if (fk.Columns.Any(k => !toTbl.KeyConstraints.Any(kc => kc.Columns.Any(c => string.Compare(c.ColumnName, k.ParentColumn.ColumnName, ignoreCase) == 0))))
					{
						if (!selectedItems.Select(si => si.DatabaseObject).OfType<KeyConstraint>().Any(kc => string.Compare(kc.Table.TableName, fk.ParentTable.ObjectName, ignoreCase) == 0))
							missing.Add(fk.ParentTable);
					}
					continue;
				}

				var item = (from si in selectedItems
							where string.Compare(si.ObjectName, fk.ParentTable.ObjectName, ignoreCase) == 0
								&& si.DatabaseObject is Table
							select si).FirstOrDefault();

				if (item != null && !item.Omit)
					continue;

				missing.Add(fk.ParentTable);
			}

			if (!existingTargetObjects.OfType<Schema>().Any(s => string.Compare(s.MappedSchemaName, DatabaseObject.Schema.MappedSchemaName, ignoreCase) == 0))
			{
				var item = (from si in selectedItems
							where si.DatabaseObject is Schema
							where string.Compare(si.ObjectName, DatabaseObject.Schema.SchemaName, ignoreCase) == 0
							select si).FirstOrDefault();

				if (item == null || item.Omit)
					missing.Add(DatabaseObject.Schema);
			}

			foreach (var trig in DatabaseObject.Triggers)
			{
				var toTrig = from tt in toTbls
							 where tt.TableName != DatabaseObject.TableName && string.Compare(tt.Schema.MappedSchemaName, DatabaseObject.Schema.MappedSchemaName, ignoreCase) == 0
							 from tr in tt.Triggers
							 where string.Compare(tr.TriggerName, trig.TriggerName, ignoreCase) == 0
							 select tr;

				foreach (var tr in toTrig)
				{
					var selectedItem = (from item in selectedItems
										where !item.Omit
										&& item.DatabaseObject.Equals(tr)
										select item).FirstOrDefault();
					if (selectedItem == null)
						missing.Add(tr.Table);
				}
			}

			return missing;
		}
	}
}
