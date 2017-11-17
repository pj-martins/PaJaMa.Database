using Npgsql;
using PaJaMa.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaJaMa.Database.Library.DatabaseObjects.DataSources
{
	public class PostgreSQLDataSource : DataSource
	{
		public PostgreSQLDataSource(string connectionString) : base(connectionString)
		{
		}

		public override string DefaultSchemaName => "public";
		internal override bool BypassKeyConstraints => true;
		internal override bool ForeignKeyDropsWithColumns => true;

		#region SQLS
		internal override string SchemaSQL => @"select schema_name as SchemaName, schema_owner as SchemaOwner from information_schema.schemata
";

		internal override string RoutineSynonymSQL => @"
select 
	ROUTINE_SCHEMA as ObjectSchema, 
	ROUTINE_NAME as Name, 
	ROUTINE_TYPE as Type,
	ROUTINE_DEFINITION as Definition
from INFORMATION_SCHEMA.ROUTINES
";

		internal override string ViewSQL => @"select distinct
	VIEW_SCHEMA as SchemaName,
	vcu.VIEW_NAME as ViewName,
	vcu.COLUMN_NAME as ColumnName,
	false as IsIdentity,
	c.DATA_TYPE as DataType,
	c.CHARACTER_MAXIMUM_LENGTH as CharacterMaximumLength,
	case when c.IS_NULLABLE = 'YES' then true else false end as IsNullable,
	VIEW_DEFINITION as Definition
from INFORMATION_SCHEMA.VIEW_COLUMN_USAGE vcu
join INFORMATION_SCHEMA.COLUMNS c on c.TABLE_SCHEMA = vcu.VIEW_SCHEMA
	and c.TABLE_NAME = c.TABLE_NAME and c.COLUMN_NAME = vcu.COLUMN_NAME
join INFORMATION_SCHEMA.VIEWS v on v.TABLE_NAME = vcu.VIEW_NAME and v.TABLE_SCHEMA = vcu.VIEW_SCHEMA
";
		internal override string TableSQL => @"select TABLE_NAME as TableName, TABLE_SCHEMA as SchemaName, null as Definition from INFORMATION_SCHEMA.TABLES 
where TABLE_TYPE = 'BASE TABLE'";

		internal override string ColumnSQL => @"
select co.TABLE_NAME as TableName, COLUMN_NAME as ColumnName, ORDINAL_POSITION as OrdinalPosition, 
	CHARACTER_MAXIMUM_LENGTH as CharacterMaximumLength, DATA_TYPE as DataType,
    case when UPPER(ltrim(rtrim(co.IS_NULLABLE))) = 'YES' then true else false end as IsNullable, case when is_identity = 'NO' then false else true end as IsIdentity,
	COLUMN_DEFAULT as ColumnDefault, null as Formula, NUMERIC_PRECISION as NumericPrecision, NUMERIC_SCALE as NumericScale,
	co.TABLE_SCHEMA as SchemaName
from INFORMATION_SCHEMA.COLUMNS co
join INFORMATION_SCHEMA.TABLES t on t.TABLE_NAME = co.TABLE_NAME
where t.TABLE_TYPE = 'BASE TABLE'
";

		internal override string ForeignKeySQL => @"
SELECT distinct
    tc.constraint_name as ForeignKeyName, tc.table_name as ChildTableName, kcu.column_name as ChildColumnName, 
    ccu.table_name AS ParentTableName, ccu.column_name AS ParentColumnName, UPDATE_RULE as UpdateRule, DELETE_RULE as DeleteRule,
	tc.CONSTRAINT_SCHEMA as ParentTableSchema, tc.CONSTRAINT_SCHEMA as ChildTableSchema
FROM 
    information_schema.table_constraints AS tc 
JOIN information_schema.key_column_usage AS kcu
	ON tc.constraint_name = kcu.constraint_name
	AND tc.table_name = kcu.table_name
JOIN information_schema.constraint_column_usage AS ccu
	ON ccu.constraint_name = tc.constraint_name
join INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS c on c.CONSTRAINT_NAME = tc.CONSTRAINT_NAME
	and c.CONSTRAINT_SCHEMA = tc.CONSTRAINT_SCHEMA
WHERE constraint_type = 'FOREIGN KEY'
";
		internal override string KeyConstraintSQL => @"
select ku.CONSTRAINT_NAME as ConstraintName, COLUMN_NAME as ColumnName, ORDINAL_POSITION as Ordinal, 
	ku.TABLE_NAME as TableName, tc.TABLE_SCHEMA as SchemaName, '' as ClusteredNonClustered, true as IsPrimaryKey, false as Descending
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS tc
INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS ku
ON tc.CONSTRAINT_TYPE = 'PRIMARY KEY' 
AND tc.CONSTRAINT_NAME = ku.CONSTRAINT_NAME
and ku.CONSTRAINT_SCHEMA = tc.CONSTRAINT_SCHEMA
";

		internal override string IndexSQL => @"
select 
	t.relname as TableName,
	i.relname as IndexName,
	a.attname as ColumnName,
	-- TODO: case when ix.indisclustered = true then 'CLUSTERED' else 'NONCLUSTERED' end as IndexType,
    '' as IndexType,
	ix.indisunique as IsUnique,
	n.nspname as SchemaName,
	-- TODO:
	1 as Ordinal,
	false as Descending
from pg_index ix
join pg_class t on t.oid = ix.indrelid
join pg_class i on i.oid = ix.indexrelid
join pg_attribute a on a.attrelid = t.oid and a.attnum = ANY(ix.indkey)
join pg_catalog.pg_namespace n on n.oid = t.relnamespace
where t.relkind = 'r' and t.relname not like 'pg_%' and indisprimary = false";

		internal override string DefaultConstraintSQL => @"select 
	t.relname as TableName,
	c.conname as ConstraintName,
	a.attname as ColumnName,
	d.adsrc as ColumnDefault,
	n.nspname as SchemaName
from pg_constraint c
join pg_class t on t.oid = c.conrelid
join pg_attribute a on a.attrelid = c.conrelid and ARRAY[attnum] <@ c.conkey
join pg_attrdef d on d.adnum = a.attnum and d.adrelid = t.oid
join pg_catalog.pg_namespace n on n.oid = t.relnamespace
";

		internal override string TriggerSQL => @"
SELECT 
	event_object_table as TableName,
	trigger_name as TriggerName,
	event_object_schema as SchemaName,
	event_manipulation as OnInsertUpdateDelete,
	action_statement as Definition,
	action_timing as BeforeAfter
FROM information_schema.triggers
";

		internal override string SequenceSQL => @"select 
    sequence_schema, 
    sequence_name as SequenceName,
    increment,
    minimum_value as MinValue,
    maximum_value as MaxValue,
    start_value as Start,
    cycle_option as Cycle
from INFORMATION_SCHEMA.SEQUENCES";

		internal override string ExtensionSQL => "select * from pg_available_extensions where installed_version <> ''";

		internal override string DatabaseSQL => "select \"datname\" as DatabaseName from \"pg_database\"";

		#endregion

		protected override Type connectionType => typeof(NpgsqlConnection);

		private List<ColumnType> _columnTypes;
		internal override List<ColumnType> ColumnTypes
		{
			get
			{
				if (_columnTypes == null)
				{
					_columnTypes = new List<ColumnType>();
					_columnTypes.Add(new ColumnType("uuid", DataType.UniqueIdentifier, typeof(Guid), "uuid_generate_v4()"));
					_columnTypes.Add(new ColumnType("timestamp with time zone", DataType.DateTimeZone, typeof(DateTime), "now()"));
					_columnTypes.Add(new ColumnType("varchar varying", DataType.VaryingChar, typeof(string), "''"));
					_columnTypes.Add(new ColumnType("integer", DataType.Integer, typeof(int), "0"));
				}
				return _columnTypes;
			}
		}

		public override string GetConvertedObjectName(string objectName)
		{
			return string.Format("\"{0}\"", objectName);
		}

		public override List<Schema> GetNonSystemSchemas(Database database)
		{
			return database.Schemas.Where(s => s.SchemaName != "pg_catalog" && s.SchemaName != "information_schema").ToList();
		}

		internal override string GetCreateIdentity(Column column)
		{
			return string.Format(" DEFAULT NEXTVAL('{0}')", column.Table.TableName + "_" + column.ColumnName + "_seq");
		}

		internal override string GetColumnAddAlterScript(Column column, bool add, string defaultValue, string postScript)
		{
			var sb = new StringBuilder();
			if (add)
			{
				sb.AppendLineFormat("ALTER TABLE {0} {6} {1} {2}{3} {4} {5};",
				   column.Table.GetObjectNameWithSchema(this),
				   column.GetQueryObjectName(this),
				   column.DataType,
				   postScript,
				   column.IsNullable ? "NULL" : "NOT NULL",
				   defaultValue,
				   add ? "ADD COLUMN" : "ALTER COLUMN"
			   );
			}
			else
			{
				sb.AppendLineFormat("ALTER TABLE {0}", column.Table.GetObjectNameWithSchema(this));
				sb.AppendLineFormat("ALTER COLUMN {0} SET DATA TYPE {1}{2},", column.GetQueryObjectName(this), column.DataType, postScript);
				sb.AppendLineFormat("ALTER COLUMN {0} {1} DEFAULT {2},", column.GetQueryObjectName(this), string.IsNullOrEmpty(defaultValue) ? "DROP" : "SET",
					defaultValue);
				sb.AppendLineFormat("ALTER COLUMN {0} {1} NOT NULL;", column.GetQueryObjectName(this), column.IsNullable ? "DROP" : "SET");
			}
			return sb.ToString();
		}

		internal override string GetPostTableCreateScript(Table table)
		{
			var schema = table.Schema.SchemaName == table.ParentDatabase.DataSource.DefaultSchemaName ?
						this.DefaultSchemaName : table.Schema.SchemaName;
			schema = this.GetConvertedObjectName(schema);
			var sb = new StringBuilder();
			var idCols = table.Columns.Where(c => c.IsIdentity);
			foreach (var idCol in idCols)
			{
				// TODO: start
				sb.Insert(0, string.Format(@"CREATE SEQUENCE IF NOT EXISTS {0}.""{1}_{2}_seq""
	start 1
	increment {3};

", schema, table.TableName.ToLower(), idCol.ColumnName.ToLower(), idCol.Increment.GetValueOrDefault(1)));
			}
			return sb.ToString();
		}

		public override string GetColumnSelectList(string[] columns)
		{
			return "\"" + string.Join("\",\r\n\t\"", columns) + "\"";
		}

		public override string GetPostTopN(int topN)
		{
			return topN <= 0 ? string.Empty : string.Format("LIMIT {0}", topN);
		}
	}

}
