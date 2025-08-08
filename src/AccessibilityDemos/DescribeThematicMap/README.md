# Accessible Thematic Map

This sample demonstrates how an ArcGIS Maps SDK app can provide meaningful, audible descriptions of thematic map content
making geographic data accessible to users relying on assistive technologies (e.g. screen readers).

As the user navigates the map, the app announces visible thematic layers, geographic locations,
and relevant statistical data &mdash; in this case, median household income.

## How to use this sample


Navigate the map using either the keyboard or mouse:
* **Keyboard navigation:** Use the arrow keys to pan, and `+` or `-` keys to zoom in or out.
* **Mouse navigation:** Click and drag to pan, or use the mouse wheel to zoom.
* **Explore highlighted features:** Press the number keys (`1` or `2`) to open pop-ups for highlighted features in the current view, or click/tap them directly.
* **Close pop-ups:** Press `Esc` or click outside the pop-up.

Enable a screen reader such as [Windows Narrator](https://support.microsoft.com/en-us/windows/chapter-1-introducing-narrator-7fe8fd72-541f-4536-7658-bfc37ddaf9c6) to hear the descriptive announcements. As you explore, the app will announce:

* The visible thematic layers at the current scale.
* The location at the map's center.
* The highest median income in view, along with its geographic area.

## Key APIs Used

* [`AutomationPeer.RaiseNotificationEvent`](https://learn.microsoft.com/en-us/dotnet/api/system.windows.automation.peers.automationpeer.raisenotificationevent) to communicate updates to screen readers.
* [`UIElement.PreviewKeyDown`](https://learn.microsoft.com/en-us/dotnet/api/system.windows.uielement.previewkeydown) event to define custom keyboard shortcuts.
* [`GeoView.NavigationCompleted`](https://developers.arcgis.com/net/api-reference/api/netwin/wpf/Esri.ArcGISRuntime.UI.Controls.GeoView.NavigationCompleted.html) event to detect navigation changes.
* [`ILayerContent.IsVisibleAtScale`](https://developers.arcgis.com/net/api-reference/api/netwin/Esri.ArcGISRuntime/Esri.ArcGISRuntime.Mapping.ILayerContent.IsVisibleAtScale.html) to check which layers are visible to the user.
* [`GraphicsOverlay`](https://developers.arcgis.com/net/api-reference/api/netwin/Esri.ArcGISRuntime/Esri.ArcGISRuntime.UI.GraphicsOverlay.html) to visually highlight described features map.

## Further Reading

* [WCAG Success Criterion 1.1.1: Non-text Content](https://www.w3.org/WAI/WCAG21/Understanding/non-text-content.html)
* [WCAG Success Criterion 2.1.1: Keyboard](https://www.w3.org/WAI/WCAG21/Understanding/keyboard)
* [WCAG Success Criterion 4.1.3: Status Messages](https://www.w3.org/WAI/WCAG21/Understanding/status-messages.html)
