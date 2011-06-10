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
        public List<string> Dependencies { get; set; }

        [ImportingConstructor]
		public DependencyFinder(EmbedTask embedTask, IBuildEngine  buildEngine)
        {
            this.embedTask = embedTask;
            this.buildEngine = buildEngine;
        }

        public void Execute()
		{
            if (embedTask.ReferenceCopyLocalPaths == null)
            {
                Dependencies = buildEngine.GetEnvironmentVariable("ReferenceCopyLocalPaths", false)
                    .Where(x => x.EndsWith(".dll") || x.EndsWith(".exe"))
                    .ToList();
                return;
            }
            Dependencies = embedTask.ReferenceCopyLocalPaths;
        }
	}
}