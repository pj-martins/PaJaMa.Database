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
	public class KeyConstraint : DatabaseObjectBase, IObjectWithParent
	{
		public string TableName { get; set; }
		[JsonIgnore]
		public DatabaseObjectWithColumns Parent { get; set; }

		public string ConstraintName { get; set; }

		public override string ObjectName
		{
			get { return ConstraintName; }
		}


		public string ColumnName { get; set; }
		public bool Descending { get; set; }
		public int Ordinal { get; set; }

		[JsonIgnore]
		[Ignore]
		public List<IndexColumn> Columns { get; set; }

		public string ClusteredNonClustered { get; set; }
		public bool IsPrimaryKey { get; set; }

		[JsonIgnore]
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

		internal override void setObjectProperties(DbConnection connection)
		{
			string tableName = this.TableName;
			var schema = Database.Schemas.First(s => s.SchemaName == this.SchemaName);
			if (!schema.Tables.Any()) Database.DataSource.PopulateTables(connection, new Schema[] { schema }, true);
			var table = schema.Tables.First(t => t.TableName == this.TableName);
			var constraint = table.KeyConstraints.FirstOrDefault(c => c.ConstraintName == this.ConstraintName && c.TableName == tableName
				&& c.SchemaName == schema.SchemaName);
			if (constraint == null)
			{
				constraint = this;
				var tbl = schema.Tables.First(t => t.TableName == this.TableName);
				constraint.Parent = tbl;
				tbl.KeyConstraints.Add(constraint);
			}
			constraint.Columns.Add(new IndexColumn() { ColumnName = this.ColumnName, Descending = this.Descending, Ordinal = this.Ordinal });
		}
	}
}
