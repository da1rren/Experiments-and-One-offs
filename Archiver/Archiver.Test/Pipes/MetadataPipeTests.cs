using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Archiver.Pipes;
using Archiver.Pipes.Interfaces;
using Archiver.Result;
using Archiver.Test.Pipes.SupportFiles;
using Moq;
using NUnit.Framework.Internal.Commands;
using NUnit.Framework;
using Xunit;
using Assert = NUnit.Framework.Assert;

namespace Archiver.Test.Pipes
{
    [TestFixture]
    public class MetadataPipeTests
    {
        [Fact]
        public async void FileShouldBeIgnoredDueToAge()
        {
            //Arrange
            var config = ReusableMocks.MockConfig();

            var file = TestUtilities.CreateFile(config.Src, "Example.doc", DateTime.Now);

            var pipe = new MetadataPipe(
                config,
                ReusableMocks.MockResultFactory(),
                ReusableMocks.MockMetadataPipe(file));

            Assert.That(Directory.GetFiles(config.Src).Length > 0);

            //Act
            await pipe.Execute();

            //Assert
            Assert.That(pipe.Buffer.Count == 0);
        }

        [Fact]
        public async void FileShouldBeIgnoredDueToIgnoreList()
        {
            //Arrange
            var config = ReusableMocks.MockConfig();

            var file = TestUtilities.CreateFile(config.Src, "_checksum.doc", DateTime.Now);

            var pipe = new MetadataPipe(
                config,
                ReusableMocks.MockResultFactory(),
                ReusableMocks.MockMetadataPipe(file));

            Assert.That(Directory.GetFiles(config.Src).Length > 0);

            //Act
            await pipe.Execute();

            //Assert
            Assert.That(pipe.Buffer.Count == 0);
        }

        [Fact]
        public async void FileShouldBeIgnoredDueToNoTypeMappingInConfig()
        {
            //Arrange
            var config = ReusableMocks.MockConfig();

            var file = TestUtilities.CreateFile(config.Src, "Example.pdf", DateTime.Now.Add(config.CurrentTimespan).AddDays(-1));

            var pipe = new MetadataPipe(
                config,
                ReusableMocks.MockResultFactory(),
                ReusableMocks.MockMetadataPipe(file));

            Assert.That(Directory.GetFiles(config.Src).Length > 0);

            //Act
            await pipe.Execute();

            //Assert
            Assert.That(pipe.Buffer.Count == 0);
        }


        [Fact]
        public async void FileShouldBeMovedDueToAge()
        {
            //Arrange
            var config = ReusableMocks.MockConfig();
            var file = TestUtilities.CreateFile(config.Src, "Example.doc", DateTime.Now.Add(config.CurrentTimespan).AddDays(-1));

            var pipe = new MetadataPipe(
                config,
                ReusableMocks.MockResultFactory(),
                ReusableMocks.MockMetadataPipe(file));

            Assert.That(Directory.GetFiles(config.Src).Length > 0);

            //Act
            await pipe.Execute();

            //Assert
            Assert.That(pipe.Buffer.Count == 1);
        }

    }
}
