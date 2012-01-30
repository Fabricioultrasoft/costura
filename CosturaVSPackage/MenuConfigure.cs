using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using EnvDTE;
using Microsoft.VisualStudio.Shell;


[Export, PartCreationPolicy(CreationPolicy.Shared)]
public class MenuConfigure
{
    OleMenuCommand configureCommand;
    OleMenuCommand disableCommand;
    CurrentProjectFinder currentProjectFinder;
    ExceptionDialog exceptionDialog;
    ConfigureMenuCallback configureMenuCallback;
    DisableMenuConfigure disableMenuConfigure;
    IMenuCommandService menuCommandService;

    [ImportingConstructor]
    public MenuConfigure(CurrentProjectFinder currentProjectFinder, ExceptionDialog exceptionDialog, ConfigureMenuCallback configureMenuCallback, DisableMenuConfigure disableMenuConfigure, IMenuCommandService menuCommandService)
    {
        this.exceptionDialog = exceptionDialog;
        this.configureMenuCallback = configureMenuCallback;
        this.disableMenuConfigure = disableMenuConfigure;
        this.menuCommandService = menuCommandService;
        this.currentProjectFinder = currentProjectFinder;
    }

    public void RegisterMenus()
    {
        var vsPackageCmdSet = new Guid("5ce0365b-947a-4dca-b016-ca823deaad0b");
        var configureCommandId = new CommandID(vsPackageCmdSet, 1);
        configureCommand = new OleMenuCommand(delegate { configureMenuCallback.ConfigureCallback(); }, configureCommandId);
        configureCommand.BeforeQueryStatus += delegate { CommandStatusCheck(); };
        menuCommandService.AddCommand(configureCommand);

        var disableCommandId = new CommandID(vsPackageCmdSet, 2);
        disableCommand = new OleMenuCommand(delegate { disableMenuConfigure.DisableCallback(); }, disableCommandId)
                             {
                                 Enabled = false
                             };
        disableCommand.BeforeQueryStatus += delegate { CommandStatusCheck(); };
        menuCommandService.AddCommand(disableCommand);
    }

    void CommandStatusCheck()
    {
        try
        {
            disableCommand.Enabled = true;
            configureCommand.Enabled = true;
            var project = currentProjectFinder.GetCurrentProject();
            if (project == null)
            {
                disableCommand.Enabled = false;
                configureCommand.Enabled = false;
                return;
            }
            disableCommand.Enabled = ContainsEmbedTask(project);
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

    static bool ContainsEmbedTask(Project project)
    {
        string fullName;
        try
        {
            fullName = project.FullName;
        }
        catch (NotImplementedException)
        {
            //HACK: can happen during an upgrade from VS 2008
            return false;
        }
        try
        {
            //HACK: for when VS incorrectly calls configure when no project is avaliable
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return false;
            }
            var xDocument = XDocument.Load(fullName);
            var target = xDocument.BuildDescendants("Target")
                .Where(x => string.Equals((string) x.Attribute("Name"), "AfterBuild", StringComparison.InvariantCultureIgnoreCase)
                ).FirstOrDefault();
            if (target == null)
            {
                return false;
            }
            return target.BuildDescendants("Costura.EmbedTask").Count() > 0;
        }
        catch (Exception exception)
        {
            throw new Exception(string.Format("Could not check project '{0}' for weaving task", fullName), exception);
        }
    }
}