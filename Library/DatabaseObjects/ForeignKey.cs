using Newtonsoft.Json;
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

		public string ChildTableSchema { get; set; }
		public string ChildTableName { get; set; }
		public string ChildColumnName { get; set; }

		[JsonIgnore]
		public Table ChildTable { get; set; }

		public string ParentTableSchema { get; set; }
		public string ParentTableName { get; set; }
		public string ParentColumnName { get; set; }

		[JsonIgnore]
		public Table ParentTable { get; set; }

		[JsonIgnore]
		[Ignore]
		public List<ForeignKeyColumn> Columns { get; set; }
		public string UpdateRule { get; set; }
		public string DeleteRule { get; set; }
		public string WithCheck { get; set; }

		[JsonIgnore]
		[Ignore]
		public bool HasBeenDropped { get; set; }

		public ForeignKey(Database database) : base(database)
		{
			Columns = new List<ForeignKeyColumn>();
		}

		internal override void setObjectProperties(DbConnection connection)
		{
			var parentSchema = Database.Schemas.First(s => s.SchemaName == this.ParentTableSchema);
			if (!parentSchema.Tables.Any()) Database.DataSource.PopulateTables(connection, new Schema[] { parentSchema }, true);
			var childSchema = Database.Schemas.First(s => s.SchemaName == this.ChildTableSchema);
			var childTable = childSchema.Tables.First(t => t.TableName == this.ChildTableName);
			var foreignKey = childTable.ForeignKeys.FirstOrDefault(f => f.ForeignKeyName == ForeignKeyName 
				&& f.ChildTableName == ChildTableName
				&& f.ChildTableSchema == childSchema.SchemaName);

			if (foreignKey == null)
			{
				foreignKey = this;
				foreignKey.ParentTable = parentSchema.Tables.First(t => t.TableName == this.ParentTableName);
				if (!foreignKey.ParentTable.Columns.Any()) Database.DataSource.PopulateChildColumns(connection, foreignKey.ParentTable);
				foreignKey.ChildTable = childTable;
				if (!foreignKey.ChildTable.Columns.Any()) Database.DataSource.PopulateChildColumns(connection, foreignKey.ChildTable);
				foreignKey.ChildTable.ForeignKeys.Add(foreignKey);
			}
			else if (foreignKey.ParentTable == null)
			{
				foreignKey.ParentTable = parentSchema.Tables.First(t => t.TableName == this.ParentTableName);
				foreignKey.ChildTable = childTable;
			}

			foreignKey.Columns.Add(new ForeignKeyColumn()
			{
				ParentColumn = foreignKey.ParentTable.Columns.First(c => c.ColumnName == this.ParentColumnName),
				ChildColumn = foreignKey.ChildTable.Columns.First(c => c.ColumnName == this.ChildColumnName)
			});
		}
	}

	public class ForeignKeyColumn
	{
		public Column ChildColumn { get; set; }
		public Column ParentColumn { get; set; }
	}
}
