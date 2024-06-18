using Esri.ArcGISRuntime.Portal;

namespace MauiSignin;

public partial class MapPage : ContentPage
{
    public MapPage()
    {
        InitializeComponent();
    }

    public MapPage(PortalItem item) : this()
    {
        mapView.Map = new Esri.ArcGISRuntime.Mapping.Map(item);
    }
}