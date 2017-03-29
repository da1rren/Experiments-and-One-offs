using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Topshelf;
using System.Configuration;
using System.Data;
using Dapper;
using NLog;

namespace Database_Login_Logger
{
    public class Service : ServiceControl
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly CancellationTokenSource _token;

        private Task _monitorLogins;

        public Service()
        {
            _token = new CancellationTokenSource();
        }

        public bool Start(HostControl hostControl)
        {
            Log.Info("Starting login monitor thread");
            _monitorLogins = Task.Run(() =>
            {
                while (!_token.IsCancellationRequested)
                {
                    var settings = Settings.GetInstance();
                    var tasks = new List<Task<IEnumerable<SpWho>>>();

                    foreach (var connectionString in settings.ConnectionStrings)
                    {
                        Log.Info("Queuing tasks");

                        var database = new Database(connectionString);
                        tasks.Add(database.ActiveLogins());
                    }

                    try
                    {
                        Task.WaitAll(tasks.ToArray());
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                    }

                    var collatedLogins = tasks
                        .Where(x => !x.IsFaulted)
                        .SelectMany(x => x.Result)
                        .Where(x => x.Login != "sa");

                    using (var logging = new SqlConnection(settings.LoggingConnectionString))
                    {
                        foreach (var login in collatedLogins)
                        {
                            var command = @"
INSERT INTO Login (
Server, SPID, Status, Login, HostName, BlkBy, DBName, Command, CPUTime, DiskIO, LastBatch, ProgramName, QueriedOn)
VALUES (@Server, @SPID, @Status, @Login, @HostName, @BlkBy, @DBName, @Command, @CPUTime, @DiskIO, @LastBatch, @ProgramName, @QueriedOn )";

                            logging.Execute(command, login);

                        }
                    }

                    Log.Info("Sleeping for 15000");
                    Thread.Sleep(settings.PollFrequency);
                }
            });

            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            _token.Cancel();
            _monitorLogins.Wait();
            return true;
        }
    }
}
