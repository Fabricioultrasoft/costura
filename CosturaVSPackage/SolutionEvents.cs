﻿using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

[Export, PartCreationPolicy(CreationPolicy.Shared)]
public class SolutionEvents : IVsSolutionEvents
{
    TaskFileProcessor taskFileProcessor;
    ExceptionDialog exceptionDialog;
    ErrorDisplayer errorDisplayer;
    AllProjectFinder allProjectFinder;

    [ImportingConstructor]
    public SolutionEvents(TaskFileProcessor taskFileProcessor, ExceptionDialog exceptionDialog, ErrorDisplayer errorDisplayer, AllProjectFinder allProjectFinder)
    {
        this.taskFileProcessor = taskFileProcessor;
        this.exceptionDialog = exceptionDialog;
        this.errorDisplayer = errorDisplayer;
        this.allProjectFinder = allProjectFinder;
    }

    public void RegisterSolutionEvents()
    {

        uint cookie;
        var vsSolution = (IVsSolution) ServiceProvider.GlobalProvider.GetService(typeof (IVsSolution));
        vsSolution.AdviseSolutionEvents(this, out cookie);
    }


    public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
    {
        try
        {
            foreach (var project in allProjectFinder.GetAllProjects())
            {
                try
                {
                    taskFileProcessor.ProcessTaskFile(project);
                }
                catch (Exception exception)
                {
                    errorDisplayer.ShowError(string.Format("Costura: An exception occured while trying to process {0}.\r\nException: {1}.", project.FullName, exception));
                }
            }
        }
        catch (Exception exception)
        {
            exceptionDialog.HandleException(exception);
        }
        return VSConstants.S_OK;
    }

    public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
    {
        return VSConstants.S_OK;
    }

    public int OnBeforeCloseSolution(object pUnkReserved)
    {
        return VSConstants.S_OK;
    }

    public int OnAfterCloseSolution(object pUnkReserved)
    {
        return VSConstants.S_OK;
    }

    public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
    {
        return VSConstants.S_OK;
    }

    public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
    {
        return VSConstants.S_OK;
    }

    public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
    {
        return VSConstants.S_OK;
    }

    public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
    {
        return VSConstants.S_OK;
    }

    public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
    {
        return VSConstants.S_OK;
    }

    public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
    {
        return VSConstants.S_OK;
    }
}