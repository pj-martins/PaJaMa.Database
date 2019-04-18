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
	public class RoutineSynonym : DatabaseObjectWithExtendedProperties
	{
		public override string ObjectName
		{
			get { return Name; }
		}

		public string Name { get; set; }
		public RoutineSynonymType Type { get; set; }

		public override string ObjectType
		{
			get { return Type.ToString(); }
		}

		public RoutineSynonym(Database database) : base(database)
		{
		}

		public override string ToString()
		{
			return Schema.SchemaName + "." + Name;
		}

		internal override void setObjectProperties(DbConnection connection, Dictionary<string, object> values)
		{
			var schema = Database.Schemas.First(s => s.SchemaName == values["SchemaName"].ToString());
			this.Definition = string.IsNullOrEmpty(this.Definition) ? string.Empty : this.Definition.Trim();
			this.Schema = schema;
			// TODO:
			if (Database.ExtendedProperties != null)
				this.ExtendedProperties = Database.ExtendedProperties.Where(ep => ep.Level1Object == ObjectName
								&& ep.SchemaName == Schema.SchemaName
								&& ep.Level1Type.ToLower() == ObjectType.ToLower()).ToList();
			this.Schema.RoutinesSynonyms.Add(this);
		}

		public enum RoutineSynonymType
		{
			Procedure,
			Function,
			Synonym
		}
	}
}
