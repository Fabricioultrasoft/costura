using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Build.Framework;
using WeavingCommon;

namespace Costura
{
    [Export, PartCreationPolicy(CreationPolicy.Shared)]
	public class DependencyFinder
	{
        EmbedTask embedTask;
        IBuildEngine buildEngine;
        BuildEnginePropertyExtractor buildEnginePropertyExtractor;
        public List<string> Dependencies;

        [ImportingConstructor]
        public DependencyFinder(EmbedTask embedTask, IBuildEngine buildEngine, BuildEnginePropertyExtractor buildEnginePropertyExtractor)
        {
            this.embedTask = embedTask;
            this.buildEngine = buildEngine;
            this.buildEnginePropertyExtractor = buildEnginePropertyExtractor;
        }

        public void Execute()
		{
            if (embedTask.ReferenceCopyLocalPaths == null)
            {
                Dependencies = buildEnginePropertyExtractor.GetEnvironmentVariable("ReferenceCopyLocalPaths", false)
                    .Where(x => x.EndsWith(".dll") || x.EndsWith(".exe"))
                    .ToList();
                return;
            }
            Dependencies = embedTask.ReferenceCopyLocalPaths;
        }
	}
}