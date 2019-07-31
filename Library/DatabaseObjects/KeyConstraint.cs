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
	public class KeyConstraint : DatabaseObjectBase, IObjectWithTable
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

        [Ignore]
        public Int64 IsPrimaryKey2
        {
            get { return IsPrimaryKey ? 1 : 0; }
            set { IsPrimaryKey = value == 1; }
        }

		public KeyConstraint(Database database) : base(database)
		{
			Columns = new List<IndexColumn>();
		}

		internal override void setObjectProperties(DbConnection connection, Dictionary<string, object> values)
		{
			string tableName = values["TableName"].ToString();
			var schema = Database.Schemas.First(s => s.SchemaName == values["SchemaName"].ToString());
			if (!schema.Tables.Any()) Database.DataSource.PopulateTables(connection, new Schema[] { schema }, true);
			var table = schema.Tables.First(t => t.TableName == values["TableName"].ToString());
			var constraint = table.KeyConstraints.FirstOrDefault(c => c.ConstraintName == this.ConstraintName && c.Table.TableName == tableName
				&& c.Table.Schema.SchemaName == schema.SchemaName);
			if (constraint == null)
			{
				constraint = this;
				constraint.Table = schema.Tables.First(t => t.TableName == values["TableName"].ToString());
				constraint.Table.KeyConstraints.Add(constraint);
			}

			var col = values.DictionaryToObject<IndexColumn>();
			constraint.Columns.Add(col);
		}
	}
}
