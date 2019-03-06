using Esri.ArcGISRuntime.Portal;
using OfflineWorkflowsSample;
using System;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace OfflineWorkflowSample.Controls
{
    public sealed partial class SceneWithTools : UserControl
    {
        public SceneWithTools()
        {
            InitializeComponent();
        }

        private void MenuButtonClicked(object sender, RoutedEventArgs e)
        {
            MapLegendSplitView.IsPaneOpen = !MapLegendSplitView.IsPaneOpen;
        }
    }
}