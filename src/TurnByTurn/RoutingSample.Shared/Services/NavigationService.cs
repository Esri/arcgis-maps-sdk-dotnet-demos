using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Http;
using Esri.ArcGISRuntime.Tasks.NetworkAnalysis;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RoutingSample.Services
{
    public struct SolveRouteResult
    {
        /// <summary>
        /// The route result.
        /// </summary>
        public RouteResult Route;

        /// <summary>
        /// The route task.
        /// </summary>
        public RouteTask Task;

        /// <summary>
        /// The route parameters.
        /// </summary>
        public RouteParameters Parameters;
    }

    public class NavigationService
    {
        private readonly string _database, _networkName;

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationService"/> class.
        /// </summary>
        public NavigationService()
        {
            _database = "sandiego.geodatabase";
            _networkName = "Streets_ND";
        }

        /// <summary>
        /// Returns a route between the specified stops.
        /// </summary>
        /// <param name="stops">The stops on the route.</param>
        /// <returns></returns>
        public async Task<SolveRouteResult> SolveRouteAsync(MapPoint from, MapPoint to)
        {
            RouteTask routeTask = null;
            RouteParameters routeParameters = null;
            RouteResult routeResult = null;

            try
            {
                // Ensure the sample data is ready to go
                await DataManager.EnsureDataPresent();

                // Create a new route using the offline database
                routeTask = await RouteTask.CreateAsync(DataManager.GetDataFolder(_database), _networkName);

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

            return new SolveRouteResult { Route = routeResult, Task = routeTask, Parameters = routeParameters };
        }
    }
}
