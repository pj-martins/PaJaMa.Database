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
	public class SchemaSynchronization : DatabaseObjectSynchronizationBase<Schema>
	{
		public SchemaSynchronization(DatabaseObjects.Database targetDatabase, Schema schema)
			: base(targetDatabase, schema)
		{
		}


		public override List<SynchronizationItem> GetSynchronizationItems(DatabaseObjectBase target, bool ignoreCase)
		{
			if (target == null)
				return base.GetSynchronizationItems(target, ignoreCase);

			var targetSchema = target as Schema;
			if (DatabaseObject.SchemaOwner != targetSchema.SchemaOwner)
			{
				var item = new SynchronizationItem(DatabaseObject);
				item.Differences.AddRange(GetPropertyDifferences(target, ignoreCase));
				item.AddScript(7, string.Format(@"ALTER AUTHORIZATION ON SCHEMA::[{0}] TO [{1}]", DatabaseObject.SchemaName, DatabaseObject.SchemaOwner));

				return new List<SynchronizationItem>() { item };
			}

			return new List<SynchronizationItem>();
		}

		public override List<DatabaseObjectBase> GetMissingDependencies(List<DatabaseObjectBase> existingTargetObjects, List<SynchronizationItem> selectedItems, 
			bool isForDrop, bool ignoreCase)
		{
			if (!isForDrop)
			{
				var princ = DatabaseObject.Database.Principals.FirstOrDefault(p => p.PrincipalName == DatabaseObject.SchemaOwner);
				if (princ != null)
				{
					var targetPrinc = existingTargetObjects.OfType<DatabasePrincipal>().FirstOrDefault(p => p.PrincipalName == princ.PrincipalName);
					if (targetPrinc == null)
					{
						var selectedItem = selectedItems.FirstOrDefault(i => i.DatabaseObject is DatabasePrincipal
							&& (i.DatabaseObject as DatabasePrincipal).PrincipalName == princ.PrincipalName);
						if (selectedItem == null)
							return new List<DatabaseObjectBase>() { princ };
					}
				}
				return new List<DatabaseObjectBase>();
			}

			var missing = new List<DatabaseObjectBase>();
			var checks = DatabaseObject.Tables.ConvertAll(t => t as DatabaseObjectBase).ToList();
			checks.AddRange(DatabaseObject.RoutinesSynonyms);
			foreach (var c in checks)
			{
				if (!selectedItems.Any(i => i.DatabaseObject.Equals(c)))
					missing.Add(c);
			}
			return missing;
		}

		public override List<SynchronizationItem> GetCreateItems()
		{
			return getStandardItems(string.Format(@"CREATE SCHEMA [{0}] AUTHORIZATION [{1}]", DatabaseObject.SchemaName, DatabaseObject.SchemaOwner));
		}
	}
}
