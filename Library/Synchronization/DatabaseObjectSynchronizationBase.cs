using PaJaMa.Common;
using PaJaMa.Database.Library.DatabaseObjects;
using PaJaMa.Database.Library.DataSources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PaJaMa.Database.Library.Synchronization
{
	public abstract class DatabaseObjectSynchronizationBase
	{
		protected DatabaseObjectBase DatabaseObject { get; private set; }

		protected DatabaseObjects.Database TargetDatabase { get; private set; }

		public DatabaseObjectSynchronizationBase(DatabaseObjects.Database targetDb, DatabaseObjectBase obj)
		{
			TargetDatabase = targetDb;
			DatabaseObject = obj;
		}

		public virtual List<SynchronizationItem> GetSynchronizationItems(DatabaseObjectBase target, bool ignoreCase)
		{
			if (target == null)
				return GetCreateItems();

			return GetAlterItems(target, ignoreCase);
		}

		public virtual List<SynchronizationItem> GetDropItems(DatabaseObjectBase sourceParent)
		{
			return getStandardDropItems(string.Format("DROP {0} [{1}]", DatabaseObject.ObjectType.ToString(), DatabaseObject.ObjectName),
				sourceParent);
		}

		public abstract List<SynchronizationItem> GetCreateItems();
		public virtual List<SynchronizationItem> GetAlterItems(DatabaseObjectBase target, bool ignoreCase)
		{
			var items = GetCreateItems();
			var dropItem = items.FirstOrDefault();
			if (dropItem == null)
			{
				dropItem = new SynchronizationItem(DatabaseObject);
				items.Insert(0, dropItem);
			}

			var diff = dropItem.Differences.FirstOrDefault();
			if (diff != null && diff.DifferenceType == DifferenceType.Create)
				dropItem.Differences.Remove(diff);

			dropItem.Differences.AddRange(GetPropertyDifferences(target, ignoreCase));
			dropItem.AddScript(0, GetRawDropText());
			return items;
		}

		public List<Difference> GetPropertyDifferences(DatabaseObjectBase target, bool caseInsensitive)
		{
			var diff = new List<Difference>();
			if (target == null)
			{
				var d = getDifference(DifferenceType.Create, DatabaseObject);
				if (d != null)
					diff.Add(d);
				return diff;
			}

			foreach (var propInf in DatabaseObject.GetType().GetProperties())
			{
				if (propInf.Name == "ObjectName" || propInf.Name == "QueryObjectName" || propInf.Name == "Description" || propInf.Name == "ObjectType") continue;

				var type = propInf.PropertyType;

				// nullable
				if (type.GetGenericArguments().Any())
					type = type.GetGenericArguments().First();

				if (type != typeof(ColumnType) && !type.IsPrimitive && !type.Equals(typeof(string)) && !type.IsSubclassOf(typeof(DatabaseObjectBase)))
					continue;

				if (propInf.HasAttribute<IgnoreAttribute>())
					continue;

				var targetVal = propInf.GetValue(target, null);
				var sourceVal = propInf.GetValue(DatabaseObject, null);

				if (sourceVal is ColumnType) sourceVal = (sourceVal as ColumnType).CreateTypeName;
				if (targetVal is ColumnType) targetVal = (targetVal as ColumnType).CreateTypeName;

				if (targetVal is DatabaseObjectBase)
					targetVal = (targetVal as DatabaseObjectBase).ObjectName;

				if (sourceVal is DatabaseObjectBase)
					sourceVal = (sourceVal as DatabaseObjectBase).ObjectName;

				if (targetVal == null && sourceVal == null)
					continue;

				if (targetVal != null && sourceVal != null)
				{
					if (caseInsensitive || propInf.HasAttribute<IgnoreCaseAttribute>())
					{
						sourceVal = sourceVal.ToString().ToLower().Trim();
						targetVal = targetVal.ToString().ToLower().Trim();
					}

					if (targetVal.Equals(sourceVal))
						continue;
				}

				var d = getDifference(DifferenceType.Alter, DatabaseObject, target,
					propInf.Name, sourceVal == null ? string.Empty : sourceVal.ToString(), targetVal == null ? string.Empty : targetVal.ToString());
				if (d != null)
					diff.Add(d);
			}
			return diff;
		}

		protected List<SynchronizationItem> getStandardDropItems(string script, DatabaseObjectBase sourceParent, int level = 0)
		{
			if (sourceParent != null && DatabaseObject.Database.DataSource.GetType().FullName != sourceParent.Database.DataSource.GetType().FullName &&
				sourceParent.Database.DataSource.IgnoreDrop(sourceParent, DatabaseObject)) 
				return new List<SynchronizationItem>();
			return getStandardItems(script, level, getDifference(DifferenceType.Drop, DatabaseObject));
		}

		protected List<SynchronizationItem> getStandardItems(string script, int level = 4, Difference difference = null)
		{
			if (difference == null)
				difference = getDifference(DifferenceType.Create, DatabaseObject);
			if (difference == null) return new List<SynchronizationItem>();
			var item = new SynchronizationItem(DatabaseObject);
			item.Differences.Add(difference);
			item.AddScript(level, script);
			return new List<SynchronizationItem>() { item };
		}

		public virtual List<DatabaseObjectBase> GetMissingDependencies(List<DatabaseObjectBase> existingTargetObjects, List<SynchronizationItem> selectedItems,
			bool isForDrop, bool ignoreCase)
		{
			return new List<DatabaseObjectBase>();
		}

		public string GetRawCreateText(bool insertGo = false)
		{
			string rawText = string.Join((insertGo ? "\r\nGO\r\n\r\n" : "\r\n"), from i in GetCreateItems()
																				 from kvp in i.Scripts
																				 where kvp.Value.Length > 0
																				 orderby (int)kvp.Key
																				 select kvp.Value);

			if (DatabaseObject is DatabaseObjectWithExtendedProperties)
			{
				foreach (var ep in (DatabaseObject as DatabaseObjectWithExtendedProperties).ExtendedProperties)
				{
					rawText += (insertGo ? "\r\nGO\r\n\r\n" : "\r\n") + new ExtendedPropertySynchronization(TargetDatabase, ep).GetRawCreateText();
				}
			}

			return rawText.Trim();
		}

		public virtual string GetRawDropText()
		{
			string rawText = string.Join("\r\n", from i in GetDropItems(null)
												 from kvp in i.Scripts
												 where kvp.Value.Length > 0
												 orderby (int)kvp.Key
												 select kvp.Value);

			return rawText.Trim();
		}

		public static DatabaseObjectSynchronizationBase GetSynchronization(DatabaseObjects.Database targetDatabase, object forObject)
		{
			var genericType = typeof(DatabaseObjectSynchronizationBase<>).MakeGenericType(forObject.GetType());
			var type = (from t in typeof(DatabaseObjectSynchronizationBase).Assembly.GetTypes()
						where t.IsSubclassOf(genericType)
						select t).First();

			return Activator.CreateInstance(type, targetDatabase, forObject) as DatabaseObjectSynchronizationBase;
		}

		protected Difference getDifference(DifferenceType differenceType, DatabaseObjectBase fromObject, DatabaseObjectBase toObject = null, string propertyName = null, string sourceValue = null, string targetValue = null)
		{
			var diff = new Difference(differenceType,
				string.IsNullOrEmpty(propertyName) ? differenceType.ToString() : propertyName,
				sourceValue,
				targetValue
			);

			if (TargetDatabase.DataSource.GetType().FullName != DatabaseObject.Database.DataSource.GetType().FullName)
			{
				if (TargetDatabase.DataSource.IgnoreDifference(diff, fromObject, toObject))
					return null;
				if (toObject != null && DatabaseObject.Database.DataSource.IgnoreDifference(diff, toObject, fromObject))
					return null;
			}
			return diff;
		}
	}

	public abstract class DatabaseObjectSynchronizationBase<TDatabaseObject> : DatabaseObjectSynchronizationBase
		where TDatabaseObject : DatabaseObjectBase
	{
		protected new TDatabaseObject DatabaseObject { get; private set; }

		public DatabaseObjectSynchronizationBase(DatabaseObjects.Database targetDb, TDatabaseObject obj)
			: base(targetDb, obj)
		{

			DatabaseObject = obj;
		}

		protected bool DataSourcesAreDifferent
		{
			get
			{
				return base.DatabaseObject.Database.DataSource.GetType().FullName != TargetDatabase.DataSource.GetType().FullName;
			}
		}
	}
}
