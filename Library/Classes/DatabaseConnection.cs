using Newtonsoft.Json;
using PaJaMa.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace PaJaMa.Database.Library
{
    public class DatabaseConnection
    {
        const string PASSWORD = "D3tabase$tudio";

        public Guid ID { get; set; }
        public string DataSourceType { get; set; }
        public string ConnectionName { get; set; }
        public string Server { get; set; }
        public int Port { get; set; }
        public string Database { get; set; }
        public string UserName { get; set; }
        public string PasswordEncrypted { get; set; }
        public bool IntegratedSecurity { get; set; }
        public string Append { get; set; }
        public string Tunnel { get; set; }
        public string TunnelUser { get; set; }
        public int TunnelPort { get; set; }
        public int TunnelForward { get; set; }
        public string TunnelKeyFile { get; set; }
        public List<QueryOutput> QueryOutputs { get; set; }

        [XmlIgnore, JsonIgnore]
        public string Password
        {
            get { return string.IsNullOrEmpty(PasswordEncrypted) ? string.Empty : EncrypterDecrypter.Decrypt(PasswordEncrypted, PASSWORD); }
            set { PasswordEncrypted = EncrypterDecrypter.Encrypt(value, PASSWORD); }
        }

        public DatabaseConnection()
        {
            this.ID = Guid.NewGuid();
            this.QueryOutputs = new List<QueryOutput>();
        }

        public void Save()
        {
            SettingsHelper.SaveUserSettings(this, $"connection_{this.ID}");
        }

        public void SaveQueryOutputs()
        {
            var conn = SettingsHelper.GetUserSettings<DatabaseConnection>($"connection_{this.ID}");
            conn.QueryOutputs = this.QueryOutputs;
            conn.Save();
        }

        public static List<DatabaseConnection> GetConnections()
        {
            var connectionFiles = new DirectoryInfo(DatabaseStudioSettings.ConfigRoot).GetFiles("connection_*.json");
            var dbConnections = new List<DatabaseConnection>();
            foreach (var file in connectionFiles)
            {
                dbConnections.Add(JsonConvert.DeserializeObject<DatabaseConnection>(File.ReadAllText(file.FullName)));
            }
            return dbConnections;
        }

        public static void SetConnections(List<DatabaseConnection> connections)
        {
            var connectionFiles = new DirectoryInfo(DatabaseStudioSettings.ConfigRoot).GetFiles("connection_*.json").ToList();
            var existingFiles = new List<string>();
            foreach (var conn in connections)
            {
                var fileName = $"connection_{conn.ID}.json";
                File.WriteAllText(Path.Combine(DatabaseStudioSettings.ConfigRoot, fileName), JsonConvert.SerializeObject(conn));
                existingFiles.Add(fileName);
            }

            foreach (var file in connectionFiles)
            {
                if (!existingFiles.Contains(file.Name))
                {
                    file.Delete();
                }
            }
        }

        public override string ToString()
        {
            return ConnectionName;
        }
    }

    public class QueryOutput
    {
        public string Database { get; set; }
        public string Query { get; set; }
    }
}
