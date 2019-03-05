using System;
using System.Linq;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using OfflineWorkflowsSample;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace OfflineWorkflowSample
{
    public sealed partial class MapWithTools : UserControl
    {
        private MainViewModel _vm => (MainViewModel)Application.Current.Resources[nameof(MainViewModel)];
        public MapWithTools()
        {
            this.InitializeComponent();
        }

        private async void Compass_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // When the compass is tapped, reset map rotation.
            await MyMapView.SetViewpointRotationAsync(0);
        }

        private void MenuButtonClicked(object sender, RoutedEventArgs e)
        {
            MapLegendSplitView.IsPaneOpen = !MapLegendSplitView.IsPaneOpen;
        }

        private async void OpenInAgol_Clicked(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel vm)
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

        private async void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await MyMapView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                MyMapView.Map.Basemap = e.AddedItems.OfType<Basemap>().FirstOrDefault();
            });

        }
    }
}
