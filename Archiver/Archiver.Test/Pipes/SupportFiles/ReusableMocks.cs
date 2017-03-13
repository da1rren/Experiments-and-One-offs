using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Archiver.Pipes.Interfaces;
using Archiver.Result;
using Moq;

namespace Archiver.Test.Pipes.SupportFiles
{
    public static class ReusableMocks
    {
        public static IConfig MockConfig()
        {
            var mock = new Mock<IConfig>();

            mock.Setup(x => x.Src)
                .Returns(SupportFiles.Utility.GetFolder());

            mock.Setup(x => x.Dest)
                .Returns(SupportFiles.Utility.GetFolder());

            mock.Setup(x => x.FileTypes)
                .Returns(new HashSet<FileType>()
                {
                    new FileType
                    {
                        Extension = ".doc",
                        IgnoredFiles = new List<string>
                        {
                            "_checksum.doc"
                        }
                    }
                });

            mock.Setup(x => x.CurrentTimespan)
                .Returns(TimeSpan.FromDays(-180));

            return mock.Object;
        }

        public static IResultFactory MockResultFactory()
        {
            var mock = new Mock<IResultFactory>();

            mock.Setup(x => x.AddResult(It.IsAny<FileResult>()));

            mock.Setup(x => x.AddError(It.IsAny<ErrorResult>()));

            return mock.Object;
        }

        public static IPipeline<string> MockMetadataPipe(params string[] files)
        {
            var mock = new Mock<IPipeline<string>>();

            var queue = new BlockingCollection<string>();

            foreach (var file in files)
                queue.Add(file);

            queue.CompleteAdding();

            mock.Setup(x => x.Buffer)
                .Returns(queue);

            return mock.Object;
        }

        public static IPipeline<FileResult> MockFileTransferPipe(params FileResult[] files)
        {
            var mock = new Mock<IPipeline<FileResult>>();

            var queue = new BlockingCollection<FileResult>();

            foreach (var file in files)
                queue.Add(file);

            queue.CompleteAdding();

            mock.Setup(x => x.Buffer)
                .Returns(queue);

            return mock.Object;
        }
    }
}
