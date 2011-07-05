using System;
using System.Reflection;
using System.Windows;

namespace Costura
{
    internal static class ILTemplate
    {
        public static void Attach()
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            foreach (var resourceName in executingAssembly.GetManifestResourceNames())
            {
                ResolveAssembly(executingAssembly, resourceName);
            }
        }

		private static void ResolveAssembly(Assembly executingAssembly, string resourceName)
		{
			if (!resourceName.StartsWith("Costura."))
			{
				return;
			}
			using (var assemblyStream = executingAssembly.GetManifestResourceStream(resourceName))
			{
				var ap = new AssemblyPart();
				try
				{
					ap.Load(assemblyStream);
				}
				catch (Exception exception)
				{
					throw new Exception(string.Format("Could not load '{0}'", resourceName), exception);
				}
			}
		}
    }
}