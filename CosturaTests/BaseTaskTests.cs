using System.Linq;
using System.Reflection;
using Mono.Cecil;
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

        [Test]
        public void EnsureOnly1RefToMscorLib()
        {
            var moduleDefinition = ModuleDefinition.ReadModule(assembly.CodeBase.Remove(0, 8));
            Assert.AreEqual(1, moduleDefinition.AssemblyReferences.Count(x=>x.Name == "mscorlib"));
        }
    }
}