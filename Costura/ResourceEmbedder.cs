using System.ComponentModel.Composition;
using System.IO;
using Mono.Cecil;
using WeavingCommon;

namespace Costura
{
    [Export, PartCreationPolicy(CreationPolicy.Shared)]
	public class ResourceEmbedder
	{
        DependencyFinder dependencyFinder;
        ModuleReader moduleReader;

        [ImportingConstructor]
        public ResourceEmbedder(DependencyFinder dependencyFinder, ModuleReader moduleReader)
        {
            this.dependencyFinder = dependencyFinder;
            this.moduleReader = moduleReader;
        }

        public void Execute()
		{
            foreach (var dependency in dependencyFinder.Dependencies)
            {
                var fullPath = Path.GetFullPath(dependency);
                Embedd(fullPath);
                var pdbFullPath = Path.ChangeExtension(fullPath, "pdb");
                if (File.Exists(pdbFullPath))
                {
                    Embedd(pdbFullPath);
                }
            }
		}

        private void Embedd(string fullPath)
        {
            var resource = new EmbeddedResource("Costura." + Path.GetFileName(fullPath), ManifestResourceAttributes.Private, File.ReadAllBytes(fullPath));
            moduleReader.Module.Resources.Add(resource);
        }

	}
}