using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Database_Login_Logger.Models;
using Database_Login_Logger.Models.Data;
using Hangfire.Console;
using Hangfire.Server;
using NLog;

namespace Database_Login_Logger.Tasks
{
    public class ActiveLoginsTask : IDatabaseTask
    {
        public ISettings Instance { get; } = Settings.GetInstance();

        private static PerformContext _context;

        public void Execute(PerformContext context, CancellationToken token)
        {
            _context = context;
            _context.WriteProgressBar();
            _context.WriteLine("Task initialized");

            if (!Instance.ActiveLogins.Enabled)
            {
                _context.WriteLine("Task is not enabled.");
                _context.WriteProgressBar(100);
                return;
            }

            var databases = new List<DatabaseServer>();

            foreach (var database in Instance.ConnectionStrings.Select(x => new DatabaseServer(x)))
            {
                if (database.Open().Result)
                {
                    _context.Write($"Connected to {database.ConnectionString}", ConsoleTextColor.Black);
                    databases.Add(database);
                }
                else
                {
                    _context.Write($"Could not connect to {database.ConnectionString}", ConsoleTextColor.Yellow);   
                }
            }

            _context.WriteProgressBar(50);

            var spWhos = databases.SelectMany(x => x.ActiveLogins()).ToList();

            if (spWhos.Any())
            {
                _context.Write($"Saving {spWhos.Count} SpWho records.", ConsoleTextColor.Black);
                Save(spWhos);
            }

            databases.ForEach(x => x.Dispose());

            _context.WriteProgressBar(100);

        }

        private void Save(IEnumerable<SpWho> whos)
        {
            using (var logging = new SqlConnection(Instance.LoggingConnectionString))
            {
                foreach (var login in whos)
                {
                    var command = @"
                                INSERT INTO Login (
                                Server, SPID, Status, Login, HostName, BlkBy, DBName, Command, CPUTime, DiskIO, LastBatch, ProgramName, QueriedOn)
                                VALUES (@Server, @SPID, @Status, @Login, @HostName, @BlkBy, @DBName, @Command, @CPUTime, @DiskIO, @LastBatch, @ProgramName, @QueriedOn )";

                    logging.Execute(command, login);
                }
            }
        }

    }
}
