Demo: Geometry Editor
=======================
Required version: ArcGIS Maps SDK for .NET 200.3.0

Demonstrates using the [`GeometryEditor`](https://developers.arcgis.com/net/api-reference/api/netwin/Esri.ArcGISRuntime/Esri.ArcGISRuntime.UI.Editing.GeometryEditor.html) for more advanced editing capabilities, including switching out [tools](https://developers.arcgis.com/net/api-reference/api/netwin/Esri.ArcGISRuntime/Esri.ArcGISRuntime.UI.Editing.GeometryEditor.Tool.html#Esri_ArcGISRuntime_UI_Editing_GeometryEditor_Tool) for different editing configurations based on the active edit tool, and utilitizing multiple editors to provide input for GeometryEngine operations, like Cut and Reshape.


### Notable classes:
* [`EditorToolbarController.cs`](EditorToolbarController.cs) - Manages the state of the different edit operations using [`ICommands`](https://learn.microsoft.com/en-us/dotnet/api/system.windows.input.icommand?view=net-8.0) for executing the different operations and reporting whether an operation can be executed. This class is UI agnostic and can be reused in other UI frameworks.
* [`EditorToolbar.xaml`](EditorToolbar.xaml) - This `UserControl` provides the UI portion of the editor toolbar, connecting the buttons to the `EditorToolbarController`.
* [`MyGeometryEditor.cs`](MyGeometryEditor.cs) - A subclass of the GeometryEditor that adds a set of predefined tools for Vertex Editing, Move, Rotate, and Inactive.


### Notes
For property change notifications and commands, the [.NET Community MVVM Toolkit](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/) is used.