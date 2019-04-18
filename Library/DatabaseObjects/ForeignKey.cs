using PaJaMa.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaJaMa.Database.Library.DatabaseObjects
{
	public class ForeignKey : DatabaseObjectBase
	{
		public override string ObjectName
		{
			get { return ForeignKeyName; }
		}

		public string ForeignKeyName { get; set; }
		public Table ChildTable { get; set; }
		public Table ParentTable { get; set; }

		[Ignore]
		public List<ForeignKeyColumn> Columns { get; set; }
		public string UpdateRule { get; set; }
		public string DeleteRule { get; set; }
		public string WithCheck { get; set; }

		[Ignore]
		public bool HasBeenDropped { get; set; }

		public ForeignKey(Database database) : base(database)
		{
			Columns = new List<ForeignKeyColumn>();
		}

		internal override void setObjectProperties(DbDataReader reader)
		{
			var foreignKeyName = reader["ForeignKeyName"].ToString();
			var childTableName = reader["ChildTableName"].ToString();
			var parentSchema = Database.Schemas.First(s => s.SchemaName == reader["ParentTableSchema"].ToString());
			if (!parentSchema.Tables.Any()) Database.DataSource.PopulateTables(new Schema[] { parentSchema }, true);
			var childSchema = Database.Schemas.First(s => s.SchemaName == reader["ChildTableSchema"].ToString());
			var childTable = childSchema.Tables.First(t => t.TableName == reader["ChildTableName"].ToString());
			var foreignKey = childTable.ForeignKeys.FirstOrDefault(f => f.ForeignKeyName == foreignKeyName 
				&& f.ChildTable.TableName == childTableName
				&& f.ChildTable.Schema.SchemaName == childSchema.SchemaName);

			if (foreignKey == null)
			{
				foreignKey = this;
				foreignKey.ParentTable = parentSchema.Tables.First(t => t.TableName == reader["ParentTableName"].ToString());
				foreignKey.ChildTable = childTable;
				foreignKey.ChildTable.ForeignKeys.Add(foreignKey);
			}

			foreignKey.Columns.Add(new ForeignKeyColumn()
			{
				ParentColumn = foreignKey.ParentTable.Columns.First(c => c.ColumnName == reader["ParentColumnName"].ToString()),
				ChildColumn = foreignKey.ChildTable.Columns.First(c => c.ColumnName == reader["ChildColumnName"].ToString())
			});
		}
	}

	public class ForeignKeyColumn
	{
		public Column ChildColumn { get; set; }
		public Column ParentColumn { get; set; }
	}
}
