﻿//using PaJaMa.Database.Library.Helpers;
//using PaJaMa.Database.Library.Workspaces.Compare;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data.SqlClient;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace PaJaMa.Database.Studio.Classes
//{
//	public class CommandLineHelper
//	{
//		public static bool ProcessArguments(string[] args)
//		{
//			try
//			{
//				bool somethingProcessed = true;
//				for (int i = 0; i < args.Length - 1; i++)
//				{
//					var arg = args[i];
//					if (arg == "-c")
//					{
//						var fileName = args[i + 1];
//						var compareWorkspace = PaJaMa.Common.XmlSerialize.DeserializeObjectFromFile<CompareWorkspace>(fileName);
//						var selectedWorkspaces = compareWorkspace.SelectedTableWorkspaces.Where(ws => ws.SelectTableForData)
//							.ToList();

//						if (selectedWorkspaces.Any())
//						{
//                            // TODO:
//							var compareHelper = new CompareHelper(typeof(SqlConnection), typeof(SqlConnection), compareWorkspace.FromConnectionString, compareWorkspace.ToConnectionString, null);
//							List<TableWorkspace> workspaces = new List<TableWorkspace>();
//							foreach (var ws in selectedWorkspaces)
//							{
//								var fromTbl = from s in compareHelper.FromDatabase.Schemas
//											  from t in s.Tables
//											  where t.ToString() == ws.SourceSchemaTableName
//											  select t;
//								var toTbl = from s in compareHelper.ToDatabase.Schemas
//											  from t in s.Tables
//											  where t.ToString() == ws.TargetSchemaTableName
//											  select t;

//								workspaces.Add(new TableWorkspace(compareHelper, fromTbl.First(), compareHelper.ToDatabase, toTbl.First())
//									{
//										SelectTableForData = ws.SelectTableForData,
//										KeepIdentity = ws.KeepIdentity,
//										RemoveAddIndexes = ws.RemoveAddIndexes,
//										RemoveAddKeys = ws.RemoveAddKeys,
//										Truncate = ws.Truncate
//									});

//							}
//							//using (var conn = new SqlConnection(compareWorkspace.ToConnectionString))
//							//{
//							//	conn.Open();
//							//	using (var trans = conn.BeginTransaction())
//							//	{
//							//		try
//							//		{
//										// new TransferHelper(new BackgroundWorker() { WorkerReportsProgress = true }).Transfer(workspaces, trans, compareWorkspace.FromConnectionString);
//										//compareHelper.Synchronize(new BackgroundWorker() { WorkerReportsProgress = true }, workspaces
//										//	.Select(ws => (WorkspaceBase)ws).ToList(), trans);
//							new SynchronizationHelper().Synchronize(compareHelper, new List<WorkspaceBase>(),
//								workspaces, workspaces.Where(ws => ws.Truncate).ToList(), new BackgroundWorker() { WorkerReportsProgress = true });
//							//			trans.Commit();
//							//		}
//							//		catch
//							//		{
//							//			trans.Rollback();
//							//			throw;
//							//		}
//							//	}
//							//	conn.Close();
//							//	SqlConnection.ClearAllPools();
//							//}
//						}
//						somethingProcessed = true;
//						i++;
//					}
//				}
//				return somethingProcessed;
//			}
//			catch
//			{
//				//  TODO: log?
//				return false;
//			}
//		}
//	}
//}
