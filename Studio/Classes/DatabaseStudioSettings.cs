﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaJaMa.Database.Studio.Classes
{
	public class DatabaseStudioSettings
	{
		public string ConnectionStrings { get; set; }
		public string SearchConnectionStrings { get; set; }
		public string MonitorConnectionStrings { get; set; }
		public string LastCompareSourceConnString { get; set; }
		public string LastCompareTargetConnString { get; set; }
		public string LastSearchConnectionString { get; set; }
		public string LastCompareSourceDriver { get; set; }
		public string LastCompareTargetDriver { get; set; }
		public string LastQueryConnectionString { get; set; }
		public string LastQueryServerType { get; set; }
		public string LastMonitorConnectionString { get; set; }
		public bool LastQueryUseDummyDA { get; set; }

		public DatabaseStudioSettings()
		{
			ConnectionStrings = "";
		}
	}
}