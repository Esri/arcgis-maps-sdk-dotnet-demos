using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Navigation;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.NetworkAnalysis;
using Esri.ArcGISRuntime.UI;
using RoutingSample.Models;
using RoutingSample.Services;
using System;
using System.ComponentModel;
using System.Diagnostics;
using Color = System.Drawing.Color;

namespace RoutingSample.ViewModels
{
	/// <summary>
	/// View model for the main page.
	/// </summary>
	public class MainViewModel : ViewModelBase
	{
        #region Fields

        // The default speed of the simulation in KPH.
        private const int DefaultSpeed = 50;

        // Map
        private GraphicsOverlayCollection _graphicsOverlays;
        private Map _map;
        private LocationDisplay _locationDisplay;

        // Routing
        private RouteResult _routeResult;
        private RouteTask _routeTask;
        private RouteParameters _routeParameters;
        private bool _isRouting;
        private bool _isRerouting;
        private string _errorMessage;

        // Tracking / Simulation
        private RouteTracker _routeTracker;
        private Maneuver _maneuver;
        private string _address;
        private DelegateCommand _navigateCommand;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        public MainViewModel()
		{
            if (IsDesignMode)
            {
                // Create a design-time maneuver with fake instructions
                _maneuver = new Maneuver
                {
                    Text = "HEAD NORTH ON MAIN ST",
                    Type = DirectionManeuverType.Straight,
                    RemainingTime = TimeSpan.FromMinutes(15)
                };
            }
            else
            {
                // We begin the simulation outside of the Living Coast Discovery Center
                Simulation = new SimulatedLocationDataSource();
                Simulation.SetLocationsWithPolyline(new Polyline(new[]
                {
                    new MapPoint(-117.1109159, 32.6400714, 0, SpatialReferences.Wgs84)
                }));

                // And here's an example destination
                Address = "LEGOLAND California, One Legoland Dr, Carlsbad, CA 92008";

                // Create the maneuver
                _maneuver = new Maneuver
                {
                    Text = "ENTER A DESTINATION TO START",
                };
            }
        }

        #region Properties

        /// <summary>
        /// Gets the map to be displayed in a <see cref="Esri.ArcGISRuntime.UI.Controls.MapView"/>.
        /// </summary>
        public Map Map => _map ?? (_map = new Map(Basemap.CreateNavigationVector()));

        /// <summary>
        /// Gets or sets the address.
        /// </summary>
        public string Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

        /// <summary>
        /// Gets the current route.
        /// </summary>
        public RouteResult RouteResult
        {
            get => _routeResult;
            private set
            {
                if (SetProperty(ref _routeResult, value))
                {
                    if (!IsRerouting)
                    {
                        // Create a new route tracker for the route
                        // We are always going to go for the 1st route available
                        CreateTracker();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the current route task.
        /// </summary>
        public RouteTask RouteTask
        {
            get => _routeTask;
            private set => SetProperty(ref _routeTask, value);
        }

        /// <summary>
        /// Gets the current route parameters.
        /// </summary>
        public RouteParameters RouteParameters
        {
            get => _routeParameters;
            private set => SetProperty(ref _routeParameters, value);
        }

        /// <summary>
        /// Gets the tracker for the current route.
        /// </summary>
        public RouteTracker RouteTracker
        {
            get => _routeTracker;
            private set
            {
                if (_routeTracker != value)
                {
                    if (_routeTracker != null)
                    {
                        _routeTracker.TrackingStatusChanged -= RouteTracker_TrackingStatusChanged;
                        _routeTracker.NewVoiceGuidance -= RouteTracker_NewVoiceGuidance;
                        _routeTracker.RerouteStarted -= RouteTracker_RerouteStarted;
                        _routeTracker.RerouteCompleted -= RouteTracker_RerouteCompleted;
                    }

                    _routeTracker = value;
                    _routeTracker.TrackingStatusChanged += RouteTracker_TrackingStatusChanged;
                    _routeTracker.NewVoiceGuidance += RouteTracker_NewVoiceGuidance;
                    _routeTracker.RerouteStarted += RouteTracker_RerouteStarted;
                    _routeTracker.RerouteCompleted += RouteTracker_RerouteCompleted;

                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the current maneuver.
        /// </summary>
        public Maneuver Maneuver
        {
            get => _maneuver;
            private set => SetProperty(ref _maneuver, value);
        }

		/// <summary>
		/// Determines whether routing is currently in progress.
		/// </summary>
		public bool IsRouting
		{
            get => _isRouting;
            private set => SetProperty(ref _isRouting, value);
		}
        
        /// <summary>
        /// Determines whether rerouting is currently in progress.
        /// </summary>
        public bool IsRerouting
        {
            get => _isRerouting;
            private set => SetProperty(ref _isRerouting, value);
        }

		/// <summary>
		/// Gets the route calculation error message (if any).
		/// </summary>
		public string ErrorMessage
		{
            get => _errorMessage;
            private set => SetProperty(ref _errorMessage, value);
		}

        /// <summary>
        /// The current location display used for displaying location on the mapView
        /// </summary>
        public LocationDisplay LocationDisplay
        {
            get => _locationDisplay;
            set
            {
                if (_locationDisplay != value)
                {
                    if (_locationDisplay != null)
                    {
                        _locationDisplay.PropertyChanged -= LocationDisplay_PropertyChanged;
                        _locationDisplay.DataSource = null;
                    }

                    _locationDisplay = value;
                    if (_locationDisplay != null)
                    {
                        _locationDisplay.PropertyChanged += LocationDisplay_PropertyChanged;
                        _locationDisplay.DataSource = Simulation;
                        _locationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Navigation;
                        _locationDisplay.IsEnabled = true;
                    }

                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the collection of <see cref="GraphicsOverlay"/> objects which holds graphic result of RouteTask.
        /// </summary>
        public GraphicsOverlayCollection RouteGraphicsOverlays
        {
            get
            {
                if (_graphicsOverlays == null)
                {
                    _graphicsOverlays = new GraphicsOverlayCollection();

                    // Current route
                    _graphicsOverlays.Add(new GraphicsOverlay
                    {
                        Renderer = new SimpleRenderer
                        {
                            Symbol = new SimpleLineSymbol
                            {
                                Width = 4,
                                Color = Color.FromArgb(232, 0, 122, 194)
                            }
                        }
                    });

                    // Traversed route
                    _graphicsOverlays.Add(new GraphicsOverlay
                    {
                        Renderer = new SimpleRenderer
                        {
                            Symbol = new SimpleLineSymbol
                            {
                                Width = 6,
                                Color = Color.LightGray
                            }
                        }
                    });
                }

                return _graphicsOverlays;
            }
        }

        /// <summary>
        /// Gets the simulation.
        /// </summary>
        public SimulatedLocationDataSource Simulation { get; }

        /// <summary>
        /// Gets the navigate command.
        /// </summary>
        public DelegateCommand NavigateCommand => _navigateCommand ?? (_navigateCommand = new DelegateCommand(OnNavigate));

        #endregion

        #region Methods

        private void OnNavigate(object parameter)
        {
            ChangeRoute();
        }

        // Updates the current route (based on destination changes)
        private async void ChangeRoute()
        {
            if (!string.IsNullOrEmpty(Address) && LocationDisplay != null && LocationDisplay.Location != null)
            {
                ErrorMessage = null;
                IsRouting = true;
                IsRerouting = false;

                try
                {
                    var navigationService = new NavigationService();

                    // Determine the location of the destination
                    var destination = await navigationService.GeocodeAsync(Address);

                    // Determine the our current location from the display
                    var start = (MapPoint)GeometryEngine.RemoveZ(LocationDisplay.Location.Position);

                    // Determine the route
                    var result = await navigationService.SolveRouteAsync(start, destination);

                    // Copy the result from the service
                    RouteParameters = result.Parameters;
                    RouteTask = result.Task;
                    RouteResult = result.Route;

                    // Display the route
                    DisplayRoute();

                    // Restart the simulation
                    Simulation.SetLocationsWithPolyline(RouteResult.Routes[0].RouteGeometry, new SimulationParameters
                    {
                        Velocity = DefaultSpeed
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error: " + ex);
                    ErrorMessage = ex.Message;
                }
                finally
                {
                    IsRouting = false;
                    IsRerouting = false;
                }
            }
        }

        // Creates a new RouteTracker for the current route
        private async void CreateTracker()
        {
            // Creates a new tracker for a route
            RouteTracker = new RouteTracker(RouteResult, 0);
            RouteTracker.VoiceGuidanceUnitSystem = UnitSystem.Imperial;

            // Before enabling rerouting, you should check that it is supported
            if (RouteTask.RouteTaskInfo.SupportsRerouting)
            {
                try
                {
                    await RouteTracker.EnableReroutingAsync(RouteTask, RouteParameters, ReroutingStrategy.ToNextWaypoint, false);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Failed to enable rerouting:");
                    Debug.WriteLine(ex);
                }
            }
        }

        // Adds the specified route to the graphics overlay
        private void DisplayRoute()
        {
            var route = RouteResult?.Routes[0];
            if (route == null)
                return;

            var lineOverlay = RouteGraphicsOverlays[0];
            if (lineOverlay.Graphics.Count > 0)
                lineOverlay.Graphics.Clear();

            lineOverlay.Graphics.Add(new Graphic { Geometry = GeometryHelpers.Flatten(route.RouteGeometry) });
        }

        private void RouteTracker_RerouteStarted(object sender, EventArgs e)
        {
            Debug.WriteLine("Reroute starting...");

            if (IsRouting || IsRerouting)
            {
                return;
            }

            // Pause the simulation?
            IsRerouting = true;
        }

        private void RouteTracker_RerouteCompleted(object sender, RouteTrackerRerouteCompletedEventArgs e)
        {
            Debug.WriteLine("Reroute completed.");

            var trackingStatus = e.TrackingStatus;
            if (trackingStatus.RouteResult != null)
            {
                RouteResult = trackingStatus.RouteResult;

                // Display the new route
                DisplayRoute();

                // Update the simulator (behavior does not change, however)
                Simulation.SetLocationsWithPolyline(RouteResult.Routes[0].RouteGeometry, new SimulationParameters
                {
                    Velocity = DefaultSpeed
                });
            }

            IsRerouting = false;
        }

        private async void RouteTracker_NewVoiceGuidance(object sender, RouteTrackerNewVoiceGuidanceEventArgs e)
        {
            // You might want to consider e.VoiceGuidance.Type
            if (e.VoiceGuidance.Type != VoiceGuidanceType.AtManeuver)
            {
                await Speech.SayAsync(e.VoiceGuidance.Text);
            }
            else
            {
                Debug.WriteLine("Arrived at maneuver.");
            }
        }

        private void RouteTracker_TrackingStatusChanged(object sender, RouteTrackerTrackingStatusChangedEventArgs e)
        {
            var trackingStatus = e.TrackingStatus;
            var currentManeuverIndex = trackingStatus.CurrentManeuverIndex;
            var route = trackingStatus.RouteResult.Routes[0];

            // Update the directions
            // We use the next maneuver to inform the user what their next action will be
            var nextManeuverIndex = currentManeuverIndex + 1;
            if (nextManeuverIndex >= route.DirectionManeuvers.Count)
                nextManeuverIndex = route.DirectionManeuvers.Count - 1;

            var nextManeuver = route.DirectionManeuvers[nextManeuverIndex];
            Maneuver.Text = nextManeuver.DirectionText.ToUpper();
            Maneuver.Type = nextManeuver.ManeuverType;

            // Update maneuver progress
            Maneuver.RemainingDistance = trackingStatus.ManeuverProgress.RemainingDistance;
            Maneuver.RemainingTime = trackingStatus.ManeuverProgress.RemainingTime;

            var progressOverlay = RouteGraphicsOverlays[1];
            if (progressOverlay.Graphics.Count == 0)
            {
                progressOverlay.Graphics.Add(new Graphic
                {
                    Geometry = e.TrackingStatus.DestinationProgress.TraversedGeometry
                });
            }
            else
            {
                progressOverlay.Graphics[0].Geometry = e.TrackingStatus.DestinationProgress.TraversedGeometry;
            }
        }

        private async void LocationDisplay_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(LocationDisplay.Location))
			{
                // When location changes, update the tracker
                var location = LocationDisplay?.Location;
                if (_routeTracker != null && location != null)
                    await _routeTracker.TrackLocationAsync(location);
			}
		}

        #endregion
    }
}
