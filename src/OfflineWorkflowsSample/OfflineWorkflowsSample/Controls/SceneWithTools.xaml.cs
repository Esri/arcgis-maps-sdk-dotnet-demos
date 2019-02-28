using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Esri.ArcGISRuntime.Portal;
using OfflineWorkflowsSample;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace OfflineWorkflowSample.Controls
{
    public sealed partial class SceneWithTools : UserControl
    {
        public SceneWithTools()
        {
            this.InitializeComponent();
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
    }
}
