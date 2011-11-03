using System;

public static class CurrentVersion
{
    public static Version Version
    {
        get { return typeof (FileExporter).Assembly.GetName().Version; }
    }
}