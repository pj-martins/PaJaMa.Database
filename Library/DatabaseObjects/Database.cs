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
		
		private void populateObjects<TDatabaseObject>(DbCommand cmd, string query, BackgroundWorker worker)
			where TDatabaseObject : DatabaseObjectBase
		{
            if (string.IsNullOrEmpty(query)) return;

			if (worker != null) worker.ReportProgress(0, $"Populating {typeof(TDatabaseObject).Name.CamelCaseToSpaced()} for {DatabaseName}...");

			var objs = new List<TDatabaseObject>();
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
		}

		public DbConnection OpenConnection()
		{
			return DataSource.OpenConnection(this.DatabaseName);
		}

		public void PopulateChildren(bool condensed, BackgroundWorker worker)
		{
            ExtendedProperties = new List<ExtendedProperty>();
			Schemas = new List<Schema>();
			ServerLogins = new List<ServerLogin>();
			Principals = new List<DatabasePrincipal>();
			Permissions = new List<Permission>();
			Credentials = new List<Credential>();
			Extensions = new List<Extension>();

			using (var conn = DataSource.OpenConnection())
			{
				using (var cmd = conn.CreateCommand())
				{
					populateObjects<ExtendedProperty>(cmd, this.DataSource.ExtendedPropertySQL, worker);
					populateObjects<DatabasePrincipal>(cmd, this.DataSource.DatabasePrincipalSQL, worker);
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
						populateObjects<Schema>(cmd, this.DataSource.SchemaSQL, worker);

					populateObjects<RoutineSynonym>(cmd, this.DataSource.RoutineSynonymSQL, worker);
					populateObjects<View>(cmd, this.DataSource.ViewSQL, worker);
					if (!condensed)
					{
						populateObjects<ServerLogin>(cmd, this.DataSource.ServerLoginSQL, worker);
						populateObjects<Permission>(cmd, this.DataSource.PermissionSQL, worker);
						populateObjects<Credential>(cmd, this.DataSource.CredentialSQL, worker);
					}
					populateObjects<Table>(cmd, this.DataSource.TableSQL, worker);
					if (!DataSource.PopulateColumns(this, cmd, worker)) populateObjects<Column>(cmd, this.DataSource.ColumnSQL, worker);
					if (!DataSource.PopulateForeignKeys(this, cmd, worker)) populateObjects<ForeignKey>(cmd, this.DataSource.ForeignKeySQL, worker);
					if (!DataSource.PopulateKeyConstraints(this, cmd, worker)) populateObjects<KeyConstraint>(cmd, this.DataSource.KeyConstraintSQL, worker);
					if (!DataSource.PopulateIndexes(this, cmd, worker)) populateObjects<Index>(cmd, this.DataSource.IndexSQL, worker);
					populateObjects<DefaultConstraint>(cmd, this.DataSource.DefaultConstraintSQL, worker);
					populateObjects<Trigger>(cmd, this.DataSource.TriggerSQL, worker);
					populateObjects<Sequence>(cmd, this.DataSource.SequenceSQL, worker);
					populateObjects<Extension>(cmd, this.DataSource.ExtensionSQL, worker);
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
