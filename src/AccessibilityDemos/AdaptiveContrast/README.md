# Adaptive Basemap Contrast

This sample demonstrates how an ArcGIS Maps SDK app can automatically adapt cartography to comply with WCAG perceivability guidelines.
It dynamically selects appropriate basemaps and grid colors based on the system's current theme and contrast settings, supporting:

* Light theme
* Dark theme
* High Contrast modes (both light and dark)

## Key APIs Used

* [`Basemap`](https://developers.arcgis.com/net/api-reference/api/netwin/Esri.ArcGISRuntime/Esri.ArcGISRuntime.Mapping.Basemap.html) to provide a visual backdrop.
* [`SystemParameters.HighContrast`](https://learn.microsoft.com/en-us/dotnet/api/system.windows.systemparameters.highcontrast) to detect system high-contrast mode.
* [`UISettings.ColorValuesChanged`](https://learn.microsoft.com/en-us/uwp/api/windows.ui.viewmanagement.uisettings.colorvalueschanged?view=winrt-19041)
  and [`SystemEvents.UserPreferenceChanged`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.win32.systemevents.userpreferencechanged)
  to respond dynamically to system theme changes.
* [`Grid`](https://developers.arcgis.com/net/api-reference/api/netwin/Esri.ArcGISRuntime/Esri.ArcGISRuntime.UI.Grid.html) to display coordinate gridlines.

## Further Reading

* [WCAG Success Criterion 1.4.3: Minimum Contrast](https://www.w3.org/WAI/WCAG20/Understanding/contrast-minimum)
* [WCAG Success Criterion 1.4.6: Enhanced Contrast](https://www.w3.org/WAI/WCAG21/Understanding/contrast-enhanced)
* [ArcGIS Blog: Working with Enhanced Contrast basemaps to improve accessibility](https://www.esri.com/arcgis-blog/products/arcgis-living-atlas/mapping/working-with-enhanced-contrast-basemaps-to-improve-accessibility)
