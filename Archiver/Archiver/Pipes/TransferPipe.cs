using Archiver.Result;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Archiver.Config;
using Archiver.Pipes.Interfaces;

namespace Archiver.Pipes
{
    public class TransferPipe : IExecute, IQueued
    {
        private readonly IConfig _config;
        private readonly IResultFactory _resultFactory;
        private readonly IPipeline<FileResult> _metadataPipe;
        private readonly List<Task> _tasks;
        private readonly object _lock = new object();

        public TransferPipe(IConfig config,
            IResultFactory results,
            IPipeline<FileResult> metadataPipe)
        {
            _config = config;
            _resultFactory = results;
            _metadataPipe = metadataPipe;
            _tasks = new List<Task>(8096);
        }

        public Task Execute()
        {
            return Task.Run(() => 
            {

                while (_metadataPipe.Buffer.TryTake(out FileResult result, Timeout.Infinite))
                {
                    lock (_lock)
                        _tasks.Add(Transfer(result));
                }

                Task.WaitAll(_tasks.ToArray());
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

        public decimal GetWaitingTasks()
        {
            lock (_lock)
            {
                return _tasks.Count(x => x.Status != TaskStatus.RanToCompletion && x.Status != TaskStatus.Faulted);
            }
        }
    }
}
