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
			return getStandardDropItems(string.Format("ALTER TABLE [{0}].[{1}] DROP CONSTRAINT [{2}]", databaseObject.Table.Schema.SchemaName,
				databaseObject.Table.TableName, databaseObject.ConstraintName));
		}

		public override List<SynchronizationItem> GetCreateItems()
		{
			string def = databaseObject.ColumnDefault;
			if (!string.IsNullOrEmpty(def) && def.StartsWith("((") && def.EndsWith("))"))
				def = def.Substring(1, def.Length - 2);

			return getStandardItems(string.Format(@"ALTER TABLE [{0}].[{1}] ADD  CONSTRAINT [{2}]  DEFAULT {3} FOR [{4}]",
				databaseObject.Table.Schema.SchemaName, databaseObject.Table.TableName, databaseObject.ConstraintName, def, databaseObject.Column.ColumnName), 7);
		}

		//public override List<SynchronizationItem> GetSynchronizationItems(DatabaseObjectBase target)
		//{
		//	if (targetDatabase.IsSQLite || databaseObject.ParentDatabase.IsSQLite)
		//		return new List<SynchronizationItem>();

		//	return base.GetSynchronizationItems(target);
		//}
	}
}
