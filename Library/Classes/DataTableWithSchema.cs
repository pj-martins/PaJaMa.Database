using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaJaMa.Database.Library.Classes
{
	public class DataTableWithSchema : DataTable
	{
		public List<string> PrimaryKeyFields { get; set; }
	}
}
