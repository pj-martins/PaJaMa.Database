using PaJaMa.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PaJaMa.Database.Library.DatabaseObjects
{
    public class Sequence : DatabaseObjectBase
    {
		public Sequence(Database database) : base(database)
		{
		}

		public override string ObjectName
        {
            get { return SequenceName; }
        }

        public string SequenceName { get; set; }
        public string Increment { get; set; }
        public string MinValue { get; set; }
        public string MaxValue { get; set; }
        public string Start { get; set; }
        public string Cycle { get; set; }

        // TODO: cache

        public override string ToString()
        {
            return Schema.SchemaName + "." + SequenceName;
        }

		internal override void setObjectProperties(DbDataReader reader)
		{
			this.Schema = ParentDatabase.Schemas.First(s => s.SchemaName == reader["sequence_schema"].ToString());
			this.Schema.Sequences.Add(this);
		}
	}
}
