# ArcGIS Maps SDK for .NET Demos

This project contains source code for simple native .NET applications that demonstrate common workflows for utilizing and displaying spatial information.
You can also find a set of smaller feature-focused samples in the [ArcGIS Maps SDK for .NET Samples](https://github.com/Esri/arcgis-runtime-samples-dotnet) repo.

### Demos

* [`Geometry Editor`](src/GeometryEditor) - A WPF .NET app that shows extending the Geometry Editor for more advanced editing workflows. 
* [`Symbol Editor`](src/SymbolEditor) - A WPF .NET app (and MSIX installer project) that shows using mobile styles, and creating/editing vector symbol layers. 
* [`Turn-by-Turn Routing`](src/TurnByTurn) - A turn-by-turn app that shows routing and navigation on WPF, UWP, and MAUI.
* [`MAUI Portal Sign In`](src/MauiSignin) - Demonstrates using OAuth to sign into an ArcGIS Portal, retrieve and save an ArcGIS Maps SDK license, and store credentials persisting across application sessions.
* [`Using an External GPS Device`](src/ExternalNmeaGPS) - A Windows Desktop app showing how to use data from an external GPS or NMEA log file to power the MapView's LocationDisplay.
* [`Esri Campus Routing`](src/CampusRouting) - A cross-platform (Xamarin) sample demonstrating indoor geocoding and routing for a campus running on Windows Universal, Android and iOS.
* [`Electronic Navigation Charts (ENC)`](src/HydrographicsSample) - Demonstrates how to work with and render ENC data using the Hydrographics package.
* [`Geocode & Route on MouseMove`](src/GeocodeAndRoutingOnMouseMove) - Demonstrates fast reverse geocoding and routing during mouse-move on Windows Desktop (WPF) and Universal Windows Platform (UWP).
* [`Viewshed analysis in tabletop AR with Xamarin.Forms`](src/ViewshedInTabletopAR) - Demonstrates how to do a viewshed analysis in a tabletop AR scenario using a moving graphic with Xamarin.Forms.
* [`Offline workflows`](src/OfflineWorkflowsSample) - Demonstrates how to browse a portal for offline-enabled maps and take them offline. Also demonstrates how to open maps previously taken offline.
* [`KML Viewer`](src/KmlViewer) - A WinUI 3 app for visualizing KML/KMZ files in 2D and 3D.
* [`Editing geometries in a SceneView`](src/SceneViewEdit) - A Windows Desktop app demonstrating how to do geometry editing in 3D using a customer editor.
* [`Background Location Tracking`](src/BackgroundLocationTracking) - A simple sample demonstrating how to keep the location tracking functionality running while the application is in the background (when the screen is off or applications are switched).
* [`WinUI Map Viewer`](src/MapViewer) - This WinUI application attempts to bring together a number of workflows and UI components into a full ArcGIS Portal Item Viewer. It relies heavily on MVVM and a set of custom controls and views, as well as Calcite for .NET Toolkit for creating the custom UI experience.

## Resources

* [ArcGIS Maps SDK for .NET](https://developers.arcgis.com/net/)

## Issues

Find a bug or want to request a new feature?  Please let us know by submitting an issue.

## Contributing

Anyone and everyone is welcome to [contribute](CONTRIBUTING.md).

## License

Copyright 2022 Esri

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

A copy of the license is available in the repository's [license.txt](license.txt) file.
