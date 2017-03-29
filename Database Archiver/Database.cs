using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using NLog;

namespace DatabaseArchiver
{
    public class Database
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public string Hostname { get; set; }

        public bool IsOnline { get; set; }

        public string ConnectionString { get; private set; }

        public string Name { get; set; }

        public Server Server { get; private set; }

        public bool BackedUp { get; set; }

        private SqlConnection _connection;

        private bool Error { get; set; }

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
            ConnectionString = $"Server={Hostname}; Database={database}; Trusted_Connection=True; MultipleActiveResultSets=True; Connection Timeout=600";
            Server = server;
        }

        public async Task Open()
        {
            await Server.OnlineDatabase(Name);
            _connection = new SqlConnection(ConnectionString);

            try
            {
                await _connection.OpenAsync();
            }
            catch
            {
                Log.Error("Failed to open connection");
                Error = true;
            }

            Log.Info($"Opened connection to {Name}");
        }

        public async Task Backup(string dest)
        {
            if(Error)
                return;

            Log.Info($"Starting backup of {Name}");

            if (!dest.EndsWith("\\"))
            {
                dest += "\\";
            }

            if (!Directory.Exists(dest))
            {
                Log.Warn($"Backup Directory does not exist or cannot be accessed.");
                Log.Warn($"Offlining {Name}.");
                await Server.OfflineDatabase(Name);
                BackedUp = false;
                return;
            }

            dest = dest + Hostname + "\\";
            Directory.CreateDirectory(dest);

            dest = dest + Name + ".bak";
            try
            {
                Log.Info($"Starting backup of {Name}.");
                await _connection.ExecuteAsync($"BACKUP DATABASE {Name} TO DISK = '{dest}'", commandTimeout: 600);
                BackedUp = true;
                Log.Info($"Backup of {Name} complete.");
                return;
            }
            catch
            {
                Log.Warn($"Failed to backup {Name}.  Attempting to offline database.");
                await Server.OfflineDatabase(Name);
                Log.Warn($"Database {Name} was offlined.");
            }

            BackedUp = false;
        }

        public async Task Delete()
        {
            if (Error)
                return;

            Log.Info($"Starting delete of Database {Name}.");

            if (!BackedUp)
            {
                Log.Warn($"Delete command refused.  Database {Name} not backed up.");
                return;
            }

            try
            {
                await _connection.ExecuteAsync($"USE MASTER");
                Log.Info($"Entering single user for {Name}.");
                await _connection.ExecuteAsync($"ALTER DATABASE {Name} SET SINGLE_USER WITH ROLLBACK IMMEDIATE", commandTimeout: 600);
                Log.Info($"Dropping {Name}");
                await _connection.ExecuteAsync($"DROP DATABASE {Name}", commandTimeout: 600);
                Log.Info($"Dropped {Name}");

            }
            catch (SqlException ex)
            {
                Log.Error(ex);
                Log.Warn($"Failed to delete {Name}.  Attempting to offline database.");
                await Server.OfflineDatabase(Name);
                Log.Warn($"Database {Name} was offlined.");
            }
        }
    }
}
