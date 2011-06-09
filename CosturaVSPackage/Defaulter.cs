﻿using Microsoft.Build.Framework;

namespace CosturaVSPackage
{
    public class Defaulter
    {
        public void ToModel(ProjectReader projectReader, ConfigureWindowModel configureWindowModel)
        {
            configureWindowModel.MessageImportance = projectReader.MessageImportance.GetValueOrDefault(MessageImportance.Low);
            configureWindowModel.TargetPath = projectReader.TargetPath;
            configureWindowModel.Overwrite = projectReader.Overwrite.GetValueOrDefault(true);
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


            if (!configureWindowModel.Overwrite)
            {
                projectInjector.Overwrite = false;
            }


            if (configureWindowModel.MessageImportance != MessageImportance.Low)
            {
                projectInjector.MessageImportance = configureWindowModel.MessageImportance;
            }
            projectInjector.ToolsDirectory = configureWindowModel.ToolsDirectory;

        }
    }
}