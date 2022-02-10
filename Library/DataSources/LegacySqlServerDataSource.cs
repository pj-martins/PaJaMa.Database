using PaJaMa.Common;
using PaJaMa.Database.Library.DatabaseObjects;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaJaMa.Database.Library.DataSources
{
	public class LegacySqlServerDataSource : SqlServerDataSource
	{
		public LegacySqlServerDataSource(DatabaseConnection connection) : base(connection)
		{
		}

		#region SQLS
		internal override string SchemaSQL => "select distinct TABLE_SCHEMA as SchemaName, TABLE_SCHEMA as SchemaOwner from INFORMATION_SCHEMA.TABLES";
		internal override string RoutineSynonymSQL => @"select ROUTINE_SCHEMA as SchemaName, ROUTINE_NAME as Name, ROUTINE_TYPE as Type, Definition = ROUTINE_DEFINITION
					from INFORMATION_SCHEMA.ROUTINES
				";

		internal override string ViewSQL => @"select 
	VIEW_SCHEMA as SchemaName,
	vcu.VIEW_NAME as ViewName,
	vcu.COLUMN_NAME as ColumnName,
	convert(bit, 0) as IsIdentity,
	c.DATA_TYPE as DataType,
	c.CHARACTER_MAXIMUM_LENGTH as CharacterMaximumLength,
	convert(bit, case when c.IS_NULLABLE = 'YES' then 1 else 0 end) as IsNullable,
	VIEW_DEFINITION as Definition
from INFORMATION_SCHEMA.VIEW_COLUMN_USAGE vcu
join INFORMATION_SCHEMA.COLUMNS c on c.TABLE_SCHEMA = vcu.VIEW_SCHEMA
	and c.TABLE_NAME = c.TABLE_NAME and c.COLUMN_NAME = vcu.COLUMN_NAME
join INFORMATION_SCHEMA.VIEWS v on v.TABLE_NAME = vcu.VIEW_NAME and v.TABLE_SCHEMA = vcu.VIEW_SCHEMA
";

		internal override string PermissionSQL => "";
		internal override string ColumnSQL => @"select co.TABLE_NAME as TableName, COLUMN_NAME as ColumnName, ORDINAL_POSITION as OrdinalPosition, 
	CHARACTER_MAXIMUM_LENGTH as CharacterMaximumLength, DATA_TYPE as DataType,
    IsNullable = convert(bit, case when UPPER(ltrim(rtrim(co.IS_NULLABLE))) = 'YES' then 1 else 0 end), convert(bit, COLUMNPROPERTY(object_id(TABLE_NAME), COLUMN_NAME, 'IsIdentity')) as IsIdentity, d.name as ConstraintName,
	COLUMN_DEFAULT as ColumnDefault, null as Formula, convert(int, NUMERIC_PRECISION) as NumericPrecision, NUMERIC_SCALE as NumericScale,
	SchemaName = co.TABLE_SCHEMA, IDENT_INCR(co.TABLE_SCHEMA + '.' + TABLE_NAME) AS Increment
from INFORMATION_SCHEMA.COLUMNS co
join syscolumns c on c.name = co.column_name
join sysobjects t on t.id = c.id
	and t.name = co.TABLE_NAME
left join
(
	select dc.colid, d.name, d.parent_obj
	from sysconstraints dc
	join sysobjects d on d.id = dc.constid
) d
on d.colid = c.colid and d.parent_obj = t.id
where t.xtype = 'U'";

		internal override string ForeignKeySQL => @"
select fk.CONSTRAINT_NAME as ForeignKeyName, fk.TABLE_NAME as ChildTableName, cc.COLUMN_NAME as ChildColumnName, 
	tc.TABLE_NAME as ParentTableName, tcc.COLUMN_NAME as ParentColumnName, UPDATE_RULE as UpdateRule, DELETE_RULE as DeleteRule,
	WithCheck = case when OBJECTPROPERTY(OBJECT_ID(QUOTENAME(c.CONSTRAINT_SCHEMA) + '.' + QUOTENAME(c.CONSTRAINT_NAME)), 'CnstIsNotTrusted') = 1 then 'NO' else '' end,
	tc.CONSTRAINT_SCHEMA as ParentTableSchema, fk.CONSTRAINT_SCHEMA as ChildTableSchema, tc.CONSTRAINT_SCHEMA as SchemaName
from INFORMATION_SCHEMA.TABLE_CONSTRAINTS fk
join INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS c on c.CONSTRAINT_NAME = fk.CONSTRAINT_NAME
	and c.CONSTRAINT_SCHEMA = fk.CONSTRAINT_SCHEMA
join INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE cc on cc.CONSTRAINT_NAME = c.CONSTRAINT_NAME
	and cc.CONSTRAINT_SCHEMA = fk.CONSTRAINT_SCHEMA
join INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc on tc.CONSTRAINT_NAME = c.UNIQUE_CONSTRAINT_NAME
join INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE tcc on tcc.CONSTRAINT_NAME = tc.CONSTRAINT_NAME
	and tcc.CONSTRAINT_SCHEMA = tc.CONSTRAINT_SCHEMA
WHERE fk.CONSTRAINT_TYPE = 'FOREIGN KEY'
";

		internal override string KeyConstraintSQL => @"
select ku.CONSTRAINT_NAME as ConstraintName, COLUMN_NAME as ColumnName, ORDINAL_POSITION as Ordinal, 
	ku.TABLE_NAME as TableName, tc.TABLE_SCHEMA as SchemaName, '' as ClusteredNonClustered, convert(bit, 1) as IsPrimaryKey, convert(bit, 0) as Descending
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS tc
INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS ku
ON tc.CONSTRAINT_TYPE = 'PRIMARY KEY' 
AND tc.CONSTRAINT_NAME = ku.CONSTRAINT_NAME
and ku.CONSTRAINT_SCHEMA = tc.CONSTRAINT_SCHEMA
";

		internal override string IndexSQL => @"select 
	t.name as TableName,
	i.name as IndexName, 
	c.name as ColumnName, 
	IndexType = case when i.status & 16 <> 0 then 'CLUSTERED' else 'NONCLUSTERED' end,
	KeyNo as Ordinal,
	IsUnique = convert(bit, case when i.status & 2 <> 0 then 1 else 0 end),
	Descending = convert(bit,isnull(INDEXKEY_PROPERTY(i.id,
                                   i.indid,
                                   keyno,
                                   'IsDescending'), 0)),
	IsPrimaryKey = convert(bit, case when i.status & 2048 <> 0 then 1 else 0 end),
	SchemaName = 'dbo'
from sysindexkeys ik
join syscolumns c on c.id = ik.id
	and ik.colid = c.colid
join sysindexes i on i.indid = ik.indid
	and i.id = ik.id
join sysobjects t on t.id = c.id
where i.name not like '_WA_Sys%' and t.xtype = 'U'";

		internal override string DefaultConstraintSQL => @"select t.name as TableName, d.name as ConstraintName, c.name as ColumnName, co.COLUMN_DEFAULT as ColumnDefault, TABLE_SCHEMA as SchemaName 
from sysconstraints dc
join sysobjects d on d.id = dc.constid
join syscolumns c on c.colid = dc.colid
join sysobjects t on t.id = d.parent_obj
	and t.id = c.id
join INFORMATION_SCHEMA.COLUMNS co on co.COLUMN_NAME = c.name and co.TABLE_NAME = t.name
where d.xtype = 'D'";

		internal override string TriggerSQL => @"select Definition = c.text, o.name as TriggerName,  convert(bit, OBJECTPROPERTY(o.id, 'ExecIsTriggerDisabled')) AS Disabled, p.name as TableName, u.name as SchemaName
from sysobjects o
join syscomments c on c.id = o.id
join sysobjects p on p.id = o.parent_obj
join sysusers u on u.uid = p.uid
where o.type = 'TR'
";

		internal override string DatabaseSQL => "select [name] as DatabaseName from master.dbo.sysdatabases where HAS_DBACCESS(name) = 1 order by [name]";

		internal override string ExtendedPropertySQL => @"
select name as PropName, value as PropValue, objtype as Level1Type, objname as Level1Object, null as Level2Type,
	null as Level2Object, 'dbo' as SchemaName, convert(bit, 0) as IgnoreSchema
from ::fn_listextendedproperty 
(NULL, 'user', 'dbo', 'function', NULL, NULL, NULL)
union all
select name, value, objtype, objname, null, null, 'dbo', convert(bit, 0) from ::fn_listextendedproperty 
(NULL, 'user', 'dbo', 'table', NULL, NULL, NULL)
union all
select name, value, objtype, objname, null, null, 'dbo', convert(bit, 0) from ::fn_listextendedproperty 
(NULL, 'user', 'dbo', 'view', NULL, NULL, NULL)
union all
select name, value, objtype, objname, null, null, 'dbo', convert(bit, 0) from ::fn_listextendedproperty 
(NULL, 'user', 'dbo', 'synonym', NULL, NULL, NULL)
";

		internal override string DatabasePrincipalSQL => @"select uid as PrincipalID, altuid as OwningPrincipalID, name as PrincipalName, 
	convert(bit, 0) as IsFixedRole,
	PrincipalType = case when issqlrole = 1 then 'DATABASEROLE'
	when isntuser = 1 then 'WINDOWSUSER'
	else 'SQLUSER'
	end
from sysusers u
";
		internal override string ServerLoginSQL => "";
		internal override string CredentialSQL => "";
		#endregion
	}
}
