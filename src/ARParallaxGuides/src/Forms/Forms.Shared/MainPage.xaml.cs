using ARParallaxGuidelines.Shared;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ARParallaxGuidelines.Forms
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public InfrastructureEditorViewModel ViewModel = new InfrastructureEditorViewModel();

        // Graphics overlays for showing pipes.
        private GraphicsOverlay _pipesOverlay = new GraphicsOverlay();

        public MainPage()
        {
            InitializeComponent();
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            // Create and add the map.
            MyMapView.Map = new Map(Basemap.CreateImagery());

            // Add a graphics overlay for the drawn pipes.
            MyMapView.GraphicsOverlays.Add(_pipesOverlay);
            _pipesOverlay.Renderer = new SimpleRenderer(new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Red, 2));

            await ViewModel.InitializeAsync();
            BindingContext = ViewModel;

            // Set the SketchEditor for the map.
            MyMapView.SketchEditor = ViewModel.SketchEditor;

#if ANDROID
            ARParallaxGuidelines.Forms.Droid.MainActivity.Instance.AskForLocationPermission(MyMapView);
#else
            // Configure location display.
            MyMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Recenter;
            await MyMapView.LocationDisplay.DataSource.StartAsync();
            MyMapView.LocationDisplay.IsEnabled = true;
#endif
            
        }

        private async void Button_Clicked_2(object sender, EventArgs e)
        {
            var graphics = _pipesOverlay.Graphics.Select(x => new Graphic(x.Geometry, x.Attributes));
            var _ = Navigation.PushAsync(new ARPage() { _pipeGraphics = graphics });
        }

        private async void Button_Clicked_1(object sender, EventArgs e)
        {
            var geometry = await ViewModel.ExecuteAddSketch();

            if (geometry != null)
            {
                var graphic = new Graphic(geometry);
                graphic.Attributes[nameof(ViewModel.ElevationOffset)] = ViewModel.ElevationOffset;
                _pipesOverlay.Graphics.Add(graphic);
            }
        }
    }
}