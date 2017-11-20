using PaJaMa.Common;
using PaJaMa.Database.Library.DatabaseObjects;
using PaJaMa.Database.Library.Synchronization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaJaMa.Database.Library.DataSources
{
	public abstract class DataSource
	{
		public List<DatabaseObjects.Database> Databases { get; private set; }
		public List<string> UnsupportedTypes { get; private set; }
		public DatabaseObjects.Database CurrentDatabase { get; private set; }
		public string DataSourceName { get; private set; }

		public string ConnectionString { get; private set; }
		public abstract string DefaultSchemaName { get; }
		public abstract string GetConvertedObjectName(string objectName);

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
		internal virtual List<string> SystemSchemaNames => new List<string>();
		internal abstract string DatabaseSQL { get; }
		internal abstract List<ColumnType> ColumnTypes { get; }

		internal virtual bool MatchConstraintsByColumns => false;
		internal virtual bool ForeignKeyDropsWithColumns => false;
		internal virtual bool BypassKeyConstraints => false;

		public DataSource(string connectionString)
		{
			ConnectionString = connectionString;
			UnsupportedTypes = new List<string>();
			Databases = getDatabases();
		}

		internal virtual string GetPreTableCreateScript(Table table) { return string.Empty; }

		protected List<DatabaseObjects.Database> getDatabases()
		{
			var databases = new List<DatabaseObjects.Database>();
			using (var conn = OpenConnection())
			{
				this.DataSourceName = conn.DataSource;
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = DatabaseSQL;
					cmd.CommandTimeout = 60000;
					using (var rdr = cmd.ExecuteReader())
					{
						if (rdr.HasRows)
						{
							while (rdr.Read())
							{
								databases.Add(new DatabaseObjects.Database(this, rdr["DatabaseName"].ToString()));
							}
						}
						rdr.Close();
					}
					conn.Close();
				}
				if (string.IsNullOrEmpty(conn.Database))
					this.CurrentDatabase = databases.First();
				else
					this.CurrentDatabase = databases.First(d => d.DatabaseName.ToLower() == conn.Database.ToLower());
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

		internal virtual bool PopulateColumns(DatabaseObjects.Database database, DbCommand cmd, bool includeSystemSchemas, BackgroundWorker worker)
		{
			return false;
		}

		internal virtual bool PopulateForeignKeys(DatabaseObjects.Database database, DbCommand cmd, bool includeSystemSchemas, BackgroundWorker worker)
		{
			return false;
		}

		internal virtual bool PopulateKeyConstraints(DatabaseObjects.Database database, DbCommand cmd, bool includeSystemSchemas, BackgroundWorker worker)
		{
			return false;
		}

		internal virtual bool PopulateIndexes(DatabaseObjects.Database database, DbCommand cmd, bool includeSystemSchemas, BackgroundWorker worker)
		{
			return false;
		}

		internal ColumnType GetColumnType(string dataType)
		{
			try
			{
				return this.ColumnTypes.First(c => c.TypeName == dataType);
			}
			catch (InvalidOperationException)
			{
				if (!UnsupportedTypes.Contains(dataType))
					this.UnsupportedTypes.Add(dataType);
				return this.ColumnTypes.First(c => c.DataType == DataType.Text);
			}
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

		internal virtual string GetColumnAddAlterScript(Column column, bool add, string postScript, string defaultValue)
		{
			return string.Format("ALTER TABLE {0} {6} {1} {2}{3} {4} {5};",
				   column.Table.GetObjectNameWithSchema(this),
				   column.GetQueryObjectName(this),
				   this.GetConvertedColumnType(column.ColumnType.DataType, true),
				   postScript,
				   column.IsNullable ? "NULL" : "NOT NULL",
				   defaultValue,
				   add ? "ADD" : "ALTER COLUMN");
		}

		internal virtual string GetIndexCreateScript(Index index)
		{
			var indexCols = index.IndexColumns.Where(i => i.Ordinal != 0);
			var includeCols = index.IndexColumns.Where(i => i.Ordinal == 0);
			var sb = new StringBuilder();

			sb.AppendLineFormat(@"CREATE {0} {1} INDEX {2} ON {3}
(
	{4}
){5};",
(bool)index.IsUnique ? "UNIQUE" : "",
index.IndexType,
this.GetConvertedObjectName(index.IndexName),
index.Table.GetObjectNameWithSchema(this),
string.Join(",\r\n\t",
indexCols.OrderBy(c => c.Ordinal).Select(c =>
	string.Format("{0} {1}", this.GetConvertedObjectName(c.ColumnName), c.Descending ? "DESC" : "ASC")).ToArray()),
	!includeCols.Any() ? string.Empty : string.Format(@"
INCLUDE (
	{0}
)", string.Join(",\r\n\t",
includeCols.Select(c =>
	string.Format("{0}", this.GetConvertedObjectName(c.ColumnName)).ToString()))
	));

			return sb.ToString();
		}

		internal virtual string GetKeyConstraintCreateScript(KeyConstraint keyConstraint)
		{
			return string.Format(@"CONSTRAINT {0}
{1}
({2})",
	keyConstraint.GetQueryObjectName(this),
	keyConstraint.IsPrimaryKey ? "PRIMARY KEY" : "UNIQUE",
	string.Join(", ",
	  keyConstraint.Columns.OrderBy(c => c.Ordinal).Select(c => this.GetConvertedObjectName(c.ColumnName)
	  )));
		}

		internal virtual string GetForeignKeyCreateScript(ForeignKey foreignKey)
		{
			return string.Format(@"
ALTER TABLE {0} ADD CONSTRAINT {1} FOREIGN KEY({2})
REFERENCES {3} ({4})
ON DELETE {5}
ON UPDATE {6}
;
",
	foreignKey.ChildTable.GetObjectNameWithSchema(this),
	foreignKey.GetQueryObjectName(this),
	string.Join(",", foreignKey.Columns.Select(c => c.ChildColumn.GetQueryObjectName(this)).ToArray()),
	foreignKey.ParentTable.GetObjectNameWithSchema(this),
	string.Join(",", foreignKey.Columns.Select(c => c.ParentColumn.GetQueryObjectName(this)).ToArray()),
	foreignKey.DeleteRule,
	foreignKey.UpdateRule
	);
		}

		internal virtual string GetColumnPostPart(Column column)
		{
			if (column.NumericPrecision != null && column.NumericScale != null &&
				(column.ColumnType.DataType == DataType.Numeric || column.ColumnType.DataType == DataType.Decimal))
				return "(" + column.NumericPrecision.ToString() + ", " + column.NumericScale.ToString() + ")";

			if (column.IsIdentity)
				return this.GetCreateIdentity(column);

			return string.Empty;
		}

		internal string GetConvertedColumnDefault(Column column, string columnDefault)
		{
			if (string.IsNullOrEmpty(columnDefault)) return string.Empty;
			var src = column.Database.DataSource.ColumnTypes.FirstOrDefault(c => c.DefaultValue == columnDefault && c.DataType == column.ColumnType.DataType);
			return src != null ? this.ColumnTypes.First(t => t.DataType == src.DataType).DefaultValue
				: columnDefault;
		}

		internal string GetConvertedColumnType(DataType dataType, bool forCreate)
		{
			return forCreate ? this.ColumnTypes.First(t => t.DataType == dataType).CreateTypeName :
				this.ColumnTypes.First(t => t.DataType == dataType).TypeName;
		}

		internal virtual bool IgnoreDifference(Difference difference, DatabaseObjectBase fromObject, DatabaseObjectBase toObject)
		{
			if (fromObject is Column)
			{
				if (difference.PropertyName == "CharacterMaximumLength")
				{
					if (difference.TargetValue == "-1" && string.IsNullOrEmpty(difference.SourceValue))
						return true;
					if (difference.SourceValue == "-1" && string.IsNullOrEmpty(difference.TargetValue))
						return true;
				}
				else if (difference.PropertyName == "ColumnType")
				{
					if (difference.TargetValue == this.GetConvertedColumnType((DataType)Enum.Parse(typeof(DataType), difference.SourceValue), false))
						return true;
				}
				else if (difference.PropertyName == "ColumnDefault")
				{
					var srcDefault = this.GetConvertedColumnDefault(fromObject as Column, difference.SourceValue);
					var targDefault = difference.TargetValue;
					if (srcDefault.Replace("(", "").Replace(")", "") == targDefault.Replace("(", "").Replace(")", ""))
						return true;
				}
				// TODO:
				//else if ((targetDatabase.IsSQLite || databaseObject.ParentDatabase.IsSQLite) && (diff.PropertyName == "NumericScale" || diff.PropertyName == "NumericPrecision"))
				//	differences.RemoveAt(i);
			}
			return false;
		}

		public static List<Type> GetDataSourceTypes()
		{
			return typeof(DataSource).Assembly.GetTypes()
				.Where(t => t.IsSubclassOf(typeof(DataSource)))
				.OrderBy(d => d.Name)
				.ToList();
		}
	}
}
