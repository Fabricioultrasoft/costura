using System;
using System.IO;
using System.Reflection;

internal static class ILTemplate
{
	public static void Attach()
	{
		var currentDomain = AppDomain.CurrentDomain;
		currentDomain.AssemblyResolve += ResolveAssembly;
	}

    public static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
    {
        var currentDomain = AppDomain.CurrentDomain;
        var assems = currentDomain.GetAssemblies();
        var name = new AssemblyName(args.Name).Name.ToLowerInvariant();
        foreach (var assembly in assems)
        {
            var fullName = assembly.FullName.ToLowerInvariant();
            var indexOf = fullName.IndexOf(',');
            if (indexOf > 1)
            {
                fullName = fullName.Substring(0, indexOf);
            }

            if (fullName == name)
            {
                return assembly;
            }
        }

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

            var pdbName = Path.ChangeExtension(assemblyResourceName, "pdb");
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