using System.ComponentModel.Composition;
using System.IO;
using VsPackageCommon;

[Export, PartCreationPolicy(CreationPolicy.Shared)]
public class FileExporter
{
    private ResourceExporter resourceExporter;

    public FileExporter()
    {
        resourceExporter = new ResourceExporter();
    }

    [ImportingConstructor]
    public FileExporter(ResourceExporter resourceExporter)
    {
        this.resourceExporter = resourceExporter;
    }

    public bool ExportTask(string directory)
    {
        return resourceExporter.Export("Costura.dll", new FileInfo(Path.Combine(directory, "Costura.dll")));
    }

    public bool ExportTask(DirectoryInfo directory)
    {
        return resourceExporter.Export("Costura.dll", new FileInfo(Path.Combine(directory.FullName, "Costura.dll")));
    }

}