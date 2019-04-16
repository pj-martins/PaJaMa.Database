﻿using MySql.Data.MySqlClient;
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

		public override string DefaultSchemaName => "";

		protected override Type connectionType => typeof(MySqlConnection);

		// TODO: owner
		internal override string SchemaSQL => "";

		internal override string ViewSQL => throw new NotImplementedException();

		internal override string TableSQL => "select TABLE_NAME as TableName, '' as SchemaName, null as Definition from INFORMATION_SCHEMA.TABLES where TABLE_TYPE = 'BASE TABLE' and TABLE_SCHEMA = '{0}'";

		internal override string ColumnSQL => @"
select TABLE_NAME as TableName, COLUMN_NAME as ColumnName, ORDINAL_POSITION as OrdinalPosition2, 
	CHARACTER_MAXIMUM_LENGTH as CharacterMaximumLength2, DATA_TYPE as DataType,
    case when co.IS_NULLABLE = 'YES' then true else false END AS IsNullable2,
	 case when EXTRA = 'auto_increment' then TRUE ELSE false end as IsIdentity2, 
	 CONCAT('DF_', TABLE_NAME, '_', COLUMN_NAME) as ConstraintName,
	  COLUMN_DEFAULT as ColumnDefault, null as Formula, NUMERIC_PRECISION as NumericPrecision2, NUMERIC_SCALE as NumericScale2,
	  '' AS SchemaName, null AS Increment
FROM INFORMATION_SCHEMA.COLUMNS co
where TABLE_SCHEMA = '{0}'";

		internal override string ForeignKeySQL => @"
SELECT distinct
    tc.constraint_name as ForeignKeyName, tc.table_name as ChildTableName, kcu.column_name as ChildColumnName, 
   c.referenced_table_name AS ParentTableName, referenced_column_name AS ParentColumnName, UPDATE_RULE as UpdateRule, DELETE_RULE as DeleteRule,
	 referenced_table_schema as ParentTableSchema, tc.TABLE_SCHEMA as ChildTableSchema, tc.CONSTRAINT_SCHEMA as SchemaName
FROM information_schema.table_constraints AS tc
JOIN information_schema.key_column_usage AS kcu
	ON tc.constraint_name = kcu.constraint_name
	AND tc.table_name = kcu.TABLE_NAME
JOIN INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS c ON c.CONSTRAINT_NAME = tc.CONSTRAINT_NAME
	AND c.CONSTRAINT_SCHEMA = tc.CONSTRAINT_SCHEMA
WHERE constraint_type = 'FOREIGN KEY'
and tc.TABLE_SCHEMA = '{0}'";

		internal override string KeyConstraintSQL => throw new NotImplementedException();

		internal override string IndexSQL => throw new NotImplementedException();

		internal override string DefaultConstraintSQL => throw new NotImplementedException();

		internal override string TriggerSQL => throw new NotImplementedException();

		internal override string DatabaseSQL => "SELECT SCHEMA_NAME AS DatabaseName FROM information_schema.schemata";

		private List<ColumnType> _columnTypes;
		public override List<ColumnType> ColumnTypes
		{
			get
			{
				if (_columnTypes == null)
				{
					_columnTypes = new List<ColumnType>();
					_columnTypes.Add(new ColumnType("int", DataType.Integer, "0"));
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
					_columnTypes.Add(new ColumnType("text", DataType.Text, "0"));
					_columnTypes.Add(new ColumnType("tinytext", DataType.Text, "0"));
					_columnTypes.Add(new ColumnType("mediumtext", DataType.Text, "0"));
					_columnTypes.Add(new ColumnType("longtext", DataType.Text, "0"));
					_columnTypes.Add(new ColumnType("enum", DataType.Integer, "0"));
					_columnTypes.Add(new ColumnType("json", DataType.Json, "0"));
					_columnTypes.Add(new ColumnType("set", DataType.Array, "0"));
				}
				return _columnTypes;
			}
		}

		public override string GetConvertedObjectName(string objectName)
		{
			throw new NotImplementedException();
		}
	}
}
