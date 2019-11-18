using PaJaMa.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaJaMa.Database.Library.DatabaseObjects
{
	public class Credential : DatabaseObjectBase
	{
		public string CredentialName { get; set; }
		public string CredentialIdentity { get; set; }

		public override string ObjectName
		{
			get { return CredentialName; }
		}

		public Credential(Database database) : base(database)
		{

		}

		internal override void setObjectProperties(DbConnection connection)
		{
			Database.Credentials.Add(this);
		}
	}
}
