using Newtonsoft.Json;
using PaJaMa.Common;
using PaJaMa.Database.Library.DataSources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaJaMa.Database.Library.DatabaseObjects
{
	public class Database
	{
		[JsonIgnore]
		public DataSource DataSource { get; internal set; }
		public string DatabaseName { get; private set; }
		public List<Schema> Schemas { get; set; }
		public List<ServerLogin> ServerLogins { get; set; }
		public List<DatabasePrincipal> Principals { get; set; }
		public List<Permission> Permissions { get; set; }
		public List<Credential> Credentials { get; set; }
		public List<Extension> Extensions { get; set; }
		public List<ExtendedProperty> ExtendedProperties { get; set; }

		public Database(DataSource dataSource, string databaseName)
		{
			this.DataSource = dataSource;
			this.DatabaseName = databaseName;
		}

		public DbConnection OpenConnection()
		{
			return DataSource.OpenConnection(this.DatabaseName);
		}

		public List<DatabaseObjectBase> GetDatabaseObjects(bool filter)
		{
			var lst = (from s in Schemas
					   from rs in s.RoutinesSynonyms
					   select rs).OfType<DatabaseObjectBase>().ToList();

			lst.AddRange((from s in Schemas
						  from v in s.Views
						  select v).OfType<DatabaseObjectBase>());

			lst.AddRange((from s in Schemas
						  from sq in s.Sequences
						  select sq).OfType<DatabaseObjectBase>());

			lst.AddRange(Principals.Where(p => !filter || !"|INFORMATION_SCHEMA|sys|guest|public|".Contains("|" + p.PrincipalName + "|")).ToList());
			lst.AddRange(Schemas.Where(s => !filter || !"|INFORMATION_SCHEMA|dbo|".Contains("|" + s.SchemaName + "|")).ToList());
			//lst.AddRange(Principals.Where(p => !"|INFORMATION_SCHEMA|sys|guest|public|dbo|".Contains("|" + p.PrincipalName + "|")).ToList());
			//lst.AddRange(Schemas.Where(s => !"|INFORMATION_SCHEMA|dbo|".Contains("|" + s.SchemaName + "|")).ToList());
			lst.AddRange(ServerLogins.Where(l => !filter || l.LoginType != LoginType.WindowsLogin).ToList());
			lst.AddRange(Permissions.ToList());
			lst.AddRange(Credentials.ToList());
			lst.AddRange(Extensions.ToList());
			return lst;
		}

		//public override string GetObjectNameWithSchema(Database targetDatabase)
		//{
		//	return GetQueryObjectName(targetDatabase.Server);
		//}

		public override string ToString()
		{
			return DatabaseName;
		}
	}
}
