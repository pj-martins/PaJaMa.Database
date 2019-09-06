using PaJaMa.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaJaMa.Database.Library.DatabaseObjects
{
	public abstract class DatabaseObjectWithColumns : DatabaseObjectWithExtendedProperties
	{
		[Ignore]
		public List<Column> Columns { get; private set; }

		public DatabaseObjectWithColumns(Database database) : base(database)
		{
			Columns = new List<Column>();
		}
	}
}
