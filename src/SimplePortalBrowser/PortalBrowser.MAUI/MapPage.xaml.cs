using Esri.ArcGISRuntime.Portal;
using PortalBrowser.ViewModels;

namespace PortalBrowser.MAUI;

public partial class MapPage : ContentPage
{
    int count = 0;

    public MapPage(MapVM mapVM)
    {
        this.BindingContext = mapVM;
        InitializeComponent();
    }

    public MapPage()
    {
        InitializeComponent();
    }
}