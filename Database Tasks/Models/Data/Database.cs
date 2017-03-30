using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using NLog;

namespace Database_Login_Logger.Models.Data
{
    public class Database : IDisposable
    {
        #region Private

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static ISettings _instance;

        private string _name;

        private SqlConnection _connection;

        private string UnwrappedName;

        #endregion
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                UnwrappedName = _name;

                if (!_name.StartsWith("["))
                {
                    _name = "[" + _name;
                }

                if (!_name.EndsWith("]"))
                {
                    _name = _name + "]";
                }
            }
        }

        public int DatabaseId { get; set; }

        public DateTime CreatedOn { get; set; }

        public string CurrentState { get; set; }

        public string RecoveryModel { get; set; }

        public string LastBackUpTime { get; set; }

        public string Hostname { get; }

        public string ConnectionString { get; }

        public Database()
        {
            _instance = Settings.GetInstance();
        }

        public async Task<bool> Open()
        {
            try
            {
                Log.Info($"Opening connection to [{Hostname}].{Name}");
                _connection = new SqlConnection($"Server={Hostname}; Database={UnwrappedName}; Trusted_Connection=true");
                await _connection.OpenAsync();
                return true;
            }
            catch (Exception ex)
            {
                Log.Warn(ex, $"Connection to [{Hostname}].[{Name}] could not be opened");
                return false;
            }
        }

        public async Task<bool> Backup()
        {
            try
            {
                var path = Path.Combine(CreateBackupDirectory(), Name + ".bak");
                Log.Info($"Backing up {Name} to {path}");

                await _connection.ExecuteAsync(
                    $"BACKUP DATABASE {Name} TO DISK = '{path}'", commandTimeout: 600);

                Log.Info($"Backing up of {Name} complete");
                return true;
            }
            catch(Exception ex)
            {
                Log.Warn(ex, $"Backup of {Name} failed");
                return false;
            }
        }

        private string CreateBackupDirectory()
        {
            var dest = Path.Combine(_instance.DatabaseArchive.Dest, Hostname);

            if (!Directory.Exists(dest))
            {
                Directory.CreateDirectory(dest);
                return dest;
            }

            return dest;
        }

        public async Task<bool> Delete()
        {
            Log.Info($"Starting delete of Database {Name}.");

            try
            {
                await _connection.ExecuteAsync($"USE MASTER");
                Log.Info($"Entering single user for {Name}.");
                await _connection.ExecuteAsync($"ALTER DATABASE {Name} SET SINGLE_USER WITH ROLLBACK IMMEDIATE", commandTimeout: 600);
                Log.Info($"Dropping {Name}");
                await _connection.ExecuteAsync($"DROP DATABASE {Name}", commandTimeout: 600);
                Log.Info($"Dropped {Name}");
                return true;

            }
            catch (Exception ex)
            {
                Log.Error(ex);
                Log.Warn($"Failed to delete {Name}.  Attempting to offline database.");
                Log.Warn($"Database {Name} was offlined.");
                return false;
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

        ~Database()
        {
            ReleaseUnmanagedResources();
        }
    }
}
