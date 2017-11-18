using PaJaMa.Database.Library.DatabaseObjects;
using PaJaMa.Database.Library.Synchronization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaJaMa.Database.Library.Workspaces.Compare
{
	public abstract class WorkspaceBase
	{
		public DatabaseObjects.Database TargetDatabase { get; set; }
		public DatabaseObjectBase TargetObject { get; set; }
		public List<SynchronizationItem> SynchronizationItems { get; private set; }

		private bool _select;
		public bool Select
		{
			get { return _select; }
			set
			{
				if (!SynchronizationItems.Any())
				{
					_select = false;
					return;
				}

				_select = value;
			}
		}

		public WorkspaceBase(DatabaseObjects.Database targetDatabase, DatabaseObjectBase targetObject)
		{
			TargetDatabase = targetDatabase;
			TargetObject = targetObject;
			SynchronizationItems = new List<SynchronizationItem>();
		}
	}

	public abstract class WorkspaceWithSourceBase : WorkspaceBase
	{
		public virtual DatabaseObjectBase SourceObject { get; set; }
		public WorkspaceWithSourceBase(DatabaseObjectBase sourceObject, DatabaseObjects.Database targetDatabase, DatabaseObjectBase targetObject,
			bool ignoreCase) : base(targetDatabase, targetObject)
		{
			SourceObject = sourceObject;
			populateDifferences(ignoreCase);
		}

		private void populateDifferences(bool ignoreCase)
		{
			var syncItem = DatabaseObjectSynchronizationBase.GetSynchronization(TargetDatabase, SourceObject);
			SynchronizationItems.AddRange(syncItem.GetSynchronizationItems(TargetObject, ignoreCase));

			if (SourceObject is DatabaseObjectWithExtendedProperties)
				SynchronizationItems.AddRange(ExtendedPropertySynchronization.GetExtendedProperties(TargetDatabase, SourceObject as DatabaseObjectWithExtendedProperties,
					TargetObject as DatabaseObjectWithExtendedProperties));
		}


	}
}
