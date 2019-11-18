using Newtonsoft.Json;
using PaJaMa.Common;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaJaMa.Database.Library.DatabaseObjects
{
	public class DefaultConstraint : DatabaseObjectBase
	{
		public DefaultConstraint(Database database) : base(database)
		{
		}

		[JsonIgnore]
		public override string ObjectName
		{
			get { return ConstraintName; }
		}

		public string TableName { get; set; }
		[JsonIgnore]
		public Table Table { get; set; }
		public string ConstraintName { get; set; }

		public string ColumnName { get; set; }
		[JsonIgnore]
		public Column Column { get; set; }
		public string ColumnDefault { get; set; }

		internal override void setObjectProperties(DbConnection connection)
		{
			var schema = Database.Schemas.First(s => s.SchemaName ==this.SchemaName);
			this.Table = schema.Tables.First(t => t.TableName == this.TableName);
			if (!this.Table.Columns.Any()) Database.DataSource.PopulateChildColumns(connection, this.Table);
			this.Column = this.Table.Columns.First(c => c.ObjectName == this.ColumnName);
			this.Table.DefaultConstraints.Add(this);
		}
	}
}
