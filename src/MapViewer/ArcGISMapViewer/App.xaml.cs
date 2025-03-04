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
#if DEBUG
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
#endif

#if !DISABLE_XAML_GENERATED_MAIN // With custom main, we'd rather want to do this code in main
            if (WinUIEx.WebAuthenticator.CheckOAuthRedirectionActivation())
                return;
            fss = SimpleSplashScreen.ShowDefaultSplashScreen();
#endif
            this.InitializeComponent();
            // Initialize the ArcGIS Maps SDK runtime before any components are created.
            ArcGISRuntimeEnvironment.Initialize(config => config
              .ConfigureAuthentication(auth => auth
                 .UseDefaultChallengeHandler() // Use the default authentication dialog
               )
            );
        }

        internal SimpleSplashScreen? SimpleSplashScreen { get; set; }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new Windows.StartupWindow();
            m_window.Activated += StartupWindow_Activated;
            m_window.Activate();
            SimpleSplashScreen?.Hide(TimeSpan.FromSeconds(1));
            SimpleSplashScreen = null;
        }

        private void StartupWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            if (m_window is not null)
                m_window.Activated -= StartupWindow_Activated;
        }

        private Window? m_window;
    }

#if DISABLE_XAML_GENERATED_MAIN
    /// <summary>
    /// Program class
    /// </summary>
    public static class Program
    {
        [global::System.STAThreadAttribute]
        static void Main(string[] args)
        {
            if (WebAuthenticator.CheckOAuthRedirectionActivation(true))
                return;
            var splash = SimpleSplashScreen.ShowDefaultSplashScreen();
            global::WinRT.ComWrappersSupport.InitializeComWrappers();
            global::Microsoft.UI.Xaml.Application.Start((p) => {
                var context = new global::Microsoft.UI.Dispatching.DispatcherQueueSynchronizationContext(global::Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread());
                global::System.Threading.SynchronizationContext.SetSynchronizationContext(context);
                new App() { SimpleSplashScreen = splash };
            });
        }
    }
#endif
}
