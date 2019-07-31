using PaJaMa.Common;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PaJaMa.Database.Library.DatabaseObjects
{
	public class DatabasePrincipal : DatabaseObjectWithExtendedProperties
	{
		public override string ObjectName
		{
			get { return PrincipalName; }
		}

		[Ignore]
		public int PrincipalID { get; set; }
		public string PrincipalName { get; set; }
		public PrincipalType PrincipalType { get; set; }
		public string DefaultSchema { get; set; }
		public string LoginName { get; set; }
		public bool IsFixedRole { get; set; }

        [Ignore]
        public int OwningPrincipalID { get; set; }

		[Ignore]
		public List<DatabasePrincipal> ChildMembers { get; set; }

		[Ignore]
		public List<DatabaseObjectBase> Ownings { get; set; }
		public DatabasePrincipal Owner { get; set; }
		public AuthenticationType AuthenticationType { get; set; }

		public string OwnerName
		{
			get { return Owner == null ? string.Empty : Owner.ObjectName; }
		}

		public override string ObjectType
		{
			get { return PrincipalType.ToString(); }
		}

		public DatabasePrincipal(Database database) : base(database)
		{
			ChildMembers = new List<DatabasePrincipal>();
			Ownings = new List<DatabaseObjectBase>();
		}

		internal override void setObjectProperties(DbConnection connection, Dictionary<string, object> values)
		{
			this.ExtendedProperties = ExtendedProperties.Where(ep => ep.Level1Object == this.PrincipalName &&
				ep.Level1Type == "USER").ToList();

			if (string.IsNullOrEmpty(this.LoginName))
				this.AuthenticationType = AuthenticationType.NONE;
			else
			{
				switch (this.PrincipalType)
				{
					case PrincipalType.SQLUser:
						this.AuthenticationType = AuthenticationType.INSTANCE;
						break;
					case PrincipalType.WindowsUser:
						this.AuthenticationType = AuthenticationType.WINDOWS;
						break;
				}
			}

			if (string.IsNullOrEmpty(this.DefaultSchema))
				this.DefaultSchema = "dbo";
			Database.Principals.Add(this);

			// TODO:
			/*

			if (_is2000OrLess)
			{
				foreach (var role in principals.Where(dp => dp.PrincipalType == PrincipalType.DatabaseRole))
				{
					cmd.CommandText = string.Format("exec sp_helprolemember [{0}]", role.PrincipalName);

					using (var rdr = cmd.ExecuteReader())
					{
						if (rdr.HasRows)
						{
							while (rdr.Read())
							{
								var child = principals.FirstOrDefault(p => p.PrincipalName == rdr["MemberName"].ToString());
								if (child == null) continue;
								role.ChildMembers.Add(child);
							}
						}
						rdr.Close();
					}
				}
			}
			else
			{
				cmd.CommandText = @"
select drb.role_principal_id as RolePrincipalID, drb.member_principal_id as MemberPrincipalID
from sys.database_role_members drb
"; ;
				using (var rdr = cmd.ExecuteReader())
				{
					if (rdr.HasRows)
					{
						while (rdr.Read())
						{
							var parent = principals.FirstOrDefault(p => p.PrincipalID == (int)rdr["RolePrincipalID"]);
							var child = principals.FirstOrDefault(p => p.PrincipalID == (int)rdr["MemberPrincipalID"]);
							if (child == null || parent == null) continue;
							parent.ChildMembers.Add(child);
						}
					}
					rdr.Close();
				}
			}
			return principals;
			*/
		}
	}

	public enum PrincipalType
	{
		SQLUser,
		WindowsUser,
		DatabaseRole,
		WindowsGroup
	}

	public enum AuthenticationType
	{
		NONE,
		WINDOWS,
		INSTANCE
	}
}
