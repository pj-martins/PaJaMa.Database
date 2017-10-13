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
        public Schema Schema { get; set; }
        public override string ObjectName
        {
            get { return SequenceName; }
        }

        public override Database ParentDatabase => Schema.ParentDatabase;

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

        public static void PopulateSequences(Database database, DbConnection connection)
		{
            if (!database.IsPostgreSQL) return;

            string qry = @"select 
    sequence_schema, 
    sequence_name as SequenceName,
    increment,
    minimum_value as MinValue,
    maximum_value as MaxValue,
    start_value as Start,
    cycle_option as Cycle
from INFORMATION_SCHEMA.SEQUENCES";

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = qry;
                using (var rdr = cmd.ExecuteReader())
                {
                    if (rdr.HasRows)
                    {
                        while (rdr.Read())
                        {
                            var sequence = rdr.ToObject<Sequence>();
                            sequence.Schema = database.Schemas.First(s => s.SchemaName == rdr["sequence_schema"].ToString());

                            sequence.Schema.Sequences.Add(sequence);
                        }
                    }
                    rdr.Close();
                }
            }
        }
	}
}
