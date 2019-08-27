using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Navigation;
using Esri.ArcGISRuntime.Tasks.NetworkAnalysis;
using System;
using System.Threading.Tasks;
#if XAMARIN
using Xamarin.Forms;
#elif WINDOWS_UWP
using Windows.UI.Xaml;
#elif WINDOWS_WPF
using System.Windows.Threading;
#endif

namespace RoutingSample
{
    public enum SimulationState
    {
        // Not doing anything
        Stopped,

        // Following the route
        Following,

        // Moving to the target point
        Seeking,

        // Wandering away from the route
        Wandering,
    }

    public class SimulatedLocationDataSource : LocationDataSource
    {
        #region Fields

        /// <summary>
        /// The speed of the simulated vehicle in KPH.
        /// </summary>
        public const double Speed = 50.0;

#if WINDOWS_WPF || WINDOWS_UWP
        private DispatcherTimer _timer;
#endif
        private MapPoint _location;
        private MapPoint _target;

        private double _course;
        private Route _route;
        private int _maneuverIndex;
        private double _maneuverProgress;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="SimulatedLocationDataSource"/>
        /// with the specified start location.
        /// </summary>
        public SimulatedLocationDataSource(MapPoint location)
        {
#if WINDOWS_WPF || WINDOWS_UWP
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += OnTimerTick;
#endif

            _location = location ?? throw new ArgumentNullException(nameof(location));
            State = SimulationState.Stopped;
        }

        #region Methods

        protected override Task OnStartAsync()
        {
#if XAMARIN
            Device.StartTimer(TimeSpan.FromSeconds(1), OnTimerTick);
#elif WINDOWS_WPF || WINDOWS_UWP
            _timer.Start();
#endif
            return Task.CompletedTask;
        }

        protected override Task OnStopAsync()
        {
#if WINDOWS_WPF || WINDOWS_UWP
            _timer.Stop();
#endif
            return Task.CompletedTask;
        }

#if XAMARIN
        private bool OnTimerTick()
#elif WINDOWS_UWP
        private void OnTimerTick(object sender, object e)
#elif WINDOWS_WPF
        private void OnTimerTick(object sender, EventArgs e)
#endif
        {
#if XAMARIN
            // TODO: Can we get actual time between ticks?
            var speed = 1.0 * Speed;
#elif WINDOWS_WPF || WINDOWS_UWP
            var speed = _timer.Interval.TotalSeconds * Speed;
#endif
            switch (State)
            {
                case SimulationState.Following:
                    UpdateFollow(speed);
                    break;

                case SimulationState.Seeking:
                    UpdateSeek(speed);
                    break;

                case SimulationState.Wandering:
                    UpdateWander(speed);
                    break;
            }

            UpdateLocation(new Location(_location, 0.001, Speed, _course, false));

#if XAMARIN
            return IsStarted;
#endif
        }

        /// <summary>
        /// Sets the route result.
        /// </summary>
        /// <param name="routeResult">The route result.</param>
        /// <param name="stop">When <c>true</c>, stops the simulation.</param>
        public void SetRoute(Route route, bool stop = false)
        {
            _route = route;// ?? throw new ArgumentNullException(nameof(route));
            _maneuverIndex = 0;
            _maneuverProgress = 0;

            if (stop)
            {
                State = SimulationState.Stopped;
            }
        }

        /// <summary>
        /// Stops the simulation.
        /// </summary>
        public void Stop()
        {
            State = SimulationState.Stopped;
        }

        /// <summary>
        /// The simulator moves to the start of the route.
        /// </summary>
        public void SeekStart()
        {
            // Snap to the closest point on route 0
            if (State != SimulationState.Seeking)
            {
                var route0 = GeometryHelpers.Flatten(_route.RouteGeometry);
                var result = GeometryEngine.NearestCoordinate(route0, _location);

                _target = result.Coordinate;
                State = SimulationState.Seeking;
            }
        }

        /// <summary>
        /// The simulator moves to the closest coordinate on the route.
        /// </summary>
        public void SeekClosestCoordinate()
        {
            if (State != SimulationState.Seeking)
            {
                var nearestResult = GeometryEngine.NearestCoordinate(_route.RouteGeometry, _location);
                var nearest = nearestResult.Coordinate;

                _target = nearest;
                State = SimulationState.Seeking;
            }
        }

        /// <summary>
        /// The simulator moves away from the route.
        /// </summary>
        public void Wander()
        {
            State = SimulationState.Wandering;
        }

        /// <summary>
        /// The simulator moves along the route.
        /// </summary>
        public void Follow()
        {
            State = SimulationState.Following;
        }

        /// <summary>
        /// Updates the tracking status of the simulation.
        /// </summary>
        /// <param name="status"></param>
        public void UpdateTrackingStatus(TrackingStatus status)
        {
            if (status.DestinationStatus == DestinationStatus.Reached)
            {
                State = SimulationState.Stopped;
            }
            else if (status.CurrentManeuverIndex > _maneuverIndex)
            {
                _maneuverIndex = status.CurrentManeuverIndex;
                _maneuverProgress = 0;
            }
        }

        private void UpdateFollow(double speed)
        {
            // Get the current maneuver
            var maneuver = GetCurrentManeuver();
            if (maneuver == null)
                return;

            var maneuverLine = maneuver.Geometry as Polyline;
            if (maneuverLine == null)
                return;

            var maneuverLength = GeometryEngine.LengthGeodetic(maneuverLine);
            //var moveProgress = _maneuverProgress + move;

            // Have we reached the end?
            if (_maneuverProgress + speed >= maneuverLength)
            {
                if (_maneuverIndex + 1 >= _route.DirectionManeuvers.Count)
                {
                    var point1 = _location;
                    var point2 = maneuverLine.Parts[0].EndPoint;

                    // Snap to the end of the maneuver
                    _course = GeometryHelpers.BearingGeodetic(point1, point2);
                    _location = new MapPoint(point2.X, point2.Y, point1.SpatialReference);
                    _maneuverProgress = 0;
                }
                else
                {
                    // Snap to front of next maneuver
                    var maneuver2 = GetNextManeuver();

                    var maneuver2Line = maneuver2.Geometry as Polyline;
                    if (maneuver2Line == null)
                        return;

                    var point1 = _location;
                    var point2 = maneuver2Line.Parts[0].StartPoint;

                    _course = GeometryHelpers.BearingGeodetic(point1, point2);
                    _location = new MapPoint(point2.X, point2.Y, point1.SpatialReference);
                    _maneuverProgress = 0;
                }
            }
            else
            {
                // Find the next point along the current maneuver
                var point1 = _location;
                var point2 = GeometryHelpers.CreatePointAlongGeodetic(maneuverLine, _maneuverProgress + speed);

                // Update progress along maneuver
                _maneuverProgress += speed;

                // Determine whether we've reached the end of the maneuver
                if (point2 != null)
                {
                    _course = GeometryHelpers.BearingGeodetic(point1, point2);
                    _location = point2;
                }
                else
                {
                    // Move to the next maneuver (otherwise it waits a tick to move)
                    UpdateFollow(speed);
                }
            }
        }

        private void UpdateSeek(double speed)
        {
            if (_location == null || _target == null)
                return;

            // Determine distance between current location and target location
            var distanceResult = GeometryEngine.DistanceGeodetic(_location, _target, LinearUnits.Meters,
                AngularUnits.Degrees, GeodeticCurveType.Geodesic);
            var distance = distanceResult.Distance;
            
            if (speed <= distance)
            {
                // Move toward the point if we're not "close enough"
                _course = GeometryHelpers.BearingGeodetic(_location, _target);
                _location = GeometryHelpers.CreatePointAlongGeodetic(_location, _course, speed);
            }
            else
            {
                // We've reached the point, start following the route
                State = SimulationState.Following;
            }
        }

        private void UpdateWander(double speed)
        {
            if (_location == null)
                return;

            // Find closest point on the route
            var nearestResult = GeometryEngine.NearestCoordinate(_route.RouteGeometry, _location);
            var nearest = nearestResult.Coordinate;

            // Move away from the nearest coordinate
            var bearingToward = GeometryHelpers.BearingGeodetic(_location, nearest);
            var bearing = GeometryHelpers.NormalizeBearing(bearingToward + 180); // directly away

            // Keep on moving
            _course = bearing;
            _location = GeometryHelpers.CreatePointAlongGeodetic(_location, bearing, speed);
        }

        private DirectionManeuver GetCurrentManeuver()
        {
            return _route.DirectionManeuvers[_maneuverIndex];
        }

        private DirectionManeuver GetNextManeuver()
        {
            _maneuverIndex++;
            if (_maneuverIndex >= _route.DirectionManeuvers.Count)
                return null;

            return _route.DirectionManeuvers[_maneuverIndex];
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the state of the simulation.
        /// </summary>
        public SimulationState State { get; private set; }

        /// <summary>
        /// Gets or sets the location of the simulation.
        /// </summary>
        public MapPoint Location
        {
            get => _location;
            set
            {
                if (value != null && _location.X != value.X && _location.Y != value.Y)
                {
                    _location = value;
                }
            }
        }

        #endregion
    }
}
