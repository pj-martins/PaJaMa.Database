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
	public class KeyConstraint : DatabaseObjectBase
	{
		public Table Table { get; set; }
		public string ConstraintName { get; set; }

		public override string ObjectName
		{
			get { return ConstraintName; }
		}

		[Ignore]
		public List<IndexColumn> Columns { get; set; }

		public string ClusteredNonClustered { get; set; }
		public bool IsPrimaryKey { get; set; }

		public KeyConstraint(Database database) : base(database)
		{
			Columns = new List<IndexColumn>();
		}

		internal override void setObjectProperties(DbDataReader reader)
		{
			string tableName = reader["TableName"].ToString();
			var schema = ParentDatabase.Schemas.First(s => s.SchemaName == reader["SchemaName"].ToString());
			var table = schema.Tables.First(t => t.TableName == reader["TableName"].ToString());
			var constraint = table.KeyConstraints.FirstOrDefault(c => c.ConstraintName == this.ConstraintName && c.Table.TableName == tableName
				&& c.Table.Schema.SchemaName == schema.SchemaName);
			if (constraint == null)
			{
				constraint = this;
				constraint.Table = schema.Tables.First(t => t.TableName == reader["TableName"].ToString());
				constraint.Table.KeyConstraints.Add(constraint);
			}

			var col = reader.ToObject<IndexColumn>();
			constraint.Columns.Add(col);
		}
	}
}
