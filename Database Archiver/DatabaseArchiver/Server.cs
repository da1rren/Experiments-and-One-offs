using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace DatabaseArchiver
{
    public class Server
    {
        public string Hostname { get; private set; }

        public string ConnectionString { get; private set; }

        private SqlConnection _connection;

        public Server(string hostname)
        {
            ConnectionString = $"Server={Hostname}; Database=Master; Trusted_Connection=True;";
            Hostname = hostname;
        }

        public async Task Open()
        {
            _connection = new SqlConnection(ConnectionString);
            await _connection.OpenAsync();
        }

        public async Task<IEnumerable<Database>> GetOfflineDatabases()
        {
            var offlineDatabases = await OfflineDatabaseNames();
            return offlineDatabases.Select(x => new Database(Hostname, x));
        }

        private async Task<IEnumerable<string>> OfflineDatabaseNames()
        {
            return await _connection.QueryAsync<string>("select name from sys.databases as db where db.state = 6");
        }

    }
}
