using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using NLog;

namespace Database_Login_Logger.Models.Data
{
    public class DatabaseServer : IDisposable
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public string ConnectionString { get; }

        private SqlConnection _connection;

        public DatabaseServer(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public async Task<bool> Open()
        {
            try
            {
                Log.Info($"Opening connection to {ConnectionString}");
                _connection = new SqlConnection(ConnectionString);
                await _connection.OpenAsync();
                return true;
            }
            catch (Exception ex)
            {
                Log.Warn(ex, $"Connection to {ConnectionString} could not be opened");
                return false;
            }
        }

        public IEnumerable<Database> Databases => GetDatabases();

        public IEnumerable<Database> OfflineDatabases => GetDatabases().Where(x => x.CurrentState == "OFFLINE");

        public IEnumerable<Database> OnlineDatabases => GetDatabases().Where(x => x.CurrentState == "ONLINE");

        public IEnumerable<Database> GetDatabases()
        {
            try
            {
                return _connection.Query<Database>(@"
SELECT 
	sdb.[name] as [Name],
	SERVERPROPERTY('MachineName') AS [Hostname],
	[database_id] as [DatabaseId],
	[create_date] as [CreatedOn],
	[state_desc] as [CurrentState],
	[recovery_model_desc] as [RecoveryModel],
	COALESCE(CONVERT(VARCHAR(12), MAX(bus.backup_finish_date), 101),'-') AS LastBackUpTime
FROM 
	sys.databases sdb 
LEFT JOIN 
	msdb.dbo.backupset bus ON bus.database_name = sdb.name
WHERE 
	sdb.[NAME] NOT IN ('Master', 'model', 'msdb', 'ReportServer', 'ReportServerTempDB')
GROUP BY 
	sdb.[name], [database_id], [create_date], [state_desc], [recovery_model_desc]
");
            }
            catch (Exception ex)
            {
                Log.Warn(ex);
                return new List<Database>();
            }
        }

        public bool OfflineDatabase(string database)
        {
            try
            {
                _connection.Execute($"ALTER DATABASE {database} SET OFFLINE WITH ROLLBACK IMMEDIATE");
                return true;
            }
            catch(Exception ex)
            {
                Log.Warn(ex, $"Could not offline {database}");
                return false;
            }
        }

        public bool OnlineDatabase(string database)
        {
            try
            {
                _connection.Execute($"ALTER DATABASE {database} SET ONLINE");
                return true;
            }
            catch (Exception ex)
            {
                Log.Warn(ex, $"Could not offline {database}");
                return false;
            }
        }

        public IEnumerable<SpWho> ActiveLogins()
        {
            var dataSource = new SqlConnectionStringBuilder(ConnectionString).DataSource;
            try
            {
                var results = _connection.Query<SpWho>("sp_who2").ToList();
                results.ForEach(x => x.Server = dataSource);
                results.ForEach(x => x.QueriedOn = DateTime.Now);
                return results;
            }
            catch (Exception ex)
            {
                Log.Warn(ex, $"Could not get spwho data from {dataSource}");
                return new List<SpWho>();
            }
        }

        private void ReleaseUnmanagedResources()
        {
            _connection?.Dispose();
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~DatabaseServer()
        {
            ReleaseUnmanagedResources();
        }
    }
}
