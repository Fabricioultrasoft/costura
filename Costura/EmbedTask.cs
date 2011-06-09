﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using WeavingCommon;
using Logger = WeavingCommon.Logger;

namespace Costura
{
    public class EmbedTask : Task, IConfig
    {
        public bool Overwrite { set; get; }
        public string TargetPath { set; get; }
        public string MessageImportance { set; get; }
        public string References { get; set; }
        public Exception Exception { get; set; }
        public string KeyFilePath { get; set; }
        //Hack:
        public List<string> ReferenceCopyLocalPaths { get; set; }
        Logger logger;

        public EmbedTask()
        {
            MessageImportance = "Low";
            Overwrite = true;
        }

        public override bool Execute()
        {
            BuildEngine.LogMessageEvent(new BuildMessageEventArgs("EmbedTask Executing (Change MessageImportance to get more or less info)", "", "EmbedTask", Microsoft.Build.Framework.MessageImportance.High));

            var stopwatch = Stopwatch.StartNew();

            try
            {
                logger = new Logger
                {
                    SenderName = "Costura",
                    BuildEngine = BuildEngine,
                };
                logger.Initialise(MessageImportance);
                Inner();
            }
            catch (Exception exception)
            {
                HandleException(exception);
            }

            finally
            {
                stopwatch.Stop();
                logger.Flush();
                BuildEngine.LogMessageEvent(new BuildMessageEventArgs(string.Format("\tFinished ({0}ms)", stopwatch.ElapsedMilliseconds), "", "EmbedTask", Microsoft.Build.Framework.MessageImportance.High));
            }
            return !logger.ErrorHasBeenRaised;
        }

        private void HandleException(Exception exception)
        {
            Exception = exception;
            if (exception is WeavingException)
            {
                logger.LogError(exception.Message);
                return;
            }

            logger.LogError(string.Format("Unhandled exception occurred {0}", exception));
        }


        void Inner()
        {
            using (var catalog = CatalogBuilder.GetCatalog())
            using (var container = new CompositionContainer(catalog))
            {
                container.ComposeExportedValue<IConfig>(this);
                container.ComposeExportedValue(this);
                container.ComposeExportedValue(BuildEngine);
                container.ComposeExportedValue(logger);
                container.GetExportedValue<TargetPathFinder>().Execute("TargetPath");

                logger.LogMessage(string.Format("\tTargetPath: {0}", TargetPath));


                container.GetExportedValue<AssemblyResolver>().Execute();
                container.GetExportedValue<ModuleReader>().Execute();

                var fileChangedChecker = container.GetExportedValue<FileChangedChecker>();
                if (!fileChangedChecker.ShouldStart())
                {
                    return;
                }

                container.GetExportedValue<MsCoreReferenceFinder>().Execute();

                container.GetExportedValue<AssemblyLoaderImporter>().Execute();
                container.GetExportedValue<ModuleLoaderImporter>().Execute();
                container.GetExportedValue<ResourceEmbedder>().Execute();
                
                container.GetExportedValue<ProjectKeyReader>().Execute();
                var savePath = GetSavePath();
                container.GetExportedValue<ModuleWriter>().Execute(savePath);
                
            }
        }

        string GetSavePath()
        {
            if (Overwrite)
            {
                return TargetPath;
            }
            var fileInfo = new FileInfo(TargetPath);
            var directoryPath = Path.Combine(fileInfo.DirectoryName, "Merged");
            Directory.CreateDirectory(directoryPath);
            return Path.Combine(directoryPath , fileInfo.Name);
        }
    }

}