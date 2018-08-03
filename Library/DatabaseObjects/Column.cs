﻿using PaJaMa.Common;
using PaJaMa.Database.Library.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PaJaMa.Database.Library.DatabaseObjects
{
	public class Column : DatabaseObjectWithExtendedProperties
	{
		public Column(Database database) : base(database)
		{
		}

		public override string ObjectName
		{
			get { return ColumnName; }
		}

		[Ignore]
		public Table Table { get; set; }

		public ColumnType ColumnType { get; set; }

		public string ColumnName { get; set; }

		[Ignore]
		public int OrdinalPosition { get; set; }

		public bool IsIdentity { get; set; }
		public int? CharacterMaximumLength { get; set; }
		public bool IsNullable { get; set; }
		public string Formula { get; set; }
		public string ColumnDefault { get; set; }
		public int? NumericPrecision { get; set; }
		public int? NumericScale { get; set; }
		public decimal? Increment { get; set; }
		public string UDTName { get; set; }

		[Ignore]
		public string ConstraintName { get; set; }

		internal override void setObjectProperties(DbDataReader reader)
		{
			var schema = Database.Schemas.First(s => s.SchemaName == reader["SchemaName"].ToString());
			ColumnType = Database.DataSource.GetColumnType(reader["DataType"].ToString(), this.ColumnDefault);
			this.Table = schema.Tables.First(t => t.TableName == reader["TableName"].ToString());
			this.Table.Columns.Add(this);
			if (Database.ExtendedProperties != null)
				this.ExtendedProperties = Database.ExtendedProperties.Where(ep => ep.Level1Object == this.Table.ObjectName
				&& ep.SchemaName == this.Table.Schema.SchemaName &&
				ep.Level2Object == this.ColumnName).ToList();
		}
	}

	public class ColumnType
	{
		public string TypeName { get; private set; }
		public string CreateTypeName { get; set; }
		public Type ClrType { get; private set; }
		public DataType DataType { get; private set; }
		public DbType DbType { get; private set; }
		public string DefaultValue { get; private set; }
		public Map[] MappedValues { get; private set; }
		public bool IsFixedSize { get; set; }

		public ColumnType(string typeName, DataType dataType, string defaultValue, params Map[] maps)
		{
			this.TypeName = typeName;
			this.CreateTypeName = typeName;
			this.DataType = dataType;
			this.MappedValues = maps;
			this.DefaultValue = defaultValue;
			var memInfo = typeof(DataType).GetMember(dataType.ToString())[0];
			this.DbType = (memInfo.GetCustomAttributes(typeof(DbTypeAttribute), true).First() as DbTypeAttribute).DbType;
		}

		public override string ToString()
		{
			return this.CreateTypeName;
		}
	}

	public enum DataType
	{
		[DbType(DbType.Guid)]
		UniqueIdentifier,
		[DbType(DbType.DateTime2)]
		DateTime,
		[DbType(DbType.DateTime2)]
		SmallDateTime,
		[DbType(DbType.Date)]
		DateOnly,
		[DbType(DbType.String)]
		VaryingChar,
		[DbType(DbType.String)]
		Char,
		[DbType(DbType.Int16)]
		SmallInteger,
		[DbType(DbType.Int32)]
		Integer,
		[DbType(DbType.Double)]
		Real,
		[DbType(DbType.Decimal)]
		Money,
		[DbType(DbType.Boolean)]
		Boolean,
		[DbType(DbType.Xml)]
		Xml,
		[DbType(DbType.String)]
		Json,
		[DbType(DbType.Double)]
		Float,
		[DbType(DbType.Binary)]
		VarBinary,
		[DbType(DbType.String)]
		Text,
		[DbType(DbType.Decimal)]
		Decimal,
		[DbType(DbType.Decimal)]
		Numeric,
		[DbType(DbType.Binary)]
		Binary,
		[DbType(DbType.Time)]
		TimeOnly,
		[DbType(DbType.Int64)]
		BigInt,
		[DbType(DbType.Object)]
		RowVersion,
		[DbType(DbType.Object)]
		Array,
		[DbType(DbType.String)]
		VarChar,
		[DbType(DbType.String)]
		NVarChar
	}

	public class DbTypeAttribute : Attribute
	{
		public DbType DbType { get; private set; }
		public DbTypeAttribute(DbType dbType)
		{
			DbType = dbType;
		}
	}

	public class Map
	{
		public object Value { get; private set; }
		public string SqlValue { get; private set; }
		public Map(object value, string sqlValue)
		{
			this.Value = value;
			this.SqlValue = sqlValue;
		}
	}
}
