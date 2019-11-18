using PaJaMa.Common;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaJaMa.Database.Library.DatabaseObjects
{
	public class ExtendedProperty : DatabaseObjectBase
	{
		public ExtendedProperty(Database database) : base(database)
		{
		}

		//const string SP_ADD = "EXEC sp_addextendedproperty N'{0}', N'{1}', 'SCHEMA', N'{2}', '{3}', N'{4}', {5}, {6}";
		//const string SP_REMOVE = "EXEC sp_dropextendedproperty N'{0}', 'SCHEMA', N'{1}', '{2}', N'{3}', {4}, {5}";

		public string PropName { get; set; }

		[Ignore]
		public object PropValue { get; set; }

		public string SchemaName { get; set; }
		public string Level1Type { get; set; }
		public string Level1Object { get; set; }
		public string Level2Type { get; set; }
		public string Level2Object { get; set; }
		public bool IgnoreSchema { get; set; }

		public override string ObjectName
		{
			get { return (string.IsNullOrEmpty(Level2Object) ? string.Empty : Level2Object + ".") + PropName; }
		}

		internal override void setObjectProperties(DbConnection connection)
		{
			this.Database.ExtendedProperties.Add(this);
		}
	}
}
