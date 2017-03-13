using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Archiver.Config;

namespace Archiver.Result
{
    public class ResultFactory : IResultFactory
    {
        private readonly IConfig _config;
        private readonly object _lock = new object();
        private static Results _results;

        public long TotalFiles { get; private set; }

        public long TotalErrors { get; private set; }

        public ResultFactory(IConfig config)
        {
            _results = new Results();
            _config = config;
        }

        public void AddResult(FileResult result)
        {
            lock (_lock)
            {
                _results.FileResults.Add(result);
            }

            TotalFiles++;
        }

        public void AddError(ErrorResult result)
        {
            lock (_lock)
            {
                _results.ErrorResults.Add(result);
            }

            TotalErrors++;
        }
        public void AddErrors(IEnumerable<ErrorResult> results)
        {
            lock (_lock)
            {
                _results.ErrorResults.AddRange(results);
            }
        }


        public void WriteFile()
        {
            _results.CompleteTime = DateTime.Now;
            _results.ExitedGracefully = true;

            var filename = Path.Combine(_config.Dest, ("Archive-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff") + ".json"));
            File.WriteAllText(filename, JsonConvert.SerializeObject(_results, Formatting.Indented));
        }

        public long TotalSize
        {
            get
            {
                lock (_lock)
                {
                    return _results.FileResults.Sum(x => x.Size);
                }
            }
        }
    }
}
