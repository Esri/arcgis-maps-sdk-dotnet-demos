# Accessible Feature Map

This sample demonstrates how an ArcGIS Maps SDK app can provide dynamic, audible descriptions of map content,
offering a functional alternative for users who rely on assistive technologies (e.g. screen readers).

When used with a screen reader (e.g. Windows Narrator), the map automatically announces meaningful location changes,
zoom levels, and summaries of visible features.

## Key APIs Used

* [`AutomationPeer.RaiseNotificationEvent`](https://learn.microsoft.com/en-us/dotnet/api/system.windows.automation.peers.automationpeer.raisenotificationevent) to send audible announcements to screen readers.
* [`GeoView.NavigationCompleted`](https://developers.arcgis.com/net/api-reference/api/netwin/wpf/Esri.ArcGISRuntime.UI.Controls.GeoView.NavigationCompleted.html) event to detect navigation changes.
* [`GeoView.GetCurrentViewpoint`](https://developers.arcgis.com/net/api-reference/api/netwin/wpf/Esri.ArcGISRuntime.UI.Controls.GeoView.GetCurrentViewpoint.html) to get the currently-visible geographic area.
* [`FeatureTable.QueryFeaturesAsync`](https://developers.arcgis.com/net/api-reference/api/netwin/Esri.ArcGISRuntime/Esri.ArcGISRuntime.Data.FeatureTable.QueryFeaturesAsync.html) to find features in viewpoint.
* [`LocatorTask.ReverseGeocodeAsync`](https://developers.arcgis.com/net/api-reference/api/netwin/Esri.ArcGISRuntime/Esri.ArcGISRuntime.Tasks.Geocoding.LocatorTask.ReverseGeocodeAsync.html) to provide human-readable description of any location.
* [`ScaleLine`](https://github.com/Esri/arcgis-maps-sdk-dotnet-toolkit/blob/main/docs/scale-line.md) component from ArcGIS Maps SDK for .NET Toolkit.

## Further Reading

* [WCAG Success Criterion 1.1.1: Non-text Content](https://www.w3.org/WAI/WCAG21/Understanding/non-text-content.html)
* [WCAG Success Criterion 4.1.3: Status Messages](https://www.w3.org/WAI/WCAG21/Understanding/status-messages.html)
* [Accessibility essentials for GIS and mapping](https://www.esri.com/arcgis-blog/products/product/mapping/accessibility-essentials-for-gis-and-mapping)
