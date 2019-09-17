using PaJaMa.Common;
using PaJaMa.Database.Library.DataSources;
using PaJaMa.Database.Library.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaJaMa.Database.Library.DatabaseObjects
{
	public abstract class DatabaseObjectBase
	{
		public virtual bool Synchronized { get; set; }

		public abstract string ObjectName { get; }

		public Database Database { get; }

		[IgnoreCase]
		public string Definition { get; set; }

		public string Description
		{
			get { return ToString() + " (" + ObjectType + ")"; }
		}

		[Ignore]
		internal Dictionary<string, object> RawValues { get; set; }

		public Schema Schema { get; set; }

		public DatabaseObjectBase(Database database)
		{
			this.Database = database;
		}

		public virtual string ObjectType
		{
			get { return this.GetType().Name; }
		}

		public override string ToString()
		{
			return ObjectName;
		}

		internal abstract void setObjectProperties(DbConnection connection, Dictionary<string, object> values);

		public virtual string GetObjectNameWithSchema(DataSource dataSource)
		{
			var schema = this.Schema == null || this.Schema.SchemaName == this.Database.DataSource.DefaultSchemaName ? dataSource.DefaultSchemaName : this.Schema.SchemaName;
			if (string.IsNullOrEmpty(schema))
				return dataSource.GetConvertedObjectName(ObjectName);

			return string.Format("{0}.{1}",
				dataSource.GetConvertedObjectName(schema),
				dataSource.GetConvertedObjectName(ObjectName));
		}

		public string GetQueryObjectName(DataSource server)
		{
            if (string.IsNullOrEmpty(ObjectName)) return string.Empty;
			return server.GetConvertedObjectName(ObjectName);
		}
	}

	public abstract class DatabaseObjectWithExtendedProperties : DatabaseObjectBase
	{
		[Ignore]
		public List<ExtendedProperty> ExtendedProperties { get; set; }

		public DatabaseObjectWithExtendedProperties(Database database) : base(database)
		{
			ExtendedProperties = new List<ExtendedProperty>();
		}
	}

	public interface IObjectWithExtendedProperty
	{
		List<ExtendedProperty> ExtendedProperties { get; set; }
	}

	public interface IObjectWithParent
	{
		DatabaseObjectWithColumns Parent { get; }
	}
}
