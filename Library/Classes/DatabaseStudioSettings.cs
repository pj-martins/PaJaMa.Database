using PaJaMa.Common;
using PaJaMa.Database.Library.Workspaces;
using System.Collections.Generic;

namespace PaJaMa.Database.Library
{
    public class DatabaseStudioSettings
    {
        public SerializableDictionary<string, List<QueryOutput>> QueryOutputs { get; set; }
        public List<DatabaseConnection> Connections { get; set; }
        public DatabaseConnection LastCompareSourceConnection { get; set; }
        public DatabaseConnection LastCompareTargetConnection { get; set; }
        public DatabaseConnection LastSearchConnection { get; set; }
        public DatabaseConnection LastQueryConnection { get; set; }
        public DatabaseConnection LastMonitorConnection { get; set; }
        public bool LastQueryUseDummyDA { get; set; }

        // [Obsolete]
        public string ConnectionStrings { get; set; }

        public DatabaseStudioSettings()
        {
            QueryOutputs = new SerializableDictionary<string, List<QueryOutput>>();
            Connections = new List<DatabaseConnection>();
        }
    }
}
