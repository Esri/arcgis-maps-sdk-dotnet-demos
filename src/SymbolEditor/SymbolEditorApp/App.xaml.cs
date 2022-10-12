using Esri.ArcGISRuntime;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SymbolEditorApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                // Deployed applications must be licensed at the Lite level or greater. 
                // See https://developers.arcgis.com/licensing for further details.

                // Pull license key listed in App.xaml
                var licenseKey = Application.Current.Resources["ArcGISRuntimeLicenseKey"] as string;
                if(!string.IsNullOrEmpty(licenseKey) && licenseKey != "INSERT_KEY_HERE")
                {
                    ArcGISRuntimeEnvironment.SetLicense(licenseKey);
                }

#warning Supply an API key in App.xaml.cs, then delete this line
                // Create an API key at https://developers.arcgis.com/api-keys/, then use it to replace YOUR_API_KEY below.
                ArcGISRuntimeEnvironment.ApiKey = "YOUR_API_KEY";


                // Initialize the ArcGIS Runtime before any components are created.
                ArcGISRuntimeEnvironment.Initialize();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "ArcGIS Runtime initialization failed.");

                // Exit application
                this.Shutdown();
            }
        }
    }
}
