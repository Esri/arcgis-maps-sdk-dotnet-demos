namespace GeoEventServerSample.Maui
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            BindingContext = VM = new MapViewModel();
        }

        public MapViewModel VM { get; private set; }

        private async void mapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.Maui.GeoViewInputEventArgs e)
        {
            var r = await mapView.IdentifyGraphicsOverlayAsync(mapView.GraphicsOverlays[0], e.Position, 1, false);
            r.GraphicsOverlay.ClearSelection();
            var g = r.Graphics?.FirstOrDefault();
            attributeView.ItemsSource = g?.Attributes;
            if (g != null)
                g.IsSelected = true;
        }
    }
}