using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Archiver.Pipes;
using Archiver.Result;
using Archiver.Test.Pipes.SupportFiles;
using NUnit.Framework;
using Xunit;
using Assert = NUnit.Framework.Assert;

namespace Archiver.Test.Pipes
{
    [TestFixture]
    public class TransferPipeTest
    {
        [Fact]
        public async void FileShouldBeMoved()
        {
            //Arrange
            var config = ReusableMocks.MockConfig();

            var file = TestUtilities.CreateFile(config.Src, "Example.doc", DateTime.Now);

            var pipe = new TransferPipe(
                config,
                ReusableMocks.MockResultFactory(),
                ReusableMocks.MockFileTransferPipe(new FileResult
                {
                    SrcPath = file,
                    Size = 0,
                    TransferQueued = DateTime.Now,
                    Extension = ".doc"
                }));

            Assert.That(Directory.GetFiles(config.Src).Length == 1);

            //Act
            await pipe.Execute();

            //Assert
            Assert.That(Directory.GetFiles(config.Dest).Length == 1);
            Assert.That(Directory.GetFiles(config.Src).Length == 0);

        }
    }
}
