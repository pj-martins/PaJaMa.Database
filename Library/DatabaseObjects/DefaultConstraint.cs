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

		public override string ObjectName
		{
			get { return ConstraintName; }
		}

		public Table Table { get; set; }
		public string ConstraintName { get; set; }
		public Column Column { get; set; }
		public string ColumnDefault { get; set; }

		internal override void setObjectProperties(DbConnection connection, Dictionary<string, object> values)
		{
			var schema = Database.Schemas.First(s => s.SchemaName == values["SchemaName"].ToString());
			this.Table = schema.Tables.First(t => t.TableName == values["TableName"].ToString());
			this.Column = this.Table.Columns.First(c => c.ObjectName == values["ColumnName"].ToString());
			this.Table.DefaultConstraints.Add(this);
		}
	}
}
