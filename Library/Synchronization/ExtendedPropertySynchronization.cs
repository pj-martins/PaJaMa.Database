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
	public class ExtendedPropertySynchronization : DatabaseObjectSynchronizationBase<ExtendedProperty>
	{
		public ExtendedPropertySynchronization(DatabaseObjects.Database targetDatabase, ExtendedProperty prop)
			: base(targetDatabase, prop)
		{
		}

		public static List<SynchronizationItem> GetExtendedProperties(DatabaseObjects.Database targetDatabase, 
			DatabaseObjectWithExtendedProperties sourceObject, DatabaseObjectWithExtendedProperties targetObject)
		{
			// TODO:?
			if (sourceObject.Database.DataSource.GetType().FullName !=
				targetDatabase.DataSource.GetType().FullName)
				return new List<SynchronizationItem>();

			var items = new List<SynchronizationItem>();
			var skips = new List<string>();

			if (targetObject != null)
			{
				foreach (var toProperty in targetObject.ExtendedProperties)
				{
					var fromProp = sourceObject.ExtendedProperties.FirstOrDefault(p => p.PropName == toProperty.PropName);
					if (fromProp == null)
					{
						items.AddRange(new ExtendedPropertySynchronization(targetObject.Database, toProperty).GetDropItems());
					}
					else
					{
						var toItems = new ExtendedPropertySynchronization(targetObject.Database, fromProp).GetSynchronizationItems(toProperty, true);
						if (toItems.Any())
							items.AddRange(toItems);

						skips.Add(toProperty.PropName);
					}
				}
			}

			foreach (var fromProperty in sourceObject.ExtendedProperties)
			{
				if (skips.Contains(fromProperty.PropName))
					continue;

				items.AddRange(new ExtendedPropertySynchronization(targetDatabase, fromProperty).GetCreateItems());
			}

			return items;
		}

		public override List<SynchronizationItem> GetAlterItems(DatabaseObjectBase target, bool ignoreCase)
		{
			var differences = base.GetPropertyDifferences(target, ignoreCase);
			if (databaseObject.IgnoreSchema)
			{
				var schemDiff = differences.FirstOrDefault(d => d.PropertyName == "SchemaName");
				if (schemDiff != null)
					differences.Remove(schemDiff);
			}

			if (differences.Any())
			{
				var syncItem = new SynchronizationItem(databaseObject);
				syncItem.Differences.AddRange(differences);
				syncItem.AddScript(0, GetRawDropText());
				syncItem.AddScript(1, GetRawCreateText());
				return new List<SynchronizationItem>() { syncItem };
			}

			return new List<SynchronizationItem>();
		}

		public override List<SynchronizationItem> GetSynchronizationItems(DatabaseObjectBase target, bool ignoreCase)
		{
			var items = base.GetSynchronizationItems(target, ignoreCase);
			var ext = target as ExtendedProperty;
			if ((ext.PropValue != null && databaseObject.PropValue == null) || (ext.PropValue == null && databaseObject.PropValue != null) ||
				(ext.PropValue != null && databaseObject.PropValue != null && ext.PropValue.ToString().Trim() != databaseObject.PropValue.ToString().Trim()))
			{
				var item = items.FirstOrDefault();
				if (item == null)
				{
					item = new SynchronizationItem(ext);
					items.Add(item);
				}
				item.Differences.Add(new Difference()
				{
					PropertyName = databaseObject.ObjectName,
					SourceValue = ext.PropValue == null ? string.Empty : databaseObject.PropValue.ToString(),
					TargetValue = databaseObject.PropValue == null ? string.Empty : ext.PropValue.ToString()
				});
				item.AddScript(1, new ExtendedPropertySynchronization(targetDatabase, databaseObject).GetRawDropText());
				item.AddScript(7, getAddScript());
			}
			return items;
		}

		public override List<SynchronizationItem> GetDropItems()
		{
			string remove = string.Format("EXEC sp_dropextendedproperty N'{0}', ", databaseObject.PropName);
			if (!databaseObject.IgnoreSchema)
				remove += string.Format("'SCHEMA', N'{0}', ", databaseObject.SchemaName);

			remove += string.Format("N'{0}', '{1}', {2}, {3}",
				databaseObject.Level1Type, databaseObject.Level1Object,
							(string.IsNullOrEmpty(databaseObject.Level2Object) ? "NULL" : string.Format("'{0}'", databaseObject.Level2Type)),
							(string.IsNullOrEmpty(databaseObject.Level2Object) ? "NULL" : string.Format("N'{0}'", databaseObject.Level2Object)));

			if (databaseObject.IgnoreSchema)
				remove += ", NULL, NULL";

			return getStandardDropItems(remove);
		}

		private string getAddScript()
		{
			string add = string.Format("EXEC sp_addextendedproperty N'{0}', N'{1}', ", databaseObject.PropName, databaseObject.PropValue.ToString().Replace("'", "''"));
			if (!databaseObject.IgnoreSchema)
				add += string.Format("'SCHEMA', N'{0}', ", databaseObject.SchemaName);

			add += string.Format("N'{0}', '{1}', {2}, {3}",
				databaseObject.Level1Type, databaseObject.Level1Object,
							(string.IsNullOrEmpty(databaseObject.Level2Object) ? "NULL" : string.Format("'{0}'", databaseObject.Level2Type)),
							(string.IsNullOrEmpty(databaseObject.Level2Object) ? "NULL" : string.Format("N'{0}'", databaseObject.Level2Object)));

			if (databaseObject.IgnoreSchema)
				add += ", NULL, NULL";

			return add;
		}

		public override List<SynchronizationItem> GetCreateItems()
		{
			return getStandardItems(getAddScript(), 7);
		}
	}

}
