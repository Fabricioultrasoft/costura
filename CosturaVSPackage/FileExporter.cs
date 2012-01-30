using System.ComponentModel.Composition;
using System.IO;

[Export, PartCreationPolicy(CreationPolicy.Shared)]
public class FileExporter
{
    ResourceExporter resourceExporter;

    public FileExporter()
    {
    }

    [ImportingConstructor]
    public FileExporter(ResourceExporter resourceExporter)
    {
        this.resourceExporter = resourceExporter;
    }

    public virtual bool ExportTask(string directory)
    {
        return resourceExporter.Export("Costura.dll", new FileInfo(Path.Combine(directory, "Costura.dll")));
    }

    public virtual bool ExportTask(DirectoryInfo directory)
    {
        return resourceExporter.Export("Costura.dll", new FileInfo(Path.Combine(directory.FullName, "Costura.dll")));
    }

}