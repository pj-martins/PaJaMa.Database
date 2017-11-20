using PaJaMa.Common;
using PaJaMa.Database.Library.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

		[Ignore]
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

		[Ignore]
		public string ConstraintName { get; set; }

		internal override void setObjectProperties(DbDataReader reader)
		{
			var schema = Database.Schemas.First(s => s.SchemaName == reader["SchemaName"].ToString());
			ColumnType = Database.DataSource.GetColumnType(reader["DataType"].ToString());
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
		public DataType DataType { get; private set; }
		public string DefaultValue { get; private set; }
		public bool FixedSize { get; set; }

		public ColumnType(string typeName, DataType dataType, string defaultValue)
		{
			this.TypeName = typeName;
			this.CreateTypeName = typeName;
			this.DataType = dataType;
			this.DefaultValue = defaultValue;
		}

		public override string ToString()
		{
			return this.DataType.ToString();
		}
	}

	public enum DataType
	{
		UniqueIdentifier,
		DateTimeZone,
		SmallDateTime,
		DateOnly,
		NVaryingChar,
		VaryingChar,
		Integer,
		BooleanTrue,
		BooleanFalse,
		Xml,
		Float,
		VarBinary,
		Text,
		Decimal,
		Numeric,
		Binary,
		TimeOnly,
		BigInt,
		RowVersion,
	}
}
