using System.Reflection;
using NUnit.Framework;

namespace CosturaTests
{
    public abstract class BaseTaskTests
    {
        string projectPath;
        Assembly assembly;

        protected BaseTaskTests(string projectPath)
        {

#if (!DEBUG)
            projectPath = projectPath.Replace("Debug", "Release");
#endif
            this.projectPath = projectPath;
        }

        [TestFixtureSetUp]
        public void Setup()
        {
            var weaverHelper = new WeaverHelper(projectPath);
            assembly = weaverHelper.Assembly;
        }


        [Test]
        public void Simple()
        {
            var instance = assembly.GetInstance("AssemblyToProcessDotNet4.ClassToTest");
            Assert.AreEqual("Hello", instance.Foo());
        }

#if(DEBUG)
        [Test]
        public void PeVerify()
        {
            Verifier.Verify(assembly.CodeBase.Remove(0, 8));
        }
#endif

    }
}