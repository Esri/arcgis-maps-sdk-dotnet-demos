using Esri.ArcGISRuntime.Geometry;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace RoutingSample
{
    public static class GeometryHelpers
    {
        // See the following for more information on the mathematics herein:
        // https://www.movable-type.co.uk/scripts/latlong.html

        /// <summary>
        /// The radius of the earth in meters.
        /// </summary>
        public const double EarthRadius = 6378137;

        /// <summary>
        /// Flattens the parts of the specified <see cref="Polyline"/> into a single segment.
        /// </summary>
        /// <param name="line">The line to be flattened.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Polyline Flatten(Polyline line) =>
            new Polyline(line.Parts.SelectMany(pt => pt.Points), line.SpatialReference);

        /// <summary>
        /// Normalizes the specified bearing to [0, 360) degrees.
        /// </summary>
        /// <param name="bearing">The bearing, in degrees, to be normalized.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double NormalizeBearing(double bearing) => bearing % 360;

        /// <summary>
        /// Returns the point at a given geodesic distance along a line.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="distance">The geodesic distance along the line.</param>
        /// <returns>
        /// A point along the line; if <paramref name="distance"/> is longer than the line, returns <c>null</c>.
        /// </returns>
        public static MapPoint CreatePointAlongGeodetic(Polyline line, double distance)
        {
            // Determine the points that bound `distance`
            var points = line.Parts.SelectMany(part => part.Points).ToArray();
            var searchDistance = 0.0;

            for (int i = 0; i < points.Length - 1; i++)
            {
                var point1 = points[i];
                var point2 = points[i + 1];

                // Ignore part boundaries
                if (point1.X == point2.X && point1.Y == point2.Y)
                    continue;

                // Find the distance between each point
                var pointDistanceResult = GeometryEngine.DistanceGeodetic(point1, point2, LinearUnits.Meters,
                    AngularUnits.Degrees, GeodeticCurveType.Geodesic);
                var pointDistance = pointDistanceResult.Distance;

                // Determine whether `distance` falls between these points
                if (distance < searchDistance + pointDistance)
                {
                    var segmentDistance = distance - searchDistance;

                    // Find the extact position of the point
                    // TODO: return bearing too?
                    return CreatePointAlongGeodetic(point1, BearingGeodetic(point1, point2), segmentDistance);
                }

                // Continue the search
                searchDistance += pointDistance;
            }

            // Point could not be found
            return null;
        }

        /// <summary>
        /// Returns the point at the given geodesic distance along a line.
        /// </summary>
        /// <param name="start">The starting point of the line.</param>
        /// <param name="heading">The heading of the line.</param>
        /// <param name="distance">The distance along the line.</param>
        /// <returns></returns>
        public static MapPoint CreatePointAlongGeodetic(MapPoint start, double heading, double distance)
        {
            // Convert to radians
            var brng = heading / 180 * Math.PI;
            var lon1 = start.X / 180 * Math.PI;
            var lat1 = start.Y / 180 * Math.PI;
            var dR = distance / EarthRadius; // Angular distance in radians

            // Find the point along the line
            var lat2 = Math.Asin(Math.Sin(lat1) * Math.Cos(dR) + Math.Cos(lat1) * Math.Sin(dR) * Math.Cos(brng));
            var lon2 = lon1 + Math.Atan2(Math.Sin(brng) * Math.Sin(dR) * Math.Cos(lat1), Math.Cos(dR) - Math.Sin(lat1) * Math.Sin(lat2));

            // Convert back to degrees
            var lon = lon2 / Math.PI * 180;
            var lat = lat2 / Math.PI * 180;

            // Normalize the values
            while (lon < -180) lon += 360;
            while (lat < -90) lat += 180;
            while (lon > 180) lon -= 360;
            while (lat > 90) lat -= 180;

            return new MapPoint(lon, lat, start.SpatialReference);
        }

        /// <summary>
        /// Returns the bearing of a point relative to another.
        /// </summary>
        /// <param name="point1">The first point.</param>
        /// <param name="point2">The second point.</param>
        /// <returns>
        /// The bearing of <paramref name="point1"/> relative to <paramref name="point2"/>.
        /// </returns>
        public static double BearingGeodetic(MapPoint point1, MapPoint point2)
        {
            // Convert the points to radians
            var lon1 = point1.X / 180 * Math.PI;
            var lat1 = point1.Y / 180 * Math.PI;
            var lon2 = point2.X / 180 * Math.PI;
            var lat2 = point2.Y / 180 * Math.PI;

            // Determine the bearing
            var y = Math.Sin(lon1 - lon2) * Math.Cos(lat2);
            var x = Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(lon1 - lon2);
            var brng = Math.Atan2(y, x) % (2 * Math.PI);

            // Convert back to degrees
            return NormalizeBearing(360 - (brng / Math.PI * 180));
        }
    }
}
