using System.Collections.Generic;

namespace Archiver.Result
{
    public interface IResultFactory
    {
        long TotalFiles { get; }
        long TotalErrors { get; }
        long TotalSize { get; }
        void AddError(ErrorResult result);
        void AddErrors(IEnumerable<ErrorResult> results);
        void AddResult(FileResult result);
        void WriteFile();
    }
}