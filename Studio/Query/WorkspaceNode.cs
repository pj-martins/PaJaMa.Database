using PaJaMa.Database.Library.DatabaseObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaJaMa.Database.Studio.Query
{
	public class SchemaNode
	{
		public Schema Schema { get; private set; }
		public SchemaNodeType SchemaNodeType { get; private set; }
		public SchemaNode(Schema schema, SchemaNodeType schemaNodeType)
		{
			this.Schema = schema;
			this.SchemaNodeType = schemaNodeType;
		}
	}

	public enum SchemaNodeType
	{
		Tables,
		Views
	}
}
