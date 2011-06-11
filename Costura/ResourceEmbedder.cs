using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using Mono.Cecil;
using WeavingCommon;

namespace Costura
{
    [Export, PartCreationPolicy(CreationPolicy.Shared)]
	public class ResourceEmbedder: IDisposable
	{
        DependencyFinder dependencyFinder;
        ModuleReader moduleReader;
        EmbedTask embedTask;
        List<Stream> streams;

            [ImportingConstructor]
        public ResourceEmbedder(DependencyFinder dependencyFinder, ModuleReader moduleReader, EmbedTask embedTask)
        {
                streams = new List<Stream>();
            this.dependencyFinder = dependencyFinder;
            this.moduleReader = moduleReader;
            this.embedTask = embedTask;
        }

        public void Execute()
		{
            foreach (var dependency in dependencyFinder.Dependencies)
            {
                var fullPath = Path.GetFullPath(dependency);
                Embedd(fullPath);
                if (!embedTask.IncludeDebugSymbols)
                {
                    continue;
                }
                var pdbFullPath = Path.ChangeExtension(fullPath, "pdb");
                if (File.Exists(pdbFullPath))
                {
                    Embedd(pdbFullPath);
                }
            }
		}

        private void Embedd(string fullPath)
        {
            var fileStream = File.OpenRead(fullPath);
            streams.Add(fileStream);
            var resource = new EmbeddedResource("Costura." + Path.GetFileName(fullPath), ManifestResourceAttributes.Private, fileStream);
            moduleReader.Module.Resources.Add(resource);
        }

        public void Dispose()
        {
            foreach (var stream in streams)
            {
                stream.Dispose();
            }
        }
	}
}