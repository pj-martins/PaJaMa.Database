using PaJaMa.Database.Library.Workspaces;
using PaJaMa.Database.Library.Workspaces.Compare;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaJaMa.Database.Library.Synchronization
{
	public class DataDifference
	{
		public TableWorkspace TableWorkspace { get; set; }
		public int SourceOnly { get; set; }
		public int TargetOnly { get; set; }
		public int Differences { get; set; }
	}
}
