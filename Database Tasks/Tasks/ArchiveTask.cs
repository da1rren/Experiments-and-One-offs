using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Database_Login_Logger.Models;
using Database_Login_Logger.Models.Data;
using Hangfire.Console;
using Hangfire.Server;
using NLog;

namespace Database_Login_Logger.Tasks
{
    public class ArchiveTask : IDatabaseTask
    {
        private static PerformContext _context;

        public ISettings Instance { get; } = Settings.GetInstance();

        public void Execute(PerformContext context, CancellationToken token)
        {
            _context = context;
            _context.WriteLine("Task initialized");

            if (!Instance.DatabaseArchive.Enabled)
            {
                _context.WriteLine("Task is not enabled.");
                return;
            }

            _context.WriteLine("Archive Starting");

            var servers = new List<DatabaseServer>();

            foreach (var connection in Instance.ConnectionStrings)
            {
                var server = new DatabaseServer(connection);

                if (server.Open().Result)
                {
                    servers.Add(server);
                }
                else
                {
                    _context.Write($"Could not connect to {connection}", ConsoleTextColor.Yellow);
                }
            }

            var offlineDbs = servers.SelectMany(x => x.OfflineDatabases).ToList();

            if (offlineDbs.Any())
            {
                ProcessDbs(offlineDbs).Wait(token);
            }

            servers.ForEach(x => x.Dispose());

            _context.WriteLine("Archive Cycle Complete");
        }

        private async Task ProcessDbs(List<Database> offlineDbs)
        {
            _context.WriteLine($"Attempting to archiving {offlineDbs.Count}");

            foreach (var db in offlineDbs)
            {
                using (
                    var server = new DatabaseServer($"Server={db.Hostname}; Database=Master; Trusted_Connection=true"))
                {
                    if (!await server.Open())
                    {
                        _context.Write($"Could not connect to master db on {db.Hostname}.  Databases from this server will be skipped.", ConsoleTextColor.Yellow);
                        continue;
                    }

                    if (!server.OnlineDatabase(db.Name))
                    {
                        _context.Write($"Unable to online [{db.Hostname}]{db.Name}. This database will be skipped", ConsoleTextColor.Yellow);
                        continue;
                    }

                    if (!await db.Open())
                    {
                        _context.Write($"Could not connect to {db.Name} on {db.Hostname}.  This databases will be skipped.", ConsoleTextColor.Yellow);
                        continue;
                    }

                    if (await db.Backup())
                    {
                        if (!await db.Delete())
                        {
                            _context.Write($"Unable to delete [{db.Hostname}]{db.Name}", ConsoleTextColor.Yellow);
                            _context.Write($"Attempting to offline [{db.Hostname}]{db.Name}", ConsoleTextColor.Yellow);
                            server.OfflineDatabase(db.Name);
                        }
                    }
                    else
                    {
                        _context.Write($"Unable to backup [{db.Hostname}]{db.Name}", ConsoleTextColor.Yellow);
                        _context.Write($"Attempting to offline [{db.Hostname}]{db.Name}", ConsoleTextColor.Yellow);
                        server.OfflineDatabase(db.Name);
                    }
                }

                db.Dispose();
            }
        }

    }
}
