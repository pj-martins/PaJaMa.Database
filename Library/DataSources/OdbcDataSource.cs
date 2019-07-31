using PaJaMa.Database.Library.DatabaseObjects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaJaMa.Database.Library.DataSources
{
	public class OdbcDataSource : DataSource
	{
		public OdbcDataSource(string connectionString) : base(connectionString)
		{
		}

		public override string DefaultSchemaName => "";

		internal override string SchemaSQL => "";

		internal override string ViewSQL => throw new NotImplementedException();

		internal override string TableSQL => throw new NotImplementedException();

		internal override string ColumnSQL => throw new NotImplementedException();

		internal override string ForeignKeySQL => throw new NotImplementedException();

		internal override string KeyConstraintSQL => throw new NotImplementedException();

		internal override string IndexSQL => throw new NotImplementedException();

		internal override string DefaultConstraintSQL => throw new NotImplementedException();

		internal override string TriggerSQL => throw new NotImplementedException();

		internal override string DatabaseSQL => "";

		protected override Type connectionType => typeof(OdbcConnection);

		private List<ColumnType> _columnTypes;
		public override List<ColumnType> ColumnTypes
		{
			get
			{
				if (_columnTypes == null)
				{
					_columnTypes = new List<ColumnType>();
					_columnTypes.Add(new ColumnType("AlphaNumeric", DataType.NVarChar, ""));
					_columnTypes.Add(new ColumnType("Number", DataType.Numeric, "0"));
					_columnTypes.Add(new ColumnType("Timestamp", DataType.DateTime, "getdate()"));
					_columnTypes.Add(new ColumnType("Short", DataType.Float, "0"));
					_columnTypes.Add(new ColumnType("Long", DataType.Decimal, "0"));
				}
				return _columnTypes;
			}
		}

		public override string GetConvertedObjectName(string objectName)
		{
			if (objectName == CurrentDatabase.DatabaseName) return string.Empty;
			return string.Format("[{0}]", objectName);
		}

		internal override string GetColumnAddAlterScript(Column column, Column targetColumn, string defaultValue, string postScript)
		{
			throw new NotImplementedException();
		}

		public override string GetPreTopN(int topN)
		{
			return topN <= 0 ? string.Empty : string.Format("TOP {0}", topN);
		}

		public override void PopulateTables(DbConnection connection, Schema[] schemas, bool andChildren)
		{
			var schema = schemas.First();
			// TODO used passed in
			using (var conn = OpenConnection(schema.Database.DatabaseName))
			{
				var dtTables = conn.GetSchema("Tables");
				var dtColumns = conn.GetSchema("Columns");
				foreach (DataRow drTable in dtTables.Rows)
				{
					var tbl = new Table(schema.Database) { TableName = drTable["TABLE_NAME"].ToString() };
					foreach (var drCol in dtColumns.Rows.OfType<DataRow>()
						.Where(r => r["TABLE_NAME"].ToString() == tbl.TableName)
						.OrderBy(r => r["COLUMN_NAME"].ToString())
						)
					{
						var strColType = drCol["TYPE_NAME"].ToString();
						var colType = this.ColumnTypes.First(c => c.TypeName == strColType);
						tbl.Columns.Add(new Column(schema.Database)
						{
							ColumnName = drCol["COLUMN_NAME"].ToString(),
							IsNullable = drCol["NULLABLE"].ToString() == "1",
							CharacterMaximumLength = Convert.ToInt16(drCol["COLUMN_SIZE"]),
							ColumnType = colType,
							OrdinalPosition = Convert.ToInt16(drCol["ORDINAL_POSITION"]),
							Table = tbl,
							Schema = schema,
							ColumnDefault = drCol["COLUMN_DEF"] == DBNull.Value ? "" : drCol["COLUMN_DEF"].ToString()

						});
					}
					schema.Tables.Add(tbl);
				}
			}
		}
	}
}
