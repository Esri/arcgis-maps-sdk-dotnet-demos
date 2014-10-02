using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace RoutingSample.Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
		public App()
		{
			Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.ClientId = "1234567890123456";
		}
    }
}
