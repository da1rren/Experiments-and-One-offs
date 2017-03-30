using System.Threading;
using System.Threading.Tasks;
using Database_Login_Logger.Models;
using Hangfire.Server;

namespace Database_Login_Logger.Tasks
{
    public interface IDatabaseTask
    {
        ISettings Instance { get; }

        void Execute(PerformContext context, CancellationToken token);
    }
}
