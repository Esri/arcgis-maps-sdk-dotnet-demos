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

        private async void OpenInAgol_Clicked(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                try
                {
                    if (vm.Map.Item is PortalItem portalItem)
                    {
                        await Launcher.LaunchUriAsync(new Uri($"https://www.arcgis.com/home/item.html?id={portalItem.ItemId}"));
                    }
                    else if (vm.Map.Item is LocalItem localItem)
                    {
                        await Launcher.LaunchUriAsync(new Uri($"https://www.arcgis.com/home/item.html?id={localItem.OriginalPortalItemId}"));
                    }
                    else
                    {
                        vm.ShowMessage("Couldn't open item in ArcGIS Online.");
                    }
                }
                catch (Exception)
                {
                    vm.ShowMessage("Couldn't open item in ArcGIS Online.");
                }
            }
        }
    }
}