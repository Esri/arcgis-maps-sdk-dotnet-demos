# Accessible Geometry Editing

This sample demonstrates how an ArcGIS Maps SDK app can make interactive geometry editing accessible to all users,
including those relying on keyboard navigation.

It features keyboard-driven map navigation, geometry editing, accessible placement of points (facilities),
drawing of polylines (barriers), accelerator keys for buttons, and custom keyboard shortcuts for common editing tasks.

## How to use this sample

* **Place a facility:** Click "Place Facility" (or press `P`) to begin placing a facility.
  Use arrow keys to center the map on the desired location, then press `Enter` to place the marker.
  Use `+`/`-` keys to zoom in or out. Press `Esc` to cancel.
* **Draw a barrier:** Click "Draw Barrier" (`D`) to begin drawing.
  Use arrow keys to position vertices, press `Enter` to add each vertex, and press `Tab` to complete the barrier.
  Press `Esc` to cancel drawing.
* **Undo/Redo:** Press `Ctrl+Z` to undo or `Ctrl+Y` to redo edits.
* **Show service areas:** Click "Show Service Areas" (`S`) to run analysis and show results.
* **Reset:** Click "Reset" (`R`) to remove all geometry.

## Key APIs Used

* [`UIElement.PreviewKeyDown`](https://learn.microsoft.com/en-us/dotnet/api/system.windows.uielement.previewkeydown) event to define custom keyboard shortcuts.
* [`GeometryEditor`](https://developers.arcgis.com/net/api-reference/api/netwin/Esri.ArcGISRuntime/Esri.ArcGISRuntime.UI.Editing.GeometryEditor.html)
  for creating and editing geometries interactively.
* [`ReticleVertexTool`](https://developers.arcgis.com/net/api-reference/api/netwin/Esri.ArcGISRuntime/Esri.ArcGISRuntime.UI.Editing.ReticleVertexTool.html)
  for vertex-based geometry editing without a pointer.

## Further Reading

* [WCAG Success Criterion 2.1.1: Keyboard](https://www.w3.org/WAI/WCAG21/Understanding/keyboard)
* [Microsoft Learn: Creating access keys in WPF](https://learn.microsoft.com/en-us/dotnet/api/system.windows.controls.accesstext)
* [Esri Developer Guide: Edit Geometry](https://developers.arcgis.com/net/edit-features/edit-geometry/)
