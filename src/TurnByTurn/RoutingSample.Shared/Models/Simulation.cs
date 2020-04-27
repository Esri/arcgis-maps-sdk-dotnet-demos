#if false
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Tasks.NetworkAnalysis;

namespace RoutingSample.Models
{
    public class Simulation : ModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Simulation"/> class.
        /// </summary>
        /// <param name="start">The start location of the simulator.</param>
        public Simulation(MapPoint start)
        {
            LocationDataSource = new SimulatedLocationDataSource(start);
        }

        ///// <summary>
        ///// Restarts the simulation.
        ///// </summary>
        //public void Restart()
        //{
        //    Simulator?.SeekStart();
        //    OnPropertyChanged(nameof(State));
        //}

        ///// <summary>
        ///// Starts the simulation along the route.
        ///// </summary>
        //public void StartFollowing()
        //{
        //    if (State == SimulationState.Stopped ||
        //        State == SimulationState.Wandering)
        //    {
        //        Simulator?.SeekClosestCoordinate();
        //        OnPropertyChanged(nameof(State));
        //    }
        //}

        ///// <summary>
        ///// Starts the simulation moving away from the route.
        ///// </summary>
        //public void StartWandering()
        //{
        //    if (State != SimulationState.Wandering)
        //    {
        //        Simulator?.Wander();
        //        OnPropertyChanged(nameof(State));
        //    }
        //}

        ///// <summary>
        ///// Stops the simulation.
        ///// </summary>
        //public void Stop()
        //{
        //    if (State != SimulationState.Stopped)
        //    {
        //        Simulator?.Stop();
        //        OnPropertyChanged(nameof(State));
        //    }
        //}

        /// <summary>
        /// Sets the route to be followed.
        /// </summary>
        /// <param name="route"></param>
        public void SetRoute(Route route) => LocationDataSource.Route = route.RouteGeometry as Polyline; //Simulator.SetRoute(route);

        ///// <summary>
        ///// Gets the simulator.
        ///// </summary>
        //public SimulatedLocationDataSource2 Simulator { get; }

        ///// <summary>
        ///// Gets the simulation state.
        ///// </summary>
        //public SimulationState State => Simulator?.State ?? SimulationState.Stopped;

        /// <summary>
        /// TODO
        /// </summary>
        public SimulatedLocationDataSource LocationDataSource { get; }
    }
}
#endif