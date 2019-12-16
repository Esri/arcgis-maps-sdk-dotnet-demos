using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.GeoAnalysis;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Xamarin.Forms;

namespace FormsDemoAR
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class ARPage : ContentPage
    {
        private MapPoint centerPoint;

        private Graphic planeGraphic;

        Polyline routePath;
        double routeLength;
        double progressOnRoute = 0;

        public ARPage()
        {
            InitializeComponent();
        }

        private async void InitializeScene()
        {
            try
            {
                // Set the translation factor to allow you to see the whole scene
                // TranslationFactor = scene width / physical table width
                arSceneView.TranslationFactor = 4440.0 / .7;

                // Disable touch interaction
                arSceneView.InteractionOptions = new SceneViewInteractionOptions { IsEnabled = false };

                // Set up the scene
                await configureScene();

                // Add the airplane graphic
                await createAndAddAirplane();

                buildFlightPath();

                // Animate the airplane
                animatePlane();

                // Show the analysis
                showViewshed();
            }
            catch { }
        }

        private async Task configureScene()
        {
            // Create a scene and add the Melbourne layer to it.
            Scene melbourneScene = new Scene();
            melbourneScene.OperationalLayers.Add(new IntegratedMeshLayer(new Uri("https://arcgisruntime.maps.arcgis.com/home/item.html?id=2367c1fbe19d4a1aa05d79d084e3d832")));

            // Show the scene in the view
            arSceneView.Scene = melbourneScene;

            // Hide the basemap surface since it isn't needed with the mesh layer
            melbourneScene.BaseSurface = new Surface();
            melbourneScene.BaseSurface.BackgroundGrid.IsVisible = false;
            melbourneScene.BaseSurface.Opacity = 0;

            // Always disable the navigation constraint in AR
            melbourneScene.BaseSurface.NavigationConstraint = NavigationConstraint.None;

            // Load the metadata for the scene and all its layers
            await melbourneScene.LoadAsync();

            // Get the center of the scene content
            centerPoint = melbourneScene.OperationalLayers.First().FullExtent.GetCenter();
            MapPoint anchorPoint = new MapPoint(centerPoint.X, centerPoint.Y, 0, centerPoint.SpatialReference);

            // Set the origin camera.
            arSceneView.OriginCamera = new Camera(anchorPoint, 0, 90, 0);
        }

        private async Task createAndAddAirplane()
        {
            // Create a graphics overlay to show the plane graphic above the ground
            GraphicsOverlay planeOverlay = new GraphicsOverlay();
            planeOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Relative;
            planeOverlay.SceneProperties.AltitudeOffset = 30 + 1200;

            // Show the overlay and configure it to render the plane graphic
            arSceneView.GraphicsOverlays.Add(planeOverlay);
            SimpleRenderer renderer3D = new SimpleRenderer();
            // Heading and roll will be automatically set based on the plane graphic's attributes
            renderer3D.SceneProperties.HeadingExpression = "[HEADING]";
            renderer3D.SceneProperties.RollExpression = "[ROLL]";
            planeOverlay.Renderer = renderer3D;

            // Download the plane model and get the path
            await DataManager.DownloadItem("21274c9a36f445db912c7c31d2eb78b7");
            string path = DataManager.GetDataFolder("21274c9a36f445db912c7c31d2eb78b7");
            string filePath = Path.Combine(path, "Boeing787", "B_787_8.dae");

            // Create the airplane symbol
            ModelSceneSymbol plane3DSymbol = await ModelSceneSymbol.CreateAsync(new Uri(filePath), 3);
            plane3DSymbol.AnchorPosition = SceneSymbolAnchorPosition.Bottom;

            // Create the graphic with an initial location, heading, roll, and the airplane symbol
            planeGraphic = new Graphic(centerPoint, plane3DSymbol);
            planeGraphic.Attributes["HEADING"] = 0.0;
            planeGraphic.Attributes["ROLL"] = -25;

            // Add the plane to the scene
            planeOverlay.Graphics.Add(planeGraphic);
        }

        private void animatePlane()
        {
            // Configure the animation timer and events
            Timer animationTimer = new Timer(16) //~ 60 fps
            {
                Enabled = true,
                AutoReset = true
            };
            animationTimer.Elapsed += (_, __) =>
            {
                // Increment the progress along the route
                double newProgress = progressOnRoute + (routeLength / 60 / 60);

                if (newProgress > routeLength)
                {
                    newProgress = 0;
                }

                // Move the plane along the path
                planeGraphic.Geometry = GeometryEngine.CreatePointAlong(routePath, newProgress);

                // Update the plane's heading
                planeGraphic.Attributes["HEADING"] = (progressOnRoute / routeLength) * 360;

                // Save the current progress
                progressOnRoute = newProgress;
            };

            animationTimer.Start();
        }

        private void buildFlightPath()
        {
            // Use geometry engine to create a circle around the center of the scene
            Geometry circle = GeometryEngine.EllipseGeodesic(new GeodesicEllipseParameters(centerPoint, 1700, 1700));

            // Create a path around the perimeter of the circle
            routePath = (Polyline)GeometryEngine.Boundary(circle);

            // Store the length of the route for use later
            routeLength = GeometryEngine.Length(routePath);
        }

        private void showViewshed()
        {
            // Create a viewshed analysis for a passenger in the airplane
            GeoElementViewshed geoViewshed = new GeoElementViewshed(
                geoElement: planeGraphic,
                horizontalAngle: 90,
                verticalAngle: 120,
                minDistance: 5,
                maxDistance: 5000,
                headingOffset: -90,
                pitchOffset: 0.0)
            {
                OffsetX = -10,
                OffsetY = 17,
                OffsetZ = 20
            };

            // Create an overlay for the analysis
            AnalysisOverlay overlay = new AnalysisOverlay();
            overlay.Analyses.Add(geoViewshed);

            // Show the analysis in the scene
            arSceneView.AnalysisOverlays.Add(overlay);
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

        private void DoubleTap_ToPlace(object sender, Esri.ArcGISRuntime.Xamarin.Forms.GeoViewInputEventArgs e)
        {
            if (arSceneView.SetInitialTransformation(e.Position))
            {
                if (arSceneView.Scene == null)
                {
                    arSceneView.RenderPlanes = false;
                    Status.Text = string.Empty;
                    InitializeScene();
                }
            }
        }


        private void PlanesDetectedChanged(object sender, bool planesDetected)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (!planesDetected)
                    Status.Text = "Keep moving your device to find surfaces";
                else if (arSceneView.Scene == null)
                    Status.Text = "Double-tap a surface to place the map";
                else
                    Status.Text = string.Empty;
            });
        }
    }
}
