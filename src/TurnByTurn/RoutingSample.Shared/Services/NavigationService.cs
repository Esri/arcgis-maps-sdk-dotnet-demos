using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Http;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Tasks.NetworkAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

#if !OFFLINE
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.Tasks.Geocoding;
#endif

namespace RoutingSample.Services
{
    public class NavigationService
    {
#if OFFLINE
        private readonly string _database, _networkName;

        /// <summary>
        /// Creates a new instance of the <see cref="NavigationService"/> class.
        /// </summary>
        public NavigationService()
        {
            _database = "sandiego.geodatabase";
            _networkName = "Streets_ND";
        }

#else

        private const string ServerUrl = "https://www.arcgis.com/sharing/rest";
        private const string GeocodeServiceUrl = "https://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer";
        private const string RoutingServiceUrl = "https://route.arcgis.com/arcgis/rest/services/World/Route/NAServer/Route_World";

        private readonly Uri _geocodeServiceUri;
        private readonly Uri _routingServiceUri;
        private readonly Credential _credential;

        /// <summary>
        /// Creates a new instance of the <see cref="NavigationService"/> class.
        /// </summary>
        public NavigationService()
        {
            _geocodeServiceUri = new Uri(GeocodeServiceUrl);
            _routingServiceUri = new Uri(RoutingServiceUrl);
            _credential = AuthenticationManager.Current.FindCredential(new Uri(ServerUrl));
        }

        /// <summary>
        /// Returns the location of the specified address.
        /// </summary>
        /// <param name="address">The address to be checked.</param>
        /// <returns>The map coorindate of the address, or <c>null</c> if it does not exist.</returns>
        public async Task<MapPoint> GeocodeLocationAsync(string address)
        {
            var locator = await LocatorTask.CreateAsync(_geocodeServiceUri);
            var results = await locator.GeocodeAsync(address);
            if (results != null && results.Count > 0)
            {
                return results.First().RouteLocation as MapPoint;
            }
            return null;
        }

        /// <summary>
        /// Returns a route between the specified stops.
        /// </summary>
        /// <param name="fromAddress"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public async Task<(RouteResult, RouteTask, RouteParameters)> SolveRouteAsync(MapPoint from, string toAddress)
        {
            var to = await GeocodeLocationAsync(toAddress);
            if (to == null)
                throw new ArgumentException("Address not found.");

            return await SolveRouteAsync(from, to);
        }
#endif

        /// <summary>
        /// Returns a route between the specified stops.
        /// </summary>
        /// <param name="stops">The stops on the route.</param>
        /// <returns></returns>
        public async Task<(RouteResult, RouteTask, RouteParameters)> SolveRouteAsync(MapPoint from, MapPoint to)
        {
            RouteTask routeTask = null;
            RouteParameters routeParameters = null;
            RouteResult routeResult = null;

            try
            {
#if OFFLINE
                // Ensure the sample data is ready to go
                await DataManager.EnsureDataPresent();

                // Create a new route using the offline database
                routeTask = await RouteTask.CreateAsync(DataManager.GetDataFolder(_database), _networkName);
#else
                // Create a new route task using the worldwide routing service
                routeTask = await RouteTask.CreateAsync(_routingServiceUri, _credential);
#endif

                // Configure the route and set the stops
                routeParameters = await routeTask.CreateDefaultParametersAsync();
                routeParameters.SetStops(new[] { new Stop(from), new Stop(to) });
                routeParameters.ReturnStops = true;
                routeParameters.ReturnDirections = true;
                routeParameters.RouteShapeType = RouteShapeType.TrueShapeWithMeasures;
                routeParameters.OutputSpatialReference = SpatialReferences.Wgs84;
                routeParameters.DirectionsDistanceUnits = UnitSystem.Metric;
                routeParameters.StartTime = DateTime.UtcNow;

                // Solve the route
                routeResult = await routeTask.SolveRouteAsync(routeParameters);
            }
            catch (Exception ex)
            {
                if (!(ex is ArcGISWebException web && web.Details.FirstOrDefault()?.Contains("Unlocated") == true))
                {
                    // There was some other error.
                    throw;
                }
            }

            return (routeResult, routeTask, routeParameters);
        }
    }
}
