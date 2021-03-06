﻿using PaJaMa.Common;
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
		[Ignore]
		public string ChildTableSchema { get; set; }
		[Ignore]
		public string ChildTableName { get; set; }
		[Ignore]
		public string ChildColumnName { get; set; }
		public Table ChildTable { get; set; }
		[Ignore]
		public string ParentTableSchema { get; set; }
		[Ignore]
		public string ParentTableName { get; set; }
		[Ignore]
		public string ParentColumnName { get; set; }
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

		internal override void setObjectProperties(DbConnection connection, Dictionary<string, object> values)
		{
			var foreignKeyName = values["ForeignKeyName"].ToString();
			var childTableName = values["ChildTableName"].ToString();
			var parentSchema = Database.Schemas.First(s => s.SchemaName == values["ParentTableSchema"].ToString());
			if (!parentSchema.Tables.Any()) Database.DataSource.PopulateTables(connection, new Schema[] { parentSchema }, true);
			var childSchema = Database.Schemas.First(s => s.SchemaName == values["ChildTableSchema"].ToString());
			var childTable = childSchema.Tables.FirstOrDefault(t => t.TableName == this.ChildTableName);
			if (childTable == null) return;
			var foreignKey = childTable.ForeignKeys.FirstOrDefault(f => f.ForeignKeyName == foreignKeyName
				&& f.ChildTable.TableName == childTableName
				&& f.ChildTable.Schema.SchemaName == childSchema.SchemaName);

			if (foreignKey == null)
			{
				foreignKey = this;
				foreignKey.ParentTable = parentSchema.Tables.First(t => t.TableName == this.ParentTableName);
				if (!foreignKey.ParentTable.Columns.Any()) Database.DataSource.PopulateChildColumns(connection, foreignKey.ParentTable);
				foreignKey.ChildTable = childTable;
				if (!foreignKey.ChildTable.Columns.Any()) Database.DataSource.PopulateChildColumns(connection, foreignKey.ChildTable);
				foreignKey.ChildTable.ForeignKeys.Add(foreignKey);
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
