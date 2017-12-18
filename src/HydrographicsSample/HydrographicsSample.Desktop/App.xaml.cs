using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace HydrographicsSample
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            if (!Directory.Exists("ArcGISRuntime100.2")) // This folder is normally deployed automatically when building
            {
                Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.InstallPath = @"c:\applications\output\windesktop_api\bin";
            }
            Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.Initialize();
        }
    }
}
