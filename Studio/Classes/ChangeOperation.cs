using PaJaMa.Database.Library.DatabaseObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaJaMa.Database.Studio.Classes
{
	public class ChangeOperation
	{
		public Dictionary<string, object> KeyValues { get; set; }
		public Dictionary<string, object> ColumnValues { get; set; }
		public ChangeType ChangeType { get; set; }

		public ChangeOperation()
		{
			KeyValues = new Dictionary<string, object>();
			ColumnValues = new Dictionary<string, object>();
		}
	}

	public enum ChangeType
	{
		NotSet,
		Add,
		Edit,
		Delete
	}
}
