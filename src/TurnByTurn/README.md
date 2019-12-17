Demo: Turn-by-Turn Navigation
=======================
Required version: ArcGIS Runtime SDK 100.6 for .NET

Sample app that shows how to write a turn-by-turn navigation application using the Network Analysis and Navigation APIs from the [ArcGIS Runtime SDK for.NET](https://developers.arcgis.com/net/). The app uses MVVM patterns and shares services, models, and viewmodels between all platforms through a shared project, but uses individual views suited to each specific platform.

### Simulation

The app uses a custom [`LocationDataSource`](https://developers.arcgis.com/net/latest/wpf/api-reference/html/T_Esri_ArcGISRuntime_Location_LocationDataSource.htm) to simulate driving from your current location to a selected destination and demonstrate the Navigation API. You can change the behavior of the simulated vehicle to one of the following:

* `Following` - The vehicle will move along the current route. If not on the route, it will first move towards it.
* `Wandering` - The vehicle will move away from the route. This behavior is most useful for demonstrating rerouting.
* `Stopped` - The vehicle stops moving.

### Notable Classes
* [`SimulatedLocationDataSource.cs`](RoutingSample.Shared/SimulatedLocationDataSource.cs) - A custom location provider that provides the simulated vehicle.
* [`RestoreAutoPanMode.cs`](RoutingSample.Shared/RestoreAutoPanMode.cs) - Restores the AutoPanMode of the map back to "Navigation" when the map hasn't be touched for a number of seconds - this allows the user to pan and zoom the map, and automatically go back into driving mode.

<img src="Screenshot.png"/>

