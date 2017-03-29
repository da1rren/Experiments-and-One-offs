using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using MoreLinq;

namespace Database_Login_Logger
{
    public class Database
    {
        public string ConnectionString { get; set; }

        public Database(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public async Task<IEnumerable<SpWho>> ActiveLogins()
        {
            var builder = new SqlConnectionStringBuilder(ConnectionString);

            using (var connection = new SqlConnection(ConnectionString))
            {
                var results = await connection.QueryAsync<SpWho>("sp_who2");
                results.ForEach(x => x.Server = builder.DataSource);
                results.ForEach(x => x.QueriedOn = DateTime.Now);

                return results;
            }
        }
    }
}
