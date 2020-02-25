using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Esri.ArcGISRuntime.ARToolkit;
using Esri.ArcGISRuntime.Symbology;
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

                        CalibrationView.IsVisible = true;
                    }
                    else
                    {
                        // Hide the base surface.
                        arSceneView.Scene.BaseSurface.Opacity = 0;

                        // Disable scene interaction.
                        arSceneView.InteractionOptions.IsEnabled = false;

                        CalibrationView.IsVisible = false;
                    }
                }
            }
        }

        private Timer _headingJoystickTimer = new Timer(100);
        private Timer _elevationJoystickTimer = new Timer(100);

        public ARPage()
        {
            InitializeComponent();

            // Used to implement a joystick effect during calibration.
            _headingJoystickTimer.Elapsed += _headingJoystickTimer_Elapsed;
            _elevationJoystickTimer.Elapsed += _elevationJoystickTimer_Elapsed;
        }

        private async void InitializeScene()
        {
            try
            {
                // Create and add the scene.
                arSceneView.Scene = new Scene(Basemap.CreateImagery());

                // Add the location data source to the AR view.
                arSceneView.LocationDataSource = _locationSource;

                // Create and add the elevation source.
                _elevationSource = new ArcGISTiledElevationSource(new Uri("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer"));
                _elevationSurface = new Surface();
                _elevationSurface.ElevationSources.Add(_elevationSource);
                arSceneView.Scene.BaseSurface = _elevationSurface;

                // Configure the surface for AR: no navigation constraint and hidden by default.
                _elevationSurface.NavigationConstraint = NavigationConstraint.None;
                _elevationSurface.Opacity = 0;

                // Configure scene view display for real-scale AR: no space effect or atmosphere effect.
                arSceneView.SpaceEffect = SpaceEffect.None;
                arSceneView.AtmosphereEffect = AtmosphereEffect.None;

                ConfigureAndAddPipes();

                ConfigureAndAddShadows();

                ConfigureAndAddLeaders();

                // Disable scene interaction.
                arSceneView.InteractionOptions = new SceneViewInteractionOptions() { IsEnabled = false };

                // Enable the calibration button.
                CalibrateButton.IsEnabled = true;
            }
            catch (System.Exception ex)
            {
                await DisplayAlert("Failed to load scene", ex.Message, "OK");
                await Navigation.PopAsync();
            }

        }

        private void ConfigureAndAddPipes()
        {
            // Create a graphics overlay for the pipes.
            GraphicsOverlay pipesOverlay = new GraphicsOverlay();

            // Use absolute surface placement to see the graphics at the correct altitude.
            pipesOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Absolute;

            // Add graphics for the pipes.
            pipesOverlay.Graphics.AddRange(_pipeGraphics);

            // Display routes as red 3D tubes.
            SolidStrokeSymbolLayer strokeSymbolLayer = new SolidStrokeSymbolLayer(0.3, System.Drawing.Color.Red, null, StrokeSymbolLayerLineStyle3D.Tube) { CapStyle = StrokeSymbolLayerCapStyle.Round };
            MultilayerPolylineSymbol tubeSymbol = new MultilayerPolylineSymbol(new[] { strokeSymbolLayer });
            pipesOverlay.Renderer = new SimpleRenderer(tubeSymbol);

            // Add the graphics overlay to the scene.
            arSceneView.GraphicsOverlays.Add(pipesOverlay);
        }

        private void ConfigureAndAddShadows()
        {
            // Create a graphics overlay for the pipe shadows.
            GraphicsOverlay shadowOverlay = new GraphicsOverlay();

            // Place the graphics directly on the ground regardless of elevation.
            shadowOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.DrapedFlat;

            // Configure the renderer.
            shadowOverlay.Renderer = new SimpleRenderer(new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Yellow, 0.3));

            // Add all underground graphics.
            _shadowPipes = _pipeGraphics.Where(g => (double)g.Attributes["ElevationOffset"] < 0).Select(g => new Graphic(g.Geometry, g.Attributes));
            shadowOverlay.Graphics.AddRange(_shadowPipes);

            // Add the overlay to the view.
            arSceneView.GraphicsOverlays.Add(shadowOverlay);
        }

        private void ConfigureAndAddLeaders()
        {
            GraphicsOverlay leadersOverlay = new GraphicsOverlay();
            leadersOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Absolute;
            leadersOverlay.Renderer = new SimpleRenderer(new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, System.Drawing.Color.Red, 0.3));

            foreach (Graphic pipeGraphic in _pipeGraphics)
            {
                Polyline pipePolyline = (Polyline)pipeGraphic.Geometry;
                double offset = (double)pipeGraphic.Attributes["ElevationOffset"];

                foreach (var part in pipePolyline.Parts)
                {
                    foreach (var point in part.Points)
                    {
                        MapPoint offsetPoint = new MapPoint(point.X, point.Y, point.Z - offset);
                        Polyline leaderLine = new Polyline(new[] { point, offsetPoint });
                        leadersOverlay.Graphics.Add(new Graphic(leaderLine));
                    }
                }
            }

            arSceneView.GraphicsOverlays.Add(leadersOverlay);
        }

        protected override void OnAppearing()
        {
            Status.Text = "Calibrate before viewing infrastructure.";
            arSceneView.StartTrackingAsync(ARLocationTrackingMode.Continuous);

            InitializeScene();
            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            arSceneView.StopTrackingAsync();
            base.OnDisappearing();
        }

        private void CalibrateButton_OnClicked(object sender, EventArgs e)
        {
            IsCalibrating = !IsCalibrating;
        }

        private void CalibrationSlider_DragStarted(object sender, EventArgs e)
        {
            if (sender == ElevationSlider)
            {
                _elevationJoystickTimer.Start();
            }
            else
            {
                _headingJoystickTimer.Start();
            }
        }

        private void CalibrationSlider_DragCompleted(object sender, EventArgs e)
        {
            if (sender == ElevationSlider)
            {
                _elevationJoystickTimer.Stop();
                ElevationSlider.Value = 0;
            }
            else
            {
                _headingJoystickTimer.Stop();
                HeadingSlider.Value = 0;
            }
        }

        private void _elevationJoystickTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Calculate the altitude offset
            var newValue = _locationSource.AltitudeOffset += JoystickConverter(ElevationSlider.Value);

            // Set the altitude offset on the location data source.
            _locationSource.AltitudeOffset = newValue;
        }

        private void _headingJoystickTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Calculate the altitude offset
            var newValue = _locationSource.HeadingOffset += JoystickConverter(HeadingSlider.Value);

            // Set the altitude offset on the location data source.
            _locationSource.HeadingOffset = newValue;
        }

        private double JoystickConverter(double value)
        {
            return Math.Pow(value, 2) / 25 * (value < 0 ? -1.0 : 1.0);
        }
    }
}
