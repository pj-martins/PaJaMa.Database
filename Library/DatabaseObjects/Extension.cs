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
		public Extension(Database database) : base(database)
		{
		}

		public override string ObjectName
		{
			get { return Name; }
		}

		public string Name { get; set; }

		public override string ToString()
		{
			return Name;
		}

		internal override void setObjectProperties(DbConnection connection)
		{
			Database.Extensions.Add(this);
		}
	}
}
