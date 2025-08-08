using System.Windows;
using Esri.ArcGISRuntime;

namespace DemoApplicationAccessibility
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            {
                base.OnStartup(e);
#warning Add your ArcGIS API key here to enable use of Esri's basemaps and geocoding services.
                ArcGISRuntimeEnvironment.ApiKey = "";
            }
        }
    }
}
