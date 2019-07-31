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
		private TableWorkspace _tableWorkspace;

		private bool _select;
		public bool Select
		{
			get { return _select; }
			set
			{
				_select = value;
				if (value)
					_tableWorkspace.SelectAll = true;
			}
		}

		public ColumnWorkspace(TableWorkspace tableWorkspace, Column column)
		{
			_tableWorkspace = tableWorkspace;
			Column = column;
		}
	}
}
