using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Navigation;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.NetworkAnalysis;
using Esri.ArcGISRuntime.UI;
using RoutingSample.Models;
using RoutingSample.Services;
using System;
using System.ComponentModel;

#if WINDOWS_UWP
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
#elif WINDOWS_WPF
using System.Media;
#endif

using Color = System.Drawing.Color;

namespace RoutingSample.ViewModels
{
	/// <summary>
	/// View model for the main page.
	/// </summary>
	public class MainViewModel : ViewModelBase
	{
        #region Fields

        // Map
        private GraphicsOverlayCollection _graphicsOverlays;
        private Map _map;
        private LocationDisplay _locationDisplay;

        // The region defined by the sample data for routing
        private readonly Envelope _routableArea = new Envelope(
            new MapPoint(-117.1883928, 32.7707247, 0, SpatialReferences.Wgs84),
            new MapPoint(-117.0018724, 32.5738892, 0, SpatialReferences.Wgs84)
        );

        private MapPoint _destination;

        // Routing
        private RouteResult _routeResult;
        private RouteTask _routeTask;
        private RouteParameters _routeParameters;
        
        private bool _isRouting;
        private bool _isRerouting;
        private string _errorMessage;

        // Tracking
        private RouteTracker _routeTracker;
        private Maneuver _maneuver;

        // Simulation
        private DelegateCommand _followCommand;
        private DelegateCommand _wanderCommand;
        private DelegateCommand _stopCommand;

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
                //_simulator = new SimulatedLocationDataSource(new MapPoint(-117.1109159, 32.6400714, 0, SpatialReferences.Wgs84));
                Simulation = new Simulation(new MapPoint(-117.1109159, 32.6400714, 0, SpatialReferences.Wgs84));

                // Create the maneuver
                _maneuver = new Maneuver
                {
                    Text = "SELECT A DESTINATION TO START"
                };
            }
        }

        #region Properties

        /// <summary>
        /// Gets the map to be displayed in a <see cref="Esri.ArcGISRuntime.UI.Controls.MapView"/>.
        /// </summary>
        public Map Map => _map ?? (_map = new Map(Basemap.CreateNavigationVector()));

        /// <summary>
        /// Gets or sets the destination point.
        /// </summary>
        public MapPoint Destination
		{
            get => _destination;
            set
            {
                if (Simulation.State == SimulationState.Stopped &&
                    SetProperty(ref _destination, value))
                {
                    ChangeRoute();
                }
            }
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
                        _locationDisplay.DataSource = Simulation.Simulator;
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
                    _graphicsOverlays.Add(new GraphicsOverlay()
                    {
                        Renderer = new SimpleRenderer()
                        {
                            Symbol = new SimpleLineSymbol()
                            {
                                Width = 4,
                                Color = Color.FromArgb(232, 0, 122, 194)
                            }
                        }
                    });

                    // Traversed route
                    _graphicsOverlays.Add(new GraphicsOverlay()
                    {
                        Renderer = new SimpleRenderer()
                        {
                            Symbol = new SimpleLineSymbol()
                            {
                                Width = 6,
                                Color = Color.LightGray
                            }
                        }
                    });

                    // Routable area boundary
                    var routableAreaOverlay = new GraphicsOverlay();
                    routableAreaOverlay.Graphics.Add(new Graphic(_routableArea, new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, Color.Red, 3)));
                    _graphicsOverlays.Add(routableAreaOverlay);
                }

                return _graphicsOverlays;
            }
        }

        /// <summary>
        /// Gets the simulation.
        /// </summary>
        public Simulation Simulation { get; }

        /// <summary>
        /// Gets the Go command, which starts routing.
        /// </summary>
        public DelegateCommand FollowCommand => _followCommand ?? (_followCommand = new DelegateCommand(OnFollowCommand, CanExecuteFollowCommand));

        /// <summary>
        /// Gets the Stop command, which stops routing.
        /// </summary>
        public DelegateCommand StopCommand => _stopCommand ?? (_stopCommand = new DelegateCommand(OnStopCommand, CanExecuteStopCommand));

        /// <summary>
        /// Gets the Wander command.
        /// </summary>
        public DelegateCommand WanderCommand => _wanderCommand ?? (_wanderCommand = new DelegateCommand(OnWanderCommand, CanExecuteWanderCommand));

        #endregion

        #region Methods

        private void OnFollowCommand(object parameter)
        {
            // Resume the simulation
            Simulation.StartFollowing();
            
            // Update commands
            FollowCommand.RaiseCanExecuteChanged();
            WanderCommand.RaiseCanExecuteChanged();
            StopCommand.RaiseCanExecuteChanged();
        }

        private bool CanExecuteFollowCommand(object parameter)
        {
            return !IsRerouting
                && !IsRerouting
                && (Simulation.State == SimulationState.Stopped || Simulation.State == SimulationState.Wandering)
                && RouteTracker != null;
        }

        private void OnWanderCommand(object parameter)
        {
            // Resume the simulation
            Simulation.StartWandering();

            // Update commands
            FollowCommand.RaiseCanExecuteChanged();
            WanderCommand.RaiseCanExecuteChanged();
            StopCommand.RaiseCanExecuteChanged();
        }

        private bool CanExecuteWanderCommand(object parameter)
        {
            return !IsRouting
                && !IsRerouting
                && Simulation.State != SimulationState.Wandering
                && RouteTracker != null;
        }

        private void OnStopCommand(object parameter)
        {
            // Pause the simulation
            Simulation.Stop();

            // Update commands
            FollowCommand.RaiseCanExecuteChanged();
            WanderCommand.RaiseCanExecuteChanged();
            StopCommand.RaiseCanExecuteChanged();
        }

        private bool CanExecuteStopCommand(object parameter)
        {
            return !IsRouting && Simulation.State != SimulationState.Stopped;
        }

        // Updates the current route (based on destination changes)
        private async void ChangeRoute()
        {
            if (Destination != null && LocationDisplay != null && LocationDisplay.Location != null)
            {
                ErrorMessage = null;
                IsRouting = true;
                IsRerouting = false;

                try
                {
                    // We must create a new from point to eliminate z-axis from the current location
                    var from = (MapPoint)GeometryEngine.RemoveZ(LocationDisplay.Location.Position);
                    var to = Destination;

                    // Determine the route
                    var (routeResult, routeTask, routeParameters) = await new NavigationService()
                        .SolveRouteAsync(from, to);

                    // Copy the result from the service
                    RouteParameters = routeParameters;
                    RouteTask = routeTask;
                    RouteResult = routeResult;

                    // Display the route
                    DisplayRoute();

                    // Restart the simulation
                    Simulation.Simulator.SetRoute(routeResult.Routes[0]);
                    Simulation.Restart();
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message;
                }
                finally
                {
                    IsRouting = false;
                    IsRerouting = false;

                    FollowCommand.RaiseCanExecuteChanged();
                    WanderCommand.RaiseCanExecuteChanged();
                    StopCommand.RaiseCanExecuteChanged();
                }
            }
        }

        // Creates a new RouteTracker for the current route
        private async void CreateTracker()
        {
            // Creates a new tracker for a route
            RouteTracker = new RouteTracker(RouteResult, 0);
            RouteTracker.VoiceGuidanceUnitSystem = UnitSystem.Imperial;

            // Configure rerouting for our journey
            await RouteTracker.EnableReroutingAsync(RouteTask, RouteParameters, ReroutingStrategy.ToNextWaypoint, false);
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
            if (IsRouting || IsRerouting)
            {
                return;
            }

            // Pause the simulation?
            IsRerouting = true;
        }

        private void RouteTracker_RerouteCompleted(object sender, RouteTrackerRerouteCompletedEventArgs e)
        {
            var trackingStatus = e.TrackingStatus;
            if (trackingStatus.RouteResult != null)
            {
                //var route = trackingStatus.RouteResult.Routes[0];
                RouteResult = trackingStatus.RouteResult;

                // Display the new route
                DisplayRoute();

                // Update the simulator (behavior does not change, however)
                Simulation.Simulator.SetRoute(RouteResult.Routes[0]);
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
                // Play a beep at the turns
#if XAMARIN
                // TODO
#elif WINDOWS_UWP
                using (var player = new MediaPlayer())
                using (var source = MediaSource.CreateFromUri(new Uri("ms-winsoundevent:Notification.Default")))
                {
                    player.Source = source;
                    player.Play();
                }
#elif WINDOWS_WPF
                SystemSounds.Exclamation.Play();
#endif
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
            if (currentManeuverIndex >= route.DirectionManeuvers.Count)
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

            // Update the simulation
            Simulation.Simulator.UpdateTrackingStatus(trackingStatus);
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
