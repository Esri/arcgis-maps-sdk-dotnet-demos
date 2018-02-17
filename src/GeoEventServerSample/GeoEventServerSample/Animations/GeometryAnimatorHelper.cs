using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if NETFX_CORE
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
#else
using System.Windows.Media.Animation;
#endif

namespace GeoEventServerSample.Animation
{
    public static class GeometryAnimatorHelper
    {
        public static AnimationHandle AnimatePointTo(Graphic graphic, MapPoint newLocation, TimeSpan duration, GraphicsOverlay animationLayer = null)
        {
            if (!(graphic.Geometry is MapPoint))
                throw new ArgumentException("Graphic must have a point geometry");

            var start = graphic.Geometry as MapPoint;
            if (start.SpatialReference != null && newLocation.SpatialReference != null && !start.SpatialReference.IsEqual(newLocation.SpatialReference))
            {
                start = GeometryEngine.Project(start, newLocation.SpatialReference) as MapPoint;
            }
            return AnimationHandle.StartAnimation(
                duration: duration,
                onPulse: (normalizedTime) =>
                 {
                     double x = (newLocation.X - start.X) * normalizedTime + start.X;
                     double y = (newLocation.Y - start.Y) * normalizedTime + start.Y;
                     double z = double.NaN;
                     bool animateZ = newLocation.HasZ && start.HasZ;
                     if (animateZ)
                     {
                         z = (newLocation.Z - start.Z) * normalizedTime + start.Z;
                     }
                     graphic.Geometry = animateZ ? new MapPoint(x, y, z, start.SpatialReference) : new MapPoint(x, y, start.SpatialReference);
                 },
                onStart: null,
                onEnd: () => graphic.Geometry = newLocation,
                 repeat: false,
                 easing: null // new QuadraticEase() { EasingMode = EasingMode.EaseInOut }
            );
        }
    }
}
