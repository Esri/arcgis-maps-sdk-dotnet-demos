using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Navigation;
using Esri.ArcGISRuntime.Tasks.NetworkAnalysis;
using System;
using System.Linq;
using System.Threading.Tasks;
#if XAMARIN
using Xamarin.Forms;
#elif NETFX_CORE
using Windows.UI.Xaml;
#else
using System.Windows.Threading;
#endif

namespace RoutingSample
{
    /*
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
    */

    /*
    [Obsolete("Don't use me anymore.")]
    public class SimulatedLocationDataSource2 : LocationDataSource
    {
        #region Fields

        /// <summary>
        /// The speed of the simulated vehicle in KPH.
        /// </summary>
        public const double Speed = 50.0;

#if !XAMARIN
        private DispatcherTimer _timer;
#endif
        private MapPoint _location;
        private MapPoint _target;

        private double _course;
        private Route _route;
        private int _maneuverIndex;
        private double _maneuverProgress;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SimulatedLocationDataSource2"/>
        /// with the specified start location.
        /// </summary>
        public SimulatedLocationDataSource2(MapPoint location)
        {
#if !XAMARIN
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += OnTimerTick;
#endif

            _location = location ?? throw new ArgumentNullException(nameof(location));
            State = SimulationState.Stopped;
        }

        #endregion

        #region Methods

        protected override Task OnStartAsync()
        {
#if XAMARIN
            Device.StartTimer(TimeSpan.FromSeconds(1), OnTimerTick);
#else
            _timer.Start();
#endif
            return Task.CompletedTask;
        }

        protected override Task OnStopAsync()
        {
#if !XAMARIN
            _timer.Stop();
#endif
            return Task.CompletedTask;
        }

#if XAMARIN
        private bool OnTimerTick()
#elif NETFX_CORE
        private void OnTimerTick(object sender, object e)
#else
        private void OnTimerTick(object sender, EventArgs e)
#endif
        {
#if XAMARIN
            // TODO: Can we get actual time between ticks?
            var speed = 1.0 * Speed;
#else
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
    */

    public class SimulatedLocationDataSource : LocationDataSource
    {
        #region Fields

        private const double DefaultSpeed = 50;
        private const double DefaultInterval = 500;

#if !XAMARIN
        private DispatcherTimer _timer;
#endif

        private double _speed;
        private MapPoint _location;
        private double _heading;

        private Polyline _route;
        private double _routeProgress;
        private double _routeLength;

        #endregion

        #region Constructors

        public SimulatedLocationDataSource(MapPoint location)
        {
#if !XAMARIN
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(DefaultInterval);
            _timer.Tick += OnTick;
#endif

            _location = location ?? throw new ArgumentNullException(nameof(location));
            _speed = DefaultSpeed;
        }

        #endregion

        #region Methods

        public void SetRoute(Route route) => Route = route?.RouteGeometry as Polyline;

        protected override Task OnStartAsync()
        {
#if !XAMARIN
            _timer.Start();
#else
            Device.StartTimer(TimeSpan.FromMilliseconds(DefaultInterval), OnTick);
#endif

            return Task.CompletedTask;
        }

        protected override Task OnStopAsync()
        {
#if !XAMARIN
            _timer.Stop();
#endif

            return Task.CompletedTask;
        }

#if XAMARIN
        private bool OnTick()
#elif NETFX_CORE
        private void OnTick(object sender, object e)
#else
        private void OnTick(object sender, EventArgs e)
#endif
        {
            if (_route == null)
            {
                UpdateLocation(new Location(_location, 0.001, 0.0, _heading, false));
                return;
            }

#if XAMARIN
            var speed = Speed;
#else
            var speed = _timer.Interval.TotalSeconds * Speed;
#endif
            
            // If possible, move to the next point along the line; otherwise, snap to the end of the line.
            var nextProgress = _routeProgress + speed;
            var next = nextProgress <= _routeLength
                ? GeometryHelpers.CreatePointAlongGeodetic(_route, nextProgress)
                : _route.Parts.Last().EndPoint;
            
            if (/*next != null &&*/ !_location.IsEqual(next))
            {
                _heading = GeometryHelpers.BearingGeodetic(_location, next);
                _location = next;
                _routeProgress = nextProgress;
            }

            UpdateLocation(new Location(_location, 0.001, speed, _heading, false));
#if XAMARIN
            return !IsStarted;
#endif
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the speed, in KPH, of the data source.
        /// </summary>
        public double Speed
        {
            get => _speed;
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(Speed));

                if (_speed != value)
                    _speed = value;
            }
        }

        /// <summary>
        /// Gets or sets the current location of the data source.
        /// </summary>
        public MapPoint Location
        {
            get => _location;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(Location));

                if (_location.X != value.X || _location.Y != value.Y)
                    _location = value;
            }
        }

        /// <summary>
        /// Gets or sets the current route.
        /// </summary>
        public Polyline Route
        {
            get => _route;
            set
            {
                if (_route != value)
                {
                    _route = value;
                    _routeLength = _route != null ? GeometryEngine.LengthGeodetic(_route) : 0;
                    _routeProgress = 0;
                }
            }
        }

        #endregion
    }
}
