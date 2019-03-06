using Esri.ArcGISRuntime;
using System;
using System.Diagnostics;
using System.Reflection;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace OfflineWorkflowSample.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        private static string RuntimeVersion = "";

        public SettingsPage()
        {
            InitializeComponent();

            if (String.IsNullOrWhiteSpace(RuntimeVersion))
            {
                var runtimeTypeInfo = typeof(ArcGISRuntimeEnvironment).GetTypeInfo();
                var rtVersion = FileVersionInfo.GetVersionInfo(runtimeTypeInfo.Assembly.Location);
                RuntimeVersion = rtVersion.FileVersion;
                VersionLabelField.Text = RuntimeVersion;
            }
        }
    }
}