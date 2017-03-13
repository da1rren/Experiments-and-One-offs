using System.Collections.Concurrent;
using System.Dynamic;
using System.Threading.Tasks;

namespace Archiver.Pipes.Interfaces
{
    public interface IPipeline<T> : IExecute
    {
        BlockingCollection<T> Buffer { get; set; }
    }

    public interface IExecute
    {
        Task Execute();
    }

    public interface IQueued
    {
        decimal GetWaitingTasks();
    }
}
