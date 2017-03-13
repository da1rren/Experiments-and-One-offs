namespace Archiver.Result
{
    public interface IResultFactory
    {
        long TotalFiles { get; }
        long TotalErrors { get; }
        void AddError(ErrorResult result);
        void AddResult(FileResult result);
        void WriteFile();
    }
}