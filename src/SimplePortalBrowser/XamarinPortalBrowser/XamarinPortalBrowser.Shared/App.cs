using Esri.ArcGISRuntime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XamarinPortalBrowser
{
	public class App : Xamarin.Forms.Application
	{
		public App ()
		{
		    // Deployed applications must be licensed at the Basic level or greater (https://developers.arcgis.com/licensing).
            // To enable Basic level functionality set the Client ID property before initializing the ArcGIS Runtime.
            // ArcGISRuntimeEnvironment.ClientId = "<Your Client ID>";

            // Initialize the ArcGIS Runtime before any components are created.
            ArcGISRuntimeEnvironment.Initialize();

            // Standard level functionality can be enabled once the ArcGIS Runtime is initialized.                
            // To enable Standard level functionality you must either:
            // 1. Allow the app user to authenticate with ArcGIS Online or Portal for ArcGIS then call the set license method with their license info.
            // ArcGISRuntimeEnvironment.License.SetLicense(LicenseInfo object returned from an ArcGIS Portal instance)
            // 2. Call the set license method with a license string obtained from Esri Customer Service or your local Esri distributor.
            // ArcGISRuntimeEnvironment.License.SetLicense("<Your License String or Strings (extensions) Here>");

            // The root page of your application
            MainPage = new StartPage();
		}

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}
