using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PaJaMa.Database.Studio.Classes
{
	public class QueryEventArgs : EventArgs
	{
		public Library.DatabaseObjects.Database Database { get; set; }
		public string InitialTable { get; set; }
		public string InitialSchema { get; set; }
		public int? InitialTopN { get; set; }
	}

	public delegate void QueryEventHandler(object sender, QueryEventArgs e);
}
