using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PaJaMa.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PaJaMa.Database.Library
{
	public class DatabaseStudioSettings
	{
		public string LastCompareSourceConnection { get; set; }
		public string LastCompareTargetConnection { get; set; }
		public string LastSearchConnection { get; set; }
		public string LastQueryConnection { get; set; }
		public string LastMonitorConnection { get; set; }
		public bool LastQueryUseDummyDA { get; set; }

		internal static readonly string ConfigRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Assembly.GetEntryAssembly().AssemblyTitle());

		public static DatabaseStudioSettings LoadSettings()
		{
			var settings = new DatabaseStudioSettings();
			settings = SettingsHelper.GetUserSettings<DatabaseStudioSettings>();
			if (settings.LastCompareSourceConnection == null && settings.LastQueryConnection == null)
			{
				var settingsFile = Path.Combine(ConfigRoot, "DatabaseStudioSettings.json");
				if (File.Exists(settingsFile))
				{
					var jsonObj = (JObject)JsonConvert.DeserializeObject(File.ReadAllText(settingsFile));
					var connections = DatabaseConnection.GetConnections();
					if (!connections.Any() && jsonObj.ContainsKey("Connections"))
					{
						var qryOutputs = (JObject)jsonObj["QueryOutputs"];
						var dbconns = new List<DatabaseConnection>();
						var connArray = (JArray)jsonObj["Connections"];
						foreach (var conn in connArray)
						{
							var dbconn = conn.ToObject<DatabaseConnection>();
							if (qryOutputs.ContainsKey(dbconn.ConnectionName))
							{
								dbconn.QueryOutputs = qryOutputs[dbconn.ConnectionName].ToObject<List<QueryOutput>>();
							}
							else
							{
								dbconn.QueryOutputs = new List<QueryOutput>();
							}
							dbconns.Add(dbconn);
						}
						DatabaseConnection.SetConnections(dbconns);
					}
				}
			}
			return settings;
		}
	}
}
