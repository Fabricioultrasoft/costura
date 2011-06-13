using System;
using System.Reflection;

namespace Costura
{
    internal static class ILTemplate
    {
        public static void Attach()
        {
            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += ResolveAssembly;
        }

        public static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name).Name;
            var assemblyResourceName = string.Format("Costura.{0}.dll", name);
            var executingAssembly = Assembly.GetExecutingAssembly();

            using (var assemblyStream = executingAssembly.GetManifestResourceStream(assemblyResourceName))
            {
                if (assemblyStream == null)
                {
                    return null;
                }
                var assemblyData = new Byte[assemblyStream.Length];
                assemblyStream.Read(assemblyData, 0, assemblyData.Length);

                var pdbName = assemblyResourceName.Substring(0, assemblyResourceName.Length - 3) + "pdb";
                using (var pdbStream = executingAssembly.GetManifestResourceStream(pdbName))
                {
                    if (pdbStream != null)
                    {
                        var pdbData = new Byte[pdbStream.Length];
                        pdbStream.Read(pdbData, 0, pdbData.Length);
                        return Assembly.Load(assemblyData, pdbData);
                    }
                }
                return Assembly.Load(assemblyData);
            }
        }
    }
}