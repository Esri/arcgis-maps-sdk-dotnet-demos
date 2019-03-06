using System;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using System.Threading.Tasks;
using Windows.Foundation;

namespace OfflineWorkflowsSample.Infrastructure.ViewServices
{
    public partial class MapViewService : GeoViewServiceBase<MapView>
    {
        /// <summary>
        /// Creates a new instance of <see cref="MapViewService"/> that is used to control attached MapView.
        /// </summary>
        public MapViewService() : base()
        {
        }
        
        /// <summary>
        /// Gets the current rotational heading of the map
        /// </summary>
        public double MapRotation => GetView().MapRotation;
        
        /// <summary>
        /// Gets the current scale of the map
        /// </summary>
        public double MapScale => GetView().MapScale;

        /// <summary>
        /// Converts a location in map coordinates to a screen coordinate relative to the upper-left corner of the map.
        /// </summary>
        /// <param name="location">The location in map coordinates to convert to screen coordinate.</param>
        /// <remarks>    
        /// The screen location returned is relative to the upper left corner of the map control. If you need a 
        /// location relative to another visual element, use the TransformToVisual method to create a transformation 
        /// between the map and another visual element.
        /// </remarks>
        /// <returns>Screen location in this map control's local display coordinate system</returns>
        public Point LocationToScreen(MapPoint location) => GetView().LocationToScreen(location);

        /// <summary>
        /// Converts a screen point relative to the upper left of the map into a location on the map.
        /// </summary>
        /// <param name="screenPoint">Screen point relative to the upper left</param>
        /// <returns>a location in map coordinates.</returns>
        public MapPoint ScreenToLocation(Point screenPoint) => GetView().ScreenToLocation(screenPoint);

        /// <summary>
        /// Animates the view to the given Viewpoint location using the provided animation curve
        /// </summary>
        /// <param name="viewpoint">Viewpoint object</param>
        /// <param name="duration">Duration of the animation</param>
        /// <param name="animationCurve">The animation curve for controlling the acceleration and decelleration of the animation.</param>
        /// <returns>True if the set view animation completed, false if it was interrupted by another view navigation.</returns>
        public Task<bool> SetViewpointAsync(
            Viewpoint viewpoint, TimeSpan duration, AnimationCurve animationCurve)
            => GetView().SetViewpointAsync(viewpoint, duration, animationCurve);

        /// <summary>
        /// Centers the view on the provided point.
        /// </summary>
        /// <param name="center">Point to center the view on.</param>
        /// <returns>True if the set view animation completed, false if it was interrupted by another view navigation.</returns>
        public Task<bool> SetViewpointCenterAsync(MapPoint center)
            => GetView().SetViewpointCenterAsync(center);
        
        /// <summary>
        /// Centers the view on the provided point and zooms to the provided scale.
        /// </summary>
        /// <param name="center">Point to center the view on.</param>
        /// <param name="scale">The reference scale to zoom to.</param>
        /// <returns>True if the set view animation completed, false if it was interrupted by another view navigation.</returns>
        public Task<bool> SetViewpointCenterAsync(MapPoint center, double scale) 
            => GetView().SetViewpointCenterAsync(center, scale);
        
        /// <summary>
        /// Centers the view on the provided point.
        /// </summary>
        /// <param name="latitude">Latitude in a WGS84 geographic coordinate system</param>
        /// <param name="longitude">Longitude in a WGS84 geographic coordinate system</param>
        /// <returns>True if the set view animation completed, false if it was interrupted by another view navigation.</returns>
        public Task<bool> SetViewpointCenterAsync(double latitude, double longitude)
            => GetView().SetViewpointCenterAsync(latitude, longitude);
        
        /// <summary>
        /// Centers the view on the provided point and zooms to the provided scale.
        /// </summary>
        /// <param name="latitude">Latitude in a WGS84 geographic coordinate system</param>
        /// <param name="longitude">Longitude in a WGS84 geographic coordinate system</param>
        /// <param name="scale">The reference scale to zoom to</param>
        /// <returns>True if the set view animation completed, false if it was interrupted by another view navigation.</returns>
        public Task<bool> SetViewpointCenterAsync(double latitude, double longitude, double scale)
            => GetView().SetViewpointCenterAsync(latitude, longitude, scale);

        /// <summary>
        /// Zooms to the provided geometry.
        /// </summary>
        /// <param name="boundingGeometry">The geometry to zoom to.</param>
        /// <returns>True if the zoom animation completed, false if it was interrupted by another view navigation.</returns>
        /// <remarks>If the bounding geometry is a point, the map will only pan.</remarks>
        public Task<bool> SetViewpointGeometryAsync(Geometry boundingGeometry)
            => GetView().SetViewpointGeometryAsync(boundingGeometry);
        
        /// <summary>
        /// Zooms to the provided geometry and leaves some padding around the geometry.
        /// </summary>
        /// <param name="boundingGeometry">The geometry to zoom to.</param>
        /// <param name="padding">Minimum amount of padding around the bounding geometry in pixels.</param>
        /// <returns>True if the zoom animation completed, false if it was interrupted by another view navigation.</returns>
        /// <remarks>If the bounding geometry is a point, the map will only pan.</remarks>
        public Task<bool> SetViewpointGeometryAsync(Geometry boundingGeometry, double padding)
            => GetView().SetViewpointGeometryAsync(boundingGeometry, padding);
       
        /// <summary>
        /// Sets the rotation angle of the map
        /// </summary>
        /// <param name="rotation">Rotation angle in degrees.</param>
        /// <returns>True if the rotation animation completed, false if it was interrupted by another view navigation.</returns>
        /// <remarks>Angle will be normalized between 0 and 360 degrees.</remarks>
        public Task<bool> SetViewpointRotationAsync(double rotation)
            => GetView().SetViewpointRotationAsync(rotation);
        
        /// <summary>
        /// Zooms to the given scale.
        /// </summary>
        /// <param name="scale">The scale to zoom to, ie '50000' to zoom to 1:50,000 scale.</param>
        /// <returns>True if the rotation animation completed, false if it was interrupted by another view navigation.</returns>
        public Task<bool> SetViewpointScaleAsync(double scale)
            => GetView().SetViewpointScaleAsync(scale);

        public Task ResetViewpointAsync()
        {
            MapView view = GetView();
            if (view.Map.InitialViewpoint != null)
            {
                return view.SetViewpointAsync(view.Map.InitialViewpoint);
            }

            // TODO: Is this valid?
            return null;
        }
    }
}
