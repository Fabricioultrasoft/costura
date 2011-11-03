using System;
using System.IO;
using System.Reflection;

internal static class ILTemplateWithTempAssembly
{
    private static string tempBasePath;

    public static void Attach()
    {
        var currentDomain = AppDomain.CurrentDomain;
        currentDomain.AssemblyResolve += ResolveAssembly;

        tempBasePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempBasePath);
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
            
            var assemblyTempFilePath = Path.Combine(tempBasePath, assemblyResourceName);
            File.WriteAllBytes(assemblyTempFilePath, assemblyData);

            var pdbName = Path.ChangeExtension(assemblyResourceName, "pdb");

            using (var pdbStream = executingAssembly.GetManifestResourceStream(pdbName))
            {
                if (pdbStream != null)
                {
                    var pdbData = new Byte[pdbStream.Length];
                    pdbStream.Read(pdbData, 0, pdbData.Length);
                    var assemblyPdbTempFilePath = Path.Combine(tempBasePath, pdbName);
                    File.WriteAllBytes(assemblyPdbTempFilePath, pdbData);
                }
            }

            return Assembly.LoadFile(assemblyTempFilePath);
        }
    }
}