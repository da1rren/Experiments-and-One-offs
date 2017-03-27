using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace DatabaseArchiver
{
    public class Database
    {
        public string Hostname { get; set; }

        public bool IsOnline { get; set; }

        public string ConnectionString { get; private set; }

        public string Name { get; set; }

        public Server Server { get; private set; }

        private SqlConnection _connection;

        public Database(Server server, string hostname, string database)
        {
            Name = database;

            if (!Name.StartsWith("["))
            {
                Name = "[" + Name;
            }

            if (!Name.EndsWith("]"))
            {
                Name = Name + "]";
            }

            Hostname = hostname;
            ConnectionString = $"Server={Hostname}; Database={database}; Trusted_Connection=True;MultipleActiveResultSets=True;";
            Server = server;
        }

        public async Task Open()
        {
            await Server.OnlineDatabase(Name);
            _connection = new SqlConnection(ConnectionString);
            await _connection.OpenAsync();
        }

        public async Task Backup(string dest)
        {
            if (!dest.EndsWith("\\"))
            {
                dest += "\\";
            }

            if (!Directory.Exists(dest))
            {
                await Server.OfflineDatabase(Name);
                return;
            }

            dest = dest + Hostname + "\\" + Name + ".bak";

            Directory.CreateDirectory(Hostname);

            await _connection.ExecuteAsync($"BACKUP DATABASE {Name} TO DISK = '{dest}'");
        }

        public async Task Delete()
        {
            await _connection.ExecuteAsync($"DROP DATABASE {Name}");
        }
    }
}
