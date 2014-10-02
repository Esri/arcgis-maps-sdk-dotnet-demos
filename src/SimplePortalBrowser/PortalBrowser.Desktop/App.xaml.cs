using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PortalBrowser.WPF
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public App()
		{
			//TODO: Set your app client id
			//Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.ClientId = "YOUR CLIENT ID GOES HERE";
			Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.Initialize();
		}
	}
}
