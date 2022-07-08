using Esri.ArcGISRuntime.Portal;

namespace MauiSignin;

public partial class MapPage : ContentPage
{
    private PortalItem item;

    public MapPage()
    {
        InitializeComponent();
    }

    public MapPage(PortalItem item) : this()
    {
        mapView.Map = new Esri.ArcGISRuntime.Mapping.Map(item);
    }
}