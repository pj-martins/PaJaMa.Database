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
	public class ServerLogin : DatabaseObjectWithExtendedProperties
	{
		public override string ObjectName
		{
			get { return LoginName; }
		}

		public override string ObjectType
		{
			get { return LoginType.ToString(); }
		}

		public ServerLogin(Database database) : base(database)
		{
		}

		public string LoginName { get; set; }
		public LoginType LoginType { get; set; }
		public bool IsDisabled { get; set; }
		public string DefaultDatabaseName { get; set; }
		public string DefaultLanguageName { get; set; }
		public bool IsExpirationChecked { get; set; }
		public bool IsPolicyChecked { get; set; }

		internal override void setObjectProperties(DbConnection connection)
		{
			this.ExtendedProperties = Database.ExtendedProperties.Where(ep => ep.ObjectType == LoginType.SQLLogin.ToString() &&
							ep.Level1Object == this.LoginName).ToList();
			Database.ServerLogins.Add(this);
		}
	}

	public enum LoginType
	{
		SQLLogin,
		WindowsLogin
	}
}
