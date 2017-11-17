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
    public abstract class DataSource
    {
        public List<Database> Databases { get; private set; }
        public Database CurrentDatabase { get; private set; }
        public string DataSourceName { get; private set; }

        public string ConnectionString { get; private set; }
        public abstract string DefaultSchemaName { get; }

        protected abstract Type connectionType { get; }

        internal virtual string ExtendedPropertySQL => "";
        internal virtual string DatabasePrincipalSQL => "";
        internal abstract string SchemaSQL { get; }
        internal virtual string RoutineSynonymSQL => "";
        internal abstract string ViewSQL { get; }
        internal virtual string ServerLoginSQL => "";
        internal virtual string PermissionSQL => "";
        internal virtual string CredentialSQL => "";
        internal abstract string TableSQL { get; }
        internal abstract string ColumnSQL { get; }
        internal abstract string ForeignKeySQL { get; }
        internal abstract string KeyConstraintSQL { get; }
        internal abstract string IndexSQL { get; }
        internal abstract string DefaultConstraintSQL { get; }
        internal abstract string TriggerSQL { get; }
        internal virtual string SequenceSQL => "";
        internal virtual string ExtensionSQL => "";
        internal abstract string DatabaseSQL { get; }

        internal abstract string GetConvertedObjectName(string objectName);
        internal abstract string GetConvertedColumnType(string columnType);

        internal virtual bool BypassConstraints => false;
        internal virtual bool BypassKeyConstraints => false;
        internal virtual bool MatchForeignKeyTablesAndColumns => false;
        internal virtual bool ForeignKeyDropsWithColumns => false;
        internal virtual bool CheckForeignKeys => false;
        internal virtual bool BypassForeignKeyRules => false;

        internal virtual string GetPostTableCreateScript(Table table) { return string.Empty; }

        public DataSource(string connectionString)
        {
            ConnectionString = connectionString;
            Databases = getDatabases();
        }

        protected virtual void DatabaseInitializing(DbConnection conn) { }

        internal virtual string GetConvertedColumnDefault(string columnDefault)
        {
            return columnDefault;
        }

        protected List<Database> getDatabases()
        {
            var databases = new List<Database>();
            using (var conn = OpenConnection())
            {
                this.DataSourceName = conn.DataSource;
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = DatabaseSQL;
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                        {
                            while (rdr.Read())
                            {
                                databases.Add(new Database(this, rdr["DatabaseName"].ToString()));
                            }
                        }
                        rdr.Close();
                    }
                    conn.Close();
                }
                this.CurrentDatabase = databases.First(d => d.DatabaseName == conn.Database);
            }
            return databases;
        }

        public DbConnection OpenConnection(string database = null)
        {
            var conn = Activator.CreateInstance(connectionType) as DbConnection;
            conn.ConnectionString = ConnectionString;
            conn.Open();
            if (Databases != null)
            {
                if (!string.IsNullOrEmpty(database))
                    CurrentDatabase = Databases.First(d => d.DatabaseName == database);

                if (CurrentDatabase.DatabaseName != conn.Database)
                    conn.ChangeDatabase(CurrentDatabase.DatabaseName);
            }
            return conn;
        }

        public void ChangeDatabase(string newDatabase)
        {
            CurrentDatabase = Databases.First(d => d.DatabaseName == newDatabase);
        }

        internal virtual bool PopulateColumns(Database database, DbCommand cmd, BackgroundWorker worker)
        {
            return false;
        }

        internal virtual bool PopulateForeignKeys(Database database, DbCommand cmd, BackgroundWorker worker)
        {
            return false;
        }

        internal virtual bool PopulateKeyConstraints(Database database, DbCommand cmd, BackgroundWorker worker)
        {
            return false;
        }

        internal virtual bool PopulateIndexes(Database database, DbCommand cmd, BackgroundWorker worker)
        {
            return false;
        }

        public virtual string GetPreTopN(int topN)
        {
            return string.Empty;
        }

        public virtual string GetPostTopN(int topN)
        {
            return string.Empty;
        }

        public virtual string GetColumnSelectList(string[] columns)
        {
            return "*";
        }

        internal virtual string GetIdentityInsertOn(Table table)
        {
            return string.Format("SET IDENTITY_INSERT {0} ON",
                            table.GetObjectNameWithSchema(this));
        }

        internal virtual string GetIdentityInsertOff(Table table)
        {
            return string.Format("SET IDENTITY_INSERT {0} OFF",
                            table.GetObjectNameWithSchema(this));
        }

        internal virtual string GetCreateIdentity(Column column)
        {
            return string.Format(" IDENTITY(1,{0})", column.Increment.GetValueOrDefault(1));
        }

        internal virtual string GetColumnAddAlterScript(Column column, bool add, string defaultValue, string postScript)
        {
            return string.Format("ALTER TABLE {0} {6} {1} {2}{3} {4} {5};",
                   column.Table.GetObjectNameWithSchema(this),
                   column.GetQueryObjectName(this),
                   column.DataType,
                   postScript,
                   column.IsNullable ? "NULL" : "NOT NULL",
                   defaultValue,
                   add ? "ADD" : "ALTER COLUMN");
        }

        public static List<Type> GetDataSourceTypes()
        {
            return typeof(DataSource).Assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(DataSource)))
                .ToList();
        }
    }

    public class ConnectionTypeAttribute : Attribute
    {
        public Type ConnectionType { get; private set; }
        public ConnectionTypeAttribute(Type connectionType)
        {
            this.ConnectionType = connectionType;
        }
    }
}
