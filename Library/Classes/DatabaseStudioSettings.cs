using MySql.Data.MySqlClient;
using PaJaMa.Common;
using PaJaMa.Database.Library.DataSources;
using PaJaMa.Database.Library.Workspaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Xml.Serialization;

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

    public class DatabaseStudioConnection
    {
        const string PASSWORD = "D3tabase$tudio";

        public string DataSourceType { get; set; }
        public string ConnectionName { get; set; }
        public string Server { get; set; }
        public int Port { get; set; }
        public string Database { get; set; }
        public string UserName { get; set; }
        public string PasswordEncrypted { get; set; }
        public bool IntegratedSecurity { get; set; }
        public string Append { get; set; }

        [XmlIgnore]
        public string Password
        {
            get { return string.IsNullOrEmpty(PasswordEncrypted) ? string.Empty : EncrypterDecrypter.Decrypt(PasswordEncrypted, PASSWORD); }
            set { PasswordEncrypted = EncrypterDecrypter.Encrypt(value, PASSWORD); }
        }

        //public static void ConvertFromLegacy(DatabaseStudioSettings settings)
        //{
        //    if (settings.ConnectionStrings == null) return;
        //    var connStrings = settings.ConnectionStrings.Split('|');
        //    List<DatabaseStudioConnection> connections = new List<DatabaseStudioConnection>();
        //    foreach (var connString in connStrings)
        //    {
        //        if (string.IsNullOrEmpty(connString)) continue;
        //        var appends = new List<string>();
        //        var conn = new DatabaseStudioConnection();
        //        try
        //        {
        //            var connStringBuilder = new SqlConnectionStringBuilder(connString);
        //            conn.Server = connStringBuilder.DataSource;
        //            conn.ConnectionName = conn.Server + " - " + connStringBuilder.InitialCatalog;
        //            conn.DataSourceType = typeof(SqlServerDataSource).FullName;
        //            conn.Database = connStringBuilder.InitialCatalog;
        //            conn.UserName = connStringBuilder.UserID;
        //            conn.Password = connStringBuilder.Password;
        //            conn.IntegratedSecurity = connStringBuilder.IntegratedSecurity;
        //        }
        //        catch
        //        {
        //            try
        //            {
        //                var connStringBuilder = new MySqlConnectionStringBuilder(connString);
        //                if (connStringBuilder.OldGuids)
        //                    appends.Add("Old Guids=true");
        //                if (connStringBuilder.AllowUserVariables)
        //                    appends.Add("Allow User Variables=True");
        //                if (connStringBuilder.AllowZeroDateTime)
        //                    appends.Add("AllowZeroDateTime=True");
        //                conn.Server = connStringBuilder.Server;
        //                conn.Port = (int)connStringBuilder.Port;
        //                conn.ConnectionName = conn.Server + ":" + conn.Port.ToString() + " - " + connStringBuilder.Database;
        //                conn.DataSourceType = typeof(MySqlDataSource).FullName;
        //                conn.Database = connStringBuilder.Database;
        //                conn.UserName = connStringBuilder.UserID;
        //                conn.Password = connStringBuilder.Password;
        //                conn.IntegratedSecurity = connStringBuilder.IntegratedSecurity;
        //            }
        //            catch
        //            {
        //                try
        //                {
        //                    var connStringBuilder = new SQLiteConnectionStringBuilder(connString);
        //                    if (string.IsNullOrWhiteSpace(connStringBuilder.DataSource)) throw new Exception("Not SQLITE");
        //                    conn.Server = connStringBuilder.DataSource;
        //                    conn.ConnectionName = connStringBuilder.DataSource;
        //                    conn.DataSourceType = typeof(SQLiteDataSource).FullName;
        //                    conn.Database = connStringBuilder.DataSource;
        //                    // conn.UserName = connStringBuilder.;
        //                    conn.Password = connStringBuilder.Password;
        //                }
        //                catch
        //                {
        //                    // throw new NotImplementedException(connString);
        //                    continue;
        //                }
        //            }
        //        }

        //        conn.Append = string.Join(";", appends.ToArray());
        //        connections.Add(conn);
        //    }
        //    settings.Connections = connections;
        //    PaJaMa.Common.SettingsHelper.SaveUserSettings<DatabaseStudioSettings>(settings);
        //}

        public override string ToString()
        {
            return ConnectionName;
        }

        public string GetConnectionString()
        {
            string connectionString = string.Empty;
            if (DataSourceType == typeof(SqlServerDataSource).FullName)
            {
                var connectionStringBuilder = new SqlConnectionStringBuilder();
                connectionStringBuilder.DataSource = Server;
                connectionStringBuilder.InitialCatalog = Database;
                connectionStringBuilder.UserID = UserName;
                connectionStringBuilder.Password = Password;
                connectionStringBuilder.IntegratedSecurity = IntegratedSecurity;
                connectionString = connectionStringBuilder.ConnectionString;
            }
            else if (DataSourceType == typeof(MySqlDataSource).FullName || DataSourceType == typeof(MySqlDataSourceForCompare).FullName)
            {
                var connectionStringBuilder = new MySqlConnectionStringBuilder();
                connectionStringBuilder.Server = Server;
                connectionStringBuilder.Port = (uint)Port;
                connectionStringBuilder.Database = Database;
                connectionStringBuilder.UserID = UserName;
                connectionStringBuilder.Password = Password;
                connectionStringBuilder.IntegratedSecurity = IntegratedSecurity;
                connectionString = connectionStringBuilder.ConnectionString;
            }
            else if (DataSourceType == typeof(SQLiteDataSource).FullName)
			{
                var connectionStringBuilder = new SQLiteConnectionStringBuilder();
                connectionStringBuilder.DataSource = Server;
                connectionStringBuilder.Password = Password;
                connectionString = connectionStringBuilder.ConnectionString;
            }
            if (string.IsNullOrEmpty(connectionString))
                throw new NotImplementedException();
            return connectionString + (string.IsNullOrEmpty(Append) ? "" : ";" + Append);
        }
    }
}
