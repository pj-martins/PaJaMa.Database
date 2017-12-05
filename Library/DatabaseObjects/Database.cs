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
		public DataSource DataSource { get; private set; }
		public string DatabaseName { get; private set; }
		public List<Schema> Schemas { get; private set; }
		public List<ServerLogin> ServerLogins { get; private set; }
		public List<DatabasePrincipal> Principals { get; private set; }
		public List<Permission> Permissions { get; private set; }
		public List<Credential> Credentials { get; private set; }
		public List<Extension> Extensions { get; private set; }
		public List<ExtendedProperty> ExtendedProperties { get; private set; }

		public Database(DataSource dataSource, string databaseName)
		{
			this.DataSource = dataSource;
			this.DatabaseName = databaseName;
		}

		private List<TDatabaseObject> populateObjects<TDatabaseObject>(DbCommand cmd, string query, bool includeSystemSchemas, BackgroundWorker worker)
			where TDatabaseObject : DatabaseObjectBase
		{
			if (string.IsNullOrEmpty(query)) return new List<TDatabaseObject>();

			if (worker != null) worker.ReportProgress(0, $"Populating {typeof(TDatabaseObject).Name.CamelCaseToSpaced()}s for {DatabaseName}...");

			var objs = new List<TDatabaseObject>();
			if (!includeSystemSchemas && DataSource.SystemSchemaNames.Count > 0)
			{
				query = $@"select * from (
				{query}
				) z where SchemaName not in ({string.Join(", ", DataSource.SystemSchemaNames.Select(s => "'" + s + "'").ToArray())})";
			}
			cmd.CommandText = query;
			using (var rdr = cmd.ExecuteReader())
			{
				if (rdr.HasRows)
				{
					while (rdr.Read())
					{
						var obj = rdr.ToObject<TDatabaseObject>(this);
						obj.setObjectProperties(rdr);
						objs.Add(obj);
					}
				}
				rdr.Close();
			}
			return objs;
		}

		public DbConnection OpenConnection()
		{
			return DataSource.OpenConnection(this.DatabaseName);
		}

		public void PopulateSchemas(bool includeSystemSchemas)
		{
			Schemas = new List<Schema>();
			if (string.IsNullOrEmpty(this.DataSource.SchemaSQL))
				Schemas.Add(new Schema(this) { SchemaName = "" });
			else
			{
				using (var conn = OpenConnection())
				{
					using (var cmd = conn.CreateCommand())
					{
						populateObjects<Schema>(cmd, this.DataSource.SchemaSQL, includeSystemSchemas, null);
					}
				}
			}
		}

		public void PopulateTables(Schema schema)
		{
			using (var conn = OpenConnection())
			{
				using (var cmd = conn.CreateCommand())
				{
					var qry = "select * from ({0}) z ";
					if (!string.IsNullOrEmpty(schema.SchemaName))
						qry += "where SchemaName = '" + schema.SchemaName + "'";

					populateObjects<Table>(cmd, string.Format(qry, this.DataSource.TableSQL), true, null);
					if (!DataSource.PopulateColumns(this, cmd, true, null))
						populateObjects<Column>(cmd, string.Format(qry, this.DataSource.ColumnSQL), true, null);
				}
				conn.Close();
			}
		}

		public void PopulateViews(Schema schema)
		{
			using (var conn = OpenConnection())
			{
				using (var cmd = conn.CreateCommand())
				{
					var qry = "select * from ({0}) z where SchemaName = '" + schema.SchemaName + "'";
					populateObjects<View>(cmd, string.Format(qry, this.DataSource.ViewSQL), true, null);
				}
				conn.Close();
			}
		}

		public void PopulateChildren(bool condensed, bool includeSystemSchemas, BackgroundWorker worker)
		{
			ExtendedProperties = new List<ExtendedProperty>();
			Schemas = new List<Schema>();
			ServerLogins = new List<ServerLogin>();
			Principals = new List<DatabasePrincipal>();
			Permissions = new List<Permission>();
			Credentials = new List<Credential>();
			Extensions = new List<Extension>();

			using (var conn = OpenConnection())
			{
				using (var cmd = conn.CreateCommand())
				{
					populateObjects<ExtendedProperty>(cmd, this.DataSource.ExtendedPropertySQL, includeSystemSchemas, worker);
					populateObjects<DatabasePrincipal>(cmd, this.DataSource.DatabasePrincipalSQL, includeSystemSchemas, worker);
					foreach (var dp in this.Principals)
					{
						if (dp.OwningPrincipalID > 0)
						{
							dp.Owner = this.Principals.First(p => p.PrincipalID == dp.OwningPrincipalID);
							dp.Owner.Ownings.Add(dp);
						}
					}
					if (string.IsNullOrEmpty(this.DataSource.SchemaSQL))
						Schemas.Add(new Schema(this) { SchemaName = "" });
					else
						populateObjects<Schema>(cmd, this.DataSource.SchemaSQL, includeSystemSchemas, worker);

					populateObjects<RoutineSynonym>(cmd, this.DataSource.RoutineSynonymSQL, includeSystemSchemas, worker);
					populateObjects<View>(cmd, this.DataSource.ViewSQL, includeSystemSchemas, worker);
					if (!condensed)
					{
						populateObjects<ServerLogin>(cmd, this.DataSource.ServerLoginSQL, includeSystemSchemas, worker);
						populateObjects<Permission>(cmd, this.DataSource.PermissionSQL, includeSystemSchemas, worker);
						populateObjects<Credential>(cmd, this.DataSource.CredentialSQL, includeSystemSchemas, worker);
					}
					populateObjects<Table>(cmd, this.DataSource.TableSQL, includeSystemSchemas, worker);
					if (!DataSource.PopulateColumns(this, cmd, includeSystemSchemas, worker)) populateObjects<Column>(cmd, this.DataSource.ColumnSQL, includeSystemSchemas, worker);
					if (!DataSource.PopulateForeignKeys(this, cmd, includeSystemSchemas, worker)) populateObjects<ForeignKey>(cmd, this.DataSource.ForeignKeySQL, includeSystemSchemas, worker);
					if (!DataSource.PopulateKeyConstraints(this, cmd, includeSystemSchemas, worker)) populateObjects<KeyConstraint>(cmd, this.DataSource.KeyConstraintSQL, includeSystemSchemas, worker);
					if (!DataSource.PopulateIndexes(this, cmd, includeSystemSchemas, worker))
						populateObjects<Index>(cmd, this.DataSource.IndexSQL, includeSystemSchemas, worker);
					populateObjects<DefaultConstraint>(cmd, this.DataSource.DefaultConstraintSQL, includeSystemSchemas, worker);
					populateObjects<Trigger>(cmd, this.DataSource.TriggerSQL, includeSystemSchemas, worker);
					populateObjects<Sequence>(cmd, this.DataSource.SequenceSQL, includeSystemSchemas, worker);
					populateObjects<Extension>(cmd, this.DataSource.ExtensionSQL, includeSystemSchemas, worker);
				}
				conn.Close();
			}
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
