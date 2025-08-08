# Accessible Identify Features

This sample demonstrates how to make identifying features accessible in ArcGIS Maps SDK for .NET,
making it easy to use for everyone including keyboard-only users.
The implementation provides keyboard-accessible identification of map features, sequential navigation,
grouping and paging of results, and accessible popups with descriptive content.

## How to use this sample

* **Navigate the map** using arrow keys to pan and `+`/`-` keys to zoom in or out. List of visible features will be updated automatically.
* **Select features**:
  * Press number keys (`1-7`) to directly select a listed feature.
  * Or switch focus to the feature list using `Tab` and use arrow keys.
  * Press `Enter` to display a popup for the selected feature.
* **Paging through features**:
  * Press `8` for previous features.
  * Press `9` for next features.
* **Close popups** using `Esc`.

## Key APIs Used

* [`MapView.IdentifyLayersAsync`](https://developers.arcgis.com/net/api-reference/api/netwin/wpf/Esri.ArcGISRuntime.UI.Controls.GeoView.IdentifyLayersAsync.html) identifies features within a spatial extent.
* [`PopupViewer`](https://github.com/Esri/arcgis-maps-sdk-dotnet-toolkit/blob/main/docs/popup-viewer.md) toolkit component.
* [`CollectionViewSource`](https://learn.microsoft.com/en-us/dotnet/api/system.windows.data.collectionviewsource) enables accessible grouping and paging of items.
* [`UIElement.PreviewKeyDown`](https://learn.microsoft.com/en-us/dotnet/api/system.windows.uielement.previewkeydown) event to define custom keyboard shortcuts.

## Further Reading

* [WCAG Success Criterion 2.1.1: Keyboard](https://www.w3.org/WAI/WCAG21/Understanding/keyboard)
* [WCAG Success Criterion 1.3.2: Meaningful Sequence](https://www.w3.org/WAI/WCAG21/Understanding/meaningful-sequence)
