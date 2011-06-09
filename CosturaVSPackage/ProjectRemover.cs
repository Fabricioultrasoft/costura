using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace CosturaVSPackage
{
    public class ProjectRemover
    {
        XDocument xDocument;

        public ProjectRemover(string projectFile)
        {
            new FileInfo(projectFile).IsReadOnly = false;
            xDocument = XDocument.Load(projectFile);
            RemoveUsingTask();
            RemoveWeavingTask();
            xDocument.Save(projectFile);
        }

        void RemoveWeavingTask()
        {
            xDocument.BuildDescendants("Target")
                .Where(x => string.Equals((string)x.Attribute("Name"), "AfterBuild", StringComparison.InvariantCultureIgnoreCase))
                .Descendants(XDocumentExtensions.BuildNamespace + "Costura.EmbedTask").Remove();
        }

        void RemoveUsingTask()
        {
            xDocument.BuildDescendants("UsingTask")
                .Where(x => (string)x.Attribute("TaskName") == "Costura.EmbedTask").Remove();
        }

    }
}