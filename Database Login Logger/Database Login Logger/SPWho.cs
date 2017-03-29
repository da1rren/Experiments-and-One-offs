using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database_Login_Logger
{
    public class SpWho
    {
        public string Server { get; set; }

        public DateTime QueriedOn { get; set; }

        public int SPID { get; set; }

        public string Status { get; set; }

        public string Login { get; set; }

        public string HostName { get; set; }

        public string BlkBy { get; set; }

        public string DBName { get; set; }

        public string Command { get; set; }

        public string CPUTime { get; set; }

        public string DiskIO { get; set; }

        public string LastBatch { get; set; }

        public string ProgramName { get; set; }
    }
}
