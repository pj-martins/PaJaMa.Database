using Newtonsoft.Json;
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
	public class View : DatabaseObjectWithColumns
	{
		[JsonIgnore]
		public override string ObjectName
		{
			get { return ViewName; }
		}

		public string ViewName { get; set; }

		public override string ToString()
		{
			return Schema.SchemaName + "." + ViewName;
		}

		public View(Database database) : base(database)
		{
		}

		internal override void setObjectProperties(DbConnection connection)
		{
			var schema = Database.Schemas.First(s => s.SchemaName == this.SchemaName);
			var currView = schema.Views.FirstOrDefault(v => v.ViewName == this.ViewName && v.Schema.SchemaName == schema.SchemaName);
			if (currView == null)
			{
				currView = this;
				currView.Schema = schema;
				if (Database.ExtendedProperties != null)
					currView.ExtendedProperties = Database.ExtendedProperties.Where(ep => ep.Level1Object == currView.ViewName && ep.SchemaName == currView.Schema.SchemaName &&
					string.IsNullOrEmpty(ep.Level2Object)).ToList();
				schema.Views.Add(currView);
			}

			// TODO: NEW
			//var col = values.DictionaryToObject<Column>(Database);
			//if (!string.IsNullOrEmpty(col.ColumnName))
			//	currView.Columns.Add(col);
		}
	}
}
