using Archiver.Result;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Archiver.Pipes.Interfaces;

namespace Archiver.Pipes
{
    public class TransferPipe : IExecute
    {
        private readonly IConfig _config;
        private readonly IResultFactory _resultFactory;
        private readonly IPipeline<FileResult> _metadataPipe;

        public TransferPipe(IConfig config,
            IResultFactory results,
            IPipeline<FileResult> metadataPipe)
        {
            _config = config;
            _resultFactory = results;
            _metadataPipe = metadataPipe;
        }

        public Task Execute()
        {
            return Task.Run(() => 
            {
                var tasks = new List<Task>(8096);

                while (!_metadataPipe.Buffer.IsCompleted)
                {
                    var result = _metadataPipe.Buffer.Take();
                    tasks.Add(Transfer(result));
                }

                Task.WaitAll(tasks.ToArray());
            });
        }

        public Task Transfer(FileResult metadata)
        {
            return Task.Run(() =>
            {
                metadata.DestPath = Path.Combine(_config.Dest, Guid.NewGuid().ToString("N")) + metadata.Extension;

                try
                {
                    using (var file = File.Open(metadata.SrcPath, FileMode.Open))
                    using (var dest = File.Open(metadata.DestPath, FileMode.CreateNew))
                    using (var sha1 = SHA1.Create())
                    {
                        //TODO
                        //Check if file stream is cause 2 reads instead of one
                        //Wrap in memory stream if it is
                        file.CopyTo(dest);
                        file.Position = 0;

                        var hash = sha1.ComputeHash(file);
                        var sb = new StringBuilder(hash.Length * 2);

                        foreach (var b in hash)
                            sb.Append(b.ToString("X2"));

                        metadata.Sha1 = sb.ToString();

                        metadata.TransferComplete = DateTime.Now;
                        _resultFactory.AddResult(metadata);
                    }

                    File.Delete(metadata.SrcPath);
                }
                catch (Exception ex)
                {
                    _resultFactory.AddError(new ErrorResult{CurrentFile = metadata.SrcPath, ErrorMessage = ex.Message});
                }
            });
        }
    }
}
