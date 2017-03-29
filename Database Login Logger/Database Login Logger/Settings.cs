using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Database_Login_Logger
{
    public class Settings
    {
        public string LoggingConnectionString { get; set; }

        public int PollFrequency { get; set; }

        public List<string> ConnectionStrings { get; set; }

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
}
