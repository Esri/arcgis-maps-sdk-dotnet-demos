using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Http;
using Esri.ArcGISRuntime.Security;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ArcGISMapViewer
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            UnhandledException += (sender, e) =>
            {
                if (global::System.Diagnostics.Debugger.IsAttached)
                {
                    Debug.WriteLine($"{e.Exception.GetType().FullName}: {e.Message} ({e.Exception.Message})");
                    foreach (var key in e.Exception.Data.Keys)
                    {
                        Debug.WriteLine($"    {key} = {e.Exception.Data[key]}");
                    }
                }
            };

            if (WinUIEx.WebAuthenticator.CheckOAuthRedirectionActivation())
                return;
            this.InitializeComponent();
            // Initialize the ArcGIS Maps SDK runtime before any components are created.
            ArcGISRuntimeEnvironment.Initialize(config => config
              .ConfigureAuthentication(auth => auth
                 .UseDefaultChallengeHandler() // Use the default authentication dialog
               )
            );
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new Windows.StartupWindow();
            m_window.Activate();
        }

        private Window? m_window;
    }
}
