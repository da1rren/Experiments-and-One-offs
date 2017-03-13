using System;
using System.IO;

namespace Archiver.Test.Pipes.SupportFiles
{
    public static class Utility
    {
        public static string GetFolder()
        {
            var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            return directory;
        }

        public static string CreateFile(string directory, string filename, DateTime? lastModified)
        {
            if(!lastModified.HasValue)
                lastModified = DateTime.Now;

            var path = Path.Combine(directory, filename);
            File.Create(path).Dispose();

            var fileInfo = new FileInfo(path) {LastWriteTime = lastModified.Value};
            return path;
        }
    }
}
