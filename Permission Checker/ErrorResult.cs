using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermissionChecker
{
    public class ErrorResult
    {
        public string CurrentFile { get; set; }

        public string ErrorMessage { get; set; }

        public override string ToString()
        {
            return CurrentFile + " | " + ErrorMessage + Environment.NewLine;
        }
    }
}
