using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace PortalBrowser.WinUI;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MapPage : Page
{
    public MapPage()
    {
        InitializeComponent();
        MyMapView.LocationDisplay.IsEnabled = true;
        BackButton.Click += GoBack;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        var item = e.Parameter as PortalItem;
        var vm = Resources["mapVM"] as ViewModels.MapVM;
        vm.PortalItem = item;
    }

    private void GoBack(object sender, RoutedEventArgs e)
    {
        if (this.Frame != null && this.Frame.CanGoBack) this.Frame.GoBack();
    }
}
