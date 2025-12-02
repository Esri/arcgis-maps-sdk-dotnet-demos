using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ArcGISMapViewer.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AboutPage : Page
    {
        public AboutPage()
        {
            this.InitializeComponent();

            var package = global::Windows.ApplicationModel.Package.Current;
            RuntimeInfo.Text += $"App version: {package.Id.Version.Major}.{package.Id.Version.Minor}.{package.Id.Version.Build}.{package.Id.Version.Revision}";
            var assembly = typeof(Esri.ArcGISRuntime.ArcGISRuntimeEnvironment).Assembly;
            var attr = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();            
            RuntimeInfo.Text += $"\nArcGIS Maps SDK version: {attr?.Version ?? assembly.GetName().Version?.ToString()}";
            assembly = typeof(Application).Assembly;
            attr = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>(); 
            RuntimeInfo.Text += $"\nWinUI version: {attr?.Version ?? assembly.GetName().Version?.ToString()}";
            RuntimeInfo.Text += $"\n.NET version: {Environment.Version}";
        }
    }
}
