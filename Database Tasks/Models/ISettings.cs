using System.Collections.Generic;

namespace Database_Login_Logger.Models
{
    public interface ISettings
    {
        ActiveLoginsSettings ActiveLogins { get; }
        DatabaseArchiveSettings DatabaseArchive { get; }
        List<string> ConnectionStrings { get; }
        string LoggingConnectionString { get; }
    }
}