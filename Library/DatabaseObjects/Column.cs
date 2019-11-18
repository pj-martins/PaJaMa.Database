using Newtonsoft.Json;
using PaJaMa.Common;
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
	public class Column : DatabaseObjectWithExtendedProperties, IObjectWithParent
	{
		public Column(Database database) : base(database)
		{
		}

		public override string ObjectName
		{
			get { return ColumnName; }
		}

		[Ignore]
		[JsonIgnore]
		public override string ProgressDisplay => $"{this.SchemaName}.{this.TableName}";

		[Ignore]
		[JsonIgnore]
		public DatabaseObjectWithColumns Parent { get; set; }

		[JsonIgnore]
		public ColumnType ColumnType { get; set; }

		public string DataType { get; set; }

		public string TableName { get; set; }
		public string ColumnName { get; set; }

		[Ignore]
		public int OrdinalPosition { get; set; }

		[JsonIgnore]
		[Ignore]
		public UInt64 OrdinalPosition2
		{
			get { return (UInt64)OrdinalPosition; }
			set { OrdinalPosition = Convert.ToInt32(value); }
		}

		public bool IsIdentity { get; set; }

		[JsonIgnore]
		[Ignore]
		public Int64 IsIdentity2
		{
			get { return IsIdentity ? 1 : 0; }
			set { IsIdentity = value == 1; }
		}

		public int? CharacterMaximumLength { get; set; }

		[JsonIgnore]
		[Ignore]
		public UInt64 CharacterMaximumLength2
		{
			get { return (UInt64)CharacterMaximumLength; }
			set
			{
				var parsed = 0;
				int.TryParse(value.ToString(), out parsed);
				CharacterMaximumLength = parsed;
			}
		}

		public bool IsNullable { get; set; }

		[JsonIgnore]
		[Ignore]
		public Int64 IsNullable2
		{
			get { return IsNullable ? 1 : 0; }
			set { IsNullable = value == 1; }
		}

		public string Formula { get; set; }
		public string ColumnDefault { get; set; }
		public int? NumericPrecision { get; set; }

		[JsonIgnore]
		[Ignore]
		public UInt64 NumericPrecision2
		{
			get { return (UInt64)NumericPrecision.GetValueOrDefault(); }
			set { NumericPrecision = Convert.ToInt16(value); }
		}

		public int? NumericScale { get; set; }

		[JsonIgnore]
		[Ignore]
		public UInt64 NumericScale2
		{
			get { return (UInt64)NumericScale.GetValueOrDefault(); }
			set { NumericScale = Convert.ToInt16(value); }
		}


		public decimal? Increment { get; set; }
		public string UDTName { get; set; }

		[JsonIgnore]
		[Ignore]
		public string ConstraintName { get; set; }

		internal override void setObjectProperties(DbConnection connection)
		{
			var schema = Database.Schemas.FirstOrDefault(s => s.SchemaName ==this.SchemaName);
			if (schema == null) throw new Exception("Schema " +this.SchemaName + " not found for column " + this.ColumnName);
			ColumnType = Database.DataSource.GetColumnType(this.DataType, this.ColumnDefault);
			DatabaseObjectWithColumns objWithColumns = null;
			objWithColumns = schema.Tables.FirstOrDefault(t => t.TableName == this.TableName);
			if (objWithColumns == null)
			{
				objWithColumns = schema.Views.FirstOrDefault(t => t.ViewName == this.TableName);
			}
			this.Parent = objWithColumns ?? throw new Exception("Object " + objWithColumns + " not found for column " + this.ColumnName);
			objWithColumns.Columns.Add(this);
			if (Database.ExtendedProperties != null)
				this.ExtendedProperties = Database.ExtendedProperties.Where(ep => ep.Level1Object == objWithColumns.ObjectName
				&& ep.SchemaName == objWithColumns.Schema.SchemaName &&
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
