using PaJaMa.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PaJaMa.Database.Library.DatabaseObjects
{
	public class Permission : DatabaseObjectWithExtendedProperties
	{
		public string SchemaName { get; set; }
		public string PermissionSchemaName { get; set; }
		public string PermissionName { get; set; }
		public List<PermissionPrincipal> PermissionPrincipals { get; set; }

		public override string ObjectName
		{
			get { return PermissionName; }
		}

		public Permission(Database database) : base(database)
		{
			PermissionPrincipals = new List<PermissionPrincipal>();
		}

		internal override void setObjectProperties(DbDataReader reader)
		{
			var permission = Database.Permissions.FirstOrDefault(p => p.SchemaName == this.SchemaName
							&& p.PermissionSchemaName == this.PermissionSchemaName && p.PermissionName == this.PermissionName);
			if (permission == null)
			{
				permission = this;
				Database.Permissions.Add(permission);
			}

			var permissionPrincipal = reader.ToObject<PermissionPrincipal>();
			permissionPrincipal.DatbasePrincipal = Database.Principals.First(p => p.PrincipalName == reader["PrincipalName"].ToString());
			permissionPrincipal.Permission = permission;
			permission.PermissionPrincipals.Add(permissionPrincipal);
		}
	}

	public class PermissionPrincipal
	{
		public string PermissionType { get; set; }
		public PermissionDescription PermissionDescription { get; set; }
		public DatabasePrincipal DatbasePrincipal { get; set; }
		public GrantType GrantType { get; set; }
		public Permission Permission { get; set; }

		public string GetCreateRemoveScript(bool create)
		{
			return string.Format(@"{0} {1} ON {2} {5} [{3}] {4}",
							create ?
							(GrantType == GrantType.GRANT_WITH_GRANT_OPTION ? GrantType.GRANT.ToString() : GrantType.ToString())
							: "REVOKE",
							PermissionType,
							PermissionDescription == PermissionDescription.SCHEMA ?
								string.Format("SCHEMA::[{0}]", Permission.PermissionSchemaName) :
								string.Format("[{0}].[{1}]", Permission.SchemaName, Permission.PermissionName),
							DatbasePrincipal.PrincipalName,
							create ? (GrantType == GrantType.GRANT_WITH_GRANT_OPTION ? "WITH GRANT OPTION" : string.Empty)
							: "CASCADE",
							create ? "TO" : "FROM");
		}

		public bool IsEqual(PermissionPrincipal permissionPrincipal)
		{
			if (PermissionType != permissionPrincipal.PermissionType)
				return false;

			if (PermissionDescription != permissionPrincipal.PermissionDescription)
				return false;

			if (DatbasePrincipal.PrincipalName != permissionPrincipal.DatbasePrincipal.PrincipalName)
				return false;

			if (GrantType != permissionPrincipal.GrantType)
				return false;

			return true;
		}
	}

	public enum PermissionDescription
	{
		DATABASE,
		OBJECT_OR_COLUMN,
		SCHEMA,
		DATABASE_PRINCIPAL,
		ASSEMBLY,
		TYPE,
		XML_SCHEMA_COLLECTION,
		MESSAGE_TYPE,
		SERVICE_CONTRACT,
		SERVICE,
		REMOTE_SERVICE_BINDING,
		ROUTE,
		FULLTEXT_CATALOG,
		SYMMETRIC_KEY,
		CERTIFICATE,
		ASYMMETRIC_KEY
	}

	public enum GrantType
	{
		DENY,
		REVOKE,
		GRANT,
		GRANT_WITH_GRANT_OPTION
	}
}
