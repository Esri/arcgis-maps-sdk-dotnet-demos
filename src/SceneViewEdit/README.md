scene editing demo
===============================================

Demo to show how to do custom editing using SceneView. Implements drawing and editing for points, polylines and polygons.

### Notable classes:
* [`SceneDrawHelper.cs`](SceneEditingDemo/Helpers/SceneDrawHelper.cs) - Static class that provides helper methods for drawing on the SceneView
* [`SceneEditHelper.cs`](SceneEditingDemo/Helpers/SceneEditHelper.cs) - Single instance class that wraps draw and edit functions

#### Using SceneEditHelper

SceneEditHelper warps operations for creating and editing geometries. 

Before using SceneEditHelper, it needs to be initialized. Typically this is done in the constructor.

````CSharp
SceneEditHelper.Current.Initialize(MySceneView);
````

<img src="SceneEditHelperDiagram.png"/>

#### Drawing geometries

````CSharp
try
{
	Geometry geometry = null; 
	Graphic graphic = null;

	// Execute draw logic
	switch ((DrawShape)DrawShapes.SelectedValue)
	{
		case DrawShape.Point:
			geometry = await SceneEditHelper.Current.CreatePointAsync(_drawTaskTokenSource.Token);
			graphic = new Graphic(geometry);
			_pointsOverlay.Graphics.Add(graphic);
			break;
		case DrawShape.Polygon:
			geometry = await SceneEditHelper.Current.CreatePolygonAsync(_drawTaskTokenSource.Token);
			graphic = new Graphic(geometry);
			_polygonsOverlay.Graphics.Add(graphic);
			break;
		case DrawShape.Polyline:
			geometry = await SceneEditHelper.Current.CreatePolylineAsync(_drawTaskTokenSource.Token);
			graphic = new Graphic(geometry);
			_polylinesOverlay.Graphics.Add(graphic);
			break;
		default:
			break;
	}
}
catch (TaskCanceledException tce)
{
	Debug.WriteLine("Previous draw operation was cancelled.");
}
````

#### Editing geometries

````CSharp
try
{
	switch (editGeometry.GeometryType)
	{
		case GeometryType.Point:
			editedGeometry = await SceneEditHelper.Current.EditPointAsync(
				editGeometry as MapPoint, 
				_drawTaskTokenSource.Token);
			break;
		case GeometryType.Polyline:
			_selection.SetHidden();
			editedGeometry = await SceneEditHelper.Current.EditPolylineAsync(
				editGeometry as Polyline, 
				_drawTaskTokenSource.Token);
			break;
		case GeometryType.Polygon:
			_selection.SetHidden();
			editedGeometry = await SceneEditHelper.Current.EditPolygonAsync(
				editGeometry as Polygon, 
				_drawTaskTokenSource.Token);
			break;
		default:
			break;
	}

	editedGraphic.Geometry = editedGeometry;
}
catch (TaskCanceledException tce)
{
	Debug.WriteLine("Previous edit operation was cancelled.");
}
````


<img src="screenshot1.png"/>
<img src="screenshot2.png"/>

[](Esri Tags: ArcGIS Runtime SDK .NET WinRT WinStore WPF WinPhone C# C-Sharp DotNet XAML MVVM)
[](Esri Language: DotNet)
