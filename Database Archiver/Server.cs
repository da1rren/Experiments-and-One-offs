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

        private object _lock = new object();

        public Server(string hostname)
        {
            Hostname = hostname;
            ConnectionString = $"Server={Hostname}; Database=Master; Trusted_Connection=True;MultipleActiveResultSets=True;";
        }

        public async Task Open()
        {
            _connection = new SqlConnection(ConnectionString);
            await _connection.OpenAsync();
        }

        public async Task<IEnumerable<Database>> GetOfflineDatabases()
        {
            var offlineDatabases = await OfflineDatabaseNames();

            return offlineDatabases.Select(x => new Database(this, Hostname, x));
        }

        public async Task OnlineDatabase(string name)
        {
            var result = await _connection.ExecuteAsync($"ALTER DATABASE {name} SET ONLINE WITH ROLLBACK IMMEDIATE");
        }

        public async Task OfflineDatabase(string name)
        {
            await _connection.ExecuteAsync($"ALTER DATABASE {name} SET OFFLINE WITH ROLLBACK IMMEDIATE");
        }

        private async Task<IEnumerable<string>> OfflineDatabaseNames()
        {
            return await _connection.QueryAsync<string>("select name from sys.databases as db where db.state = 6");
        }

    }
}
