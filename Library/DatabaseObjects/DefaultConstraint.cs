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

		internal override void setObjectProperties(DbDataReader reader)
		{
			var schema = ParentDatabase.Schemas.First(s => s.SchemaName == reader["SchemaName"].ToString());
			this.Table = schema.Tables.First(t => t.TableName == reader["TableName"].ToString());
			this.Column = this.Table.Columns.First(c => c.ObjectName == reader["ColumnName"].ToString());
			this.Table.DefaultConstraints.Add(this);
		}
	}
}
