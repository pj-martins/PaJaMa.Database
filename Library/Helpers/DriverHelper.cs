using PaJaMa.Database.Library.DatabaseObjects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PaJaMa.Database.Library.Helpers
{
    public class DriverHelper
    {
        private DbConnection _connection;

        public DriverHelper(DbConnection connection)
        {
            _connection = connection;
        }

        public static Type[] GetDatabaseTypes()
        {
            List<Type> types = new List<Type>() {
                typeof(System.Data.SqlClient.SqlConnection),
                typeof(System.Data.OleDb.OleDbConnection),
                typeof(System.Data.Odbc.OdbcConnection),
				typeof(System.Data.SQLite.SQLiteConnection),
				typeof(Npgsql.NpgsqlConnection)
            };

            
            return types.ToArray();
        }

        public string GetTable(string tableName, string databaseName = null, string schema = null)
        {
            if (_connection is SqlConnection)
            {
                return string.Format("[{0}].[{1}].[{2}]", databaseName ?? _connection.Database, schema ?? "dbo", tableName);
            }
            else if (_connection.GetType().Name.ToLower().Contains("sqlite"))
            {
                return string.Format("[{0}]", tableName);
            }
            else if (_connection.GetType().Name.ToLower().Contains("npgsql"))
            {
                return string.Format("\"{0}\".\"{1}\".\"{2}\"", databaseName ?? _connection.Database, schema ?? "public", tableName);
            }
            return tableName;
        }

        public string GetPreTopN(int topN)
        {
            var top = _connection is SqlConnection || _connection is OdbcConnection || _connection is OleDbConnection;
            if (top)
            {
                return "top " + topN.ToString();
            }
            return string.Empty;
        }

        public string GetPostTopN(int topN)
        {
            var top = _connection is SqlConnection || _connection is OdbcConnection || _connection is OleDbConnection;
            if (!top)
            {
                return "limit " + topN.ToString();
            }
            return string.Empty;
        }

        public string GetColumnList(string[] columns)
        {
            if (_connection is SqlConnection)
            {
                return "[" + string.Join("],\r\n\t[", columns) + "]";
            }
            else if (_connection.GetType().Name.ToLower().Contains("sqlite"))
            {
                return string.Join(",\r\n\t", columns);
            }
            else if (_connection.GetType().Name.ToLower().Contains("npgsql"))
            {
                return "\"" + string.Join("\",\r\n\t\"", columns) + "\"";
            }
            return "*";
        }

        public List<string> GetDatabases()
        {
            if (_connection.GetType().Name.ToLower().Contains("sqlite"))
            {
                return new List<string>() { "Default" };
            }

            return _connection.GetSchema("Databases").Rows.OfType<DataRow>().Select(dr => dr["database_name"].ToString()).ToList();
        }

        public static string GetConvertedColumnDefault(DatabaseObjects.Database targetDatabase, string columnDefault)
        {
            if (string.IsNullOrEmpty(columnDefault)) return columnDefault;

            if (targetDatabase.IsSQLServer)
                return columnDefault.Replace("now", "getdate").Replace("uuid_generate_v4", "newid");

            if (targetDatabase.IsPostgreSQL)
                return columnDefault.Replace("getdate", "now").Replace("newid", "uuid_generate_v4");

            return columnDefault;
        }

        public static string GetConvertedColumnType(DatabaseObjects.Database targetDatabase, string columnType)
        {
            if (targetDatabase.IsSQLServer)
            {
                return columnType
                    .Replace("timestamp with time zone", "datetime")
                    .Replace("uuid", "uniqueidentifier")
                    .Replace("character varying", "nvarchar")
                    .Replace("jsonb", "text")
					.Replace("integer", "int")
                ;
            }
            if (targetDatabase.IsSQLite)
			{
				return columnType
					.Replace("int", "integer")

				;
			}

            //if (targetDatabase.IsPostgreSQL)
            //    return columnType.Replace("getdate()", "now()").Replace("newid()", "uuid_generate_v4()");

            return columnType;
        }

        public static string GetConvertedObjectName(DatabaseObjects.Database targetDatabase, string objectName)
        {
			if (string.IsNullOrEmpty(objectName)) return objectName;

            var colFormat = "{0}";
            if (targetDatabase.IsPostgreSQL)
                colFormat = "\"{0}\"";
            else
                colFormat = "[{0}]";
            return string.Format(colFormat, objectName);
        }

		public static string GetSchemaTableName(DatabaseObjects.Database targetDatabase, Table table)
		{
			var schema = DriverHelper.GetConvertedSchemaName(targetDatabase, table.Schema.SchemaName);
			schema = DriverHelper.GetConvertedObjectName(targetDatabase, schema);
			var tbl = DriverHelper.GetConvertedObjectName(targetDatabase, table.TableName);
			return string.Format("{0}{1}",
				string.IsNullOrEmpty(schema) ? string.Empty : schema + ".", tbl);
		}

        public static string GetConvertedSchemaName(DatabaseObjects.Database targetDatabase, string schemaName)
        {
            if (targetDatabase.IsSQLServer && (schemaName == "public" || string.IsNullOrEmpty(schemaName))) return "dbo";
            if (targetDatabase.IsPostgreSQL && (schemaName == "dto" || string.IsNullOrEmpty(schemaName))) return "public";
			if (targetDatabase.IsSQLite) return "";
            return schemaName;
        }
    }
}
