﻿using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using VsPackageCommon;

[Export, PartCreationPolicy(CreationPolicy.Shared)]
public class DisableMenuConfigure
{
    private CurrentProjectFinder currentProjectFinder;
    private ErrorDisplayer errorDisplayer;
    private ExceptionDialog exceptionDialog;

    [ImportingConstructor]
    public DisableMenuConfigure(CurrentProjectFinder currentProjectFinder, ErrorDisplayer errorDisplayer, ExceptionDialog exceptionDialog)
    {
        this.exceptionDialog = exceptionDialog;
        this.errorDisplayer = errorDisplayer;
        this.currentProjectFinder = currentProjectFinder;
    }

    public void DisableCallback()
    {
        try
        {
            var project = currentProjectFinder.GetCurrentProject();
            errorDisplayer.ShowInfo(string.Format("Costura: Removed from the project '{0}'. However no binary files will be removed in case they are being used by other projects.", project.Name));
            new ProjectRemover(project.FullName);
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

}