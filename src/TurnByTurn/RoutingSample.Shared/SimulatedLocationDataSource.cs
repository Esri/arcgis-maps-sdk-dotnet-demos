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
    public class SimulatedLocationDataSource : LocationDataSource
    {
        #region Fields

        private const double DefaultSpeed = 50;
        private const double DefaultInterval = 1000;

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
#if XAMARIN
                return !IsStarted;
#else
                return;
#endif
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
            
            if (!_location.IsEqual(next))
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
