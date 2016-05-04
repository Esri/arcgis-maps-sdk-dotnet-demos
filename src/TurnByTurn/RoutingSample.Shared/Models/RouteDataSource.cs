using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.NetworkAnalyst;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if NETFX_CORE
using Windows.UI;
#else
using System.Windows.Media;
#endif

namespace RoutingSample
{
	/// <summary>
	/// Data source that wraps a route result and based on a location exposes properties
	/// like next turn, distance, etc
	/// </summary>
	public class RouteDataSource : ModelBase
	{
		private readonly RouteResult m_route;

		/// <summary>
		/// Initializes a new instance of the <see cref="RouteDataSource"/> class.
		/// </summary>
		/// <param name="route">The route.</param>
		public RouteDataSource(RouteResult route)
		{
			m_route = route;
			if (IsDesignMode) //Design time data
			{
				DistanceToDestination = 1000;
				DistanceToWaypoint = 500;
				TimeToWaypoint = new TimeSpan(1, 2, 3);
				TimeToDestination = new TimeSpan(2, 3, 4);
				NextManeuver = "Turn right onto Main St.";
			}
			else
			{
				InitializeRoute();
			}
		}

        public GraphicsOverlayCollection RouteResultOverlays { get; private set; }

        public string NextManeuver { get; private set; }
		public MapPoint WaypointLocation { get; private set; }

		public double DistanceToDestination { get; private set; }

		public string MilesToDestination
		{
			get { return MetersToMilesFeet(DistanceToDestination); }
		}

		public TimeSpan TimeToDestination { get; private set; }

		public TimeSpan TimeToWaypoint { get; private set; }

		public double DistanceToWaypoint { get; private set; }

		public string MilesToWaypoint
		{
			get { return MetersToMilesFeet(DistanceToWaypoint); }
		}

		public MapPoint SnappedLocation { get; private set; }

		public Uri ManeuverImage { get; private set; }

		private string MetersToMilesFeet(double distance)
		{

			var miles = LinearUnits.Miles.ConvertFromMeters(distance);
			if (miles >= 10)
				return string.Format("{0:0} mi", miles);
			if (miles >= 1)
				return string.Format("{0:0.0} mi", miles);
			else if (miles >= .25)
				return string.Format("{0:0.00} mi", miles);
			else //less than .25mi
				return string.Format("{0:0} ft", LinearUnits.Feet.ConvertFromMeters(distance));
		}

		private void InitializeRoute()
		{
            var routeLinesOverlay = new GraphicsOverlay()
            {
                Renderer = new SimpleRenderer()
                {
                    Symbol = new SimpleLineSymbol()
                    {
                        Width = 10,
                        Color = Color.FromArgb(75, 50, 50, 255)
                    }
                }
            };
            var maneuversOverlay = new GraphicsOverlay()
            {
                Renderer = new SimpleRenderer()
                {
                    Symbol = new SimpleLineSymbol()
                    {
                        Width = 10,
                        Color = Colors.Black
                    }
                }
            };
            RouteResultOverlays = new GraphicsOverlayCollection();
            RouteResultOverlays.Add(routeLinesOverlay);
            RouteResultOverlays.Add(maneuversOverlay);
            foreach (var directions in m_route.Routes)
			{
                routeLinesOverlay.Graphics.Add(new Graphic() { Geometry = CombineParts(directions.RouteGeometry as Polyline) });
				var turns = (from a in directions.DirectionManeuvers select a.Geometry).OfType<Polyline>().Select(line => line.Parts.GetPartsAsPoints().First().First());
				foreach (var m in turns)
				{
                    maneuversOverlay.Graphics.Add(new Graphic() { Geometry = m });
				}
			}
		}

		/// <summary>
		/// Call this to set your current location and update directions based on that.
		/// </summary>
		/// <param name="location"></param>
		public void SetCurrentLocation(MapPoint location)
		{
			List<string> propertyNames = new List<string>(new string[] {
						"NextManeuver","WaypointLocation", "SnappedLocation", "CurrentDirection", "TimeToWaypoint", 
						"DistanceToDestination", "DistanceToWaypoint", "TimeToDestination",
						"MilesToDestination", "MilesToWaypoint", 
					});
			DirectionManeuver closest = null;
			double distance = double.NaN;
			MapPoint snappedLocation = null;
			Route direction = null;
			// Find the route part that we are currently on by snapping to each segment and see which one is the closest
			foreach (var dir in m_route.Routes)
			{
				var closestCandidate = (from a in dir.DirectionManeuvers
										where a.Geometry is Polyline
										select new { Direction = a, Proximity = GeometryEngine.NearestCoordinate(a.Geometry, location) }).OrderBy(b => b.Proximity.Distance).FirstOrDefault();
				if (double.IsNaN(distance) || distance < closestCandidate.Proximity.Distance)
				{
					distance = closestCandidate.Proximity.Distance;
					closest = closestCandidate.Direction;
					snappedLocation = closestCandidate.Proximity.Point;
					direction = dir;
				}
			}
			if (closest != null)
			{
				var directions = direction.DirectionManeuvers.ToList();
				var idx = directions.IndexOf(closest);
				if (idx < directions.Count)
				{
					DirectionManeuver next = directions[idx + 1];

					//calculate how much is left of current route segment
					var segment = closest.Geometry as Polyline;
					var proximity = GeometryEngine.NearestVertex(segment, snappedLocation);
					double frac = 1 - GetFractionAlongLine(segment, proximity, snappedLocation);
					TimeSpan timeLeft = new TimeSpan((long)(closest.Duration.Ticks * frac));
					double segmentLengthLeft = (Convert.ToDouble(closest.Length)) * frac;
					//Sum up the time and lengths for the remaining route segments
					TimeSpan totalTimeLeft = timeLeft;
					double totallength = segmentLengthLeft;
					for (int i = idx + 1; i < directions.Count; i++)
					{
						totalTimeLeft += directions[i].Duration;
						totallength += directions[i].Length;
					}

					//Update properties
					TimeToWaypoint = TimeSpan.FromSeconds(Math.Round(timeLeft.TotalSeconds));
					TimeToDestination = TimeSpan.FromSeconds(Math.Round(totalTimeLeft.TotalSeconds));
					DistanceToWaypoint = Math.Round(segmentLengthLeft);
					DistanceToDestination = Math.Round(totallength);
					SnappedLocation = snappedLocation;
					var maneuverType = next.ManeuverType;
					WaypointLocation = segment.Parts.Last().LastOrDefault().EndPoint;
#if NETFX_CORE || WINDOWS_PHONE
					var maneuverUri = new Uri(string.Format("ms-appx:///Assets/Maneuvers/{0}.png", maneuverType));
#else
					var maneuverUri = new Uri(string.Format("pack://application:,,,/Assets/Maneuvers/{0}.png", maneuverType));
#endif
					if (ManeuverImage != maneuverUri)
					{
						ManeuverImage = maneuverUri;
						propertyNames.Add("ManeuverImage");
					}
					NextManeuver = next.DirectionText;

					RaisePropertiesChanged(propertyNames);
				}
			}
		}

		private static Polyline CombineParts(Polyline line)
		{
			List<MapPoint> vertices = new List<MapPoint>();
			foreach(var part in line.Parts.GetPartsAsPoints())
			{
				foreach (var p in part)
					vertices.Add(p);
			}
			return new Polyline(vertices, line.SpatialReference);
		}
		
		// calculates how far down a line a certain point on the line is located as a value from 0..1
		private double GetFractionAlongLine(Polyline segment, ProximityResult proximity, MapPoint location)
		{
			double distance1 = 0;
			double distance2 = 0;
			int pointIndex = proximity.PointIndex;
			int vertexCount = segment.Parts.GetPartsAsPoints().First().Count();
			var vertexPoint = segment.Parts.GetPartsAsPoints().ElementAt(proximity.PartIndex).ElementAt(pointIndex);
			MapPoint previousPoint;
			int onSegmentIndex = 0;
			//Detect which line segment we currently are on
			if (pointIndex == 0) //Snapped to first vertex
				onSegmentIndex = 0;
			else if (pointIndex == vertexCount - 1) //Snapped to last vertex
				onSegmentIndex = segment.Parts.GetPartsAsPoints().First().Count() - 2;
			else
			{
				MapPoint nextPoint = segment.Parts.GetPartsAsPoints().First().ElementAt(pointIndex + 1);
				var d1 = GeometryEngine.Distance(vertexPoint, nextPoint);
				var d2 = GeometryEngine.Distance(location, nextPoint);
				if (d1 < d2)
					onSegmentIndex = pointIndex - 1;
				else
					onSegmentIndex = pointIndex;
			}
			previousPoint = segment.Parts.GetPartsAsPoints().First().First();
			for (int j = 1; j < onSegmentIndex + 1; j++)
			{
				MapPoint point = segment.Parts.GetPartsAsPoints().First().ElementAt(j);
				distance1 += GeometryEngine.Distance(previousPoint, point);
				previousPoint = point;
			}
			distance1 += GeometryEngine.Distance(previousPoint, location);
			previousPoint = segment.Parts.GetPartsAsPoints().First().ElementAt(onSegmentIndex + 1);
			distance2 = GeometryEngine.Distance(location, previousPoint);
			previousPoint = vertexPoint;
			for (int j = onSegmentIndex + 2; j < segment.Parts[0].Count; j++)
			{
				MapPoint point = segment.Parts.GetPartsAsPoints().First().ElementAt(j);
				distance2 += GeometryEngine.Distance(previousPoint, point);
				previousPoint = point;
			}


			//var previousPoint = proximity.PointIndex ? segment.GetPoint(proximity.PartIndex + 1, 0) : segment.GetPoint(proximity.PartIndex, proximity.PointIndex + 1);
			if (distance1 + distance2 == 0)
				return 1;
			return distance1 / (distance1 + distance2);
		}
	}
}
