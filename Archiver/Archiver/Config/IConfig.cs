using System;
using System.Collections.Generic;

namespace Archiver
{
    public interface IConfig
    {
        TimeSpan CurrentTimespan { get; set; }

        string Src { get; set; }

        string Dest { get; set; }

        HashSet<FileType> FileTypes { get; set; }

        void Validate();
    }
}