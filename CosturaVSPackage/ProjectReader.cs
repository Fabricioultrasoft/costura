using System;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Build.Framework;

namespace CosturaVSPackage
{
    public class ProjectReader
    {
        string projectFile;
        public string ToolsDirectory { set; get; }
        public string TargetPath { set; get; }
        public MessageImportance? MessageImportance { get; set; }

        public ProjectReader(string projectFile)
        {
            this.projectFile = projectFile;
            SetWeavingProps();
            ToolsDirectory = GetToolsDirectory(projectFile);
        }

        public static string GetToolsDirectory(string projectFile)
        {
            var xDocument = XDocument.Load(projectFile);
            var elements =
                from el in xDocument.BuildDescendants("UsingTask")
                where (string)el.Attribute("TaskName") == "Costura.EmbedTask"
                select el.Attribute("AssemblyFile");
            var firstOrDefault = elements.FirstOrDefault();
            if (firstOrDefault != null)
            {
                var value = firstOrDefault.Value;
                return value.Substring(0, value.IndexOf("Costura.dll"));
            }
            return null;
        }
        void SetWeavingProps()
        {
            var xDocument = XDocument.Load(projectFile);
            var children =
                from target in xDocument.BuildDescendants("Target")
                let targetName = (string)target.Attribute("Name")
                where string.Equals(targetName, "AfterCompile", StringComparison.InvariantCultureIgnoreCase)

                from weavingTask in target.BuildDescendants("Costura.EmbedTask")
                select new
                           {
                               TargetPath = (string)weavingTask.Attribute("TargetPath"),
                               MessageImportance = ConvertToEnum((string)weavingTask.Attribute("MessageImportance")),
                           };

            var first = children.FirstOrDefault();
            if (first == null)
            {
                return;
            }
            TargetPath = first.TargetPath;
            MessageImportance = first.MessageImportance;
        }
        public static bool? ToBool(XAttribute attribute)
        {
            if (attribute == null)
            {
                return null;
            }
            return bool.Parse(attribute.Value);
        }

        static MessageImportance? ConvertToEnum(string messageImportance)
        {
            if (!string.IsNullOrWhiteSpace(messageImportance))
            {
                MessageImportance messageImportanceEnum;
                if (Enum.TryParse(messageImportance, out messageImportanceEnum))
                {
                    return messageImportanceEnum;
                }
            }
            return null;
        }
    }
}