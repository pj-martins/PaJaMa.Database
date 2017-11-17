﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaJaMa.Database.Library.DatabaseObjects
{
	public class Database
	{
		public string DatabaseName { get; private set; }
		public string DataSource { get; private set; }
		public List<Schema> Schemas { get; private set; }
		public List<ServerLogin> ServerLogins { get; private set; }
		public List<DatabasePrincipal> Principals { get; private set; }
		public List<Permission> Permissions { get; private set; }
		public List<Credential> Credentials { get; private set; }
        public List<Extension> Extensions { get; private set; }
        public string ConnectionString { get; private set; }
		public bool Is2000OrLess { get; private set; }
		public Type ConnectionType { get; private set; }
		public bool IsPostgreSQL
		{
			get { return ConnectionType == typeof(Npgsql.NpgsqlConnection); }
		}

		public bool IsSQLite
		{
			get { return ConnectionType == typeof(System.Data.SQLite.SQLiteConnection); }
		}

		public bool IsSQLServer
		{
			get { return ConnectionType == typeof(SqlConnection); }
		}

		public string DefaultSchemaName
		{
			get
			{
				if (IsSQLServer)
					return "dbo";
				if (IsPostgreSQL)
					return "public";
				return string.Empty;
			}
		}

		public DbConnection GetConnection()
		{
			var conn = Activator.CreateInstance(ConnectionType) as DbConnection;
			conn.ConnectionString = ConnectionString;
			return conn;
		}

		public Database(Type connectionType, string connectionString)
		{
			ConnectionType = connectionType;
			ConnectionString = connectionString;

			Schemas = new List<Schema>();
			ServerLogins = new List<ServerLogin>();
			Principals = new List<DatabasePrincipal>();
			Permissions = new List<Permission>();
			Credentials = new List<Credential>();
            Extensions = new List<Extension>();

			using (var conn = GetConnection())
			{
				conn.Open();
				var parts = conn.ServerVersion.Split('.');
				if (Convert.ToInt16(parts[0]) <= 8)
					Is2000OrLess = true;
				DatabaseName = conn.Database;
				DataSource = conn.DataSource;
				conn.Close();
			}
		}

		public void PopulateChildren(bool condensed, BackgroundWorker worker)
		{
			using (var conn = Activator.CreateInstance(ConnectionType) as DbConnection)
			{
				conn.ConnectionString = ConnectionString;
				conn.Open();
				var extendedProperties = ExtendedProperty.GetExtendedProperties(conn, Is2000OrLess);
				Schema.PopulateSchemas(this, conn, extendedProperties);
				Table.PopulateTables(this, conn, extendedProperties, worker);
                Sequence.PopulateSequences(this, conn);
				DatabaseObjectBase.PopulateObjects(this, conn, extendedProperties, condensed, worker);
                Extension.PopulateExtensions(this, conn);
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

		public void ChangeDatabase(string newDatabase)
		{
			if (ConnectionType == typeof(System.Data.SqlClient.SqlConnection))
			{
				var connBuilder = new System.Data.SqlClient.SqlConnectionStringBuilder(ConnectionString);
				connBuilder.InitialCatalog = newDatabase;
				ConnectionString = connBuilder.ConnectionString;
			}
			else
				throw new NotImplementedException();
		}

		public static void PopulateTables(Database database, DbConnection connection, List<ExtendedProperty> allExtendedProperties,
            BackgroundWorker worker)
        {
            if (worker != null) worker.ReportProgress(0, "Populating tables for " + connection.Database + "...");
            string qry = database.IsSQLite ?
                @"select name as TABLE_NAME, 'Default' as TABLE_SCHEMA, sql as Definition
from sqlite_master
where type = 'table'
" :
            @"select TABLE_NAME, TABLE_SCHEMA, null as Definition from INFORMATION_SCHEMA.TABLES where TABLE_TYPE = 'BASE TABLE'";
            if (database.IsPostgreSQL)
                qry += " and table_schema <> 'pg_catalog' and table_schema <> 'information_schema'";
            var tables = new List<Table>();
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = qry;
                using (var rdr = cmd.ExecuteReader())
                {
                    if (rdr.HasRows)
                    {
                        while (rdr.Read())
                        {
                            string tableName = rdr["TABLE_NAME"].ToString();
                            var table = new Table();
                            if (rdr["Definition"] != DBNull.Value)
                                table.Definition = rdr["Definition"].ToString();
                            table.TableName = tableName;
                            table.Schema = database.Schemas.First(s => s.SchemaName == rdr["TABLE_SCHEMA"].ToString());
                            table.ExtendedProperties = allExtendedProperties.Where(ep => ep.Level1Object == table.TableName && ep.ObjectSchema == table.Schema.SchemaName &&
                                string.IsNullOrEmpty(ep.Level2Object)).ToList();
                            table.Schema.Tables.Add(table);
                        }
                    }
                    rdr.Close();
                }
            }


            if (worker != null) worker.ReportProgress(0, "Populating columns for " + connection.Database + "...");
            Column.PopulateColumnsForTable(database, connection, allExtendedProperties);

            if (worker != null) worker.ReportProgress(0, "Populating foreign keys for " + connection.Database + "...");
            ForeignKey.PopulateKeys(database, connection);

            if (worker != null) worker.ReportProgress(0, "Populating primary keys for " + connection.Database + "...");
            KeyConstraint.PopulateKeys(database, connection);

            if (worker != null) worker.ReportProgress(0, "Populating indexes for " + connection.Database + "...");
            Index.PopulateIndexes(database, connection);

            if (worker != null) worker.ReportProgress(0, "Populating constraints for " + connection.Database + "...");
            DefaultConstraint.PopulateConstraints(database, connection);

            if (worker != null) worker.ReportProgress(0, "Populating triggers for " + connection.Database + "...");
            Trigger.PopulateTriggers(database, connection);
        }



		public override string ToString()
		{
			return DatabaseName;
		}
	}
}
