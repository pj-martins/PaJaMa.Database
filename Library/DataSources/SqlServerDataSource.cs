using PaJaMa.Database.Library.DatabaseObjects;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace PaJaMa.Database.Library.DataSources
{
	public class SqlServerDataSource : DataSource
	{
		public SqlServerDataSource(string connectionString) : base(connectionString)
		{
		}

		public override string DefaultSchemaName => "dbo";
		internal override bool NamedConstraints => true;
		public override List<string> SurroundingCharacters => new List<string>() { "[", "]" };

		#region SQLS
		internal override string SchemaSQL => @"select SCHEMA_NAME as SchemaName, SCHEMA_OWNER as SchemaOwner from {0}.INFORMATION_SCHEMA.SCHEMATA";

		internal override string RoutineSynonymSQL => @"select ROUTINE_SCHEMA as SchemaName, ROUTINE_NAME as Name, ROUTINE_TYPE as Type, Definition = OBJECT_DEFINITION(OBJECT_ID(ROUTINE_SCHEMA + '.' + ROUTINE_NAME)) 
					from {0}.INFORMATION_SCHEMA.ROUTINES
				union all
				select s.name, sy.name, 'SYNONYM', 'CREATE SYNONYM [' + s.name + '].[' + sy.name + '] FOR ' + replace(base_object_name, '[' + db_name(parent_object_id) + '].', '') from sys.synonyms sy
				join {0}.sys.schemas s on s.schema_id = sy.schema_id";

		internal override string ViewSQL => @"select
	s.name as SchemaName,
	v.name as ViewName,
	vc.name as ColumnName,
	is_identity as IsIdentity,
	t.name as DataType,
	columnproperty(vc.object_id, vc.name, 'charmaxlen') as CharacterMaximumLength,
	vc.is_nullable as IsNullable,
	OBJECT_DEFINITION(OBJECT_ID(s.name + '.' + v.name)) as Definition
from {0}.sys.views v
join {0}.sys.columns vc on vc.object_id = v.object_id
join {0}.sys.types t on t.user_type_id = vc.system_type_id
join {0}.sys.schemas s on s.schema_id = v.schema_id";

		internal override string ServerLoginSQL => @"
select p.name as LoginName, 
    p.name as SchemaName,
	p.default_database_name as DefaultDatabaseName,
	p.default_language_name as DefaultLanguageName,
	replace(p.type_desc, '_', '') as LoginType,
	isnull(l.is_expiration_checked, 0) as IsExpirationChecked, 
	isnull(l.is_disabled, 0) as IsDisabled, 
	isnull(l.is_policy_checked, 0) as IsPolicyChecked
from {0}.sys.server_principals p
left join {0}.sys.sql_logins l on l.principal_id = p.principal_id
where p.type in ('U', 'S') and p.name not in ('INFORMATION_SCHEMA', 'sys', 'guest', 'public', 'dbo')
-- and p.sid in (select sid from {0}.sys.database_principals)
";

		internal override string PermissionSQL => @"select s.name as SchemaName, s2.name as PermissionSchemaName,
					coalesce(s2.name, o.name) as PermissionName, state_desc as GrantType, 
					permission_name as PermissionType, pr.Name as PrincipalName
				from {0}.sys.database_permissions p
				join {0}.sys.database_principals pr on pr.principal_id = p.grantee_principal_id
				join {0}.sys.objects o on o.object_id = p.major_id
				join {0}.sys.schemas s on s.schema_id = o.schema_id
				left join {0}.sys.schemas s2 on s2.schema_id = p.major_id
";

		internal override string CredentialSQL => "select name as CredentialName, credential_identity as CredentialIdentity from {0}.sys.credentials";

		internal override string TableSQL => "select TABLE_NAME as TableName, TABLE_SCHEMA as SchemaName, null as Definition from {0}.INFORMATION_SCHEMA.TABLES where TABLE_TYPE = 'BASE TABLE'";

		internal override string ColumnSQL => @"select TABLE_NAME as TableName, COLUMN_NAME as ColumnName, ORDINAL_POSITION as OrdinalPosition, 
	CHARACTER_MAXIMUM_LENGTH as CharacterMaximumLength, DATA_TYPE as DataType,
    IsNullable = convert(bit, case when UPPER(ltrim(rtrim(co.IS_NULLABLE))) = 'YES' then 1 else 0 end), convert(bit, COLUMNPROPERTY(object_id(co.TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'IsIdentity')) as IsIdentity, d.name as ConstraintName,
	isnull(d.definition, COLUMN_DEFAULT) as ColumnDefault, cm.definition as Formula, convert(int, NUMERIC_PRECISION) as NumericPrecision, NUMERIC_SCALE as NumericScale,
	SchemaName = co.TABLE_SCHEMA, IDENT_INCR(co.TABLE_SCHEMA + '.' + TABLE_NAME) AS Increment
from {0}.INFORMATION_SCHEMA.COLUMNS co
join {0}.sys.all_columns c on c.name = co.column_name
join {0}.sys.tables t on t.object_id = c.object_id
	and t.name = co.TABLE_NAME
join {0}.sys.schemas sc on sc.schema_id = t.schema_id
	and sc.name = co.TABLE_SCHEMA
left join {0}.sys.default_constraints d on d.object_id = c.default_object_id
left join {0}.sys.computed_columns cm on cm.name = co.column_name and c.is_computed = 1 and cm.object_id = t.object_id
where 1 = 1 ";

		internal override string ForeignKeySQL => @"
select fk.name as ForeignKeyName, ct.name as ChildTableName, cc.name as ChildColumnName, pt.name as ParentTableName, 
	pc.name as ParentColumnName, 
	replace(update_referential_action_desc, '_', ' ') as UpdateRule,
	replace(delete_referential_action_desc, '_', ' ') as DeleteRule,
	case when is_not_trusted = 1 then 'NO' else '' end as WithCheck,
	ps.name as ParentTableSchema,
	cs.name as ChildTableSchema,
    ps.name as SchemaName
from {0}.sys.foreign_keys fk
join {0}.sys.tables ct on ct.object_id = fk.parent_object_id
join {0}.sys.tables pt on pt.object_id = fk.referenced_object_id
join {0}.sys.foreign_key_columns fkc on fkc.constraint_object_id = fk.object_id
join {0}.sys.all_columns cc on cc.object_id = fkc.parent_object_id
	and cc.column_id = fkc.parent_column_id
join {0}.sys.all_columns pc on pc.object_id = fkc.referenced_object_id
	and pc.column_id = fkc.referenced_column_id
join {0}.sys.schemas cs on cs.schema_id = ct.schema_id
join {0}.sys.schemas ps on ps.schema_id = pt.schema_id";

		internal override string KeyConstraintSQL => @"
select kc.name as ConstraintName, c.name as ColumnName, key_ordinal as Ordinal, t.name as TableName, s.name as SchemaName,
	i.type_desc as ClusteredNonClustered, i.is_primary_key as IsPrimaryKey, ic.is_descending_key as Descending
from {0}.sys.key_constraints kc
join {0}.sys.tables t on t.object_id = kc.parent_object_id
join {0}.sys.schemas s on s.schema_id = t.schema_id
join {0}.sys.index_columns ic on ic.object_id = t.object_id
	and ic.index_id = kc.unique_index_id
join {0}.sys.columns c on c.column_id = ic.column_id and c.object_id = ic.object_id
join {0}.sys.indexes i on i.index_id = ic.index_id and i.object_id = ic.object_id
";

		internal override string IndexSQL => @"SELECT 
	 TableName = t.name,
     IndexName = ind.name,
     ColumnName = col.name,
	 IndexType = ind.type_desc,
	 Ordinal = ic.key_ordinal,
	 IsUnique = is_unique,
	 Descending = is_descending_key,
	 SchemaName = sc.name
FROM 
     {0}.sys.indexes ind 
INNER JOIN 
     {0}.sys.index_columns ic ON  ind.object_id = ic.object_id and ind.index_id = ic.index_id 
INNER JOIN 
     {0}.sys.columns col ON ic.object_id = col.object_id and ic.column_id = col.column_id 
INNER JOIN 
     {0}.sys.tables t ON ind.object_id = t.object_id 
join {0}.sys.schemas sc on sc.schema_id = t.schema_id
WHERE 
     t.is_ms_shipped = 0 and is_unique_constraint = 0 and is_primary_key = 0";

		internal override string DefaultConstraintSQL => @"SELECT t.name as TableName, co.name as ConstraintName, c.Name as ColumnName, co.definition as ColumnDefault, s.name as SchemaName
FROM {0}.sys.all_columns c
INNER JOIN {0}.sys.tables t ON c.object_id = t.object_id
INNER JOIN {0}.sys.schemas s ON t.schema_id = s.schema_id
INNER JOIN {0}.sys.default_constraints co ON c.default_object_id = co.object_id";

		internal override string TriggerSQL => @"select Definition = OBJECT_DEFINITION(t.object_id), t.name as TriggerName, Disabled = is_disabled, o.name as TableName, SchemaName = sc.name
From {0}.sys.triggers t
join {0}.sys.tables o on o.object_id = t.parent_id
join {0}.sys.schemas sc on sc.schema_id = o.schema_id
";

		internal override string DatabaseSQL => "select [name] as DatabaseName from sys.databases where HAS_DBACCESS(name) = 1 order by [name]";

		internal override string ExtendedPropertySQL => @"
select ep.name as PropName, ep.value as PropValue, 'PROCEDURE' as Level1Type, p.name as Level1Object, null as Level2Type, null as Level2Object, sc.name as SchemaName, IgnoreSchema = convert(bit, 0)
FROM {0}.sys.extended_properties AS ep
JOIN {0}.sys.procedures p on p.object_id = ep.major_id
join {0}.sys.schemas sc on sc.schema_id = p.schema_id
union all
select ep.name as PropName, ep.value as PropValue, 'VIEW' as Level1Type, v.name as Level1Object, null as Level2Type, null as Level2Object, s.name as SchemaName, IgnoreSchema = convert(bit, 0)
FROM {0}.sys.extended_properties AS ep
join {0}.sys.views v on v.object_id = ep.major_id
join {0}.sys.schemas s on s.schema_id = v.schema_id
union all
select ep.name as PropName, ep.value as PropValue, 'FUNCTION' as Level1Type, p.name as Level1Object, null as Level2Type, null as Level2Object, sc.name as SchemaName, IgnoreSchema = convert(bit, 0)
FROM {0}.sys.extended_properties AS ep
JOIN {0}.sys.objects p on p.object_id = ep.major_id
 and type in ('FN', 'IF', 'TF')
join {0}.sys.schemas sc on sc.schema_id = p.schema_id
union all
select ep.name as PropName, ep.value as PropValue, 'SYNONYM' as Level1Type, sy.name as Level1Object, null as Level2Type, null as Level2Object, s.name as SchemaName, IgnoreSchema = convert(bit, 0)
FROM {0}.sys.extended_properties AS ep
JOIN {0}.sys.synonyms sy on sy.object_id = ep.major_id
join {0}.sys.schemas s on s.schema_id = sy.schema_id
union all
select ep.name as PropName, ep.value as PropValue, 'TABLE' as Level1Type, t.name as Level1Object, 'COLUMN' as Level2Type, c.name as Level2Object, s.name as SchemaName, IgnoreSchema = convert(bit, 0)
FROM {0}.sys.extended_properties AS ep
JOIN {0}.sys.tables AS t ON ep.major_id = t.object_id 
join {0}.sys.schemas s on s.schema_id = t.schema_id
left JOIN {0}.sys.columns AS c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id
union all
select ep.name as PropName, ep.value as PropValue, 'USER' as Level1Type, p.name as Level1Object, null as Level2Type, null as Level2Object, p.default_schema_name as SchemaName, IgnoreSchema = convert(bit, 1)
FROM {0}.sys.extended_properties AS ep
join {0}.sys.database_principals p on p.principal_id = ep.major_id
where ep.class = 4
union all
select ep.name as PropName, ep.value as PropValue, 'SCHEMA' as Level1Type, s.name as Level1Object, null as Level2Type, null as Level2Object, s.name as SchemaName, IgnoreSchema = convert(bit, 1)
FROM {0}.sys.extended_properties AS ep
join {0}.sys.schemas s on s.schema_id = ep.major_id
where ep.class_desc = 'SCHEMA'
";

		internal override string DatabasePrincipalSQL => @"
select dp.principal_id as PrincipalID, dp.owning_principal_id as OwningPrincipalID, dp.name as PrincipalName, replace(dp.type_desc, '_', '') as PrincipalType, 
	dp.default_schema_name as DefaultSchema, dp.default_schema_name as SchemaName, sp.name as LoginName, dp.is_fixed_role as IsFixedRole
from {0}.sys.database_principals dp
left join {0}.sys.server_principals sp on sp.sid = dp.sid
-- where dp.name not in ('INFORMATION_SCHEMA', 'sys', 'guest', 'public')
";

		#endregion

		protected override Type connectionType => typeof(SqlConnection);

		private List<ColumnType> _columnTypes;
		public override List<ColumnType> ColumnTypes
		{
			get
			{
				if (_columnTypes == null)
				{
					var dtMaps = new Map[] {
						new Map("mintime", "(getdate())"),
						new Map("now", "(getdate())")
					};

					_columnTypes = new List<ColumnType>();
					_columnTypes.Add(new ColumnType("uniqueidentifier", DataType.UniqueIdentifier, "(newid())", new Map("newid", "(newid())")));
					_columnTypes.Add(new ColumnType("datetime", DataType.DateTime, "(getdate())", dtMaps));
					_columnTypes.Add(new ColumnType("datetime2", DataType.DateTime, "(getdate())", dtMaps));
					_columnTypes.Add(new ColumnType("smalldatetime", DataType.SmallDateTime, "(getdate())", dtMaps));
					_columnTypes.Add(new ColumnType("varchar", DataType.VaryingChar, "''"));
					_columnTypes.Add(new ColumnType("varchar", DataType.VarChar, "''"));
					_columnTypes.Add(new ColumnType("nvarchar", DataType.NVarChar, "''"));
					_columnTypes.Add(new ColumnType("char", DataType.Char, "''"));
					_columnTypes.Add(new ColumnType("nchar", DataType.Char, "''"));
					_columnTypes.Add(new ColumnType("int", DataType.Integer, "((0))"));
					_columnTypes.Add(new ColumnType("smallint", DataType.SmallInteger, "((0))"));
					_columnTypes.Add(new ColumnType("tinyint", DataType.SmallInteger, "0"));
					_columnTypes.Add(new ColumnType("real", DataType.Real, "((0))"));
					_columnTypes.Add(new ColumnType("bit", DataType.Boolean, "((0))", new Map(false, "((0))"), new Map(true, "((1))")));
					_columnTypes.Add(new ColumnType("money", DataType.Money, "0") { IsFixedSize = true });
					_columnTypes.Add(new ColumnType("float", DataType.Float, "0"));
					_columnTypes.Add(new ColumnType("varbinary", DataType.VarBinary, "0"));
					_columnTypes.Add(new ColumnType("xml", DataType.Xml, "''"));
					_columnTypes.Add(new ColumnType("text", DataType.Text, "''") { IsFixedSize = true });
					_columnTypes.Add(new ColumnType("image", DataType.Text, "''") { IsFixedSize = true });
					// TODO: not supported
					_columnTypes.Add(new ColumnType("text", DataType.Json, "''") { IsFixedSize = true });
					_columnTypes.Add(new ColumnType("ntext", DataType.Text, "''") { IsFixedSize = true });
					_columnTypes.Add(new ColumnType("decimal", DataType.Decimal, "0"));
					_columnTypes.Add(new ColumnType("date", DataType.DateOnly, "(getdate())", dtMaps));
					_columnTypes.Add(new ColumnType("binary", DataType.Binary, "0"));
					_columnTypes.Add(new ColumnType("time", DataType.TimeOnly, "(getdate())", dtMaps));
					_columnTypes.Add(new ColumnType("bigint", DataType.BigInt, "0"));
					_columnTypes.Add(new ColumnType("timestamp", DataType.RowVersion, ""));
					_columnTypes.Add(new ColumnType("rowversion", DataType.RowVersion, ""));
					_columnTypes.Add(new ColumnType("numeric", DataType.Numeric, "0"));


				}
				return _columnTypes;
			}
		}

		internal override List<string> SystemSchemaNames => new List<string>() {
"guest",
"INFORMATION_SCHEMA",
"sys",
"db_owner",
"db_accessadmin",
"db_securityadmin",
"db_ddladmin",
"db_backupoperator",
"db_datareader",
"db_datawriter",
"db_denydatareader",
"db_denydatawriter"
		};

		public override string GetConvertedObjectName(string objectName)
		{
			return string.IsNullOrEmpty(objectName) ? string.Empty : string.Format("[{0}]", objectName);
		}

		public override string GetPreTopN(int topN)
		{
			return topN <= 0 ? string.Empty : string.Format("TOP {0}", topN);
		}

		public override string GetColumnSelectList(string[] columns)
		{
			return "[" + string.Join("],\r\n\t[", columns) + "]";
		}

		internal override string GetIdentityInsertOn(Table table)
		{
			return string.Empty;
		}

		internal override string GetIdentityInsertOff(Table table)
		{
			return string.Empty;
		}

		internal override string GetForeignKeyCreateScript(ForeignKey foreignKey)
		{
			var createString = string.Format(@"
ALTER TABLE {0} WITH {7}CHECK ADD CONSTRAINT {1} FOREIGN KEY({2})
REFERENCES {3} ({4})
ON DELETE {5}
ON UPDATE {6}

",
	foreignKey.ChildTable.GetObjectNameWithSchema(this),
	foreignKey.GetQueryObjectName(this),
	string.Join(",", foreignKey.Columns.Select(c => c.ChildColumn.GetQueryObjectName(this)).ToArray()),
	foreignKey.ParentTable.GetObjectNameWithSchema(this),
	string.Join(",", foreignKey.Columns.Select(c => c.ParentColumn.GetQueryObjectName(this)).ToArray()),
	foreignKey.DeleteRule,
	foreignKey.UpdateRule,
	foreignKey.WithCheck
	);
			createString += string.Format(@"
ALTER TABLE {0}
CHECK CONSTRAINT {1}
", foreignKey.ChildTable.GetObjectNameWithSchema(this),
foreignKey.GetQueryObjectName(this));
			return createString;
		}

		internal override string GetKeyConstraintCreateScript(KeyConstraint keyConstraint)
		{
			return string.Format(@"CONSTRAINT {0}
{1}
{2}
({3})", keyConstraint.GetQueryObjectName(this), keyConstraint.IsPrimaryKey ? "PRIMARY KEY" : "UNIQUE", keyConstraint.ClusteredNonClustered, string.Join(", ",
	  keyConstraint.Columns.OrderBy(c => c.Ordinal).Select(c => string.Format("{0} {1}", this.GetConvertedObjectName(c.ColumnName), c.Descending ? "DESC" : "ASC"))));
		}

		internal override string GetColumnPostPart(Column column)
		{
			var targetType = this.ColumnTypes.First(t => t.DataType == column.ColumnType.DataType);
			if (column.CharacterMaximumLength != null && !targetType.IsFixedSize)
			{
				string max = column.CharacterMaximumLength.ToString();
				if (max == "-1")
					max = "max";
				return "(" + max + ")";
			}

			return base.GetColumnPostPart(column);
		}

		internal override bool IgnoreDrop(DatabaseObjectBase sourceParent, DatabaseObjectBase obj)
		{
			if (obj is DefaultConstraint && sourceParent is Table)
			{
				if ((sourceParent as Table).Columns.Any(c => c.IsIdentity && c.ColumnName == (obj as DefaultConstraint).Column.ColumnName))
					return true;
			}
			return base.IgnoreDrop(sourceParent, obj);
		}

		public override string GetRenameScript(DatabaseObjectBase databaseObject, string targetName)
		{
			if (databaseObject is Table)
			{
				return $"exec sp_rename '{databaseObject.Schema.SchemaName}.{databaseObject.ObjectName}', '{targetName}'";
			}
			else if (databaseObject is IObjectWithTable)
			{
				var objWithTable = databaseObject as IObjectWithTable;
				return $"exec sp_rename '{objWithTable.Table.Schema.SchemaName}.{objWithTable.Table.TableName}.{databaseObject.ObjectName}', '{targetName}'";
			}

			return $"exec sp_rename '{databaseObject.ObjectName}', '{targetName}'";
		}
	}
}
