using Esri.ArcGISRuntime;
using System;
using System.Diagnostics;
using System.Reflection;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace OfflineWorkflowSample.Views
{
    public sealed partial class SettingsPage : Page
    {
        private static string _runtimeVersion = "";

        public SettingsPage()
        {
            InitializeComponent();

            if (String.IsNullOrWhiteSpace(_runtimeVersion))
            {
                var runtimeTypeInfo = typeof(ArcGISRuntimeEnvironment).GetTypeInfo();
                var rtVersion = FileVersionInfo.GetVersionInfo(runtimeTypeInfo.Assembly.Location);
                _runtimeVersion = rtVersion.FileVersion;
            }
            VersionLabelField.Text = _runtimeVersion;
        }
    }
}