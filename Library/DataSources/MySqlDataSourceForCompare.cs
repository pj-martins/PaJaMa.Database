using MySql.Data.MySqlClient;
using PaJaMa.Database.Library.DatabaseObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaJaMa.Database.Library.DataSources
{
	// EDR USES CROSS DB FKS YIKES!!! SO WE NEED TO ACT AS IF DBS ARE SCHEMAS ON ONE DB (MYSQL ONLY SUPPORTS 1 SCHEMA PER DB)
	public class MySqlDataSourceForCompare : MySqlDataSource
	{
		public MySqlDataSourceForCompare(string connectionString) : base(connectionString)
		{
		}

		internal override string SchemaSQL => "SELECT SCHEMA_NAME AS SchemaName, 'TODO' as SchemaOwner FROM information_schema.schemata";


		internal override string TableSQL => "select TABLE_NAME as TableName, TABLE_SCHEMA as SchemaName, null as Definition from INFORMATION_SCHEMA.TABLES where TABLE_TYPE = 'BASE TABLE'";

		internal override string ColumnSQL => @"
select co.TABLE_NAME as TableName, COLUMN_NAME as ColumnName, ORDINAL_POSITION as OrdinalPosition2, 
	CHARACTER_MAXIMUM_LENGTH as CharacterMaximumLength2, DATA_TYPE as DataType,
    case when co.IS_NULLABLE = 'YES' then true else false END AS IsNullable2,
	 case when EXTRA = 'auto_increment' then TRUE ELSE false end as IsIdentity2, 
	 '' as ConstraintName,
	  COLUMN_DEFAULT as ColumnDefault, GENERATION_EXPRESSION as Formula, NUMERIC_PRECISION as NumericPrecision2, NUMERIC_SCALE as NumericScale2,
	  t.TABLE_SCHEMA AS SchemaName, null AS Increment
FROM INFORMATION_SCHEMA.COLUMNS co
JOIN INFORMATION_SCHEMA.TABLES t on t.TABLE_NAME = co.TABLE_NAME and t.TABLE_SCHEMA = co.TABLE_SCHEMA
WHERE t.TABLE_TYPE = 'BASE TABLE' AND t.TABLE_SCHEMA <> 'tmp'";

		internal override string ForeignKeySQL => @"
SELECT distinct
    tc.constraint_name as ForeignKeyName, tc.table_name as ChildTableName, kcu.column_name as ChildColumnName, 
   c.referenced_table_name AS ParentTableName, referenced_column_name AS ParentColumnName, UPDATE_RULE as UpdateRule, DELETE_RULE as DeleteRule,
	  referenced_table_schema as ParentTableSchema, tc.TABLE_SCHEMA as ChildTableSchema,  tc.TABLE_SCHEMA as SchemaName
FROM information_schema.table_constraints AS tc
JOIN information_schema.key_column_usage AS kcu
	ON tc.constraint_name = kcu.constraint_name
	AND tc.table_name = kcu.TABLE_NAME
JOIN INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS c ON c.CONSTRAINT_NAME = tc.CONSTRAINT_NAME
	AND c.CONSTRAINT_SCHEMA = tc.CONSTRAINT_SCHEMA
WHERE constraint_type = 'FOREIGN KEY' AND referenced_table_schema IS NOT NULL AND referenced_table_schema <> 'tmp' AND tc.CONSTRAINT_SCHEMA <> 'tmp'";

        internal override string KeyConstraintSQL => @"select distinct ku.CONSTRAINT_NAME as ConstraintName, COLUMN_NAME as ColumnName, ORDINAL_POSITION as Ordinal2, 
	ku.TABLE_NAME as TableName, tc.TABLE_SCHEMA as SchemaName, '' as ClusteredNonClustered, true as IsPrimaryKey2, false as Descending2
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS tc
INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS ku
ON tc.CONSTRAINT_TYPE = 'PRIMARY KEY' 
AND tc.CONSTRAINT_NAME = ku.CONSTRAINT_NAME
and ku.CONSTRAINT_SCHEMA = tc.CONSTRAINT_SCHEMA
AND tc.CONSTRAINT_SCHEMA <> 'tmp'";


		internal override string DatabaseSQL => "";

		internal override string CombinedSQL => $@"
SELECT z.*, TABLE_NAME AS TableName, COLUMN_NAME AS ColumnName, c.TABLE_SCHEMA as SchemaName, c.GENERATION_EXPRESSION as Formula
FROM information_schema.columns c
LEFT JOIN (
SELECT DISTINCT
    tc.constraint_name AS ForeignKeyName, tc.table_name AS ChildTableName, kcu.column_name AS ChildColumnName, 
   c.referenced_table_name AS ParentTableName, referenced_column_name AS ParentColumnName, UPDATE_RULE AS UpdateRule, DELETE_RULE AS DeleteRule,
	  referenced_table_schema AS ParentTableSchema, tc.TABLE_SCHEMA AS ChildTableSchema,  tc.TABLE_SCHEMA
FROM information_schema.table_constraints AS tc
JOIN information_schema.key_column_usage AS kcu
	ON tc.constraint_name = kcu.constraint_name
	AND tc.table_name = kcu.TABLE_NAME
JOIN INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS c ON c.CONSTRAINT_NAME = tc.CONSTRAINT_NAME
	AND c.CONSTRAINT_SCHEMA = tc.CONSTRAINT_SCHEMA
WHERE constraint_type = 'FOREIGN KEY'
	AND referenced_table_schema IS NOT NULL
	AND tc.CONSTRAINT_SCHEMA <> 'tmp'
) z
ON z.ChildTableName = c.table_name
	AND z.ChildColumnName = c.column_name
	AND z.TABLE_SCHEMA = c.TABLE_SCHEMA
WHERE c.TABLE_SCHEMA not in ({string.Join(", ", SystemSchemaNames.Select(s => "'" + s + "'").ToArray())})
";
    }
}
