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
	public class DefaultConstraintSynchronization : DatabaseObjectSynchronizationBase<DefaultConstraint>
	{
		public DefaultConstraintSynchronization(DatabaseObjects.Database targetDatabase, DefaultConstraint constraint)
			: base(targetDatabase, constraint)
		{
		}


		public override List<SynchronizationItem> GetDropItems()
		{
            if (targetDatabase.IsPostgreSQL)
            {
                if (databaseObject.Table.KeyConstraints.Any(fk => fk.ObjectName == databaseObject.ObjectName))
                    return new List<SynchronizationItem>();
            }
			return getStandardDropItems(string.Format("ALTER TABLE {0} DROP CONSTRAINT {1}{2}", databaseObject.Table.ObjectNameWithSchema,
				databaseObject.QueryObjectName, targetDatabase.IsPostgreSQL ? ";" : ""));
		}

		public override List<SynchronizationItem> GetCreateItems()
		{
            if (targetDatabase.IsPostgreSQL)
            {
                if (databaseObject.Table.KeyConstraints.Any(fk => fk.ObjectName == databaseObject.ObjectName))
                    return new List<SynchronizationItem>();
            }

            string def = databaseObject.ColumnDefault;
			if (!string.IsNullOrEmpty(def) && def.StartsWith("((") && def.EndsWith("))"))
				def = def.Substring(1, def.Length - 2);

			return getStandardItems(string.Format(@"ALTER TABLE {0} ADD  CONSTRAINT {1}  DEFAULT {2} FOR {3}{4}",
				databaseObject.Table.ObjectNameWithSchema, databaseObject.QueryObjectName, def, databaseObject.Column.QueryObjectName,
                targetDatabase.IsPostgreSQL ? ";" : ""), 7);
		}

		//public override List<SynchronizationItem> GetSynchronizationItems(DatabaseObjectBase target)
		//{
		//	if (targetDatabase.IsSQLite || databaseObject.ParentDatabase.IsSQLite)
		//		return new List<SynchronizationItem>();

		//	return base.GetSynchronizationItems(target);
		//}
	}
}
