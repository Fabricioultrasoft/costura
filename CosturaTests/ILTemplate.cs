using System;
using System.Reflection;

namespace CosturaTests
{
    class ILTemplate
    {
        public void Foo()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnCurrentDomainOnAssemblyResolve;

        }

        private Assembly OnCurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name).Name;
            var assemblyResourceName = string.Format("MergedAssembly.{0}.dll", name);
            var executingAssembly = Assembly.GetExecutingAssembly();

            using (var assemblyStream = executingAssembly.GetManifestResourceStream(assemblyResourceName))
            {
                var assemblyData = new Byte[assemblyStream.Length];
                assemblyStream.Read(assemblyData, 0, assemblyData.Length);
                return Assembly.Load(assemblyData);
            }
        }
    }
}