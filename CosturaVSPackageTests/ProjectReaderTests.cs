using System.IO;
using CosturaVSPackage;
using Microsoft.Build.Framework;
using NUnit.Framework;

namespace CosturaVSPackageTests
{
    [TestFixture]
    public class ProjectReaderTests
    {

        [Test]
        public void WithNoWeaving()
        {
            var sourceProjectFile = new FileInfo(@"TestProjects\ProjectWithNoWeaving.csproj");
            var targetFileInfo = sourceProjectFile.CopyTo(sourceProjectFile.FullName + "ProjectReaderTest", true);
            try
            {

                var reader = new ProjectReader(targetFileInfo.FullName);

                Assert.IsNull(reader.Overwrite);
                Assert.IsNull(reader.ToolsDirectory);
                Assert.IsNull(reader.MessageImportance);
                Assert.IsNull(reader.TargetPath);
            }
            finally
            {
                targetFileInfo.Delete();
            }

        }

        [Test]
        public void WithExistingWeaving()
        {
            var sourceProjectFile = new FileInfo(@"TestProjects\ProjectWithWeaving.csproj");
            var targetFileInfo = sourceProjectFile.CopyTo(sourceProjectFile.FullName + "ProjectReaderTest", true);
            try
            {
                var reader = new ProjectReader(targetFileInfo.FullName);
                Assert.IsTrue(reader.Overwrite.Value);
                Assert.AreEqual("@(IntermediateAssembly)", reader.TargetPath);
                Assert.AreEqual("$(SolutionDir)Tools\\", reader.ToolsDirectory);
                Assert.AreEqual(MessageImportance.High, reader.MessageImportance);
            }
            finally
            {
                targetFileInfo.Delete();
            }
        }


        [Test]
        public void WithMinimalWeaving()
        {

            var sourceProjectFile = new FileInfo(@"TestProjects\ProjectWithMinimalWeaving.csproj");
            var targetFileInfo = sourceProjectFile.CopyTo(sourceProjectFile.FullName + "ProjectReaderTest", true);
            try
            {
                var reader = new ProjectReader(targetFileInfo.FullName);
                Assert.IsNull(reader.Overwrite);
                Assert.IsNull(reader.TargetPath);
                Assert.IsNull(reader.TargetPath);
                Assert.AreEqual(@"$(SolutionDir)Tools\", reader.ToolsDirectory);
                Assert.IsNull(reader.MessageImportance);
            }
            finally
            {
                targetFileInfo.Delete();
            }

        }


    }
}