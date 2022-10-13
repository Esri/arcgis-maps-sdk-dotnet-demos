using Esri.ArcGISRuntime.Portal;
using PortalBrowser.ViewModels;

namespace PortalBrowser.MAUI;

public partial class MainPage : TabbedPage
{
    int count = 0;

    public MainPage()
    {
        InitializeComponent();
    }

    private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
    {
        var item = (sender as Grid)?.BindingContext as PortalItem;
        if (item is PortalItem pItem)
        {
            MapVM mapVm = new MapVM() { PortalItem = pItem };
            this.Navigation.PushAsync(new MapPage(mapVm));
        }
    }
}