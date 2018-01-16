using PaJaMa.Database.Library.DatabaseObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PaJaMa.Database.Library.DataSources
{
	public class SQLiteDataSource : SqlServerDataSource
	{
		public SQLiteDataSource(string connectionString) : base(connectionString)
		{
		}

		internal override string ViewSQL => "select *, sql as Definition, name as ViewName, '' as SchemaName from sqlite_master where type = 'view'";
		internal override string TriggerSQL => "select *, sql as Definition, name as TriggerName, tbl_name as TableName, '' as SchemaName from sqlite_master where type = 'trigger'";

		public override string DefaultSchemaName => "";

		internal override string SchemaSQL => "";

		internal override string TableSQL => @"select name as TableName, '' as SchemaName, sql as Definition
from sqlite_master
where type = 'table'
";

		internal override string DatabaseSQL => "";
		internal override string ExtendedPropertySQL => "";
		internal override string CredentialSQL => "";
		internal override string DatabasePrincipalSQL => "";
		internal override string PermissionSQL => "";
		internal override string ServerLoginSQL => "";
		internal override string RoutineSynonymSQL => "";
		internal override string DefaultConstraintSQL => "";

		internal override string ColumnSQL => throw new NotImplementedException();

		internal override string ForeignKeySQL => throw new NotImplementedException();

		internal override string KeyConstraintSQL => throw new NotImplementedException();

		internal override string IndexSQL => throw new NotImplementedException();

		internal override bool MatchConstraintsByColumns => true;

		protected override Type connectionType => typeof(SQLiteConnection);

		private List<ColumnType> _columnTypes;
		public override List<ColumnType> ColumnTypes
		{
			get
			{
				if (_columnTypes == null)
				{
					_columnTypes = base.ColumnTypes;
					_columnTypes.Add(new ColumnType("integer", DataType.Integer, "((0))"));
					var dt = _columnTypes.First(ct => ct.DataType == DataType.SmallDateTime);
					_columnTypes.Remove(dt);
					dt = _columnTypes.First(ct => ct.DataType == DataType.DateTime);
					_columnTypes.Remove(dt);
					_columnTypes.Add(new ColumnType("smalldatetime", DataType.SmallDateTime, "CURRENT_TIMESTAMP", new Map("mintime", "")));
					_columnTypes.Add(new ColumnType("datetime", DataType.DateTime, "CURRENT_TIMESTAMP", new Map("mintime", "")));

				}
				return _columnTypes;
			}
		}

		internal override bool PopulateColumns(DatabaseObjects.Database database, DbCommand cmd, bool includeSystemSchemas, BackgroundWorker worker)
		{
			if (worker != null) worker.ReportProgress(0, $"Populating columns for {database.DatabaseName}...");

			foreach (var tbl in database.Schemas.First().Tables)
			{
				cmd.CommandText = $"pragma table_info({tbl.TableName})";
				using (var rdr = cmd.ExecuteReader())
				{
					if (rdr.HasRows)
					{
						while (rdr.Read())
						{
							var col = new Column(database);
							col.ColumnName = rdr["name"].ToString();
							var colType = rdr["type"].ToString();
							var m = Regex.Match(colType, "(.*nvarchar)\\((\\d*)\\)", RegexOptions.IgnoreCase);
							if (m.Success)
							{
								colType = m.Groups[1].Value;
								col.CharacterMaximumLength = Convert.ToInt16(m.Groups[2].Value);
							}
							m = Regex.Match(colType, "(.*numeric)\\((\\d*),(\\d*)\\)", RegexOptions.IgnoreCase);
							if (m.Success)
							{
								colType = m.Groups[1].Value;
								col.NumericPrecision = Convert.ToInt16(m.Groups[2].Value);
								col.NumericScale = Convert.ToInt16(m.Groups[3].Value);
							}
							if (string.IsNullOrEmpty(colType)) colType = "text";
							col.ColumnType = ColumnTypes.First(t => t.TypeName.ToLower() == colType.ToLower());

							col.IsNullable = rdr["notnull"].ToString() == "0";
							var def = rdr["dflt_value"];
							if (def != DBNull.Value)
								col.ColumnDefault = def.ToString();
							col.IsIdentity = rdr["pk"].ToString() == "1";
							col.Table = tbl;
							tbl.Columns.Add(col);
						}
					}
					rdr.Close();
				}
			}

			return true;
		}

		internal override bool PopulateForeignKeys(DatabaseObjects.Database database, DbCommand cmd, bool includeSystemSchemas, BackgroundWorker worker)
		{
			if (worker != null) worker.ReportProgress(0, $"Populating foreign keys for {database.DatabaseName}...");

			foreach (var tbl in database.Schemas.First().Tables)
			{
				cmd.CommandText = $"pragma foreign_key_list({tbl.TableName})";
				using (var rdr = cmd.ExecuteReader())
				{
					if (rdr.HasRows)
					{
						while (rdr.Read())
						{
							var foreignKey = new ForeignKey(database);
							foreignKey.ForeignKeyName = "fk_" + tbl.TableName + "_" + rdr["from"].ToString() + "_" + rdr["to"].ToString();
							foreignKey.ParentTable = tbl.Schema.Tables.First(t => t.TableName == rdr["table"].ToString());
							foreignKey.ChildTable = tbl;
							foreignKey.ChildTable.ForeignKeys.Add(foreignKey);

							foreignKey.Columns.Add(new ForeignKeyColumn()
							{
								ParentColumn = foreignKey.ParentTable.Columns.First(c => c.ColumnName == rdr["to"].ToString()),
								ChildColumn = foreignKey.ChildTable.Columns.First(c => c.ColumnName == rdr["from"].ToString())
							});
						}
					}
				}
			}

			return true;
		}

		internal override bool PopulateKeyConstraints(DatabaseObjects.Database database, DbCommand cmd, bool includeSystemSchemas, BackgroundWorker worker)
		{
			if (worker != null) worker.ReportProgress(0, $"Populating key constraints for {database.DatabaseName}...");

			foreach (var tbl in database.Schemas.SelectMany(s => s.Tables))
			{
				cmd.CommandText = "pragma table_info(" + tbl.TableName + ")";
				using (var rdr = cmd.ExecuteReader())
				{
					if (rdr.HasRows)
					{
						while (rdr.Read())
						{
							if ((long)rdr["pk"] == 1)
							{
								var constraint = new KeyConstraint(database);
								constraint.ConstraintName = tbl + "_pkey";
								constraint.IsPrimaryKey = true;
								constraint.Table = tbl;
								constraint.Table.KeyConstraints.Add(constraint);

								var col = new IndexColumn();
								col.ColumnName = rdr["name"].ToString();
								constraint.Columns.Add(col);
							}
						}
					}
					rdr.Close();
				}
			}

			return true;
		}

		internal override bool PopulateIndexes(DatabaseObjects.Database database, DbCommand cmd, bool includeSystemSchemas, BackgroundWorker worker)
		{
			if (worker != null) worker.ReportProgress(0, "Populating indexes for " + database.DatabaseName + "...");
			
			foreach (var tbl in database.Schemas.First().Tables)
			{
				cmd.CommandText = $"pragma index_list({tbl.TableName})";
				DataTable dt = new DataTable();
				using (var rdr = cmd.ExecuteReader())
				{
					dt.Load(rdr);
				}

				foreach (DataRow dr in dt.Rows)
				{
					var index = tbl.Indexes.FirstOrDefault(i => i.IndexName == dr["name"].ToString()
						&& i.Table.TableName == tbl.TableName);

					if (index == null)
					{
						index = new Index(database)
						{
							IndexName = dr["name"].ToString(),
							// TODO
							IndexType = "NONCLUSTERED",
							IsUnique = dr["unique"].ToString() == "1",
							Table = tbl
						};
						index.IndexColumns = new List<IndexColumn>();
						index.Table.Indexes.Add(index);
					}

					cmd.CommandText = $"pragma index_info({index.IndexName})";
					using (var rdr = cmd.ExecuteReader())
					{
						if (rdr.HasRows)
						{
							while (rdr.Read())
							{
								index.IndexColumns.Add(new IndexColumn()
								{
									ColumnName = rdr["name"].ToString(),
									Ordinal = Convert.ToInt16(rdr["seqno"])
								});
							}
						}
					}
				}

				foreach (var index in tbl.Indexes)
				{
					cmd.CommandText = $"select * from sqlite_master where type = 'index' and name = '{index.IndexName}' and tbl_name = '{index.Table.TableName}'";
					using (var rdr = cmd.ExecuteReader())
					{
						if (rdr.HasRows)
						{
							while (rdr.Read())
							{
								index.Definition = rdr["sql"].ToString();
							}
						}
					}
				}
			}

			return true;
		}
	}
}
