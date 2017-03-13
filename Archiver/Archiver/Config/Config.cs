using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Archiver.Config
{
    public class Config : IConfig
    {
        public string Src { get; set; }

        public string Dest { get; set; }

        public TimeSpan CurrentTimespan { get; set; }

        public HashSet<FileType> FileTypes { get; set; }

        public void Validate()
        {
            if (string.IsNullOrEmpty(Src))
                throw new ArgumentException($"{nameof(Src)} - CurrentLocation is required");

            if (string.IsNullOrEmpty(Dest))
                throw new ArgumentException($"{nameof(Dest)} - Dest is required");

            if (!FileTypes.Any())
                throw new ArgumentException($"{nameof(FileTypes)} - Please add a file type");

            if (CurrentTimespan > TimeSpan.Zero)
                throw new ArgumentException($"{nameof(CurrentTimespan)} - Current timespan is invalid");

            if (FileTypes.Count !=
                FileTypes.Select(x => x.Extension)
                .Distinct(StringComparer.CurrentCultureIgnoreCase)
                .Count())
                throw new ArgumentException($"{nameof(FileTypes)} - contains duplicate values");
        }

        public static Config Load(string path)
        {
            var config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(path));
            config.Validate();
            return config;
        }
    }

    public class FileType
    {
        public string Extension { get; set; }

        public List<string> IgnoredFiles { get; set; }
    }
}