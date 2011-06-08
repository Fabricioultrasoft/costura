using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using EnvDTE;
using Microsoft.Build.Framework;
using VsPackageCommon;

namespace CosturaVSPackage
{
    [Export, PartCreationPolicy(CreationPolicy.Shared)]
    public class ConfigureMenuCallback
    {

        FileExporter fileExporter;
        TaskFileReplacer taskFileReplacer;
        CurrentProjectFinder currentProjectFinder;
        FullPathResolver fullPathResolver;
        ExceptionDialog exceptionDialog;

        [ImportingConstructor]
        public ConfigureMenuCallback(CurrentProjectFinder currentProjectFinder, FileExporter fileExporter, TaskFileReplacer taskFileReplacer, FullPathResolver fullPathResolver, ExceptionDialog exceptionDialog)
        {
            this.currentProjectFinder = currentProjectFinder;
            this.fullPathResolver = fullPathResolver;
            this.exceptionDialog = exceptionDialog;
            this.taskFileReplacer = taskFileReplacer;
            this.fileExporter = fileExporter;
        }


        public void ConfigureCallback()
        {
            try
            {
                //http://mrpmorris.blogspot.com/2007/05/convert-absolute-path-to-relative-path.html
                var project = currentProjectFinder.GetCurrentProject();

                var projectReader = new ProjectReader(project.FileName);

                var model = new ConfigureWindowModel();
                var defaulter = new Defaulter();
                defaulter.ToModel(projectReader, model);

                var configureWindow = new ConfigureWindow(model);
                new WindowInteropHelper(configureWindow)
                    {
                        Owner = GetActiveWindow()
                    };
                if (configureWindow.ShowDialog().GetValueOrDefault())
                {
                    Configure(model, project);
                }
            }
            catch (COMException exception)
            {
                exceptionDialog.HandleException(exception);
            }
            catch (Exception exception)
            {
                exceptionDialog.HandleException(exception);
            }
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetActiveWindow();


        void Configure(ConfigureWindowModel model, Project project)
        {

            var directoryInfo = fullPathResolver.GetFullPath(model.ToolsDirectory, project);
            var targetFile = new FileInfo(Path.Combine(directoryInfo.FullName, "Costura.dll"));
            if (!targetFile.Exists || VersionChecker.IsVersionNewer(targetFile))
            {
                if (!fileExporter.ExportTask(directoryInfo))
                {
                    taskFileReplacer.AddFile(directoryInfo);
                }
            }

            var defaulter = new Defaulter();
            var projectInjector = new ProjectInjector
                                      {
                                          ProjectFile = project.FileName
                                      };
            defaulter.FromModel(projectInjector, model);
            projectInjector.Execute();
        }


    }
    public class Defaulter
    {
        public void ToModel(ProjectReader projectReader, ConfigureWindowModel configureWindowModel)
        {
            configureWindowModel.MessageImportance = projectReader.MessageImportance.GetValueOrDefault(MessageImportance.Low);
            configureWindowModel.TargetPath = projectReader.TargetPath;
            configureWindowModel.DeriveTargetPathFromBuildEngine = projectReader.TargetPath == null;
            configureWindowModel.ToolsDirectory = GetValueOrDefault(projectReader.ToolsDirectory, @"$(SolutionDir)Tools\");

        }
        public static string GetValueOrDefault(string str, string defaultValue)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return defaultValue;
            }
            return str;
        }

        public void FromModel(ProjectInjector projectInjector, ConfigureWindowModel configureWindowModel)
        {

            if (!configureWindowModel.DeriveTargetPathFromBuildEngine)
            {
                projectInjector.TargetPath = configureWindowModel.TargetPath;
            }


            if (configureWindowModel.MessageImportance != MessageImportance.Low)
            {
                projectInjector.MessageImportance = configureWindowModel.MessageImportance;
            }
            projectInjector.ToolsDirectory = configureWindowModel.ToolsDirectory;

        }
    }
}