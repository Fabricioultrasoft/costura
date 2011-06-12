using System;
using System.Reflection;
using NUnit.Framework;

namespace CosturaTests
{
    [TestFixture]
    public  class AssemblyWithLoadNonExistentAssemblyTests
    {

        [Test]
        public void EnsureNoException()
        {
            var projectPath = @"AssemblyWithLoadNonExistentAssembly\AssemblyWithLoadNonExistentAssembly.csproj";
#if (!DEBUG)
            projectPath = projectPath.Replace("Debug", "Release");
#endif
            var weaverHelper = new WeaverHelper(projectPath);
            var assembly = weaverHelper.Assembly;
            var instance = assembly.GetInstance("AssemblyWithLoadNonExistentAssembly.ClassToTest");

            var expected= GetExceptionString(() => Assembly.Load("BadAssemblyName"));
            var actual = GetExceptionString(() => instance.MethodThatDoesLoading());
            Assert.AreEqual(expected, actual);
        }

        private string GetExceptionString(Action action)
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
            return null;
        }
    }
}