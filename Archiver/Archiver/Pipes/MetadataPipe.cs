using Archiver.Result;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Archiver.Config;
using Archiver.Pipes.Interfaces;

namespace Archiver.Pipes
{
    public class MetadataPipe : IPipeline<FileResult>
    {
        public BlockingCollection<FileResult> Buffer { get; set; } = new BlockingCollection<FileResult>(65536);

        private readonly IConfig _config;
        private IResultFactory _resultFactory;
        private readonly IPipeline<string> _filePipe;
        private readonly DateTime _archiveDate;

        public MetadataPipe(IConfig config,
            IResultFactory results,
            IPipeline<string> filePipe)
        {
            _config = config;
            _filePipe = filePipe;
            _resultFactory = results;
            _archiveDate = DateTime.Now.Add(_config.CurrentTimespan);
        }

        public Task Execute()
        {
            return Task.Run(() =>
            {
                var types = _config.FileTypes;

                while (_filePipe.Buffer.TryTake(out string file, Timeout.Infinite))
                {
                    var metadata = new FileInfo(file);

                    if (metadata.LastWriteTime > _archiveDate)
                        continue;

                    var type = types.FirstOrDefault(x => x.Extension == metadata.Extension);

                    if (type == null)
                        continue;

                    if (type.IgnoredFiles.Any(x => x.Equals(Path.GetFileName(file), StringComparison.CurrentCultureIgnoreCase)))
                        continue;

                    Buffer.Add(new FileResult
                    {
                        SrcPath = file,
                        Size = metadata.Length,
                        TransferQueued = DateTime.Now,
                        Extension = metadata.Extension
                    });
                }

                Buffer.CompleteAdding();
            });
           
        }
    }
}
