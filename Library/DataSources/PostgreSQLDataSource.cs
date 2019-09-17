using Npgsql;
using PaJaMa.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PaJaMa.Database.Library.Synchronization;
using PaJaMa.Database.Library.DatabaseObjects;
using System.Text.RegularExpressions;

namespace PaJaMa.Database.Library.DataSources
{
	public class PostgreSQLDataSource : DataSource
	{
		public PostgreSQLDataSource(string connectionString) : base(connectionString)
		{
		}

		public override string DefaultSchemaName => "public";
		internal override bool ForeignKeyDropsWithColumns => true;
		internal override bool MatchConstraintsByColumns => true;
		internal override bool BypassKeyConstraints => true;

		internal override List<string> SystemSchemaNames => new List<string>() { "pg_catalog", "information_schema" };
		public override List<string> SurroundingCharacters => new List<string>() { "\"" };

		#region SQLS
		internal override string SchemaSQL => @"select schema_name as SchemaName, schema_owner as SchemaOwner from {0}.information_schema.schemata
";

		internal override string RoutineSynonymSQL => @"
select 
	n.nspname as SchemaName, 
	proname as Name, 
	-- t.typname as Type,
	'Function' as Type,
	pg_get_functiondef(p.oid) as Definition
from pg_proc p
-- JOIN pg_type t on p.prorettype = t.oid
JOIN pg_namespace n on n.oid = p.pronamespace
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
from {0}.INFORMATION_SCHEMA.VIEW_COLUMN_USAGE vcu
join {0}.INFORMATION_SCHEMA.COLUMNS c on c.TABLE_SCHEMA = vcu.VIEW_SCHEMA
	and c.TABLE_NAME = c.TABLE_NAME and c.COLUMN_NAME = vcu.COLUMN_NAME
join {0}.INFORMATION_SCHEMA.VIEWS v on v.TABLE_NAME = vcu.VIEW_NAME and v.TABLE_SCHEMA = vcu.VIEW_SCHEMA
";
		internal override string TableSQL => @"select TABLE_NAME as TableName, TABLE_SCHEMA as SchemaName, null as Definition from {0}.INFORMATION_SCHEMA.TABLES 
where TABLE_TYPE = 'BASE TABLE'";

		internal override string ColumnSQL => @"
select co.TABLE_NAME as TableName, COLUMN_NAME as ColumnName, ORDINAL_POSITION as OrdinalPosition, 
	CHARACTER_MAXIMUM_LENGTH as CharacterMaximumLength, DATA_TYPE as DataType,
    case when UPPER(ltrim(rtrim(co.IS_NULLABLE))) = 'YES' then true else false end as IsNullable, case when is_identity = 'NO' then false else true end as IsIdentity,
	COLUMN_DEFAULT as ColumnDefault, ConstraintName, null as Formula, NUMERIC_PRECISION as NumericPrecision, NUMERIC_SCALE as NumericScale,
	co.TABLE_SCHEMA as SchemaName, udt_name as UDTName
from {0}.INFORMATION_SCHEMA.COLUMNS co
join {0}.INFORMATION_SCHEMA.TABLES t on t.TABLE_NAME = co.TABLE_NAME
left join
(
	select 
		t.relname as TableName,
		coalesce(c.conname, 'DF_' || t.relname || '_' || a.attname) as ConstraintName,
		a.attname as ColumnName,
		d.adsrc as ColumnDefault,
		n.nspname as SchemaName
	from pg_constraint c
	join pg_class t on t.oid = c.conrelid
	join pg_attribute a on a.attrelid = c.conrelid and ARRAY[attnum] <@ c.conkey
	join pg_attrdef d on d.adnum = a.attnum and d.adrelid = t.oid
	join pg_catalog.pg_namespace n on n.oid = t.relnamespace
) dc on dc.tablename = co.table_name and dc.columnname = co.column_name and dc.schemaname = co.table_schema and dc.columndefault = co.column_default
where t.TABLE_TYPE = 'BASE TABLE'
";

		internal override string ForeignKeySQL => @"
SELECT distinct
    tc.constraint_name as ForeignKeyName, tc.table_name as ChildTableName, kcu.column_name as ChildColumnName, 
    ccu.table_name AS ParentTableName, ccu.column_name AS ParentColumnName, UPDATE_RULE as UpdateRule, DELETE_RULE as DeleteRule,
	ccu.TABLE_SCHEMA as ParentTableSchema, tc.TABLE_SCHEMA as ChildTableSchema, tc.CONSTRAINT_SCHEMA as SchemaName
FROM 
    {0}.information_schema.table_constraints AS tc 
JOIN {0}.information_schema.key_column_usage AS kcu
	ON tc.constraint_name = kcu.constraint_name
	AND tc.table_name = kcu.table_name
JOIN {0}.information_schema.constraint_column_usage AS ccu
	ON ccu.constraint_name = tc.constraint_name
join {0}.INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS c on c.CONSTRAINT_NAME = tc.CONSTRAINT_NAME
	and c.CONSTRAINT_SCHEMA = tc.CONSTRAINT_SCHEMA
WHERE constraint_type = 'FOREIGN KEY'
";
		internal override string KeyConstraintSQL => @"
select ku.CONSTRAINT_NAME as ConstraintName, COLUMN_NAME as ColumnName, ORDINAL_POSITION as Ordinal, 
	ku.TABLE_NAME as TableName, tc.TABLE_SCHEMA as SchemaName, '' as ClusteredNonClustered, true as IsPrimaryKey, false as Descending
FROM {0}.INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS tc
INNER JOIN {0}.INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS ku
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
	row_number() over (partition by t.relname, i.relname, a.attname)::integer as Ordinal,
	ix.indisunique as IsUnique,
	n.nspname as SchemaName,
	-- TODO:
	false as Descending
from
    pg_class t,
    pg_class i,
    pg_index ix,
    pg_attribute a,
    pg_catalog.pg_namespace n
where
    t.oid = ix.indrelid
    and i.oid = ix.indexrelid
    and a.attrelid = t.oid
    and a.attnum = ANY(ix.indkey)
    and t.relkind = 'r'
	and indisprimary = false
	and n.oid = t.relnamespace
";

		internal override string DefaultConstraintSQL => @"select 
	c.table_name as TableName,
	coalesce(constraintname, 'DF_' || c.table_name || '_' || c.column_name) as ConstraintName, 
	c.column_name as ColumnName, 
	c.column_default as ColumnDefault, 
	c.table_schema as SchemaName
from {0}.INFORMATION_SCHEMA.COLUMNS c
left join
(
	select 
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
) co on co.tablename = c.table_name and co.columnname = c.column_name and co.schemaname = c.table_schema and co.columndefault = c.column_default
where c.column_default is not null
";

		internal override string TriggerSQL => @"
SELECT 
	event_object_table as TableName,
	trigger_name as TriggerName,
	event_object_schema as SchemaName,
	event_manipulation as OnInsertUpdateDelete,
	action_statement as Definition,
	action_timing as BeforeAfter
FROM {0}.information_schema.triggers
";

		internal override string SequenceSQL => @"select 
    sequence_schema as SchemaName, 
    sequence_name as SequenceName,
    increment,
    minimum_value as MinValue,
    maximum_value as MaxValue,
    start_value as Start,
    cycle_option as Cycle
from {0}.INFORMATION_SCHEMA.SEQUENCES";

		internal override string ExtensionSQL => "select *, ''::text as SchemaName from pg_available_extensions where installed_version <> ''";

		internal override string DatabaseSQL => "select \"datname\" as DatabaseName from \"pg_database\"";

		#endregion

		protected override Type connectionType => typeof(NpgsqlConnection);

		private List<ColumnType> _columnTypes;
		public override List<ColumnType> ColumnTypes
		{
			get
			{
				if (_columnTypes == null)
				{
					var dtMaps = new Map[] {
						new Map("mintime", "now()"),
						new Map("now", "now()")
					};
					_columnTypes = new List<ColumnType>();
					_columnTypes.Add(new ColumnType("uuid", DataType.UniqueIdentifier, "uuid_generate_v4()", new Map("newid", "uuid_generate_v4()")));
					_columnTypes.Add(new ColumnType("timestamp with time zone", DataType.DateTime, "now()", dtMaps));
					_columnTypes.Add(new ColumnType("timestamp without time zone", DataType.DateTime, "now()", dtMaps));
					_columnTypes.Add(new ColumnType("timestamp with time zone", DataType.SmallDateTime, "now()", dtMaps));
					_columnTypes.Add(new ColumnType("timestamp without time zone", DataType.SmallDateTime, "now()", dtMaps));
					_columnTypes.Add(new ColumnType("varchar varying", DataType.VaryingChar, "''") { CreateTypeName = "varchar" });
					_columnTypes.Add(new ColumnType("character varying", DataType.VaryingChar, "''") { CreateTypeName = "varchar" });
					_columnTypes.Add(new ColumnType("varchar varying", DataType.VarChar, "''") { CreateTypeName = "varchar" });
					_columnTypes.Add(new ColumnType("character varying", DataType.VarChar, "''") { CreateTypeName = "varchar" });
					_columnTypes.Add(new ColumnType("varchar varying", DataType.NVarChar, "''") { CreateTypeName = "varchar" });
					_columnTypes.Add(new ColumnType("character varying", DataType.NVarChar, "''") { CreateTypeName = "varchar" });
					_columnTypes.Add(new ColumnType("integer", DataType.Integer, "0"));
					_columnTypes.Add(new ColumnType("smallint", DataType.SmallInteger, "0"));
					_columnTypes.Add(new ColumnType("real", DataType.Real, "0"));
					_columnTypes.Add(new ColumnType("boolean", DataType.Boolean, "false", new Map(false, "false"), new Map(true, "true")));
					_columnTypes.Add(new ColumnType("double precision", DataType.Float, "0"));
					_columnTypes.Add(new ColumnType("money", DataType.Money, "0"));
					_columnTypes.Add(new ColumnType("bytea", DataType.VarBinary, "0") { IsFixedSize = true });
					_columnTypes.Add(new ColumnType("xml", DataType.Xml, "''"));
					_columnTypes.Add(new ColumnType("text", DataType.Text, "''") { IsFixedSize = true });
					_columnTypes.Add(new ColumnType("json", DataType.Json, "") { IsFixedSize = true });
					_columnTypes.Add(new ColumnType("jsonb", DataType.Json, "") { IsFixedSize = true });
					_columnTypes.Add(new ColumnType("decimal", DataType.Decimal, "0"));
					_columnTypes.Add(new ColumnType("numeric", DataType.Numeric, "0"));
					_columnTypes.Add(new ColumnType("date", DataType.DateOnly, "now()", dtMaps));
					_columnTypes.Add(new ColumnType("bytea", DataType.Binary, "0") { IsFixedSize = true });
					_columnTypes.Add(new ColumnType("time", DataType.TimeOnly, "now()", dtMaps));
					_columnTypes.Add(new ColumnType("time without time zone", DataType.TimeOnly, "now()", dtMaps));
					_columnTypes.Add(new ColumnType("time with time zone", DataType.TimeOnly, "now()", dtMaps));
					_columnTypes.Add(new ColumnType("bigint", DataType.BigInt, "0"));
					_columnTypes.Add(new ColumnType("serial", DataType.RowVersion, ""));
					_columnTypes.Add(new ColumnType("ARRAY", DataType.Array, "array[]"));

				}
				return _columnTypes;
			}
		}

		public override string GetConvertedObjectName(string objectName)
		{
			return string.Format("\"{0}\"", objectName);
		}

		internal override string GetCreateIdentity(Column column)
		{
			return string.Format(" DEFAULT NEXTVAL('{0}')", column.Parent.ObjectName + "_" + column.ColumnName + "_seq");
		}

		internal override string GetColumnAddAlterScript(Column column, Column targetColumn, string postScript, string defaultValue)
		{
			var dataType = this.GetConvertedColumnType(column, true);
			var sb = new StringBuilder();
			if (targetColumn == null)
			{
				sb.AppendLineFormat("ALTER TABLE {0} {6} {1} {2}{3} {4} {5};",
				   column.Parent.GetObjectNameWithSchema(this),
				   column.GetQueryObjectName(this),
				   dataType,
				   postScript,
				   column.IsNullable ? "NULL" : "NOT NULL",
				   defaultValue,
				   "ADD COLUMN"
			   );
			}
			else
			{
				sb.AppendLineFormat("ALTER TABLE {0}", column.Parent.GetObjectNameWithSchema(this));
				sb.AppendLineFormat("ALTER COLUMN {0} SET DATA TYPE {1}{2},", column.GetQueryObjectName(this), dataType, postScript);
				sb.AppendLineFormat("ALTER COLUMN {0} {1} DEFAULT {2},", column.GetQueryObjectName(this), string.IsNullOrEmpty(defaultValue) ? "DROP" : "SET",
					defaultValue);
				sb.AppendLineFormat("ALTER COLUMN {0} {1} NOT NULL;", column.GetQueryObjectName(this), column.IsNullable ? "DROP" : "SET");
			}
			return sb.ToString();
		}

		internal override string GetIdentityInsertOn(Table table)
		{
			return string.Empty;
		}

		internal override string GetIdentityInsertOff(Table table)
		{
			return string.Empty;
		}

		public override string GetConvertedColumnType(Column column, bool forCreate)
		{
			var dataType = base.GetConvertedColumnType(column, forCreate);
			if (!forCreate && this.GetType() == column.Database.DataSource.GetType())
				dataType = column.ColumnType.TypeName;
			if (dataType == "ARRAY" && !string.IsNullOrEmpty(column.UDTName))
			{
				// TODO: TEST
				var arrayType = column.UDTName;
				if (arrayType.StartsWith("_"))
					arrayType = arrayType.Substring(1);
				dataType = arrayType + "[]";
			}
			return dataType;
		}

		internal override string GetPreTableCreateScript(Table table)
		{
			var schema = table.Schema.SchemaName == table.Database.DataSource.DefaultSchemaName ?
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

		internal override string GetIndexCreateScript(Index index)
		{
			var indexCols = index.IndexColumns.Where(i => i.Ordinal != 0);
			if (indexCols.Count() < 1)
				indexCols = index.IndexColumns;

			var sb = new StringBuilder();

			sb.AppendLineFormat(@"CREATE {0} INDEX {1} ON {2}
(
	{3}
);",
(bool)index.IsUnique ? "UNIQUE" : "",
this.GetConvertedObjectName(index.IndexName),
index.Table.GetObjectNameWithSchema(this),
string.Join(",\r\n\t",
indexCols.OrderBy(c => c.Ordinal).Select(c =>
	string.Format("{0} {1}", this.GetConvertedObjectName(c.ColumnName), c.Descending ? "DESC" : "ASC")).ToArray())
	);

			return sb.ToString();
		}

		internal override string GetIndexDropScript(Index index)
		{
			return string.Format("DROP INDEX {0}", index.GetQueryObjectName(this));
		}

		internal override string GetColumnPostPart(Column column)
		{
			var targetType = this.ColumnTypes.First(t => t.DataType == column.ColumnType.DataType);
			if (column.CharacterMaximumLength.GetValueOrDefault() > 0 && !targetType.IsFixedSize)
				return "(" + column.CharacterMaximumLength.ToString() + ")";

			return base.GetColumnPostPart(column);
		}

		internal override string GetColumnDefault(Column column, string columnDefault)
		{
			var def = base.GetColumnDefault(column, columnDefault);
			//if (!string.IsNullOrEmpty(columnDefault) && columnDefault.Contains("::"))
			//{
			//	var match = Regex.Match(columnDefault, "'(.*)'::.*");
			//	if (match.Success)
			//		def = match.Groups[1].Value;
			//}
			return def;
		}

		internal override bool IgnoreDifference(Difference difference, DataSource fromDataSource, DataSource toDataSource, DatabaseObjectBase fromObject, DatabaseObjectBase toObject)
		{
			if (base.IgnoreDifference(difference, fromDataSource, toDataSource, fromObject, toObject)) return true;

			if (fromObject is Column)
			{
				var column = fromObject as Column;
				if (difference.PropertyName == "ColumnDefault")
				{
					if (difference.TargetValue.StartsWith("nextval") || difference.SourceValue.StartsWith("nextval"))
						return true;
				}
				else if (difference.PropertyName == "NumericPrecision" || difference.PropertyName == "IsIdentity" || difference.PropertyName == "UDTName")
				{
					return true;
				}
				else if (difference.PropertyName == "NumericScale" && difference.TargetValue.Replace("0", "") == difference.SourceValue.Replace("0", ""))
				{
					return true;
				}
			}
			else if (fromObject is ForeignKey && (
				difference.PropertyName == "ForeignKeyName" ||
						difference.PropertyName == "WithCheck" ||
						difference.PropertyName == "UpdateRule" ||
						difference.PropertyName == "DeleteRule"))
				return true;
			else if (fromObject is Index && toObject is Index && difference.PropertyName == "Columns")
			{
				var tcs = (toObject as Index).IndexColumns.Where(ic => ic.Ordinal != 0);
				var ics = (fromObject as Index).IndexColumns.Where(ic => ic.Ordinal != 0);
				return tcs.Select(t => t.ColumnName).OrderBy(c => c).SequenceEqual(ics.Select(i => i.ColumnName).OrderBy(c => c));
			}

			return false;
		}

		internal override void CheckUnnecessaryItems(List<SynchronizationItem> items)
		{
			base.CheckUnnecessaryItems(items);
			for (int i = items.Count - 1; i >= 0; i--)
			{
				var item = items[i];
				if (item.DatabaseObject is ForeignKey && item.Differences.Any(d => d.DifferenceType == DifferenceType.Drop))
				{
					var fk = item.DatabaseObject as ForeignKey;
					var dropcols = items.Where(c => c.DatabaseObject is Column && c.Differences.Any(d => d.DifferenceType == DifferenceType.Drop));
					foreach (var fkc in fk.Columns)
					{
						if (dropcols.Any(dc => dc.DatabaseObject.ObjectName == fkc.ParentColumn.ObjectName || dc.DatabaseObject.ObjectName == fkc.ChildColumn.ObjectName))
						{
							items.RemoveAt(i);
							break;
						}
					}
				}
			}
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
