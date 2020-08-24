using PaJaMa.Common;
using PaJaMa.Database.Library.DatabaseObjects;
using PaJaMa.Database.Library.Synchronization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

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
		public abstract List<string> SurroundingCharacters { get; }

		protected abstract Type connectionType { get; }

		internal virtual string ExtendedPropertySQL => "";
		internal virtual string DatabasePrincipalSQL => "";
		internal abstract string SchemaSQL { get; }
		internal virtual string RoutineSynonymSQL => "";
		internal abstract string ViewSQL { get; }
		internal virtual string ServerLoginSQL => "";
		internal virtual string PermissionSQL => "";
		internal virtual string CredentialSQL => "";
		internal virtual string SchemaViewSQL => "";
		internal abstract string CombinedSQL { get; }
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

		internal virtual bool ForeignKeyDropsWithColumns => false;
		internal virtual bool BypassKeyConstraints => false;

		internal virtual bool DefaultNamedConstraints => false;
		public bool NamedConstraints { get; set; }

		public DataSource(string connectionString)
		{
			ConnectionString = connectionString;
			UnsupportedTypes = new List<string>();
			Databases = getDatabases();
		}

		internal virtual string GetPreTableCreateScript(Table table) { return string.Empty; }
		internal virtual void CheckUnnecessaryItems(List<SynchronizationItem> items) { }

		protected List<DatabaseObjects.Database> getDatabases()
		{
			var databases = new List<DatabaseObjects.Database>();
			using (var conn = OpenConnection(string.Empty))
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

		public DbConnection OpenConnection(string database)
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

		private List<TDatabaseObject> populateObjects<TDatabaseObject>(DatabaseObjects.Database database, DbCommand cmd, string query, string forSchema, bool includeSystemSchemas, string additionalPreWhere, string additionalPostWhere, BackgroundWorker worker)
			where TDatabaseObject : DatabaseObjectBase
		{
			if (string.IsNullOrEmpty(query)) return new List<TDatabaseObject>();

			if (worker != null)
			{
				if (worker.CancellationPending) return null;
				worker.ReportProgress(0, $"Populating {typeof(TDatabaseObject).Name.CamelCaseToSpaced()}s for {database.DatabaseName}...");
			}

			var objs = new List<TDatabaseObject>();
			query += additionalPreWhere;
			if (!string.IsNullOrEmpty(forSchema))
			{
				query = $@"select * from (
				{query}
				) z where {GetConvertedObjectName("SchemaName")} = '{forSchema}'";
			}
			else if (!includeSystemSchemas && SystemSchemaNames.Count > 0)
			{
				query = $@"select * from (
				{query}
				) z where {GetConvertedObjectName("SchemaName")} is null or {GetConvertedObjectName("SchemaName")} not in ({string.Join(", ", SystemSchemaNames.Select(s => "'" + s + "'").ToArray())})";
			}
			query += additionalPostWhere;
			cmd.CommandText = query;
			cmd.CommandTimeout = 60000;
			using (var rdr = cmd.ExecuteReader())
			{
				if (rdr.HasRows)
				{
					while (rdr.Read())
					{
						var obj = rdr.ToObject<TDatabaseObject>(database);
						objs.Add(obj);
						var values = new Dictionary<string, object>();
						for (int i = 0; i < rdr.FieldCount; i++)
						{
							var col = rdr.GetName(i);
							values.Add(col, rdr[col]);
						}
						obj.RawValues = values;
						if (worker != null)
						{
							if (worker.CancellationPending)
							{
								rdr.Close();
								return null;
							}
							worker.ReportProgress(0, $"Populating {typeof(TDatabaseObject).Name.CamelCaseToSpaced()}s for {database.DatabaseName} - {obj.ProgressDisplay}...");
						}
					}
				}
				rdr.Close();
			}
			foreach (var obj in objs)
			{
				obj.setObjectProperties(cmd.Connection, obj.RawValues);
			}
			return objs;
		}

		private List<View> _schemaViews;
		public virtual List<View> GetSchemaViews(DbConnection connection, DatabaseObjects.Database database)
		{
			// if (_schemaViews != null) return _schemaViews;
			_schemaViews = new List<View>();
			if (string.IsNullOrEmpty(this.SchemaViewSQL)) return _schemaViews;
			using (var cmd = connection.CreateCommand())
			{
				_schemaViews = populateObjects<View>(database, cmd, string.Format(this.SchemaViewSQL, database.DatabaseName), string.Empty, true, string.Empty, string.Empty, null);
			}
			return _schemaViews;
		}

		public virtual void PopulateTables(DbConnection connection, Schema[] schemas, bool andChildren)
		{ 
			// TODO: assumes all schemas are from same db
			var conn = connection ?? OpenConnection(schemas.First().Database.DatabaseName);
			using (var cmd = conn.CreateCommand())
			{
				foreach (var schema in schemas)
				{
					populateObjects<Table>(schema.Database, cmd, string.Format(this.TableSQL, schema.Database.DatabaseName), schema.SchemaName, true, string.Empty, string.Empty, null);
					if (andChildren) PopulateColumns(schema.Database, cmd, schema.SchemaName, false, null);
				}

				if (andChildren)
				{
					foreach (var schema in schemas)
					{
						if (schema.Tables.Count > 0)
						{
							var qry = $"select * from ({{0}}) z where ChildTableName in ({string.Join(",", schema.Tables.Select(t => "'" + t.TableName + "'"))})";

							PopulateForeignKeys(schema.Database, cmd, schema.SchemaName, false, null);

							qry = $"select * from ({{0}}) z where TableName in ({string.Join(",", schema.Tables.Select(t => "'" + t.TableName + "'"))})";

							PopulateKeyConstraints(schema.Database, cmd, true, null);
							PopulateIndexes(schema.Database, cmd, true, null);
							if (!string.IsNullOrEmpty(this.DefaultConstraintSQL))
								populateObjects<DefaultConstraint>(schema.Database, cmd, string.Format(qry, string.Format(this.DefaultConstraintSQL, schema.Database.DatabaseName)), string.Empty, true, string.Empty, string.Empty, null);
							if (!string.IsNullOrEmpty(this.TriggerSQL))
								populateObjects<Trigger>(schema.Database, cmd, string.Format(qry, string.Format(this.TriggerSQL, schema.Database.DatabaseName)), string.Empty, true, string.Empty, string.Empty, null);
						}
					}
				}
			}
			if (connection == null)
			{
				conn.Close();
				conn.Dispose();
			}
		}

		private void populateChildren<TDatabaseObject>(DbConnection connection, DatabaseObjectBase parent, List<TDatabaseObject> arrayToClear, string sql, string additionalPreWhere, string additionalPostWhere)
			where TDatabaseObject : DatabaseObjectBase
		{
			var conn = connection ?? OpenConnection(parent.Database.DatabaseName);
			// TODO: assumes all schemas are from same db
			using (var cmd = conn.CreateCommand())
			{
				arrayToClear.Clear();
				populateObjects<TDatabaseObject>(parent.Database, cmd, string.Format(sql, parent.Database.DatabaseName), parent.Schema.SchemaName, true, additionalPreWhere, additionalPostWhere, null);
			}
			if (connection == null)
			{
				conn.Close();
				conn.Dispose();
			}
		}

		protected virtual string childColumnsWhere(DatabaseObjectWithColumns obj) => $" and co.TABLE_NAME = '{obj.ObjectName}'" + (string.IsNullOrEmpty(obj.Schema.SchemaName) ? string.Empty : $" and co.TABLE_SCHEMA = '{obj.Schema.SchemaName}'");
		public virtual void PopulateChildColumns(DbConnection connection, DatabaseObjectWithColumns obj)
		{
			populateChildren<Column>(connection, obj, obj.Columns, this.ColumnSQL, childColumnsWhere(obj), string.Empty);
		}

		protected virtual string foreignKeysTableWhere(Table table) => $" and {GetConvertedObjectName("ChildTableName")} = '{table.TableName}'";
		public virtual void PopulateForeignKeysForTable(DbConnection connection, Table table)
		{
			populateChildren<ForeignKey>(connection, table, table.ForeignKeys, this.ForeignKeySQL, string.Empty, foreignKeysTableWhere(table));
		}

		protected virtual string keysTableWhere(Table table) => $" and {GetConvertedObjectName("TableName")} = '{table.TableName}'";
		public virtual void PopulateKeysForTable(DbConnection connection, Table table)
		{
			populateChildren<KeyConstraint>(connection, table, table.KeyConstraints, this.KeyConstraintSQL, string.Empty, keysTableWhere(table));
		}

		protected virtual string constraintsTableWhere(Table table) => $" and {GetConvertedObjectName("TableName")} = '{table.TableName}'";
		public virtual void PopulateConstraintsForTable(DbConnection connection, Table table)
		{
			populateChildren<DefaultConstraint>(connection, table, table.DefaultConstraints, this.DefaultConstraintSQL, string.Empty, constraintsTableWhere(table));
		}

		protected virtual string triggersTableWhere(Table table) => $" and {GetConvertedObjectName("TableName")} = '{table.TableName}'";
		public virtual void PopulateTriggersForTable(DbConnection connection, Table table)
		{
			populateChildren<Trigger>(connection, table, table.Triggers, this.TriggerSQL, string.Empty, triggersTableWhere(table));
		}

		protected virtual string indexesTableWhere(Table table) => $" and {GetConvertedObjectName("TableName")} = '{table.TableName}'";
		public virtual void PopulateIndexesForTable(DbConnection connection, Table table)
		{
			populateChildren<Index>(connection, table, table.Indexes, this.IndexSQL, string.Empty, indexesTableWhere(table));
		}

		public void PopulateSchemas(DbConnection connection, DatabaseObjects.Database database)
		{
			database.Schemas = new List<Schema>();
			if (string.IsNullOrEmpty(this.SchemaSQL))
				database.Schemas.Add(new Schema(database) { SchemaName = "" });
			else
			{
				var conn = connection ?? OpenConnection(database.DatabaseName);
				using (var cmd = conn.CreateCommand())
				{
					populateObjects<Schema>(database, cmd, string.Format(this.SchemaSQL, database.DatabaseName), string.Empty, false, string.Empty, string.Empty, null);
				}
				if (connection == null)
				{
					conn.Dispose();
					conn = null;
				}
			}
		}

		public void PopulateViews(Schema schema)
		{
			using (var conn = OpenConnection(schema.Database.DatabaseName))
			{
				using (var cmd = conn.CreateCommand())
				{
					populateObjects<View>(schema.Database, cmd, string.Format(this.ViewSQL, schema.Database.DatabaseName), schema.SchemaName, true, string.Empty, string.Empty, null);
				}
				conn.Close();
			}
		}

		public void PopulateRoutinesSynonyms(Schema schema)
		{
			using (var conn = OpenConnection(schema.Database.DatabaseName))
			{
				using (var cmd = conn.CreateCommand())
				{
					populateObjects<RoutineSynonym>(schema.Database, cmd, string.Format(this.RoutineSynonymSQL, schema.Database.DatabaseName), schema.SchemaName, true, string.Empty, string.Empty, null);
				}
				conn.Close();
			}
		}

		public void PopulateTablesAndColumns(DatabaseObjects.Database database, BackgroundWorker worker)
		{
			if (database == null) database = CurrentDatabase;
			database.Schemas = new List<Schema>();
			var fks = new List<ForeignKey>();
			using (var conn = OpenConnection(database.DatabaseName))
			{
				using (var cmd = conn.CreateCommand())
				{
					if (string.IsNullOrEmpty(this.SchemaSQL))
						database.Schemas.Add(new Schema(database) { SchemaName = "" });
					else
						populateObjects<Schema>(database, cmd, string.Format(this.SchemaSQL, database.DatabaseName), string.Empty, false, string.Empty, string.Empty, worker);

					cmd.CommandText = string.Format(this.CombinedSQL, database.DatabaseName);
					using (var rdr = cmd.ExecuteReader())
					{
						if (rdr.HasRows)
						{
							while (rdr.Read())
							{
								var rawTbl = rdr.ToObject<Table>(database);
								var schema = database.Schemas.First(s => s.SchemaName == rawTbl.SchemaName);
								var tbl = schema.Tables.FirstOrDefault(t => t.TableName == rawTbl.TableName);
								if (tbl == null)
								{
									tbl = rawTbl;
									if (worker != null) worker.ReportProgress(0, $"Populating Table {tbl.TableName} for {database.DatabaseName}...");
									tbl.Schema = schema;
									schema.Tables.Add(tbl);
								}

								var col = rdr.ToObject<Column>(database);
								if (!tbl.Columns.Any(c => c.ColumnName == col.ColumnName))
								{
									col.Schema = schema;
									// col.ColumnType = database.DataSource.GetColumnType(rdr["DataType"].ToString(), col.ColumnDefault);
									col.Parent = tbl;
									tbl.Columns.Add(col);
								}

								if (rdr["ForeignKeyName"] != DBNull.Value)
								{
									var fk = rdr.ToObject<ForeignKey>(database);
									// TODO: WHY???
									if (fk.ParentTableSchema == "tmp" || fk.ChildTableSchema == "tmp") continue;
									fk.Schema = schema;
									// tbl.ForeignKeys.Add(fk);
									fks.Add(fk);
								}
								
								//var values = new Dictionary<string, object>();
								//for (int i = 0; i < rdr.FieldCount; i++)
								//{
								//	var col = rdr.GetName(i);
								//	values.Add(col, rdr[col]);
								//}
								//obj.RawValues = values;
								//if (worker != null) worker.ReportProgress(0, $"Populating {typeof(TDatabaseObject).Name.CamelCaseToSpaced()}s for {database.DatabaseName} - {obj.ProgressDisplay}...");
							}
						}
						rdr.Close();
					}
				}

				// PopulateTables(conn, database.Schemas.ToArray(), false);
				foreach (var fk in fks)
				{
					fk.ParentTable = database.Schemas.Find(s => s.SchemaName == fk.ParentTableSchema).Tables.Find(t => t.TableName == fk.ParentTableName);
					fk.ChildTable = database.Schemas.Find(s => s.SchemaName == fk.ChildTableSchema).Tables.Find(t => t.TableName == fk.ChildTableName);
					fk.Columns.Add(new ForeignKeyColumn()
					{
						ChildColumn = fk.ChildTable.Columns.First(c => c.ColumnName == fk.ChildColumnName),
						ParentColumn = fk.ParentTable.Columns.First(c => c.ColumnName == fk.ParentColumnName)
					});
					var curr = fk.ChildTable.ForeignKeys.FirstOrDefault(x => x.ForeignKeyName == fk.ForeignKeyName);
					if (curr != null)
					{
						curr.Columns.AddRange(fk.Columns);
					}
					else
					{
						fk.ChildTable.ForeignKeys.Add(fk);
					}
				}
			}
		}

		public void PopulateChildren(DatabaseObjects.Database database, bool condensed, BackgroundWorker worker)
		{
			if (database == null) database = CurrentDatabase;
			database.ExtendedProperties = new List<ExtendedProperty>();
			database.Schemas = new List<Schema>();
			database.ServerLogins = new List<ServerLogin>();
			database.Principals = new List<DatabasePrincipal>();
			database.Permissions = new List<Permission>();
			database.Credentials = new List<Credential>();
			database.Extensions = new List<Extension>();

			using (var conn = OpenConnection(database.DatabaseName))
			{
				using (var cmd = conn.CreateCommand())
				{
					populateObjects<ExtendedProperty>(database, cmd, string.Format(this.ExtendedPropertySQL, database.DatabaseName), string.Empty, false, string.Empty, string.Empty, worker);
					populateObjects<DatabasePrincipal>(database, cmd, string.Format(this.DatabasePrincipalSQL, database.DatabaseName), string.Empty, true, string.Empty, string.Empty, worker);
					foreach (var dp in database.Principals)
					{
						if (dp.OwningPrincipalID > 0)
						{
							dp.Owner = database.Principals.First(p => p.PrincipalID == dp.OwningPrincipalID);
							dp.Owner.Ownings.Add(dp);
						}
					}
					if (string.IsNullOrEmpty(this.SchemaSQL))
						database.Schemas.Add(new Schema(database) { SchemaName = "" });
					else
						populateObjects<Schema>(database, cmd, string.Format(this.SchemaSQL, database.DatabaseName), string.Empty, false, string.Empty, string.Empty, worker);

					populateObjects<RoutineSynonym>(database, cmd, string.Format(this.RoutineSynonymSQL, database.DatabaseName), string.Empty, false, string.Empty, string.Empty, worker);
					populateObjects<View>(database, cmd, string.Format(this.ViewSQL, database.DatabaseName), string.Empty, false, string.Empty, string.Empty, worker);
					if (!condensed)
					{
						populateObjects<ServerLogin>(database, cmd, string.Format(this.ServerLoginSQL, database.DatabaseName), string.Empty, true, string.Empty, string.Empty, worker);
						populateObjects<Permission>(database, cmd, string.Format(this.PermissionSQL, database.DatabaseName), string.Empty, true, string.Empty, string.Empty, worker);
						populateObjects<Credential>(database, cmd, string.Format(this.CredentialSQL, database.DatabaseName), string.Empty, true, string.Empty, string.Empty, worker);
					}
					populateObjects<Table>(database, cmd, string.Format(this.TableSQL, database.DatabaseName), string.Empty, false, string.Empty, string.Empty, worker);
					PopulateColumns(database, cmd, string.Empty, false, worker);
					PopulateForeignKeys(database, cmd, string.Empty, false, worker);
					PopulateKeyConstraints(database, cmd, false, worker);
					PopulateIndexes(database, cmd, false, worker);

					populateObjects<DefaultConstraint>(database, cmd, string.Format(this.DefaultConstraintSQL, database.DatabaseName), string.Empty, false, string.Empty, string.Empty, worker);
					populateObjects<Trigger>(database, cmd, string.Format(this.TriggerSQL, database.DatabaseName), string.Empty, false, string.Empty, string.Empty, worker);
					populateObjects<Sequence>(database, cmd, string.Format(this.SequenceSQL, database.DatabaseName), string.Empty, false, string.Empty, string.Empty, worker);
					populateObjects<Extension>(database, cmd, string.Format(this.ExtensionSQL, database.DatabaseName), string.Empty, false, string.Empty, string.Empty, worker);
				}
				conn.Close();
			}
		}

		internal virtual void PopulateColumns(DatabaseObjects.Database database, DbCommand cmd, string forSchema, bool includeSystemSchemas, BackgroundWorker worker)
		{
			populateObjects<Column>(database, cmd, string.Format(this.ColumnSQL, database.DatabaseName), forSchema, false, string.Empty, string.Empty, worker);
		}

		internal virtual void PopulateForeignKeys(DatabaseObjects.Database database, DbCommand cmd, string schemaName, bool includeSystemSchemas, BackgroundWorker worker)
		{
			populateObjects<ForeignKey>(database, cmd, string.Format(this.ForeignKeySQL, database.DatabaseName), schemaName, false, string.Empty, string.Empty, worker);
		}

		internal virtual void PopulateKeyConstraints(DatabaseObjects.Database database, DbCommand cmd, bool includeSystemSchemas, BackgroundWorker worker)
		{
			populateObjects<KeyConstraint>(database, cmd, string.Format(this.KeyConstraintSQL, database.DatabaseName), string.Empty, false, string.Empty, string.Empty, worker);
		}

		internal virtual void PopulateIndexes(DatabaseObjects.Database database, DbCommand cmd, bool includeSystemSchemas, BackgroundWorker worker)
		{
			populateObjects<Index>(database, cmd, string.Format(this.IndexSQL, database.DatabaseName), string.Empty, false, string.Empty, string.Empty, worker);
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
				   column.Parent.GetObjectNameWithSchema(this),
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

		internal virtual string GetIndexDropScript(Index index)
		{
			return string.Format("DROP INDEX {0}.{1}",
				index.Table.GetObjectNameWithSchema(this),
				index.GetQueryObjectName(this));
		}

		internal virtual string GetForeignKeyDropScript(ForeignKey foreignKey)
		{
			return "ALTER TABLE {0} DROP CONSTRAINT {1};";
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
			var parts = columnDefault.Split(':');
			if (parts.Length > 1 && column.Database.DataSource.GetType() != this.GetType())
				columnDefault = parts[0];
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

		[Obsolete("Override getDiff in object")]
		internal virtual bool IgnoreDifference(Difference difference, DataSource fromDataSource, DataSource toDataSource, DatabaseObjectBase fromObject, DatabaseObjectBase toObject)
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
					var targetType = toDataSource.ColumnTypes.First(t => t.CreateTypeName == difference.TargetValue || t.TypeName == difference.TargetValue);
					var srcType = fromDataSource.ColumnTypes.First(t => t.CreateTypeName == difference.SourceValue || t.TypeName == difference.SourceValue);
					if (srcType.DataType == targetType.DataType)
					{
						return true;
					}
					else if (toDataSource.GetType() != fromDataSource.GetType())
					{
						var targetEquivalent = toDataSource.ColumnTypes.FirstOrDefault(c => c.DataType == srcType.DataType);
						if (targetEquivalent != null && targetEquivalent.TypeName == difference.TargetValue)
							return true;
					}
				}
				else if (difference.PropertyName == "ColumnDefault")
				{
					var srcDefault = toDataSource.GetColumnDefault(fromObject as Column, difference.SourceValue);
					var targDefault = toDataSource.GetColumnDefault(fromObject as Column, difference.TargetValue);
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

		public virtual List<string> GetReservedKeywords()
		{
			var keywords = new List<string>();
			keywords.Add("SELECT");
			keywords.Add("FROM");
			keywords.Add("LEFT");
			keywords.Add("INNER");
			keywords.Add("JOIN");
			keywords.Add("ON");
			keywords.Add("COALESCE");
			keywords.Add("LIMIT");
			keywords.Add("ORDER");
			keywords.Add("DESC");
			keywords.Add("WHERE");
			keywords.Add("IS");
			keywords.Add("NOT");
			keywords.Add("NULL");
			keywords.Add("SET");
			keywords.Add("AND");
			keywords.Add("OR");
			keywords.Add("MAX");
			keywords.Add("MIN");
			keywords.Add("INSERT");
			keywords.Add("UPDATE");
			keywords.Add("DELETE");
			keywords.Add("INTO");
			keywords.Add("VALUES");
			keywords.Add("CASE");
			keywords.Add("WHEN");
			keywords.Add("THEN");
			keywords.Add("ELSE");
			keywords.Add("END");
			keywords.Add("GROUP");
			keywords.Add("BY");
			keywords.Add("AS");
			keywords.Add("IF");
			keywords.Add("HAVING");
			keywords.Add("COUNT");
			keywords.Add("ALTER");
			keywords.Add("TABLE");
			keywords.Add("ADD");
			keywords.Add("CREATE");
			keywords.Add("TOP");
			keywords.Add("DISTINCT");
			keywords.Add("DEFAULT");
			keywords.Add("IDENTITY");
			keywords.Add("UNION");
			keywords.Add("ALL");
			keywords.Add("LIKE");
			keywords.Add("REPLACE");
			return keywords;
		}

		private string getColumnInsert(Column column, object value)
		{
			if (value == DBNull.Value || value == null) return "NULL";
			var dataType = column.ColumnType.DataType;
			var attr = Common.EnumHelper.GetAttribute<DbTypeAttribute>(dataType);
			// TODO: qi
			if ((attr.DbType != System.Data.DbType.Int16 &&
				attr.DbType != System.Data.DbType.Int32 &&
				attr.DbType != System.Data.DbType.Int64 &&
				attr.DbType != System.Data.DbType.Double &&
				attr.DbType != System.Data.DbType.Decimal) || value is string)
			{
				return $"'{value.ToString().Replace("'", "''")}'";
			}
			var val = value.ToString().Replace("'", "''");
			return string.IsNullOrEmpty(val) ? "0" : val;
		}

		public virtual string GetInsertScript(DbConnection connection, Table table, string filter)
		{
			if (!table.Columns.Any()) this.PopulateChildColumns(connection, table);
			var sbScript = new StringBuilder();
			var colsLine = string.Join(", ", table.Columns.Select(c => GetConvertedObjectName(c.ColumnName)));
			var tableLine = GetConvertedObjectName(table.Database.DatabaseName) + "." +
					(table.Schema != null && !string.IsNullOrEmpty(table.Schema.SchemaName) ? $"{GetConvertedObjectName(table.Schema.SchemaName)}." : "") + GetConvertedObjectName(table.TableName);
			sbScript.AppendLine($"INSERT INTO {tableLine} ({colsLine})");
			using (var cmd = connection.CreateCommand())
			{
				cmd.CommandText = $"select {colsLine} from {tableLine} {filter}";
				using (var rdr = cmd.ExecuteReader())
				{
					bool firstIn = true;
					while (rdr.HasRows && rdr.Read())
					{
						string insertLine = "SELECT " + string.Join(", ", table.Columns.Select(c => this.getColumnInsert(c, rdr[c.ColumnName])));

						if (!firstIn) sbScript.AppendLine("UNION ALL");
						sbScript.AppendLine(insertLine);
						firstIn = false;
					}

				}
			}
			return sbScript.ToString();
		}

		public virtual string GetRenameScript(DatabaseObjectBase databaseObject, string targetName)
		{
			throw new NotImplementedException();
		}

		// TODO: schemas??
		public virtual List<DatabaseObjectBase> SearchObjects(DbConnection connection, string databaseSearch, string tableSearch, string columnSearch, BackgroundWorker worker = null)
		{
			if (connection.State != ConnectionState.Open)
				connection.Open();

			var objs = new List<DatabaseObjectBase>();
			using (var cmd = connection.CreateCommand())
			{
				int totalProgress = Databases.Count;
				int currProgress = 1;
				// TODO: performance?
				foreach (var db in Databases)
				{
					if (db.DatabaseName == "information_schema") continue;
					if (!string.IsNullOrEmpty(databaseSearch) && !db.DatabaseName.ToLower().Contains(databaseSearch.ToLower())) continue;

					if (worker.CancellationPending) return null;
					if (worker != null) worker.ReportProgress(100 * (currProgress++) / totalProgress, "Searching " + db.DatabaseName);
					var dt = new System.Data.DataTable();
					if (!string.IsNullOrEmpty(columnSearch))
					{
						cmd.CommandText = $"select SchemaName, TableName, ColumnName from ({string.Format(this.ColumnSQL, db.DatabaseName)}) z where ColumnName like '{columnSearch.Replace("'", "''").Replace("*", "%")}'";
						if (!string.IsNullOrEmpty(tableSearch))
							cmd.CommandText += $" and TableName like '{tableSearch.Replace("'", "''").Replace("*", "%")}'";
						using (var rdr = cmd.ExecuteReader()) dt.Load(rdr);
					}
					else if (!string.IsNullOrEmpty(tableSearch))
					{
						cmd.CommandText = $"select SchemaName, TableName, null as ColumnName from ({string.Format(this.TableSQL, db.DatabaseName)}) z where TableName like '{tableSearch.Replace("'", "''").Replace("*", "%")}'";
						using (var rdr = cmd.ExecuteReader()) dt.Load(rdr);
					}

					foreach (System.Data.DataRow dataRow in dt.Rows)
					{
						if (db.Schemas == null || !db.Schemas.Any()) PopulateSchemas(connection, db);
						var schema = string.IsNullOrEmpty(dataRow["SchemaName"].ToString()) ? db.Schemas[0] : db.Schemas.First(s => s.SchemaName == dataRow["SchemaName"].ToString());
						if (!schema.Tables.Any()) PopulateTables(connection, new Schema[] { schema }, false);
						var tbl = schema.Tables.First(t => t.TableName == dataRow["TableName"].ToString());
						if (dataRow["ColumnName"] != DBNull.Value)
						{
							if (!tbl.Columns.Any()) PopulateChildColumns(connection, tbl);
							objs.Add(tbl.Columns.First(c => c.ColumnName == dataRow["ColumnName"].ToString()));
						}
						else
						{
							objs.Add(tbl);
						}
					}
				}
			}
			return objs;
		}
	}
}
