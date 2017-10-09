using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaJaMa.Database.Library.Workspaces
{
	public class QueryWorkspace
	{
		public string ConnectionString { get; set; }
		public string ConnectionType { get; set; }
		public string Database { get; set; }
		public List<QueryOutput> Queries { get; set; }

		public QueryWorkspace()
		{
			Queries = new List<QueryOutput>();
		}
	}

	public class QueryOutput
	{
		public string Database { get; set; }
		public string Query { get; set; }
	}
}
