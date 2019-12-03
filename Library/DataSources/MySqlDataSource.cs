using MySql.Data.MySqlClient;
using PaJaMa.Database.Library.DatabaseObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaJaMa.Database.Library.DataSources
{
	public class MySqlDataSource : DataSource
	{
		public MySqlDataSource(string connectionString) : base(connectionString)
		{
		}

        internal override List<string> SystemSchemaNames => new List<string>() { "information_schema", "performance_schema", "tmp", "sys" };

        public override string DefaultSchemaName => "";
		public override List<string> SurroundingCharacters => new List<string>() { "`" };

		protected override Type connectionType => typeof(MySqlConnection);

		// TODO: owner
		internal override string SchemaSQL => "";

        internal override string ViewSQL => @"
select
	c.TABLE_SCHEMA as SchemaName,
	c.TABLE_NAME as ViewName,
	c.COLUMN_NAME as ColumnName,
	false as IsIdentity2,
	c.DATA_TYPE as DataType,
	c.CHARACTER_MAXIMUM_LENGTH as CharacterMaximumLength2,
	case when c.IS_NULLABLE = 'YES' then true else false end as IsNullable2,
	VIEW_DEFINITION as Definition
FROM INFORMATION_SCHEMA.COLUMNS c
JOIN INFORMATION_SCHEMA.VIEWS t on t.TABLE_NAME = c.TABLE_NAME
	AND t.TABLE_SCHEMA = c.TABLE_SCHEMA
";

		internal override string SchemaViewSQL => @"
select
	'' as SchemaName,
	t.TABLE_NAME as ViewName,
	null as Definition
FROM INFORMATION_SCHEMA.TABLES t
WHERE t.TABLE_SCHEMA = 'information_schema'";

		internal override string TableSQL => "select TABLE_NAME as TableName, '' as SchemaName, null as Definition from INFORMATION_SCHEMA.TABLES where TABLE_TYPE = 'BASE TABLE' and TABLE_SCHEMA = '{0}'";

		internal override string ColumnSQL => @"
select co.TABLE_NAME as TableName, COLUMN_NAME as ColumnName, ORDINAL_POSITION as OrdinalPosition2, 
	CHARACTER_MAXIMUM_LENGTH as CharacterMaximumLength2, DATA_TYPE as DataType,
    case when co.IS_NULLABLE = 'YES' then true else false END AS IsNullable2,
	 case when EXTRA = 'auto_increment' then TRUE ELSE false end as IsIdentity2, 
	 CONCAT('DF_', co.TABLE_NAME, '_', COLUMN_NAME) as ConstraintName,
	  COLUMN_DEFAULT as ColumnDefault, null as Formula, NUMERIC_PRECISION as NumericPrecision2, NUMERIC_SCALE as NumericScale2,
	  '' AS SchemaName, null AS Increment
FROM INFORMATION_SCHEMA.COLUMNS co
JOIN INFORMATION_SCHEMA.TABLES t on t.TABLE_NAME = co.TABLE_NAME and t.TABLE_SCHEMA = co.TABLE_SCHEMA
WHERE (t.TABLE_TYPE = 'BASE TABLE' or t.TABLE_TYPE = 'SYSTEM VIEW') and co.TABLE_SCHEMA = '{0}'";

		internal override string ForeignKeySQL => @"
SELECT distinct
    tc.constraint_name as ForeignKeyName, tc.table_name as ChildTableName, kcu.column_name as ChildColumnName, 
   c.referenced_table_name AS ParentTableName, referenced_column_name AS ParentColumnName, UPDATE_RULE as UpdateRule, DELETE_RULE as DeleteRule,
	 '' as ParentTableSchema, '' as ChildTableSchema, '' as SchemaName
FROM information_schema.table_constraints AS tc
JOIN information_schema.key_column_usage AS kcu
	ON tc.constraint_name = kcu.constraint_name
	AND tc.table_name = kcu.TABLE_NAME
JOIN INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS c ON c.CONSTRAINT_NAME = tc.CONSTRAINT_NAME
	AND c.CONSTRAINT_SCHEMA = tc.CONSTRAINT_SCHEMA
WHERE constraint_type = 'FOREIGN KEY' AND referenced_table_schema IS NOT NULL and referenced_table_schema = '{0}'";

        internal override string KeyConstraintSQL => @"select distinct ku.CONSTRAINT_NAME as ConstraintName, COLUMN_NAME as ColumnName, ORDINAL_POSITION as Ordinal2, 
	ku.TABLE_NAME as TableName, '' as SchemaName, '' as ClusteredNonClustered, true as IsPrimaryKey2, false as Descending2
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS tc
INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS ku
ON tc.CONSTRAINT_TYPE = 'PRIMARY KEY' 
AND tc.CONSTRAINT_NAME = ku.CONSTRAINT_NAME
and ku.CONSTRAINT_SCHEMA = tc.CONSTRAINT_SCHEMA
and tc.TABLE_SCHEMA = '{0}'";


        internal override string IndexSQL => "";

        internal override string DefaultConstraintSQL => "";

        internal override string TriggerSQL => "";

		internal override string DatabaseSQL => "SELECT SCHEMA_NAME AS DatabaseName FROM information_schema.schemata";

		internal override string CombinedSQL => throw new NotImplementedException();


		private List<ColumnType> _columnTypes;
		public override List<ColumnType> ColumnTypes
		{
			get
			{
				if (_columnTypes == null)
				{
					_columnTypes = new List<ColumnType>();
					_columnTypes.Add(new ColumnType("int", DataType.Integer, "0"));
                    _columnTypes.Add(new ColumnType("mediumint", DataType.Integer, "0"));
                    _columnTypes.Add(new ColumnType("integer", DataType.Integer, "0"));
					_columnTypes.Add(new ColumnType("smallint", DataType.SmallInteger, "0"));
					_columnTypes.Add(new ColumnType("tinyint", DataType.SmallInteger, "0"));
					_columnTypes.Add(new ColumnType("bigint", DataType.BigInt, "0"));
					_columnTypes.Add(new ColumnType("decimal", DataType.Decimal, "0"));
					_columnTypes.Add(new ColumnType("numeric", DataType.Numeric, "0"));
					_columnTypes.Add(new ColumnType("float", DataType.Float, "0"));
					_columnTypes.Add(new ColumnType("double", DataType.Float, "0"));
					_columnTypes.Add(new ColumnType("date", DataType.DateOnly, "0"));
					_columnTypes.Add(new ColumnType("datetime", DataType.DateTime, "0"));
					_columnTypes.Add(new ColumnType("timestamp", DataType.DateTime, "0"));
					_columnTypes.Add(new ColumnType("time", DataType.TimeOnly, "0"));
					_columnTypes.Add(new ColumnType("varchar", DataType.VarChar, "''"));
					_columnTypes.Add(new ColumnType("char", DataType.Char, "''"));
					_columnTypes.Add(new ColumnType("varbinary", DataType.VarBinary, "0"));
					_columnTypes.Add(new ColumnType("binary", DataType.Binary, "0"));
					_columnTypes.Add(new ColumnType("blob", DataType.Binary, "0"));
					_columnTypes.Add(new ColumnType("mediumblob", DataType.Binary, "0"));
                    _columnTypes.Add(new ColumnType("longblob", DataType.Binary, "0"));
                    _columnTypes.Add(new ColumnType("text", DataType.Text, "0"));
					_columnTypes.Add(new ColumnType("tinytext", DataType.Text, "0"));
					_columnTypes.Add(new ColumnType("mediumtext", DataType.Text, "0"));
					_columnTypes.Add(new ColumnType("longtext", DataType.Text, "0"));
					_columnTypes.Add(new ColumnType("enum", DataType.Integer, "0"));
					_columnTypes.Add(new ColumnType("json", DataType.Json, "0"));
					_columnTypes.Add(new ColumnType("set", DataType.Array, "0"));
                    _columnTypes.Add(new ColumnType("bit", DataType.Integer, "0"));
                }
				return _columnTypes;
			}
		}

		public override string GetConvertedObjectName(string objectName)
		{
            return string.Format("`{0}`", objectName);
        }

        public override string GetPostTopN(int topN)
        {
            return topN <= 0 ? string.Empty : string.Format("LIMIT {0}", topN);
        }

        internal override string GetForeignKeyDropScript(ForeignKey foreignKey)
        {
            return "ALTER TABLE {0} DROP FOREIGN KEY {1};";
        }

		protected override string keysTableWhere(Table table)
		{
			return $" and ku.TABLE_NAME = '{table.TableName}'";
		}

		protected override string foreignKeysTableWhere(Table table)
		{
			return $" and tc.TABLE_NAME = '{table.TableName}'";
		}
	}
}
