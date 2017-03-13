using System;
using System.Collections.Generic;
using System.Text;

namespace Archiver.Result
{
    public class Results
    {
        public DateTime StartTime { get; set; }

        public DateTime CompleteTime { get; set; }

        public bool ExitedGracefully { get; set; }

        public List<FileResult> FileResults { get; set; }

        public List<ErrorResult> ErrorResults { get; set; }

        public long TotalFiles { get; set; }

        public Results()
        {
            StartTime = DateTime.Now;
            FileResults = new List<FileResult>();
            ErrorResults = new List<ErrorResult>();
        }
    }

    public class FileResult
    {
        public string SrcPath { get; set; }

        public string DestPath { get; set; }

        public long Size { get; set; }

        public string Extension { get; set; }

        public DateTime TransferQueued { get; set; }

        public DateTime TransferComplete { get; set; }

        public string Sha1 { get; set; }

    }

    public class ErrorResult
    {
        public string CurrentFile { get; set; }

        public string ErrorMessage { get; set; }
    }
}
