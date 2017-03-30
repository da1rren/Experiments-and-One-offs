using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Database_Login_Logger.Models
{
    public class Settings : ISettings
    {
        public string LoggingConnectionString { get; set; }

        public List<string> ConnectionStrings { get; set; }

        public ActiveLoginsSettings ActiveLogins { get; set; }

        public DatabaseArchiveSettings DatabaseArchive { get; set; }

        private static readonly object Lock = new object();

        private static Settings _instance;

        private Settings()
        {
            
        }

        public static Settings GetInstance()
        {
            lock (Lock)
            {
                if (_instance != null)
                {
                    return _instance;
                }

                _instance = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("Config.json"));
                return _instance;
            }
        }

    }

    public class ActiveLoginsSettings
    {
        public bool Enabled { get; set; }

        public string PollInterval { get; set; }
    }

    public class DatabaseArchiveSettings
    {
        public bool Enabled { get; set; }

        public string PollInterval { get; set; }

        public string Dest { get; set; }
    }
}
