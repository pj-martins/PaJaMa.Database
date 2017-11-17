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

		public string ColumnName { get; set; }

		[Ignore]
		public int OrdinalPosition { get; set; }

		public bool IsIdentity { get; set; }
		public string DataType { get; set; }
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
			var schema = ParentDatabase.Schemas.First(s => s.SchemaName == reader["SchemaName"].ToString());
			this.Table = schema.Tables.First(t => t.TableName == reader["TableName"].ToString());
			this.Table.Columns.Add(this);
			this.ExtendedProperties = ParentDatabase.ExtendedProperties.Where(ep => ep.Level1Object == this.Table.ObjectName 
				&& ep.ObjectSchema == this.Table.Schema.SchemaName &&
				ep.Level2Object == this.ColumnName).ToList();
		}
	}
}
