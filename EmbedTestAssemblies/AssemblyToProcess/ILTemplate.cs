using System;
using System.Reflection;

namespace AssemblyToProcessDotNet4
{
    static class ILTemplate
    {
        public static void Attach()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnCurrentDomainOnAssemblyResolve;
        }

        static Assembly OnCurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name).Name;
            var assemblyResourceName = string.Format("WeavingTask.{0}.dll", name);
            var executingAssembly = Assembly.GetExecutingAssembly();

            using (var assemblyStream = executingAssembly.GetManifestResourceStream(assemblyResourceName))
            {
                if (assemblyStream == null)
                {
                    throw new Exception(string.Format("Could not retrieve {0}", assemblyResourceName));
                }
                var assemblyData = new Byte[assemblyStream.Length];
                assemblyStream.Read(assemblyData, 0, assemblyData.Length);
                return Assembly.Load(assemblyData);
            }
        }
    }
}