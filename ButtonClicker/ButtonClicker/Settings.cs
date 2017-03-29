using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace ButtonClicker
{
    public class Settings
    {
        public string Title { get; set; }

        public string DefaultWindow { get; set; }

        public bool JiggleMouse { get; set; }

        public int PollDelay { get; set; }

        public List<KeySequence> KeysSequences { get; set; }

        private static readonly object Lock = new object();

        private static Settings _instance;

        private Settings()
        {
            
        }

        public static Settings Get()
        {
            lock (Lock)
            {
                if (_instance != null)
                    return _instance;

                _instance = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("config.json"));
                return _instance;
            }
        }

    }

    public class KeySequence
    {
        public string DisplayName { get; set; }

        public List<string> Sequence { get; set; }
    }
}
