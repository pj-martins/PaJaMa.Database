using PaJaMa.Database.Library.DatabaseObjects;
using PaJaMa.Database.Library.Synchronization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaJaMa.Database.Library.Workspaces.Compare
{
	public class DropWorkspace : WorkspaceBase
	{
		public string Name
		{
			get { return TargetObject.ToString(); }
		}

		public string Type
		{
			get { return TargetObject.ObjectType; }
		}

		public DropWorkspace(DatabaseObjectBase dbObject)
			: base(dbObject.Database, dbObject)
		{
			var sync = DatabaseObjectSynchronizationBase.GetSynchronization(dbObject.Database, dbObject);
			SynchronizationItems.AddRange(sync.GetDropItems());
		}

		public override string ToString()
		{
			return Name;
		}
	}

	public class SerializableDropWorkspace
	{
		public string Name { get; set; }
		public string Type { get; set; }

		public static SerializableDropWorkspace GetFromDropWorkspace(DropWorkspace ws)
		{
			return new SerializableDropWorkspace()
			{
				Name = ws.Name,
				Type = ws.Type
			};
		}
	}
}
