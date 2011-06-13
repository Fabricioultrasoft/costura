using System.Reflection;

namespace AssemblyWithLoadNonExistentAssembly
{
    public class ClassToTest
    {
        public void MethodThatDoesLoading()
        {
            Assembly.Load("BadAssemblyName");
        }
    }
}
