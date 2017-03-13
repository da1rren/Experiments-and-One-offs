using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Archiver.Pipes;
using Archiver.Result;
using Archiver.Test.Pipes.SupportFiles;
using NUnit.Framework;
using Xunit;
using Assert = NUnit.Framework.Assert;

namespace Archiver.Test.Pipes
{
    public class FilePipeTests
    {
        [Fact]
        public async void ShouldDiscoverNestedFiles()
        {
            //Arrange
            var config = ReusableMocks.MockConfig();
            var nestedFolder = Path.Combine(config.Src, Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(nestedFolder);

            var file = TestUtilities.CreateFile(config.Src, "Example1.doc",
                DateTime.Now.Add(config.CurrentTimespan).AddDays(-1));

            var nestedFile = TestUtilities.CreateFile(nestedFolder, "Example2.doc",
                DateTime.Now.Add(config.CurrentTimespan).AddDays(-1));

            var token = new CancellationTokenSource();

            var pipe = new FilePipe(
                config,
                ReusableMocks.MockResultFactory(),
                token.Token);

            //Act
            await pipe.Execute();

            //Assert
            Assert.That(pipe.Buffer.Count == 2);
        }
    }
}
