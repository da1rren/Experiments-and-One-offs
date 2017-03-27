using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace DatabaseArchiver
{
    class Program
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static readonly string[] Hosts = {"NHSG-TRAKAPPS"/*, "ARI-SQL-PROSE", "OTTAWA", "NHSG-SQL-HOST1", "ARI-SQL-ECCI"*/};

        private static readonly List<Server> Servers = new List<Server>();

        static void Main(string[] args)
        {
            MainAsync(args).Wait();
        }

        static async Task MainAsync(string[] args)
        {
            Console.WriteLine(@"______      _        _                       ___           _     _                ");
            Console.WriteLine(@"|  _  \    | |      | |                     / _ \         | |   (_)               ");
            Console.WriteLine(@"| | | |__ _| |_ __ _| |__   __ _ ___  ___  / /_\ \_ __ ___| |__  ___   _____ _ __ ");
            Console.WriteLine(@"| | | / _` | __/ _` | '_ \ / _` / __|/ _ \ |  _  | '__/ __| '_ \| \ \ / / _ \ '__|");
            Console.WriteLine(@"| |/ / (_| | || (_| | |_) | (_| \__ \  __/ | | | | | | (__| | | | |\ V /  __/ |   ");
            Console.WriteLine(@"|___/ \__,_|\__\__,_|_.__/ \__,_|___/\___| \_| |_/_|  \___|_| |_|_| \_/ \___|_|   ");
            Console.WriteLine(@"                                                                                  ");
            Console.WriteLine(@"                                                                                  ");

            Log.Info("Initializing");

            var databases = await GetAllOfflineDatabases();


            var databaseTasks = new List<Task>();

            foreach (var database in databases)
            {
                databaseTasks.Add(database.Open());
            }

            await Task.WhenAll(databaseTasks);
            databaseTasks.Clear();

            foreach (var database in databases)
            {
                databaseTasks.Add(database.Backup(@"\\ehealthbackups\eHealthBackups\SQLDevBackups\Archive"));
            }

            await Task.WhenAll(databaseTasks);
            Console.WriteLine("Ready to delete databases");
            Console.WriteLine("ENSURE THEY HAVE BEEN BACKED UP");
            Console.WriteLine("Press any key to delete");
            Console.ReadKey();
            databaseTasks.Clear();

            foreach (var database in databases)
            {
                databaseTasks.Add(database.Delete());
            }

            await Task.WhenAll(databaseTasks);

        }

        private static async Task<List<Database>>  GetAllOfflineDatabases()
        {
            var connectTasks = new List<Task>();

            foreach (var host in Hosts)
            {
                var server = new Server(host);
                connectTasks.Add(server.Open());
                Servers.Add(server);
            }

            await Task.WhenAll(connectTasks.ToArray());

            return Servers.Select(x => x
                .GetOfflineDatabases())
                .SelectMany(task => task.Result)
                .ToList();
        }

        

    }
}
