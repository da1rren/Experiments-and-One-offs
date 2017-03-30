using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Topshelf;
using System.Configuration;
using System.Data;
using Dapper;
using Database_Login_Logger.Models;
using Database_Login_Logger.Models.Data;
using Database_Login_Logger.Tasks;
using Hangfire;
using Hangfire.Console;
using Hangfire.Storage;
using NLog;
using Microsoft.Owin.Hosting;

namespace Database_Login_Logger
{
    public class Service : ServiceControl
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly CancellationTokenSource _token;

        private BackgroundJobServer _server;

        public Service()
        {
            _token = new CancellationTokenSource();
        }

        public bool Start(HostControl hostControl)
        {
            var settings = Settings.GetInstance();

            Log.Info("Starting hangfire");
            GlobalConfiguration.Configuration
                .UseSqlServerStorage(settings.LoggingConnectionString)
                .UseConsole();

            _server = new BackgroundJobServer();

            Log.Info("Purging Hangfire queues");
            using (var connection = JobStorage.Current.GetConnection())
            {
                foreach (var recurringJob in connection.GetRecurringJobs())
                {
                    RecurringJob.RemoveIfExists(recurringJob.Id);
                }
            }

            Log.Info("Starting web interface");
            WebApp.Start<Startup>("http://localhost:9090");

            Log.Info("Queuing tasks");
            RecurringJob.AddOrUpdate(() => new ActiveLoginsTask().Execute(null, _token.Token), settings.ActiveLogins.PollInterval);

            RecurringJob.AddOrUpdate(() => new ArchiveTask().Execute(null, _token.Token), settings.DatabaseArchive.PollInterval);

            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            _server.Dispose();
            _token.Cancel();
            return true;
        }
    }
}
