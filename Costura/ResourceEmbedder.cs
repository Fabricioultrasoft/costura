using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Mono.Cecil;
using WeavingCommon;

namespace Costura
{
    [Export, PartCreationPolicy(CreationPolicy.Shared)]
	public class ResourceEmbedder
	{
        ModuleReader moduleReader;
        EmbedTask embedTask;
        IBuildEngine buildEngine;

        [ImportingConstructor]
		public ResourceEmbedder(ModuleReader moduleReader, EmbedTask embedTask, IBuildEngine  buildEngine)
        {
            this.moduleReader = moduleReader;
            this.embedTask = embedTask;
            this.buildEngine = buildEngine;
        }

        public void Execute()
		{
            var dependencies = GetCopyLocalFiles();
            foreach (var dependency in dependencies)
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
            var resource = new EmbeddedResource("WeavingTask." + Path.GetFileName(fullPath), ManifestResourceAttributes.Private, File.ReadAllBytes(fullPath));
            moduleReader.Module.Resources.Add(resource);
        }

        private List<string> GetCopyLocalFiles()
        {
            if (embedTask.ReferenceCopyLocalPaths == null)
            {
                return buildEngine.GetEnvironmentVariable("ReferenceCopyLocalPaths", true)
                    .Where(x => x.EndsWith(".dll") || x.EndsWith(".exe"))
                    .ToList();
            }
            return embedTask.ReferenceCopyLocalPaths;
        }
	}
}