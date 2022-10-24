using PortalBrowser.ViewModels;

namespace PortalBrowser.MAUI;

public partial class MapPage : ContentPage
{
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