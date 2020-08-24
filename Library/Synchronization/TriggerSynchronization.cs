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
	public class TriggerSynchronization : DatabaseObjectSynchronizationBase<Trigger>
	{
		public TriggerSynchronization(DatabaseObjects.Database targetDatabase, Trigger trigger)
			: base(targetDatabase, trigger)
		{
		}

		private int getIndex()
		{
			return DatabaseObject.Table.Triggers.IndexOf(DatabaseObject) + 1;
		}

		public override List<SynchronizationItem> GetDropItems(DatabaseObjectBase sourceParent)
		{
			return getStandardDropItems(string.Format("DROP TRIGGER [{0}].[{1}]", 
				DatabaseObject.Table.Schema.SchemaName, DatabaseObject.TriggerName), sourceParent, getIndex());
		}

		public override List<SynchronizationItem> GetCreateItems()
		{
			var items = getStandardItems(DatabaseObject.Definition.Trim(), level: 10 * getIndex());
			if (DatabaseObject.Disabled)
			{
				var diff = getDifference(DifferenceType.Alter, DatabaseObject, null, "Disabled");
				if (diff != null)
				{
					var item = new SynchronizationItem(DatabaseObject);
					item.Differences.Add(diff);
					item.AddScript(11 * getIndex(), string.Format("DISABLE TRIGGER [{0}] ON [{1}]", DatabaseObject.TriggerName, DatabaseObject.Table.TableName));
					items.Add(item);
				}
			}
			return items;
		}

		public override List<SynchronizationItem> GetSynchronizationItems(DatabaseObjectBase target, bool ignoreCase, bool condensed)
		{
			if (condensed) return new List<SynchronizationItem>();
			if (target == null)
				return base.GetSynchronizationItems(target, ignoreCase, false);

			if (GetRawCreateText().ToLower() == new TriggerSynchronization(TargetDatabase, target as Trigger).GetRawCreateText().ToLower()) return new List<SynchronizationItem>();

			var targetTrigger = target as Trigger;

			if (targetTrigger.Table.TableName != DatabaseObject.Table.TableName)
				return base.GetSynchronizationItems(target, ignoreCase, false);

			var items = new List<SynchronizationItem>();

			var diff = getDifference(target == null ? DifferenceType.Create : DifferenceType.Alter, DatabaseObject, target);
			if (diff != null)
			{
				var item = new SynchronizationItem(DatabaseObject);
				item.Differences.Add(diff);

				var createAlter = DatabaseObject.Definition.Trim();
				if (target != null)
				{
					var targetDef = targetTrigger.Definition.Trim();

					// it was only disabled
					if (targetDef.ToLower() == createAlter.ToLower())
						createAlter = string.Empty;
					else
						createAlter = Regex.Replace(createAlter, "CREATE TRIGGER", "ALTER TRIGGER", RegexOptions.IgnoreCase);
				}

				if (!string.IsNullOrEmpty(createAlter))
				{
					item.AddScript(10 * getIndex(), createAlter);
					items.Add(item);
				}

				if (DatabaseObject.Disabled)
				{
					diff = getDifference(DifferenceType.Alter, DatabaseObject, target, "Disabled", "false", "true");
					if (diff != null)
					{
						item = new SynchronizationItem(DatabaseObject);
						item.Differences.Add(diff);
						item.AddScript(11 * getIndex(), string.Format("DISABLE TRIGGER [{0}] ON [{1}]", DatabaseObject.TriggerName, DatabaseObject.Table.TableName));
						items.Add(item);
					}
				}
				else if (target != null && DatabaseObject.Disabled != (target as Trigger).Disabled)
				{
					diff = getDifference(DifferenceType.Alter, DatabaseObject, target, "Disabled",
						DatabaseObject.Disabled.ToString(), (target as Trigger).Disabled.ToString());
					if (diff != null)
					{
						item = new SynchronizationItem(DatabaseObject);
						item.Differences.Add(diff);
						item.AddScript(11 * getIndex(), string.Format("{2} TRIGGER [{0}] ON [{1}]", DatabaseObject.TriggerName, DatabaseObject.Table.TableName, DatabaseObject.Disabled ? "DISABLE" : "ENABLE"));
					}
					items.Add(item);
				}
			}
			return items;
		}

	}
}
