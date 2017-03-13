using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Archiver.Result;

namespace Archiver.Utility
{
    public class FileTraverser : IEnumerator<string>, IEnumerable<string>
    {
        public Queue<string> Files { get; set; }

        public List<ErrorResult> ErrorResults { get; set; }

        public Stack<string> ToVisit { get; set; }

        public CancellationToken Token { get; set; }

        public FileTraverser(string currentLocation, CancellationToken token)
        {
            Token = token;
            Files = new Queue<string>();
            ToVisit = new Stack<string>();
            ErrorResults = new List<ErrorResult>();
            ToVisit.Push(currentLocation);
        }

        public bool MoveNext()
        {
            //If there are any files left in the queue
            if (Files.Any())
            {
                Current = Files.Dequeue();
                return true;
            }

            if (Token.IsCancellationRequested)
                return false;

            //Keep iterating until we find the next set of files
            while (ToVisit.Any() && !Files.Any())
            {
                var currentRoot = ToVisit.Pop();

                try
                {
                    //Get all directories in current dir
                    //Stop processing branch if there is an exception
                    foreach (var directory in Directory.GetDirectories(currentRoot))
                        ToVisit.Push(directory);
                }
                catch (Exception ex)
                {
                    ErrorResults.Add(new ErrorResult
                    {
                        CurrentFile = currentRoot,
                        ErrorMessage = ex.Message
                    });
                }

                //Enqueue found files
                foreach (var file in Directory.GetFiles(currentRoot))
                    Files.Enqueue(file);
            }

            var any = Files.Any();

            if (any)
                Current = Files.Dequeue();

            return any;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public string Current { get; set; }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<string> GetEnumerator()
        {
            return this;
        }

        public void Dispose()
        {
            //Nothing
        }

    }
}