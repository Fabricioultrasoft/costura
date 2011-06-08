using System;
using System.IO;
using System.Reflection;

namespace Costura
{
static class ILTemplate
{
    public static void Attach()
    {
        var currentDomain = AppDomain.CurrentDomain;
        currentDomain.AssemblyResolve += OnCurrentDomainOnAssemblyResolve;
    }

    public static Assembly OnCurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
    {
        var name = new AssemblyName(args.Name).Name;
        var assemblyResourceName = string.Format("WeavingTask.{0}.dll", name);
        var executingAssembly = Assembly.GetExecutingAssembly();

        using (var assemblyStream = executingAssembly.GetManifestResourceStream(assemblyResourceName))
        {
            //if (assemblyStream == null)
            //{
            //    throw new Exception(string.Format("Could not retrieve {0}", assemblyResourceName));
            //}
            var assemblyData = new Byte[assemblyStream.Length];
            assemblyStream.Read(assemblyData, 0, assemblyData.Length);

            using (var pdbStream = executingAssembly.GetManifestResourceStream(Path.ChangeExtension(assemblyResourceName, "pdb")))
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