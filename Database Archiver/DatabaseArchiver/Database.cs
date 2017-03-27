using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
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

        public string Database { get; set; }

        private SqlConnection _connection;

        public Database(string hostname, string database)
        {
            Database = database;
            Hostname = hostname;
            ConnectionString = $"Server={Hostname}; Database={database}; Trusted_Connection=True;";
        }

        public async Task Open()
        {
            _connection = new SqlConnection(ConnectionString);
            await _connection.OpenAsync();
        }

        public async Task Online()
        {
            await _connection.ExecuteAsync($"ALTER DATABASE {Database} SET MULTI_USER ");
        }

        public async Task Backup(string dest)
        {
            
        }

        public async Task Delete()
        {
            
        }
    }
}
