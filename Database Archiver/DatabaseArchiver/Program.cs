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

        private static readonly string[] Hosts = {"NHSG-TRAKAPPS", "ARI-SQL-PROSE", "OTTAWA", "NHSG-SQL-HOST1", "ARI-SQL-ECCI"};

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

            var servers = new List<Server>();

            foreach (var host in Hosts)
            {
                var server = new Server(host);
                await server.Open();
                servers.Add(server);
            }

        }

    }
}
