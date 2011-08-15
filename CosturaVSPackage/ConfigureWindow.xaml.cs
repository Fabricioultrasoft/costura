using System.Windows;


public partial class ConfigureWindow
{

    public ConfigureWindow(ConfigureWindowModel model)
    {
        Model = model;
        InitializeComponent();

        DataContext = Model;
    }

    public ConfigureWindowModel Model { get; private set; }

    private void Ok(object sender, RoutedEventArgs e)
    {
        var errors = Model.GetErrors();
        if (errors != null)
        {
            MessageBox.Show(errors, "Errors", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        DialogResult = true;
        Close();
    }


    private void Cancel(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void SetDefaultToolsDirectory(object sender, RoutedEventArgs e)
    {
        Model.ToolsDirectory = @"$(SolutionDir)Tools\";
    }

    private void SetDefaultTargetPath(object sender, RoutedEventArgs e)
    {
        Model.TargetPath = "@(TargetPath)";
    }

}