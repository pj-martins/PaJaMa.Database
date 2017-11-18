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
	public class CredentialSynchronization : DatabaseObjectSynchronizationBase<Credential>
	{
		public CredentialSynchronization(DatabaseObjects.Database targetDatabase, Credential credential) : base(targetDatabase, credential)
		{
		}

		public override List<SynchronizationItem> GetCreateItems()
		{
			return getStandardItems(string.Format(@"CREATE CREDENTIAL [{0}] WITH IDENTITY = '{1}'", DatabaseObject.CredentialName, DatabaseObject.CredentialIdentity));
		}

		public override List<SynchronizationItem> GetSynchronizationItems(DatabaseObjectBase target, bool ignoreCase)
		{
			if (target == null)
				return base.GetSynchronizationItems(target, ignoreCase);

			var targetCredential = target as Credential;
			if (DatabaseObject.CredentialIdentity != targetCredential.CredentialIdentity)
			{
				var item = new SynchronizationItem(DatabaseObject);
				item.Differences.AddRange(GetPropertyDifferences(target, ignoreCase));
				item.AddScript(7, string.Format(@"ALTER CREDENTIAL [{0}] WITH IDENTITY = '{1}'", DatabaseObject.CredentialName, DatabaseObject.CredentialIdentity));

				return new List<SynchronizationItem>() { item };
			}

			return new List<SynchronizationItem>();
		}
	}
}
