// /*******************************************************************************
//  * Copyright 2012-2018 Esri
//  *
//  *  Licensed under the Apache License, Version 2.0 (the "License");
//  *  you may not use this file except in compliance with the License.
//  *  You may obtain a copy of the License at
//  *
//  *  http://www.apache.org/licenses/LICENSE-2.0
//  *
//  *   Unless required by applicable law or agreed to in writing, software
//  *   distributed under the License is distributed on an "AS IS" BASIS,
//  *   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  *   See the License for the specific language governing permissions and
//  *   limitations under the License.
//  ******************************************************************************/

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.UI;
using System;

namespace GeoEventServerSample.Animations
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
                easing: new QuadraticEase() { EasingMode = EasingMode.EaseInOut }
            );
        }
    }
}
