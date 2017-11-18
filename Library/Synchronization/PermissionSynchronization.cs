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
	public class PermissionSynchronization : DatabaseObjectSynchronizationBase<Permission>
	{
		public PermissionSynchronization(DatabaseObjects.Database targetDatabase, Permission permission)
			: base(targetDatabase, permission)
		{
		}

		public override List<SynchronizationItem> GetDropItems()
		{
			StringBuilder sb = new StringBuilder();
			foreach (var princ in DatabaseObject.PermissionPrincipals)
			{
				sb.AppendLine(princ.GetCreateRemoveScript(false));
			}
			return getStandardDropItems(sb.ToString());
		}

		private string getCreateScript()
		{
			StringBuilder sb = new StringBuilder();
			foreach (var princ in DatabaseObject.PermissionPrincipals)
			{
				sb.AppendLine(princ.GetCreateRemoveScript(true));
			}
			return sb.ToString();
		}

		public override List<SynchronizationItem> GetCreateItems()
		{
			return getStandardItems(getCreateScript());
		}

		public override List<DatabaseObjectBase> GetMissingDependencies(List<DatabaseObjectBase> existingTargetObjects, List<SynchronizationItem> selectedItems,
			bool isForDrop, bool ignoreCase)
		{
			var match = Regex.Match(GetRawCreateText(), @"ON SCHEMA::\[(.*?)\]");
			if (match.Success)
			{
				var target = existingTargetObjects.OfType<Schema>().FirstOrDefault(t => t.ObjectName == match.Groups[1].Value);

				if (target == null)
					target = selectedItems.Select(i => i.DatabaseObject).OfType<Schema>().FirstOrDefault(t => t.ObjectName == match.Groups[1].Value);

				if (target == null)
					return new List<DatabaseObjectBase>() { DatabaseObject.Database.Schemas.First(d => d.ObjectName == match.Groups[1].Value) };
			}
			else
			{
				var missingPrincipals = new List<DatabaseObjectBase>();

				foreach (var pp in DatabaseObject.PermissionPrincipals)
				{
					if (!existingTargetObjects.OfType<DatabasePrincipal>().Any(dp => dp.PrincipalName == pp.DatbasePrincipal.PrincipalName)
						&& !selectedItems.Select(i => i.DatabaseObject).OfType<DatabasePrincipal>().Any(dp => dp.PrincipalName == pp.DatbasePrincipal.PrincipalName))
						missingPrincipals.Add(pp.DatbasePrincipal);
				}

				if (missingPrincipals.Any())
					return missingPrincipals;
			}

			return base.GetMissingDependencies(existingTargetObjects, selectedItems, isForDrop, ignoreCase);
		}

		public override List<SynchronizationItem> GetSynchronizationItems(DatabaseObjectBase target, bool ignoreCase)
		{
			if (target == null)
				return base.GetSynchronizationItems(target, ignoreCase);

			var items = new List<SynchronizationItem>();
			var targetPermission = target as Permission;
			List<PermissionPrincipal> skips = new List<PermissionPrincipal>();
			foreach (var pp in DatabaseObject.PermissionPrincipals)
			{
				var tpp = targetPermission.PermissionPrincipals.FirstOrDefault(p => pp.IsEqual(p));
				if (tpp == null)
				{
					var diff = getDifference(DifferenceType.Create, DatabaseObject);
					if (diff != null)
					{
						var item = new SynchronizationItem(DatabaseObject);
						item.Differences.Add(diff);
						item.AddScript(2, pp.GetCreateRemoveScript(true));
						items.Add(item);
					}
					skips.Add(pp);
				}
			}

			foreach (var pp in targetPermission.PermissionPrincipals)
			{
				if (skips.Any(s => s.PermissionType == pp.PermissionType && s.DatbasePrincipal.PrincipalName == pp.DatbasePrincipal.PrincipalName))
					continue;

				var tpp = DatabaseObject.PermissionPrincipals.FirstOrDefault(p => pp.IsEqual(p));
				if (tpp == null)
				{
					var diff = getDifference(DifferenceType.Drop, DatabaseObject);
					if (diff != null)
					{
						var item = new SynchronizationItem(DatabaseObject);
						item.Differences.Add(diff);
						item.AddScript(2, pp.GetCreateRemoveScript(false));
						items.Add(item);
					}
				}
			}

			return items;
		}
	}
}
