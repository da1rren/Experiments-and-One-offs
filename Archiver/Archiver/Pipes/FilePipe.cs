using Archiver.Result;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Archiver.Pipes.Interfaces;

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
                foreach (var file in Recurse(_config.Src))
                {
                    Buffer.Add(file, _token);
                }

                Buffer.CompleteAdding();
            }, _token);
        }

        private IEnumerable<string> Recurse(string folder)
        {
            if (_token.IsCancellationRequested) return new List<string>();

            try
            {
                foreach (var directory in Directory.GetDirectories(folder))
                {
                    Recurse(directory);
                }

                return Directory.GetFiles(folder);
            }
            catch (Exception ex)
            {
                _resultFactory.AddError(new ErrorResult
                {
                    CurrentFile = folder,
                    ErrorMessage = ex.Message
                });
            }

            return new List<string>();
        }

    }
}
