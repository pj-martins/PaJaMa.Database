using PaJaMa.Common;
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

		public Database ParentDatabase { get; }

		[IgnoreCase]
		public string Definition { get; set; }

		public string Description
		{
			get { return ToString() + " (" + ObjectType + ")"; }
		}

		public Schema Schema { get; set; }

		public DatabaseObjectBase(Database database)
		{
			this.ParentDatabase = database;
		}

		public virtual string ObjectType
		{
			get { return this.GetType().Name; }
		}

		public override string ToString()
		{
			return ObjectName;
		}

		internal abstract void setObjectProperties(DbDataReader reader);

		public virtual string GetObjectNameWithSchema(DataSource server)
		{
			var schema = this.Schema == null ? server.DefaultSchemaName : this.Schema.SchemaName;
			if (string.IsNullOrEmpty(schema))
				return server.GetConvertedObjectName(ObjectName);

			return string.Format("{0}.{1}",
				server.GetConvertedObjectName(schema),
				server.GetConvertedObjectName(ObjectName));
		}

		public string GetQueryObjectName(DataSource server)
		{
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
}
