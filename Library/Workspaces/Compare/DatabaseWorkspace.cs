using PaJaMa.Database.Library.DatabaseObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PaJaMa.Database.Library.Workspaces.Compare
{
	public class DatabaseWorkspace : WorkspaceWithSourceBase
	{
		public DatabaseWorkspace(DatabaseObjectBase sourceObject, DatabaseObjects.Database targetDatabase, DatabaseObjectBase targetObject, bool ignoreCase, bool condensed) 
            : base(sourceObject, targetDatabase, targetObject, ignoreCase, false, condensed)
		{
		}
	}
}
