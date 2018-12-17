using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaJaMa.Database.Studio.Compare
{
	public class CompareFormSettings
	{
		public int TablesSplitterDistance { get; set; }
		public int TableDifferencesSplitterDistance { get; set; }
		public int ObjectsSplitterDistance { get; set; }
		public int ObjectDifferencesSplitterDistance { get; set; }
		public int DropsSplitterDistance { get; set; }
		public int DropDifferencesSplitterDistance { get; set; }
	}
}
