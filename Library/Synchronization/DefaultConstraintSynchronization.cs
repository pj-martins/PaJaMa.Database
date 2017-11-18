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
			if (targetDatabase.DataSource.BypassKeyConstraints)
			{
				if (databaseObject.Table.KeyConstraints.Any(fk => fk.ObjectName == databaseObject.ObjectName))
					return new List<SynchronizationItem>();
			}
			return getStandardDropItems(string.Format("ALTER TABLE {0} DROP CONSTRAINT {1};", databaseObject.Table.GetObjectNameWithSchema(targetDatabase.DataSource),
				databaseObject.GetQueryObjectName(targetDatabase.DataSource)));
		}

		public override List<SynchronizationItem> GetCreateItems()
		{
			if (targetDatabase.DataSource.BypassKeyConstraints)
			{
				if (databaseObject.Table.KeyConstraints.Any(fk => fk.ObjectName == databaseObject.ObjectName))
					return new List<SynchronizationItem>();
			}

			string def = databaseObject.ColumnDefault;
			if (!string.IsNullOrEmpty(def) && def.StartsWith("((") && def.EndsWith("))"))
				def = def.Substring(1, def.Length - 2);

			return getStandardItems(string.Format(@"ALTER TABLE {0} ADD  CONSTRAINT {1}  DEFAULT {2} FOR {3};",
				databaseObject.Table.GetObjectNameWithSchema(targetDatabase.DataSource), databaseObject.GetQueryObjectName(targetDatabase.DataSource), def, databaseObject.Column.GetQueryObjectName(targetDatabase.DataSource)
				), 7);
		}
	}
}
