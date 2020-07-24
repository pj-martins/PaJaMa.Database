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
	public class ServerLoginSynchronization : DatabaseObjectSynchronizationBase<ServerLogin>
	{
		public ServerLoginSynchronization(DatabaseObjects.Database targetDatabase, ServerLogin login)
			: base(targetDatabase, login)
		{
		}

		public override List<SynchronizationItem> GetDropItems(DatabaseObjectBase sourceParent)
		{
			if (DatabaseObject.LoginType == LoginType.SQLLogin)
			{
				return getStandardDropItems(string.Format("DROP LOGIN [{0}]", DatabaseObject.ObjectName), sourceParent);
			}
			return getStandardDropItems(string.Format("DROP USER [{0}]", DatabaseObject.ObjectName), sourceParent);
		}

		private string getLoginScript(bool create)
		{
			string script = (create ? "CREATE " : "ALTER ")
				+ string.Format("LOGIN [{0}]{1} WITH PASSWORD=N'p@ssw0rd', DEFAULT_DATABASE=[{2}], DEFAULT_LANGUAGE=[{3}]",
				DatabaseObject.LoginName, DatabaseObject.LoginType == LoginType.WindowsLogin ? " FROM WINDOWS" : "", DatabaseObject.DefaultDatabaseName, DatabaseObject.DefaultLanguageName);

			script += string.Format(", CHECK_EXPIRATION={0}", DatabaseObject.IsExpirationChecked ? "ON" : "OFF");
			script += string.Format(", CHECK_POLICY={0}", DatabaseObject.IsPolicyChecked ? "ON" : "OFF");

			return script;
		}

		public override List<SynchronizationItem> GetCreateItems()
		{
			var items = getStandardItems(getLoginScript(true));
			if (DatabaseObject.IsDisabled)
			{
				var diff = getDifference(DifferenceType.Alter, DatabaseObject, null, "Disabled");
				if (diff != null)
				{
					var item = new SynchronizationItem(DatabaseObject);
					item.Differences.Add(diff);
					item.AddScript(7, string.Format("\r\nALTER LOGIN [{0}] DISABLE", DatabaseObject.LoginName));
					items.Add(item);
				}
			}
			return items;
		}

		public override List<SynchronizationItem> GetAlterItems(DatabaseObjectBase target, bool ignoreCase, bool condensed)
		{
			if (condensed) return new List<SynchronizationItem>();
			var diff = GetPropertyDifferences(target, ignoreCase);
			if (diff.Count == 1 && diff[0].PropertyName == "IsDisabled")
			{
				var d = getDifference(DifferenceType.Alter, DatabaseObject, target, "Disabled");
				if (d != null)
				{
					var item = new SynchronizationItem(DatabaseObject);
					item.Differences.Add(d);
					item.AddScript(7, string.Format("\r\nALTER LOGIN [{0}] {1}", DatabaseObject.LoginName, DatabaseObject.IsDisabled ? "DISABLE" : "ENABLE"));
					return new List<SynchronizationItem>() { item };
				}
			}
			return new List<SynchronizationItem>();
		}
	}
}
