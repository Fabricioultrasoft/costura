using System;
using CosturaVsPackage;

public static class CurrentVersion
{
    public static Version Version
    {
        get { return typeof (FileExporter).Assembly.GetName().Version; }
    }
}