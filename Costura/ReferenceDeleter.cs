using System;
using System.ComponentModel.Composition;
using System.IO;
using WeavingCommon;

namespace Costura
{
    [Export, PartCreationPolicy(CreationPolicy.Shared)]
    public class ReferenceDeleter
    {
        DependencyFinder dependencyFinder;
        EmbedTask embedTask;
        Logger logger;

        [ImportingConstructor]
        public ReferenceDeleter(DependencyFinder dependencyFinder, EmbedTask embedTask, Logger logger)
        {
            this.dependencyFinder = dependencyFinder;
            this.embedTask = embedTask;
            this.logger = logger;
        }

        public void Execute()
        {
            if (!embedTask.DeleteReferences)
            {
                return;
            }
            var directoryName = Path.GetDirectoryName(embedTask.TargetPath);
            foreach (var dependency in dependencyFinder.Dependencies)
            {
                foreach (var fileToDelete in Directory.EnumerateFiles(directoryName, Path.GetFileNameWithoutExtension(dependency) + "*"))
                {
                    try
                    {
                        File.Delete(fileToDelete);
                    }
                    catch (Exception exception)
                    {
                        logger.LogWarning(string.Format("Tried to delete '{0}' but could not due to the following exception: {1}", fileToDelete, exception));
                    }   
                }
            }
        }
    }
}