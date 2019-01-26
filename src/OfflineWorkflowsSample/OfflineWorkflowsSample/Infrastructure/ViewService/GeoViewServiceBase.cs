using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Geometry;
using Windows.Foundation;

namespace OfflineWorkflowsSample.Infrastructure.ViewServices
{
    public abstract partial class GeoViewServiceBase<TGeoView>
        where TGeoView : GeoView
    { 
        /// <summary>
        /// Creates a new instance of <see cref="GeoViewServiceBase{TGeoView}"/> class. 
        /// </summary>
        public GeoViewServiceBase()
        {
            ServiceId = Guid.NewGuid();
        }

        /// <summary>
        /// Get <see cref="GeoView"/> that is attached to the view service.
        /// </summary>
        /// <returns>The attached view.</returns>
        /// <exception cref="NullReferenceException">Return null reference exception if the view isn't attached.</exception>
        /// <remarks>Used internally instead of <see cref="View"/> to throw error message for the user that tells what to do. </remarks>
        protected TGeoView GetView()
        {
            if (View == null)
                throw new NullReferenceException("GeoView hasn't been set into the service.");

            return View;
        }

        /// <summary>
        /// Indicates whether <see cref="GeoView"/> is attached to the controller.
        /// </summary>
        public bool HasGeoView => View != null;

        /// <summary>
        /// Gets a <see cref="Guid"/> that identifies the view service. This is useful when multiple view services are used simultaneously.
        /// </summary>
        public Guid ServiceId { get; }

        /// <summary>
        /// Gets the current <see cref="DrawStatus"/>.
        /// </summary>
        public DrawStatus DrawStatus => GetView().DrawStatus;

        /// <summary>
        /// Gets a <see cref="bool"/> that indicates whether the <see cref="GeoView"/> is currently navigating.  
        /// </summary>
        public bool IsNavigating => GetView().IsNavigating;

        /// <summary>
        /// Gets a <see cref="bool"/> that indicates whether the <see cref="GeoView"/> has wrap around feature enabled.
        /// </summary>
        public bool IsWrapAroundEnabled => GetView().IsWrapAroundEnabled;

        /// <summary>
        /// Gets or sets a value indicating whether Esri attribution text is visible.
        /// </summary>
        /// <remarks>If you are using data from ArcGIS Online, you need to show the attribution.</remarks>
        public bool IsAttributionTextVisible
        {
            get { return GetView().IsAttributionTextVisible; }
            set { GetView().IsAttributionTextVisible = value; }
        }

        /// <summary>
        ///  Gets the full attribution text for all active layers, concatenated into a single string.
        /// </summary>
        public string AttributionText => GetView().AttributionText;

        /// <summary>
        /// Gets a value indicating whether a callout is currently open
        /// </summary>
        public bool IsCalloutVisible => GetView().IsCalloutVisible;

        /// <summary>
        /// Gets the current spatial reference of the map
        /// </summary>
        public SpatialReference SpatialReference => GetView().SpatialReference;

        /// <summary>
        /// Creates an image snapshot of the current map view
        /// </summary>
        /// <returns>
        /// Returns <see cref="RuntimeImage"/> that is a snapshot of the <see cref="GeoView"/>. Note that UI controls
        /// aren't attached to the image. See extensions <see cref="RuntimeImageExtensions"/>
        /// </returns>
        public Task<RuntimeImage> ExportImageAsync()
            => GetView().ExportImageAsync();

        /// <summary>
        /// Gets the current <see cref="Viewpoint"/>of the view.
        /// </summary>
        /// <param name="viewpointType">Type of the viewpoint returned</param>
        /// <returns>Viewpoint that was requested.</returns>
        public Viewpoint GetCurrentViewpoint(ViewpointType viewpointType)
            => GetView().GetCurrentViewpoint(viewpointType);

        /// <summary>
        /// Gets the <see cref="LayerViewState"/> of the given layer.
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public LayerViewState GetLayerViewState(Layer layer)
            => View.GetLayerViewState(layer);

        /// <summary>
        /// Sets the view to the given Viewpoint location
        /// </summary>
        /// <param name="viewpoint">Viewpoint object</param>
        public void SetViewpoint(Viewpoint viewpoint) 
            => GetView().SetViewpoint(viewpoint);

        /// <summary>
        /// Cancels any pending or currently running SetViewpointAsync operations.
        /// </summary>
        public void CancelSetViewpointOperations()
            => GetView().CancelSetViewpointOperations();
        
        /// <summary>
        /// Animates the view to the given Viewpoint location
        /// </summary>
        /// <param name="viewpoint">Viewpoint object</param>
        /// <returns>True if the set view animation completed, false if it was interrupted by another view navigation.</returns>
        public Task<bool> SetViewpointAsync(Viewpoint viewpoint) 
            => GetView().SetViewpointAsync(viewpoint);

        /// <summary>
        /// Animates the view to the given Viewpoint location
        /// </summary>
        /// <param name="viewpoint">Viewpoint object</param>
        /// <param name="duration">Duration of the animation</param>
        /// <returns>
        /// True if the set view animation completed, false if it was interrupted by another view navigation
        /// </returns>
        public Task<bool> SetViewpointAsync(Viewpoint viewpoint, TimeSpan duration) 
            => GetView().SetViewpointAsync(viewpoint, duration);

        /// <summary>
        ///  Initiates an identify operation on the specified graphics overlay which will return the visible topmost graphic.
        /// </summary>
        /// <param name="graphicsOverlay">The overlay on which to run the identify.</param>
        /// <param name="screenCoordinate">The location at which to run identify in screen coordinates.</param>
        /// <param name="tolerance">The width and height in screen coordinates of the square centered on screen coordinate that will be used in the identify.</param>
        /// <param name="returnPopupsOnly">Controls whether the graphics property of the result is populated.</param>
        /// <returns> A task that represents the asynchronous identify operation on the specified graphics overlay. //</returns>
        /// <exception cref="ArgumentNullException">graphicsOverlay</exception>
        public Task<IdentifyGraphicsOverlayResult> IdentifyGraphicsOverlayAsync(
            GraphicsOverlay graphicsOverlay, Point screenCoordinate, double tolerance, bool returnPopupsOnly)
            => GetView().IdentifyGraphicsOverlayAsync(graphicsOverlay, screenCoordinate, tolerance, returnPopupsOnly);

        /// <summary>
        ///  Initiates an identify operation on the specified graphics overlay.
        /// </summary>
        /// <param name="graphicsOverlay">The overlay on which to run the identify.</param>
        /// <param name="screenCoordinate">The location at which to run identify in screen coordinates.</param>
        /// <param name="tolerance">The width and height in screen coordinates of the square centered on screen coordinate that will be used in the identify.</param>
        /// <param name="returnPopupsOnly">Controls whether the graphics property of the result is populated.</param>
        /// <param name="maximumResults">The maximum size of the result set to return.</param>
        /// <returns> A task that represents the asynchronous identify operation on the specified graphics overlay. //</returns>
        /// <exception cref="ArgumentNullException">graphicsOverlay</exception>
        public Task<IdentifyGraphicsOverlayResult> IdentifyGraphicsOverlayAsync(
            GraphicsOverlay graphicsOverlay, Point screenCoordinate, double tolerance, bool returnPopupsOnly, long maximumResults)
            => GetView().IdentifyGraphicsOverlayAsync(graphicsOverlay, screenCoordinate, tolerance, returnPopupsOnly, maximumResults);

        /// <summary>
        /// Initiate an identify operation on all graphics overlays which will return the single visible topmost graphic per overlay only.
        /// </summary>
        /// <param name="screenCoordinate">The location at which to run identify in screen coordinates.</param>
        /// <param name="tolerance">The width and height in screen coordinates of the square centered on screen coordinate that will be used in the identify.</param>
        /// <param name="returnPopupsOnly">Controls whether the graphics property of the result is populated.</param>
        /// <returns>A task that represents the asynchronous identify operation on all graphics overlays in the view.</returns>
        public Task<IReadOnlyList<IdentifyGraphicsOverlayResult>> IdentifyGraphicsOverlaysAsync(
            Point screenCoordinate, double tolerance, bool returnPopupsOnly)
            => GetView().IdentifyGraphicsOverlaysAsync(screenCoordinate, tolerance, returnPopupsOnly);

        /// <summary>
        /// Initiate an identify operation on all graphics overlays.
        /// </summary>
        /// <param name="screenCoordinate">The location at which to run identify in screen coordinates.</param>
        /// <param name="tolerance">The width and height in screen coordinates of the square centered on screen coordinate that will be used in the identify.</param>
        /// <param name="returnPopupsOnly">Controls whether the graphics property of the result is populated.</param>
        /// <param name="maximumResultsPerOverlay">The maximum size of the result set to return.</param>
        /// <returns>A task that represents the asynchronous identify operation on all graphics overlays in the view</returns>
        public Task<IReadOnlyList<IdentifyGraphicsOverlayResult>> IdentifyGraphicsOverlaysAsync(
            Point screenCoordinate, double tolerance, bool returnPopupsOnly, long maximumResultsPerOverlay)
            => GetView().IdentifyGraphicsOverlaysAsync(screenCoordinate, tolerance, returnPopupsOnly, maximumResultsPerOverlay);
        
        /// <summary>
        /// Initiates an identify operation on the specified layer which will return the single visible topmost geolement only.
        /// </summary>
        /// <param name="layer">The layer on which to run the identify.</param>
        /// <param name="screenCoordinate">The location at which to run identify in screen coordinates.</param>
        /// <param name="tolerance">The width and height in screen coordinates of the square centered on screen coordinate that will be used in the identify.</param>
        /// <param name="returnPopupsOnly">Controls whether the graphics property of the result is populated.</param>
        /// <returns>A task that represents the asynchronous identify operation on the specified layer.</returns>
        public Task<IdentifyLayerResult> IdentifyLayerAsync(
            Layer layer, Point screenCoordinate, double tolerance, bool returnPopupsOnly)
            => GetView().IdentifyLayerAsync(layer, screenCoordinate, tolerance, returnPopupsOnly);
        
        /// <summary>
        /// Initiates an identify operation on the specified layer which will return the single visible topmost geolement only.
        /// </summary>
        /// <param name="layer">The layer on which to run the identify.</param>
        /// <param name="screenCoordinate">The location at which to run identify in screen coordinates.</param>
        /// <param name="tolerance">The width and height in screen coordinates of the square centered on screen coordinate that will be used in the identify.</param>
        /// <param name="returnPopupsOnly">Controls whether the graphics property of the result is populated.</param>
        /// <param name="cancellationToken"> A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the asynchronous identify operation on the specified layer.</returns>
        public Task<IdentifyLayerResult> IdentifyLayerAsync(
            Layer layer, Point screenCoordinate, double tolerance, bool returnPopupsOnly, CancellationToken cancellationToken)
            => GetView().IdentifyLayerAsync(layer, screenCoordinate, tolerance, returnPopupsOnly, cancellationToken);

        /// <summary>
        /// Initiates an identify operation on the specified layer.
        /// </summary>
        /// <param name="layer">The layer on which to run the identify.</param>
        /// <param name="screenCoordinate">The location at which to run identify in screen coordinates.</param>
        /// <param name="tolerance">The width and height in screen coordinates of the square centered on screen coordinate that will be used in the identify</param>
        /// <param name="returnPopupsOnly">Controls whether the graphics property of the result is populated.</param>
        /// <param name="maximumResults">The maximum size of the result set of geoelements.</param>
        /// <returns>A task that represents the asynchronous identify operation on the specified layer.</returns>
        public Task<IdentifyLayerResult> IdentifyLayerAsync(
            Layer layer, Point screenCoordinate, double tolerance, bool returnPopupsOnly, long maximumResults)
            => GetView().IdentifyLayerAsync(layer, screenCoordinate, tolerance, returnPopupsOnly, maximumResults);

        /// <summary>
        /// Initiates an identify operation on the specified layer.
        /// </summary>
        /// <param name="layer">The layer on which to run the identify.</param>
        /// <param name="screenCoordinate">The location at which to run identify in screen coordinates.</param>
        /// <param name="tolerance">The width and height in screen coordinates of the square centered on screen coordinate that will be used in the identify</param>
        /// <param name="returnPopupsOnly">Controls whether the graphics property of the result is populated.</param>
        /// <param name="maximumResults">The maximum size of the result set of geoelements.</param>
        /// <param name="cancellationToken"> A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the asynchronous identify operation on the specified layer.</returns>     
        public Task<IdentifyLayerResult> IdentifyLayerAsync(
            Layer layer, Point screenCoordinate, double tolerance, bool returnPopupsOnly, long maximumResults, CancellationToken cancellationToken)
            => GetView().IdentifyLayerAsync(layer, screenCoordinate, tolerance, returnPopupsOnly, maximumResults, cancellationToken);


        /// <summary>
        /// Initiates an identify operation on all layers in the view which will return the single visible topmost geoelement per layer only.
        /// </summary>
        /// <param name="screenCoordinate">The location at which to run identify in screen coordinates.</param>
        /// <param name="tolerance">The width and height in screen coordinates of the square centered on screen coordinate that will be used in the identify</param>
        /// <param name="returnPopupsOnly">Controls whether the graphics property of the result is populated.</param>
        /// <returns> A task that represents the asynchronous identify operation on all layers in the view.</returns>
        public Task<IReadOnlyList<IdentifyLayerResult>> IdentifyLayersAsync(
            Point screenCoordinate, double tolerance, bool returnPopupsOnly)
            => GetView().IdentifyLayersAsync(screenCoordinate, tolerance, returnPopupsOnly);

        /// <summary>
        /// Initiates an identify operation on all layers in the view.
        /// </summary>
        /// <param name="screenCoordinate">The location at which to run identify in screen coordinates.</param>
        /// <param name="tolerance">The width and height in screen coordinates of the square centered on screen coordinate that will be used in the identify</param>
        /// <param name="returnPopupsOnly">Controls whether the graphics property of the result is populated.</param>
        /// <param name="maximumResultsPerLayer">The maximum number of geoelements to return per layer.</param>
        /// <returns>A task that represents the asynchronous identify operation on all layers in the view.</returns>
        public Task<IReadOnlyList<IdentifyLayerResult>> IdentifyLayersAsync(
            Point screenCoordinate, double tolerance, bool returnPopupsOnly, long maximumResultsPerLayer)
            => GetView().IdentifyLayersAsync(screenCoordinate, tolerance, returnPopupsOnly, maximumResultsPerLayer);

        /// <summary>
        /// Initiates an identify operation on all layers in the view.
        /// </summary>
        /// <param name="screenCoordinate">The location at which to run identify in screen coordinates.</param>
        /// <param name="tolerance">The width and height in screen coordinates of the square centered on screen coordinate that will be used in the identify</param>
        /// <param name="returnPopupsOnly">Controls whether the graphics property of the result is populated.</param>
        /// <param name="cancellationToken"> A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the asynchronous identify operation on all layers in the view.</returns>
        public Task<IReadOnlyList<IdentifyLayerResult>> IdentifyLayersAsync(
            Point screenCoordinate, double tolerance, bool returnPopupsOnly, CancellationToken cancellationToken)
            => GetView().IdentifyLayersAsync(screenCoordinate, tolerance, returnPopupsOnly, cancellationToken);
        
        /// <summary>
        /// Initiates an identify operation on all layers in the view.
        /// </summary>
        /// <param name="screenCoordinate">The location at which to run identify in screen coordinates.</param>
        /// <param name="tolerance">The width and height in screen coordinates of the square centered on screen coordinate that will be used in the identify</param>
        /// <param name="returnPopupsOnly">Controls whether the graphics property of the result is populated.</param>
        /// <param name="maximumResultsPerLayer">The maximum number of geoelements to return per layer.</param>
        /// <param name="cancellationToken"> A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the asynchronous identify operation on all layers in the view.</returns>
        public Task<IReadOnlyList<IdentifyLayerResult>> IdentifyLayersAsync(
            Point screenCoordinate, double tolerance, bool returnPopupsOnly, long maximumResultsPerLayer, CancellationToken cancellationToken)
            => GetView().IdentifyLayersAsync(screenCoordinate, tolerance, returnPopupsOnly, maximumResultsPerLayer, cancellationToken);

        /// <summary>
        /// Dismisses a callout if it's open.
        /// </summary>
        public void DismissCallout() => GetView().DismissCallout();

        /// <summary>
        /// Shows a callout based on a <see cref="Esri.ArcGISRuntime.UI.CalloutDefinition"/> at the given location.
        /// </summary>
        /// <param name="location">Location to anchor the callout to.</param>
        /// <param name="definition">The callout definition.</param>
        public void ShowCalloutAt(MapPoint location, CalloutDefinition definition)
            => GetView().ShowCalloutAt(location, definition);

        /// <summary>
        /// Shows a callout for the given geoelement at an appropriate location for the tap location by snapping to the geometry of the element.
        /// </summary>
        /// <param name="element">The GeoElement used to calculate the placement of the callout.</param>
        /// <param name="tapPosition">The position the user tapped the view to use for calculating an adjusted callout location.</param>
        /// <param name="definition">The callout definition.</param>
        public void ShowCalloutForGeoElement(GeoElement element, Point tapPosition, CalloutDefinition definition)
            => GetView().ShowCalloutForGeoElement(element, tapPosition, definition);

        public void AddGraphicsOverlay(GraphicsOverlay overlay) => GetView().GraphicsOverlays.Add(overlay);

        public void ClearOverlays() => GetView().GraphicsOverlays.Clear();
    }
}
