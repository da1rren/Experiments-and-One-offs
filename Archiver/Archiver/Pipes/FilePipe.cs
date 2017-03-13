using Archiver.Result;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Archiver.Config;
using Archiver.Pipes.Interfaces;
using Archiver.Utility;

namespace Archiver.Pipes
{
    public class FilePipe : IPipeline<string>
    {
        public BlockingCollection<string> Buffer { get; set; } = new BlockingCollection<string>(65536);

        private readonly IConfig _config;
        private readonly IResultFactory _resultFactory;
        private CancellationToken _token;

        public FilePipe(IConfig config,
            IResultFactory results,
            CancellationToken token)
        {
            _config = config;
            _resultFactory = results;
            _token = token;
        }

        public Task Execute()
        {
            return Task.Run(() =>
            {
                var traverser = new FileTraverser(_config.Src, _token);
                foreach (var file in traverser)
                {
                    Buffer.Add(file, _token);
                }

                _resultFactory.AddErrors(traverser.ErrorResults);
                Buffer.CompleteAdding();
            }, _token);
        }
    }
}
