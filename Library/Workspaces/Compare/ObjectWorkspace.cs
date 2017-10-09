using PaJaMa.Database.Library.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PaJaMa.Database.Library.DatabaseObjects;

namespace PaJaMa.Database.Library.Workspaces.Compare
{
	// excludes tables
	public class ObjectWorkspace : WorkspaceWithSourceBase
	{
		public string Type
		{
			get { return SourceObject.ObjectType; }
		}

		public string Source { get { return SourceObject.ToString(); } }
		public string Target { get { return TargetObject == null ? string.Empty : TargetObject.ToString(); } }

		public ObjectWorkspace(CompareHelper compareHelper, DatabaseObjectBase sourceObject, DatabaseObjects.Database targetDatabase, DatabaseObjectBase targetObject) 
            : base(sourceObject, targetDatabase, targetObject)
		{
		}

		public override string ToString()
		{
			return Source;
		}
	}

	public class SerializableObjectWorkspace
	{
		public string SourceObjectName { get; set; }
		public string TargetObjectName { get; set; }
		public string ObjectType { get; set; }

		public static SerializableObjectWorkspace GetFromObjectWorkspace(ObjectWorkspace ws)
		{
			return new SerializableObjectWorkspace()
			{
				SourceObjectName = ws.Source,
				TargetObjectName = ws.Target,
				ObjectType = ws.Type
			};
		}
	}

	public class ObjectWorkspaceList
	{
		public List<ObjectWorkspace> Workspaces { get; private set; }
		public List<DropWorkspace> DropWorkspaces { get; private set; }

		public ObjectWorkspaceList()
		{
			Workspaces = new List<ObjectWorkspace>();
			DropWorkspaces = new List<DropWorkspace>();
		}

		private static bool objectsAreEqual(DatabaseObjectBase obj1, DatabaseObjectBase obj2)
		{
			if (obj1.ObjectType != obj2.ObjectType) return false;
			if (obj1.ObjectType == typeof(Schema).Name)
			{
				if (obj1.ObjectName == DriverHelper.GetConvertedSchemaName(obj1.ParentDatabase, obj2.ObjectName))
					return true;
			}
			return obj1.ToString() == obj2.ToString();
		}

		public static ObjectWorkspaceList GetObjectWorkspaces(CompareHelper compareHelper)
		{
			var lst = new ObjectWorkspaceList();

			var fromObjs = compareHelper.FromDatabase.GetDatabaseObjects(true);
			var toObjs = compareHelper.ToDatabase.GetDatabaseObjects(true);

			foreach (var def in fromObjs)
			{
				if (def.ObjectType == typeof(Schema).Name && (
					compareHelper.FromDatabase.IsSQLite || compareHelper.ToDatabase.IsSQLite))
					continue;

				DatabaseObjectBase sourceDef = def;
				DatabaseObjectBase targetDef = toObjs.FirstOrDefault(t => objectsAreEqual(t, def));
				lst.Workspaces.Add(new ObjectWorkspace(compareHelper, sourceDef, compareHelper.ToDatabase, targetDef));
			}


			foreach (var def in toObjs
				.Where(x => !fromObjs.Any(d => d.ToString() == x.ToString() && d.ObjectType == x.ObjectType)))
			{
				if (compareHelper.FromDatabase.IsSQLite)
				{
					if (def.ObjectType == typeof(Schema).Name) continue;
					if (def.ObjectType == "WindowsUser") continue;
					if (def.ObjectType == "SQLUser") continue;
					if (def.ObjectType == "DatabaseRole") continue;
					if (def.ObjectType == "SQLLogin") continue;
				}
				lst.DropWorkspaces.Add(new DropWorkspace(def));
			}

			return lst;
		}
	}
}
