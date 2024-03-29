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
using PaJaMa.Database.Library.DataSources;

namespace PaJaMa.Database.Library.Helpers
{
	public class CompareHelper : IDisposable
	{
		public event PromptEventHandler Prompt;

		public bool IsFrom2000OrLess { get; private set; }
		public bool IsTo2000OrLess { get; private set; }
		public List<TableWorkspace> TablesToSync { get; private set; }

		public DataSource FromDataSource { get; private set; }
		public DataSource ToDataSource { get; private set; }
		
		public bool IgnoreCase { get; set; }

		public CompareHelper(DataSource fromDataSource, DataSource toDataSource, bool isForData, BackgroundWorker worker)
		{
			FromDataSource = fromDataSource;
			ToDataSource = toDataSource;
			if (isForData)
			{
				FromDataSource.PopulateTablesAndColumns(null, worker);
				ToDataSource.PopulateTablesAndColumns(null, worker);
			}
			else
			{
				FromDataSource.PopulateChildren(null, false, worker);
				ToDataSource.PopulateChildren(null, false, worker);
			}
		}

		public void Init(BackgroundWorker worker)
		{
			FromDataSource.PopulateChildren(null, false, worker);
			ToDataSource.PopulateChildren(null, false, worker);
		}

		public bool Synchronize(BackgroundWorker worker, List<WorkspaceBase> workspaces, DbTransaction trans)
		{
			Dictionary<WorkspaceBase, List<string>> scriptStrings = new Dictionary<WorkspaceBase, List<string>>();

			int totalProgress = 0;
			bool ignorePrompt = false;

			// for drops remove fks first
			var dropTables = from ws in workspaces.OfType<DropWorkspace>()
							 where ws.TargetObject is Table && (ws.TargetObject as Table).ForeignKeys.Any()
							 select ws.TargetObject as Table;
			if (dropTables.Any())
			{
				using (var cmd = trans.Connection.CreateCommand())
				{
					cmd.Transaction = trans;
					foreach (var t in dropTables)
					{
						t.RemoveForeignKeys(cmd);
					}
				}
			}

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
								var args = new PromptEventArgs("Failed to synchronize. Continue? \"" + (kvp.Key is WorkspaceWithSourceBase && kvp.Key.TargetObject == null ? (kvp.Key as WorkspaceWithSourceBase).SourceObject.ObjectName : kvp.Key.TargetObject.ObjectName)
									+ "\": " + ex.Message + "\r\n\r\n" + script + ".");
								Prompt(this, args);
								switch (args.Result)
								{
									case PromptResult.No:
										return false;
									case PromptResult.Yes:
										break;
									case PromptResult.YesToAll:
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
								var args = new PromptEventArgs("Failed to synchronize \"" + key.ToString()
									+ "\": " + ex.Message + ".");
								Prompt(this, args);
								switch (args.Result)
								{
									case PromptResult.No:
										return false;
									case PromptResult.YesToAll:
										ignorePrompt = true;
										break;
									case PromptResult.Yes:
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

			var toObjects = ToDataSource.CurrentDatabase.GetDatabaseObjects(false).ConvertAll(s => (DatabaseObjectBase)s);
			toObjects.AddRange(from s in ToDataSource.CurrentDatabase.Schemas
							   from t in s.Tables
							   select t);

			foreach (var ws in selectedWorkspaces)
			{
				var sync = DatabaseObjectSynchronizationBase.GetSynchronization(parent.Database, parent);

				var missingDendencies = sync.GetMissingDependencies(toObjects, ws.SynchronizationItems.Where(si => !si.Omit).ToList(), isForDrop, IgnoreCase);
				foreach (var child in missingDendencies)
				{
					if (checkedObjects.Contains(child))
						continue;

					if (child is ForeignKey fk && ws is TableWorkspace tw && 
						(tw.RemoveAddKeys || selectedWorkspaces.Any(sw => sw is TableWorkspace tw2 && tw2.Select && tw2.SourceTable == fk.ParentTable)))
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

        public void Dispose()
        {
            if (this.FromDataSource != null)
            {
				this.FromDataSource.Dispose();
            }

			if (this.ToDataSource != null)
            {
				this.ToDataSource.Dispose();
            }
        }
    }
}