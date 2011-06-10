﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Build.Framework;

namespace CosturaVSPackage
{
    public class ProjectInjector
    {
        public string TargetPath { set; get; }
        public string ToolsDirectory { set; get; }
        public MessageImportance? MessageImportance { set; get; }
        public bool? Overwrite { set; get; }
        public string ProjectFile { set; get; }
        public bool? DeleteReferences { get; set; }

        XDocument xDocument;

        public void Execute()
        {
            new FileInfo(ProjectFile).IsReadOnly = false;
            xDocument = XDocument.Load(ProjectFile);
            InjectUsingEmbedTask();
            InjectEmbedTask();
            xDocument.Save(ProjectFile);
        }

        void InjectEmbedTask()
        {

            var target = GetOrCreateAfterBuildTarget();

            var embedTask = target.BuildDescendants("Costura.EmbedTask").FirstOrDefault();
            if (embedTask != null)
            {
                embedTask.Remove();
            }


            var xAttributes = new List<XAttribute>();
            if (Overwrite != null)
            {
                xAttributes.Add(new XAttribute("Overwrite", Overwrite));
            }
            if (DeleteReferences != null)
            {
                xAttributes.Add(new XAttribute("DeleteReferences", DeleteReferences));
            }
            if (MessageImportance != null)
            {
                xAttributes.Add(new XAttribute("MessageImportance", MessageImportance));
            }
            if (!string.IsNullOrWhiteSpace(TargetPath))
            {
                xAttributes.Add(new XAttribute("TargetPath", TargetPath));
            }
            target.Add(new XElement(XDocumentExtensions.BuildNamespace + "Costura.EmbedTask", xAttributes.ToArray()));
        }


        XElement GetOrCreateAfterBuildTarget()
        {
            var target = xDocument.BuildDescendants("Target")
                .Where(x => string.Equals((string)x.Attribute("Name"), "AfterBuild", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (target == null)
            {
                target = new XElement(XDocumentExtensions.BuildNamespace + "Target", new XAttribute("Name", "AfterBuild"));
                xDocument.Root.Add(target);
            }
            return target;
        }

        void InjectUsingEmbedTask()
        {
            var count = xDocument.BuildDescendants("UsingTask")
                .Where(x => (string)x.Attribute("TaskName") == "Costura.EmbedTask").ToList();
            foreach (var xElement in count)
            {
                xElement.Remove();
            }

            xDocument.Root.Add(
                new XElement(XDocumentExtensions.BuildNamespace + "UsingTask",
                             new XAttribute("TaskName", "Costura.EmbedTask"),
                             new XAttribute("AssemblyFile", Path.Combine(ToolsDirectory, @"Costura.dll"))));
        }

    }
}