using PaJaMa.Common;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PaJaMa.Database.Library.DatabaseObjects
{
    public class Trigger : DatabaseObjectBase
    {
		public Trigger(Database database) : base(database)
		{
		}

		public string TriggerName { get; set; }
        public Table Table { get; set; }

        public override string ObjectName
        {
            get { return TriggerName; }
        }

        public bool Disabled { get; set; }

        

        public override string ToString()
        {
            return Table.Schema.SchemaName.ToString() + "." + TriggerName;
        }

		internal override void setObjectProperties(DbDataReader reader)
		{
			var schema = Database.Schemas.First(s => s.SchemaName == reader["SchemaName"].ToString());
			this.Table = (from t in schema.Tables
						  where t.TableName == reader["TableName"].ToString()
						  select t).First();

			this.Table.Triggers.Add(this);
		}
	}

    public enum TriggerEvent
    {
        INSERT,
        UPDATE,
        DELETE
    }
}
