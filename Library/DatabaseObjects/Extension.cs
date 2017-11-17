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
	public class Extension : DatabaseObjectBase
	{
		public override string ObjectName
		{
			get { return Name; }
		}

		public string Name { get; set; }

        private Database _parentDatabase;
        public override Database ParentDatabase
        {
            get { return _parentDatabase; }
        }

        public override string ToString()
		{
			return Name;
		}

		public static void PopulateExtensions(Database database, DbConnection connection)
		{
            if (!database.IsPostgreSQL) return;

            string qry = "select * from pg_available_extensions where installed_version <> ''";

            using (var cmd = connection.CreateCommand())
			{
				cmd.CommandText = qry;
				using (var rdr = cmd.ExecuteReader())
				{
					if (rdr.HasRows)
					{
						while (rdr.Read())
						{
							var ext = rdr.ToObject<Extension>();
                            ext._parentDatabase = database;
                            database.Extensions.Add(ext);
						}
					}
				}
			}
		}
	}
}
