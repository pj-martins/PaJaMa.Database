﻿using PaJaMa.Common;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PaJaMa.Database.Library.DatabaseObjects
{
    public class Schema : DatabaseObjectWithExtendedProperties
    {
        [Ignore]
        public List<Table> Tables { get; private set; }

        [Ignore]
        public List<View> Views { get; private set; }

        [Ignore]
        public List<RoutineSynonym> RoutinesSynonyms { get; private set; }

        [Ignore]
        public List<Sequence> Sequences { get; private set; }

        public override string ObjectName
        {
            get { return SchemaName; }
        }

        public string SchemaName { get; set; }
        public string SchemaOwner { get; set; }

        public string MappedSchemaName
        {
            get
            {
                if (SchemaName == ParentDatabase.DefaultSchemaName)
                    return "__DEFAULT__";
                return SchemaName;
            }
        }

        private Database _database;
        public override Database ParentDatabase => _database;

        public Schema()
        {
            Tables = new List<Table>();
            RoutinesSynonyms = new List<RoutineSynonym>();
            Views = new List<View>();
            Sequences = new List<Sequence>();
        }

        public static void PopulateSchemas(Database database, DbConnection connection, List<ExtendedProperty> extendedProperties)
        {
            database.Schemas.Clear();

            if (database.IsSQLite)
            {
                var schema = new Schema()
                {
                    _database = database,
                    SchemaName = ""
                };
                database.Schemas.Add(schema);
                return;
            }

            string qry = string.Empty;
            if (database.IsPostgreSQL)
            {
                qry = @"select schema_name as SchemaName, schema_owner as SchemaOwner from information_schema.schemata
where schema_name <> 'pg_catalog' and schema_name <> 'information_schema'";
            }
            else if (database.Is2000OrLess || database.ConnectionType != typeof(SqlConnection))
                qry = "select distinct TABLE_SCHEMA as SchemaName, TABLE_SCHEMA as SchemaOwner from INFORMATION_SCHEMA.TABLES";
            else
                qry = @"select s.name as SchemaName, p.name as SchemaOwner
				 from sys.schemas s
				join sys.database_principals p on p.principal_id = s.principal_id
";

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = qry;
                using (var rdr = cmd.ExecuteReader())
                {
                    if (rdr.HasRows)
                    {
                        while (rdr.Read())
                        {
                            var schema = rdr.ToObject<Schema>();
                            schema.ExtendedProperties = extendedProperties
                                .Where(ep => ep.Level1Type == typeof(Schema).Name.ToUpper() && ep.Level1Object == schema.SchemaName)
                                .ToList();
                            schema._database = database;
                            var owner = database.Principals.FirstOrDefault(p => schema.SchemaOwner == p.ObjectName);
                            if (owner != null)
                                owner.Ownings.Add(schema);
                            database.Schemas.Add(schema);
                        }
                    }
                    rdr.Close();
                }
            }
        }
    }
}
