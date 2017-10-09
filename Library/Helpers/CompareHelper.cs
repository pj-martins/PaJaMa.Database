﻿using PaJaMa.Common;
using PaJaMa.Database.Library.Workspaces.Compare;
using PaJaMa.Database.Library.DatabaseObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PaJaMa.Database.Library.Synchronization;

namespace PaJaMa.Database.Library.Helpers
{
	public class CompareHelper
	{
		public event DialogEventHandler Prompt;

		public bool IsFrom2000OrLess { get; private set; }
		public bool IsTo2000OrLess { get; private set; }
		public List<TableWorkspace> TablesToSync { get; private set; }

		public DatabaseObjects.Database FromDatabase { get; private set; }
		public DatabaseObjects.Database ToDatabase { get; private set; }

		public CompareHelper(Type fromDriverType, Type toDriverType, string fromConnectionString, string toConnectionString, BackgroundWorker worker)
		{
			FromDatabase = new DatabaseObjects.Database(fromDriverType, fromConnectionString);
			ToDatabase = new DatabaseObjects.Database(toDriverType, toConnectionString);
			FromDatabase.PopulateChildren(false, worker);
			ToDatabase.PopulateChildren(false, worker);
		}

		public void Init(BackgroundWorker worker)
		{
			FromDatabase = new DatabaseObjects.Database(FromDatabase.ConnectionType, FromDatabase.ConnectionString);
			ToDatabase = new DatabaseObjects.Database(ToDatabase.ConnectionType, ToDatabase.ConnectionString);
			FromDatabase.PopulateChildren(false, worker);
			ToDatabase.PopulateChildren(false, worker);
		}

		public bool Synchronize(BackgroundWorker worker, List<WorkspaceBase> workspaces, DbTransaction trans)
		{
			Dictionary<WorkspaceBase, List<string>> scriptStrings = new Dictionary<WorkspaceBase, List<string>>();

			int totalProgress = 0;
			bool ignorePrompt = false;

			// create tables first
			List<SynchronizationItem> foreignKeys = new List<SynchronizationItem>();

			foreach (var ws in workspaces)
			{
				if (!ws.SynchronizationItems.Any()) continue;

				scriptStrings.Add(ws, new List<string>());
				var allScripts = new Dictionary<int, StringBuilder>();
				foreach (var item in ws.SynchronizationItems)
				{
					if (!item.Omit && item.DatabaseObject is ForeignKey)
					{
						foreignKeys.Add(item);
						totalProgress++;
						item.Omit = true;
					}

					if (item.Omit)
						continue;

					// TODO: need others?
					if (ws is DropWorkspace)
					{
						if (item.DatabaseObject.Synchronized)
							continue;

						item.DatabaseObject.Synchronized = true;
					}

					foreach (var kvp in item.Scripts)
					{
						if (kvp.Value.Length <= 0) continue;
						if (!allScripts.ContainsKey(kvp.Key)) allScripts.Add(kvp.Key, new StringBuilder());
						allScripts[kvp.Key].AppendLine(kvp.Value.ToString());
					}
				}
				foreach (var kvp in allScripts.OrderBy(s => s.Key))
				{
					if (kvp.Value.Length <= 0) continue;
					totalProgress++;
					scriptStrings[ws].Add(kvp.Value.ToString());
				}
			}

			using (var cmd = trans.Connection.CreateCommand())
			{
				cmd.Transaction = trans;

				int i = 0;
				foreach (var kvp in scriptStrings)
				{
					foreach (var script in kvp.Value)
					{
						i++;
						worker.ReportProgress(100 * i / totalProgress, string.Format("Synchronizing {0} {1} of {2}", kvp.Key.ToString(), i.ToString(), totalProgress.ToString()));
						cmd.CommandText = script;
						cmd.CommandTimeout = 600;
						try
						{
							cmd.ExecuteNonQuery();
						}
						catch (Exception ex)
						{
							if (!ignorePrompt)
							{
								var args = new DialogEventArgs("Failed to synchronize \"" + (kvp.Key is WorkspaceWithSourceBase && kvp.Key.TargetObject == null ? (kvp.Key as WorkspaceWithSourceBase).SourceObject.ObjectName : kvp.Key.TargetObject.ObjectName)
									+ "\": " + ex.Message + ". Would you like to continue?");
								Prompt(this, args);
								switch (args.Result)
								{
									case Common.YesYesToAllNo.No:
										return false;
									case Common.YesYesToAllNo.Yes:
										break;
									case Common.YesYesToAllNo.YesToAll:
										ignorePrompt = true;
										break;
								}
							}
						}
					}
				}

				foreach (var key in foreignKeys)
				{
					i++;
					worker.ReportProgress(100 * i / totalProgress, string.Format("Synchronizing {0} {1} of {2}", key.ToString(), i.ToString(), totalProgress.ToString()));
					foreach (var script in key.Scripts.OrderBy(s => s.Key))
					{
						cmd.CommandText = script.Value.ToString();
						cmd.CommandTimeout = 600;
						try
						{
							cmd.ExecuteNonQuery();
						}
						catch (Exception ex)
						{
							if (!ignorePrompt)
							{
								var args = new DialogEventArgs("Failed to synchronize \"" + key.ToString()
									+ "\": " + ex.Message + ". Would you like to continue?");
								Prompt(this, args);
								switch (args.Result)
								{
									case Common.YesYesToAllNo.No:
										return false;
									case Common.YesYesToAllNo.YesToAll:
										ignorePrompt = true;
										break;
									case Common.YesYesToAllNo.Yes:
										break;
								}
							}
						}
					}
				}
			}

			return true;
		}

		public Dictionary<DatabaseObjectBase, List<DatabaseObjectBase>> GetMissingDependencies(List<WorkspaceBase> selectedWorkspaces)
		{
			var missing = new Dictionary<DatabaseObjectBase, List<DatabaseObjectBase>>();
			populateMissingDependencies(selectedWorkspaces, missing);
			return missing;
		}

		private void populateMissingDependencies(List<WorkspaceBase> selectedWorkspaces, Dictionary<DatabaseObjectBase, List<DatabaseObjectBase>> missing)
		{
			foreach (var ws in selectedWorkspaces)
			{
				recursivelyPopulateMissingDependencies(ws is WorkspaceWithSourceBase ? (ws as WorkspaceWithSourceBase).SourceObject : ws.TargetObject,
					selectedWorkspaces, missing, new List<DatabaseObjectBase>(), ws is DropWorkspace);
			}
		}

		private void recursivelyPopulateMissingDependencies(DatabaseObjectBase parent, List<WorkspaceBase> selectedWorkspaces, Dictionary<DatabaseObjectBase, List<DatabaseObjectBase>> missing,
			List<DatabaseObjectBase> checkedObjects, bool isForDrop)
		{
			if (checkedObjects.Contains(parent)) return;

			checkedObjects.Add(parent);

			var toObjects = ToDatabase.GetDatabaseObjects(false).ConvertAll(s => (DatabaseObjectBase)s);
			toObjects.AddRange(from s in ToDatabase.Schemas
							   from t in s.Tables
							   select t);

			var selectedItems = from ws in selectedWorkspaces
								from si in ws.SynchronizationItems
								where !si.Omit
								select si;

			var sync = DatabaseObjectSynchronizationBase.GetSynchronization(parent.ParentDatabase, parent);

			var missingDendencies = sync.GetMissingDependencies(toObjects, selectedItems.ToList(), isForDrop);
			foreach (var child in missingDendencies)
			{
				if (checkedObjects.Contains(child))
					continue;

				checkedObjects.Add(child);

				if (!missing.ContainsKey(parent))
					missing.Add(parent, new List<DatabaseObjectBase>());

				if (!missing[parent].Contains(child))
					missing[parent].Add(child);

				recursivelyPopulateMissingDependencies(child, selectedWorkspaces, missing, checkedObjects, isForDrop);
			}
		}
	}
}