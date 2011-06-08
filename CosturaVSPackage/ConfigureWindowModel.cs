﻿using System.ComponentModel;
using System.Text;
using Microsoft.Build.Framework;

namespace CosturaVSPackage
{
    public class ConfigureWindowModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        string toolsDirectory;
        public string ToolsDirectory
        {
            get { return toolsDirectory; }
            set
            {
                toolsDirectory = value;
                OnPropertyChanged("ToolsDirectory");
            }
        }

        string targetPath;
        public string TargetPath
        {
            get { return targetPath; }
            set
            {
                targetPath = value;
                OnPropertyChanged("TargetPath");
            }
        }

 
        bool deriveTargetPathFromBuildEngine;
        public bool DeriveTargetPathFromBuildEngine
        {
            get { return deriveTargetPathFromBuildEngine; }
            set
            {
                deriveTargetPathFromBuildEngine = value;
                OnPropertyChanged("DeriveTargetPathFromBuildEngine");
            }
        }

        MessageImportance messageImportance;
        public MessageImportance MessageImportance
        {
            get { return messageImportance; }
            set
            {
                messageImportance = value;
                OnPropertyChanged("MessageImportance");
            }
        }


        string version;
        public string Version
        {
            get { return version; }
            set
            {
                version = value;
                OnPropertyChanged("Version");
            }
        }

        public ConfigureWindowModel()
        {
            Version = CurrentVersion.Version.ToString();
        }

        void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public string GetErrors()
        {
            var stringBuilder = new StringBuilder();
            if (!DeriveTargetPathFromBuildEngine)
            {
                if (string.IsNullOrWhiteSpace(TargetPath))
                {
                    stringBuilder.AppendLine("TargetPath is required if you have selected DeriveTargetPathFromBuildEngine.");
                }
            }
            if (string.IsNullOrWhiteSpace(ToolsDirectory))
            {
                stringBuilder.AppendLine("ToolsDirectory is required.");
            }
            if (stringBuilder.Length == 0)
            {
                return null;
            }
            return stringBuilder.ToString();
        }

    }
}