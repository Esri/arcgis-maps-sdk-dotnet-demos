using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ARParallaxGuidelines.Forms
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class ARPage : ContentPage
    {
        // Pipe graphics that have been passed in by the PipePlacer class.
        public IEnumerable<Graphic> _pipeGraphics;
        private IEnumerable<Graphic> _shadowPipes;

        // Elevation for the scene.
        private ArcGISTiledElevationSource _elevationSource;
        private Surface _elevationSurface;

        // Track when user is changing between AR and GPS localization.
        private bool _changingScale;

        // Location data source for AR and route tracking.
        private AdjustableLocationDataSource _locationSource = new AdjustableLocationDataSource();

        // Track whether calibration is in progress and update the UI when that changes.
        private bool _isCalibrating = false;

        private bool IsCalibrating
        {
            get => _isCalibrating;
            set
            {
                if (_isCalibrating != value)
                {
                    _isCalibrating = value;
                    if (_isCalibrating)
                    {
                        // Show the base surface so that the user can calibrate using the base surface on top of the real world.
                        arSceneView.Scene.BaseSurface.Opacity = 0.5;

                        // Enable scene interaction.
                        arSceneView.InteractionOptions.IsEnabled = true;
                    }
                    else
                    {
                        // Hide the base surface.
                        arSceneView.Scene.BaseSurface.Opacity = 0;

                        // Disable scene interaction.
                        arSceneView.InteractionOptions.IsEnabled = false;
                    }
                }
            }
        }

        public ARPage()
        {
            InitializeComponent();
        }

        private async void InitializeScene()
        {
            try
            {
                var scene = new Scene(Basemap.CreateImagery());
                scene.BaseSurface = new Surface();
                scene.BaseSurface.BackgroundGrid.IsVisible = false;
                scene.BaseSurface.ElevationSources.Add(new ArcGISTiledElevationSource(new Uri("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer")));
                scene.BaseSurface.NavigationConstraint = NavigationConstraint.None;
                await scene.LoadAsync();
                arSceneView.Scene = scene;
            }
            catch (System.Exception ex)
            {
                await DisplayAlert("Failed to load scene", ex.Message, "OK");
                await Navigation.PopAsync();
            }

        }

        protected override void OnAppearing()
        {
            Status.Text = "Move your device in a circular motion to detect surfaces";
            arSceneView.StartTrackingAsync();
            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            arSceneView.StopTrackingAsync();
            base.OnDisappearing();
        }
    }
}
