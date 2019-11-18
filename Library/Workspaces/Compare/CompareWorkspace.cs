using PaJaMa.Database.Library.DataSources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaJaMa.Database.Library.Workspaces.Compare
{
	public class CompareWorkspace
	{
		public string FromConnectionString { get; set; }
		public string ToConnectionString { get; set; }
		public string FromDatabase { get; set; }
		public string ToDatabase { get; set; }
        public string FromDriver { get; set; }
        public string ToDriver { get; set; }

		public List<SerializableTableWorkspace> SelectedTableWorkspaces { get; set; }
		public List<SerializableObjectWorkspace> SelectedObjectWorkspaces { get; set; }
		public List<SerializableDropWorkspace> SelectedDropWorkspaces { get; set; }
	}

	public class CompareSnapshot
	{
		public string FromConnectionString { get; set; }
		public string ToConnectionString { get; set; }
		//public string FromDatabase { get; set; }
		//public string ToDatabase { get; set; }
		public string FromDriver { get; set; }
		public string ToDriver { get; set; }
		public DatabaseObjects.Database FromDatabase { get; set; }
		public DatabaseObjects.Database ToDatabase { get; set; }
		//public List<RawDatabaseObject> FromObjects { get; set; }
		//public List<RawDatabaseObject> ToObjects { get; set; }
	}
}

