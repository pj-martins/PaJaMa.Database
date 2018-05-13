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
		public abstract List<ColumnType> ColumnTypes { get; }

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
		
		internal virtual bool MatchConstraintsByColumns => false;
		internal virtual bool ForeignKeyDropsWithColumns => false;
		internal virtual bool BypassKeyConstraints => false;
		internal virtual bool NamedConstraints => false;

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
				if (string.IsNullOrEmpty(DatabaseSQL))
				{
					databases.Add(new DatabaseObjects.Database(this, conn.Database));
					this.CurrentDatabase = databases[0];
				}
				else
				{
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

		internal ColumnType GetColumnType(string dataType, string columnDefault)
		{
			try
			{
				ColumnType columnType = null;
				if (!string.IsNullOrEmpty(columnDefault))
					columnType = this.ColumnTypes.FirstOrDefault(c => c.TypeName == dataType);
				if (columnType == null)
					columnType = this.ColumnTypes.First(c => c.TypeName == dataType);
				return columnType;
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

		internal virtual string GetColumnAddAlterScript(Column column, Column targetColumn, string postScript, string defaultValue)
		{
			var colType = this.GetConvertedColumnType(column.ColumnType.DataType, true);
			if (this.GetType() == column.Database.DataSource.GetType())
				colType = column.ColumnType.TypeName;
			return string.Format("ALTER TABLE {0} {6} {1} {2}{3} {4} {5};",
				   column.Table.GetObjectNameWithSchema(this),
				   column.GetQueryObjectName(this),
				   colType,
				   postScript,
				   column.IsNullable ? "NULL" : "NOT NULL",
				   defaultValue,
				   targetColumn == null ? "ADD" : "ALTER COLUMN");
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
			if (column.NumericPrecision != null && (column.ColumnType.DataType == DataType.Numeric || column.ColumnType.DataType == DataType.Decimal))
				return "(" + column.NumericPrecision.ToString() + ", " + column.NumericScale.ToString() + ")";

			if (column.IsIdentity)
				return this.GetCreateIdentity(column);

			return string.Empty;
		}

		internal virtual string GetColumnDefault(Column column, string columnDefault)
		{
			if (string.IsNullOrEmpty(columnDefault)) return string.Empty;
			var src = column.Database.DataSource.ColumnTypes.First(c => c.DataType == column.ColumnType.DataType);
			var srcmapped = src.MappedValues.FirstOrDefault(m => m.SqlValue == columnDefault);
			if (srcmapped != null)
			{
				var dest = this.ColumnTypes.First(c => c.DataType == src.DataType)
					.MappedValues.FirstOrDefault(m => m.Value.ToString() == srcmapped.Value.ToString());
				if (dest != null)
					return dest.SqlValue;
				columnDefault = srcmapped.Value.ToString();
			}
			if (column.ColumnType.DataType == DataType.DateTime
				|| column.ColumnType.DataType == DataType.SmallDateTime)
			{
				DateTime tempDT;
				if (DateTime.TryParse(columnDefault.Replace("(", "").Replace(")", "").Replace("'", ""), out tempDT))
					return "('" + tempDT.ToString() + "')";
			}

			return columnDefault;
		}

		public virtual string GetConvertedColumnType(Column column, bool forCreate)
		{
			if (column.Database.DataSource.GetType() == this.GetType())
				return forCreate ? column.ColumnType.CreateTypeName : column.ColumnType.TypeName;

			return GetConvertedColumnType(column.ColumnType.DataType, forCreate);
		}

		public string GetConvertedColumnType(DataType dataType, bool forCreate)
		{
			return forCreate ? this.ColumnTypes.First(t => t.DataType == dataType).CreateTypeName :
				this.ColumnTypes.First(t => t.DataType == dataType).TypeName;
		}

		private string maxReplacement(string max)
		{
			return max
				.Replace("-1", "")
				.Replace((int.MaxValue / 2).ToString(), "")
				.Replace(int.MaxValue.ToString(), "")
				;
		}

		internal virtual bool IgnoreDifference(Difference difference, DatabaseObjectBase fromObject, DatabaseObjectBase toObject)
		{
			if (fromObject is Column)
			{
				if (difference.PropertyName == "NumericScale" || difference.PropertyName == "NumericPrecision")
				{
					if ((fromObject as Column).ColumnType.DataType != DataType.Numeric && (fromObject as Column).ColumnType.DataType != DataType.Decimal)
						return true;
				}
				else if (difference.PropertyName == "CharacterMaximumLength")
				{
					if (maxReplacement(difference.TargetValue) == maxReplacement(difference.SourceValue))
						return true;
				}
				else if (difference.PropertyName == "ColumnType")
				{
					if (difference.TargetValue == this.GetConvertedColumnType((DataType)Enum.Parse(typeof(DataType), difference.SourceValue, true), false))
						return true;
				}
				else if (difference.PropertyName == "ColumnDefault")
				{
					var srcDefault = this.GetColumnDefault(fromObject as Column, difference.SourceValue);
					var targDefault = this.GetColumnDefault(fromObject as Column, difference.TargetValue);
					if (srcDefault.Replace("(", "").Replace(")", "") == targDefault.Replace("(", "").Replace(")", ""))
						return true;
				}
			}
			return false;
		}

		internal virtual bool IgnoreDrop(DatabaseObjectBase sourceParent, DatabaseObjectBase obj)
		{
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
