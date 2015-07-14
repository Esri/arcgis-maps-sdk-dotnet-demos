Demo: KML 2D/3D Viewer
=======================
Required version: ArcGIS Runtime SDK 10.2.6 for .NET

Windows Store app for opening KML and KMZ Files in a 3D or 2D View.

### Notable classes:
* [`XInputSceneController.cs`](XInputHelper\XInputSceneController.cs) - Adds support for using a game controller to navigate the 3D View
* [`CompassHeading.xaml.cs`](KmlViewer.Windows\CompassHeading.xaml.cs) - Compass control for visualizing viewing direction in 3D
* [`JoystickControl.xaml.cs`](KmlViewer.Windows\JoystickControl.xaml.cs) - Touch-friendly joystick for controlling camera pitch 3D
* [`KmlTreeView.xaml.cs`](KmlViewer.Windows\KmlTreeView.xaml.cs) - An efficient KML Tree View control matching the Google Earth behavior.

<img src="Screenshot.png"/>

[](Esri Tags: ArcGIS API WinStore WinRT KML 3D C# CSharp)
[](Esri Language: DotNet)
