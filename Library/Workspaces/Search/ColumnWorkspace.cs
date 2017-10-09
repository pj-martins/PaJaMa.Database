using PaJaMa.Common;
using PaJaMa.Database.Library.DatabaseObjects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaJaMa.Database.Library.Workspaces.Search
{
	public class ColumnWorkspace
	{
		public Column Column { get; private set; }
		public bool Select { get; set; }

		public ColumnWorkspace(Column column)
		{
			Column = column;
		}
	}
}
