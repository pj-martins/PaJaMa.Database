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
	public class Schema : DatabaseObjectWithExtendedProperties
	{
		[Ignore]
		public List<Table> Tables { get; private set; }

		[Ignore]
		public List<View> Views { get; private set; }

		[Ignore]
		public List<RoutineSynonym> RoutinesSynonyms { get; private set; }

		[Ignore]
		public List<Sequence> Sequences { get; private set; }

		public bool IsSystemSchema
		{
			get { return this.Database.DataSource.SystemSchemaNames.Contains(this.SchemaName); }
		}

		public override string ObjectName
		{
			get { return SchemaName; }
		}

		public string SchemaName { get; set; }
		public string SchemaOwner { get; set; }

		public string MappedSchemaName
		{
			get
			{
				if (SchemaName == Database.DataSource.DefaultSchemaName)
					return "__DEFAULT__";
				return SchemaName;
			}
		}

		public Schema(Database database) : base(database)
		{
			Tables = new List<Table>();
			RoutinesSynonyms = new List<RoutineSynonym>();
			Views = new List<View>();
			Sequences = new List<Sequence>();
		}

		internal override void setObjectProperties(DbConnection connection, Dictionary<string, object> values)
		{
			if (Database.ExtendedProperties != null)
				this.ExtendedProperties = Database.ExtendedProperties
								.Where(ep => ep.Level1Type == typeof(Schema).Name.ToUpper() && ep.Level1Object == this.SchemaName)
								.ToList();
			if (Database.Principals != null)
			{
				var owner = Database.Principals.FirstOrDefault(p => SchemaOwner == p.ObjectName);
				if (owner != null)
					owner.Ownings.Add(this);
			}
			Database.Schemas.Add(this);
		}
	}
}
