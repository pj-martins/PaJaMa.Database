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
	public class DatabasePrincipalSynchronization : DatabaseObjectSynchronizationBase<DatabasePrincipal>
	{
		public DatabasePrincipalSynchronization(DatabaseObjects.Database targetDatabase, DatabasePrincipal principal)
			: base(targetDatabase, principal)
		{
		}


		public override List<SynchronizationItem> GetDropItems()
		{
			if (DatabaseObject.PrincipalType == PrincipalType.DatabaseRole)
			{
				var items = new List<SynchronizationItem>();
				SynchronizationItem item;
				StringBuilder sb = new StringBuilder();
				foreach (var child in DatabaseObject.ChildMembers)
				{
					var diff = getDifference(DifferenceType.Drop, child, null, "Drop - " + child.ObjectName);
					if (diff == null) continue;
					item = new SynchronizationItem(child);
					item.Differences.Add(diff);
					item.AddScript(1, string.Format("ALTER ROLE [{0}] DROP MEMBER [{1}]", DatabaseObject.ObjectName, child.ObjectName));
					items.Add(item);
				}
				var d2 = getDifference(DifferenceType.Drop, DatabaseObject, null, "Drop - " + DatabaseObject.ObjectName);
				if (d2 != null)
				{
					item = new SynchronizationItem(DatabaseObject);
					item.Differences.Add(d2);
					item.AddScript(2, string.Format("DROP ROLE [{0}]", DatabaseObject.ObjectName));
					items.Add(item);
				}
				return items;
			}
			return getStandardDropItems(string.Format("DROP USER [{0}]", DatabaseObject.ObjectName));
		}

		public override List<SynchronizationItem> GetCreateItems()
		{
			if (DatabaseObject.PrincipalType == PrincipalType.DatabaseRole)
			{
				var sb = new StringBuilder();
				sb.AppendLineFormat(@"CREATE ROLE [{0}] {1}", DatabaseObject.PrincipalName, DatabaseObject.Owner == null ? string.Empty :
					string.Format("AUTHORIZATION [{0}]", DatabaseObject.Owner.ObjectName));
				foreach (var dp in DatabaseObject.ChildMembers)
				{
					sb.AppendLineFormat("ALTER ROLE [{0}] ADD MEMBER [{1}]", DatabaseObject.ObjectName, dp.ObjectName);
				}

				return getStandardItems(sb.ToString());
			}

			return getStandardItems(getLoginScript(true));
		}

		private string getLoginScript(bool create) => create ?
				string.Format(
						(DatabaseObject.AuthenticationType == AuthenticationType.NONE ?
						@"CREATE USER [{0}] WITHOUT LOGIN WITH DEFAULT_SCHEMA=[{2}]" :
						 @"CREATE USER [{0}] FOR LOGIN [{1}] WITH DEFAULT_SCHEMA=[{2}]")
						, DatabaseObject.PrincipalName, DatabaseObject.LoginName, DatabaseObject.DefaultSchema) :
						string.Format(@"ALTER USER [{0}] WITH LOGIN = [{1}], DEFAULT_SCHEMA=[{2}]", DatabaseObject.PrincipalName,
						DatabaseObject.LoginName, DatabaseObject.DefaultSchema);

		public override List<SynchronizationItem> GetSynchronizationItems(DatabaseObjectBase target, bool ignoreCase)
		{
			var items = new List<SynchronizationItem>();

			if (DatabaseObject.PrincipalType != PrincipalType.DatabaseRole)
			{
				if (target == null)
					return base.GetSynchronizationItems(target, ignoreCase);

				var diffs = GetPropertyDifferences(target, ignoreCase);
				if (diffs.Any())
				{
					if (target != null && DatabaseObject.PrincipalName == "dbo")
					{
						var item = new SynchronizationItem(DatabaseObject);
						item.Differences.AddRange(diffs);
						item.AddScript(2, string.Format("sp_changedbowner '{0}'", DatabaseObject.LoginName));
						return new List<SynchronizationItem>() { item };
					}
					else
					{
						var item = new SynchronizationItem(DatabaseObject);
						item.Differences.AddRange(diffs);
						item.AddScript(2, string.Format("ALTER USER [{0}] WITH DEFAULT_SCHEMA = [{1}]{2}", DatabaseObject.PrincipalName,
							DatabaseObject.DefaultSchema, string.IsNullOrEmpty(DatabaseObject.LoginName) ?
							string.Empty : string.Format(", LOGIN = [{0}]", DatabaseObject.LoginName)));
						return new List<SynchronizationItem>() { item };
					}
				}

				return new List<SynchronizationItem>();
			}

			var dp = target as DatabasePrincipal;
			if (dp == null)
			{
				var diff = getDifference(DifferenceType.Create, DatabaseObject);
				if (diff != null)
				{
					var item = new SynchronizationItem(DatabaseObject);
					item.Differences.Add(diff);

					if (!item.Scripts.ContainsKey(7))
						item.Scripts.Add(7, new StringBuilder());

					item.AddScript(7, string.Format(@"CREATE ROLE [{0}] {1}", DatabaseObject.PrincipalName, DatabaseObject.Owner == null ? string.Empty :
							string.Format("AUTHORIZATION [{0}]", DatabaseObject.Owner.ObjectName)));

					items.Add(item);
				}
			}
			else
			{
				if (DatabaseObject.Owner.PrincipalName != dp.Owner.PrincipalName)
				{
					var diff = getDifference(DifferenceType.Alter, DatabaseObject, dp, "Owner", DatabaseObject.Owner.PrincipalName, dp.Owner.PrincipalName);
					if (diff != null)
					{
						var item = new SynchronizationItem(DatabaseObject);
						item.Differences.Add(diff);

						if (!item.Scripts.ContainsKey(7))
							item.Scripts.Add(7, new StringBuilder());

						item.AddScript(7, string.Format("ALTER AUTHORIZATION ON ROLE::[{0}] TO [{1}]", DatabaseObject.ObjectName, DatabaseObject.Owner.PrincipalName));
						items.Add(item);
					}
				}

				var drops = dp.ChildMembers.Where(m => !DatabaseObject.ChildMembers.Any(x => x.ObjectName == m.ObjectName));
				foreach (var drop in drops)
				{
					var diff = getDifference(DifferenceType.Alter, drop, null, "Member", "Drop", drop.ObjectName);
					if (diff != null)
					{
						var item = new SynchronizationItem(DatabaseObject);
						item.Differences.Add(diff);

						if (!item.Scripts.ContainsKey(7))
							item.Scripts.Add(7, new StringBuilder());

						item.AddScript(7, string.Format("ALTER ROLE [{0}] DROP MEMBER [{1}]", DatabaseObject.ObjectName, drop.ObjectName));
						items.Add(item);
					}
				}
			}

			var creates = DatabaseObject.ChildMembers.Where(m => target == null || !dp.ChildMembers.Any(x => x.ObjectName == m.ObjectName));
			foreach (var create in creates)
			{
				var diff = getDifference(DifferenceType.Alter, create, target, "Member", "Create", create.ObjectName);
				if (diff != null)
				{
					var item = new SynchronizationItem(DatabaseObject);
					item.Differences.Add(diff);

					if (!item.Scripts.ContainsKey(7))
						item.Scripts.Add(7, new StringBuilder());

					item.AddScript(7, string.Format("ALTER ROLE [{0}] ADD MEMBER [{1}]", DatabaseObject.ObjectName, create.ObjectName));
					items.Add(item);
				}
			}

			return items;
		}

		public override List<DatabaseObjectBase> GetMissingDependencies(List<DatabaseObjectBase> existingTargetObjects, List<SynchronizationItem> selectedItems,
			bool isForDrop, bool ignoreCase)
		{
			var missing = new List<DatabaseObjectBase>();
			var checks = new List<DatabaseObjectBase>();
			//if (isForDrop)
			//{
			//	//if (this.ObjectType == Classes.ObjectType.DatabaseRole)
			//	//{
			//	//	if (this.Owner != null)
			//	//		checks.Add(this.Owner);
			//	//}
			//	//else
			//	{
			//		checks.AddRange(Ownings.ToList());
			//	}
			//}
			//else
			{
				checks = DatabaseObject.ChildMembers.OfType<DatabaseObjectBase>().ToList();
				if (DatabaseObject.Owner != null)
					checks.Add(DatabaseObject.Owner);

				if (!string.IsNullOrEmpty(DatabaseObject.LoginName))
				{
					var slogin = DatabaseObject.Database.ServerLogins.FirstOrDefault(l => l.LoginName == DatabaseObject.LoginName);
					if (slogin != null)
						checks.Add(slogin);
				}
			}

			foreach (var child in checks)
			{
				//if ("|dbo|".Contains(child.ObjectName))
				//	continue;

				if (!existingTargetObjects.OfType<DatabaseObjectBase>().Any(o => o.ObjectType == child.ObjectType && o.ObjectName == child.ObjectName)
					&& !selectedItems.Select(i => i.DatabaseObject).OfType<DatabaseObjectBase>().Any(o => o.ObjectType == child.ObjectType && o.ObjectName == child.ObjectName))
				{
					//if (child is DatabasePrincipal && (child as DatabasePrincipal).SynchronizationItem.Omit)
					//	continue;

					missing.Add(child);
				}
			}
			return missing;
		}
	}
}
