using System.Windows;

namespace PortalBrowser.WPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // TODO: Set your license and API keys
        Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.Initialize();
    }
}
