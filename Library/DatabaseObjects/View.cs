using PaJaMa.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PaJaMa.Database.Library.DatabaseObjects
{
	public class View : DatabaseObjectWithExtendedProperties
	{
		public override string ObjectName
		{
			get { return ViewName; }
		}

		public string ViewName { get; set; }

		[Ignore]
		public List<Column> Columns { get; set; }

		public override string ToString()
		{
			return Schema.SchemaName + "." + ViewName;
		}

		public View(Database database) : base(database)
		{
			Columns = new List<Column>();
		}

		internal override void setObjectProperties(DbDataReader reader)
		{
			var schema = ParentDatabase.Schemas.First(s => s.SchemaName == reader["ObjectSchema"].ToString());
			var viewName = reader["ViewName"].ToString();
			var currView = schema.Views.FirstOrDefault(v => v.ViewName == viewName && v.Schema.SchemaName == schema.SchemaName);
			if (currView == null)
			{
				currView = reader.ToObject<View>(ParentDatabase);
				currView.Schema = schema;
				currView.ExtendedProperties = ParentDatabase.ExtendedProperties.Where(ep => ep.Level1Object == currView.ViewName && ep.ObjectSchema == currView.Schema.SchemaName &&
					string.IsNullOrEmpty(ep.Level2Object)).ToList();
				schema.Views.Add(currView);
			}

			var col = reader.ToObject<Column>(ParentDatabase);
			if (!string.IsNullOrEmpty(col.ColumnName))
				currView.Columns.Add(col);
		}
	}
}
