using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows;

namespace WpfClusterer
{
    public class PointClusterer : DependencyObject, IEnumerable<Graphic>, INotifyCollectionChanged
	{
		private System.ComponentModel.BackgroundWorker clusterThread;
        //private ObservableCollection<Graphic> graphicClusters;

        private List<Graphic> GraphicClusters
        {
            get { return GraphicsOverlay.Graphics.ToList(); }
            set
            {
                GraphicsOverlay.Graphics.Clear();
                if (value == null)
                {
                    return;
                }

                foreach (var clusterGraphic in value)
                {
                    GraphicsOverlay.Graphics.Add(clusterGraphic);
                }
            }
        }
        private double m_clusterResolution = double.NaN;

		public PointClusterer()
		{
            Graphics = new List<Graphic>();

		}
        public GraphicsOverlay GraphicsOverlay { get;} = new GraphicsOverlay();

        private void CancelAsync()
		{
			lock (lockObject)
			{
				if (clusterThread != null)
				{
					clusterThread.RunWorkerCompleted -= clusterThread_RunWorkerCompleted;
					if (clusterThread.IsBusy)
						clusterThread.CancelAsync();
					clusterThread = null;
				}
			}
		}

		private void ClusterGraphicsAsync()
		{
			//CancelAsync();
			if (Graphics == null || Graphics.Count == 0 || double.IsNaN(ClusterResolution))
			{
				OnClusteringCompleted(null);
				return;
			}
			if (Radius == 0)
			{
				OnClusteringCompleted(Graphics);
				return;
			}
			lock (lockObject)
			{
				clusterThread = new System.ComponentModel.BackgroundWorker() { WorkerSupportsCancellation = true };
				clusterThread.RunWorkerCompleted += clusterThread_RunWorkerCompleted;
				var graphicsClone = Graphics.ToArray();
				double resolution = m_clusterResolution = ClusterResolution;
				int radius = Radius;
				clusterThread.DoWork += (s, e) =>
				{
					e.Result = ClusterMapPoints(graphicsClone, radius, resolution, clusterThread);
					lock (lockObject)
					{
						clusterThread = null;
					}
				};
				clusterThread.RunWorkerAsync(new object[] { graphicsClone, resolution });
			}
		}

		object lockObject = new object();
		MarkerSymbol pointSymbol = new SimpleMarkerSymbol();

		private void clusterThread_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
		{
			if (!e.Cancelled && e.Result != null)
			{
				Dictionary<int, Cluster> m_orig = e.Result as Dictionary<int, Cluster>;
				if (m_orig == null) return;

                List<Graphic> clusters =  new List<Graphic>();
				int max = 0;
				foreach (int key in m_orig.Keys) //Determine the size of the largest cluster
				{
					max = Math.Max(m_orig[key].Count, max);
				}
				foreach (int key in m_orig.Keys)
				{
					if (m_orig[key].Graphics.Count == 1)
					{
						var g = m_orig[key].Graphics[0];
						if (g.Symbol == null)
							g.Symbol = pointSymbol;
						if (currentBounds != null)
							g.Attributes["WithinBounds"] = Contains(currentBounds, (MapPoint)g.Geometry);
						clusters.Add(g);
					}
					else
					{
						var graphics = m_orig[key].Graphics;
						Graphic cluster = OnCreateGraphic(graphics,
							new MapPoint(m_orig[key].X, m_orig[key].Y), max);                        
						//cluster.Attributes["Graphics"] = graphics;
						cluster.Attributes["Count"] = graphics.Count;
						if (currentBounds != null)
							cluster.Attributes["WithinBounds"] = Contains(currentBounds, (MapPoint)cluster.Geometry);
						clusters.Add(cluster);
					}
				}
				OnClusteringCompleted(clusters);
			}
		}

		private void OnClusteringCompleted(List<Graphic> clusters)
		{
			this.GraphicClusters = null;
			if (CollectionChanged != null)
			{
				CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			}
			this.GraphicClusters = clusters;
			if (CollectionChanged != null && clusters != null && clusters.Count > 0)
			{
				var graphicsInBounds = (from a in clusters where !a.Attributes.ContainsKey("WithinBounds") || (bool)a.Attributes["WithinBounds"] select a).ToList();
				CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, graphicsInBounds, 0));
			}
		}

		/// <summary>
		/// Creates a graphic that represents a cluster.
		/// </summary>
		/// <param name="cluster">The graphics cluster.</param>
		/// <param name="center">The center of the graphic.</param>
		/// <param name="maxClusterCount">The size of the largest cluster in the collection.</param>
		/// <returns>Graphic</returns>
		protected virtual Graphic OnCreateGraphic(ObservableCollection<Graphic> cluster, MapPoint center, int maxClusterCount)
		{
			Symbol symbol = null;
			int count = cluster.Count;
			if (symbolCache.ContainsKey(count))
				symbol = symbolCache[count];
			else
			{
				CompositeSymbol csymbol = new CompositeSymbol();
				double size = (cluster.Count + 450) / 30; //min size = 15
				size = Math.Max(15, (Math.Log(cluster.Count / 10.0) * 10 + 20));

				csymbol.Symbols.Add(new SimpleMarkerSymbol()
				{
					Size = size * .75,
					Color = Color.CornflowerBlue,
					Outline = new SimpleLineSymbol() { Color = Color.White, Width = 1 }
				});
				var textsymbol = new TextSymbol()
				{
					HorizontalAlignment = Esri.ArcGISRuntime.Symbology.HorizontalAlignment.Center,
					VerticalAlignment = Esri.ArcGISRuntime.Symbology.VerticalAlignment.Middle,
					Text = count.ToString(),
                    Size = 9,
					Color = Color.White
				};
				csymbol.Symbols.Add(textsymbol);
				symbol = csymbol;
				if (count < 100) //Cache the first 100 symbols
					symbolCache[count] = symbol;
			}

            var attributes = new Dictionary<string, object>
            {
                //{ "Graphics", new ObservableCollection<Graphic>() },
                { "Count", 0 },
                { "WithinBounds", false }
            };
			return new Graphic(center, attributes, symbol);
		}

		private Dictionary<int, Symbol> symbolCache = new Dictionary<int, Symbol>();

		#region Clustering algorithm

		private static Dictionary<int, Cluster> ClusterMapPoints(IEnumerable<Graphic> graphics, int Radius, double resolution, BackgroundWorker worker)
		{
			if (worker == null || worker.CancellationPending) return null;
			double m_diameter = Radius * 2 * resolution;
			bool m_overlapExists = false;
			MapPoint ll = GetLowerLeft(graphics);
			Dictionary<int, Cluster> m_orig = assignMapPointsToClusters(graphics, ll, m_diameter);
			do // Keep merging overlapping clusters until none overlap.
			{
				if (worker.CancellationPending) return null;
				m_orig = MergeOverlappingClusters(m_diameter, m_orig, ll, out m_overlapExists, worker);
			}
			while (m_overlapExists);
			ObservableCollection<Graphic> source = new ObservableCollection<Graphic>();
			if (worker.CancellationPending) return null;
			else return m_orig;
		}

		private static Dictionary<int, Cluster> MergeOverlappingClusters(double m_diameter, Dictionary<int, Cluster> m_orig, MapPoint lowerLeft, out bool m_overlapExists, BackgroundWorker worker)
		{
			m_overlapExists = false;
			// Create a new set to hold non-overlapping clusters.            
			Dictionary<int, Cluster> dest = new Dictionary<int, Cluster>();
			foreach (int key in m_orig.Keys)
			{
				Cluster cluster = m_orig[key];
				// skip merged cluster
				if (cluster.Count == 0)
				{
					continue;
				}
				// Search all immediately adjacent clusters.
				m_overlapExists = SearchAndMerge(cluster, -1, 0, m_diameter, m_orig, m_overlapExists);
				m_overlapExists = SearchAndMerge(cluster, 0, 1, m_diameter, m_orig, m_overlapExists);
				m_overlapExists = SearchAndMerge(cluster, 0, -1, m_diameter, m_orig, m_overlapExists);
				m_overlapExists = SearchAndMerge(cluster, 1, 1, m_diameter, m_orig, m_overlapExists);
				m_overlapExists = SearchAndMerge(cluster, 1, -1, m_diameter, m_orig, m_overlapExists);
				m_overlapExists = SearchAndMerge(cluster, -1, 1, m_diameter, m_orig, m_overlapExists);
				m_overlapExists = SearchAndMerge(cluster, -1, -1, m_diameter, m_orig, m_overlapExists);

				// Find the new cluster centroid values.                
				int cx = (int)Math.Round((cluster.X - lowerLeft.X) / m_diameter);
				int cy = (int)Math.Round((cluster.Y - lowerLeft.Y) / m_diameter);
				cluster.cx = cx;
				cluster.cy = cy;
				// Compute new dictionary key.
				int ci = (cx << 16) | cy;
				dest[ci] = cluster;
				if (worker.CancellationPending) return null;
			}
			return dest;
		}

		private static bool SearchAndMerge(Cluster cluster, int ox, int oy, double m_diameter, Dictionary<int, Cluster> m_orig, bool m_overlapExists)
		{
			int cx = cluster.cx + ox;
			int cy = cluster.cy + oy;
			int ci = (cx << 16) | cy;
			if (m_orig.ContainsKey(ci) && m_orig[ci].Count > 0)
			{
				Cluster found = m_orig[ci] as Cluster;
				double dx = found.X - cluster.X;
				double dy = found.Y - cluster.Y;
				double dd = Math.Sqrt(dx * dx + dy * dy);
				// Check if there is a overlap based on distance. 
				if (dd < m_diameter && found != cluster)
				{
					m_overlapExists = true;
					Merge(cluster, found);
				}
			}
			return m_overlapExists;
		}

		/// <summary>
		/// Adjust centroid weighted by the number of map points in the cluster.
		/// The more map points a cluster has, the less it moves.  
		/// </summary>
		/// <param name="lhs"></param>
		/// <param name="rhs"></param>
		private static void Merge(Cluster lhs, Cluster rhs)
		{
			double nume = lhs.Count + rhs.Count;
			lhs.X = (lhs.Count * lhs.X + rhs.Count * rhs.X) / nume;
			lhs.Y = (lhs.Count * lhs.Y + rhs.Count * rhs.Y) / nume;
			foreach (Graphic g in rhs.Graphics)
				lhs.Graphics.Add(g);
			rhs.Graphics.Clear(); // mark the cluster as merged.
		}

		private static MapPoint GetLowerLeft(IEnumerable<Graphic> graphics)
		{
			double x = double.MaxValue;
			double y = double.MaxValue;
			foreach (Graphic g in graphics)
			{
				if (g == null || g.Geometry == null) continue;
				MapPoint geom = g.Geometry as MapPoint;
				if (geom == null) geom = g.Geometry.Extent.GetCenter();
				if (double.IsNaN(geom.X) || double.IsNaN(geom.Y)
					|| double.IsInfinity(geom.X) || double.IsInfinity(geom.Y)) continue;
				x = Math.Min(x, geom.X);
				y = Math.Min(y, geom.Y);
			}
			return new MapPoint(x, y);
		}

		private static Dictionary<int, Cluster> assignMapPointsToClusters(IEnumerable<Graphic> Sink, MapPoint lowerLeft, double m_diameter)
		{
			Dictionary<int, Cluster> m_orig = new Dictionary<int, Cluster>();
			MapPoint mapPoint = null;
			foreach (Graphic g in Sink)
			{
				if (g.Geometry == null) continue;
				if (g.Geometry is MapPoint)
					mapPoint = g.Geometry as MapPoint;
				else
				{
					Envelope extent = g.Geometry.Extent;
					if (extent == null) continue;
					mapPoint = extent.GetCenter();
				}

				// Convert world map point to screen values.
				double sx = mapPoint.X;
				double sy = mapPoint.Y;

				// Convert to cluster x/y values.                                        
				int cx = (int)Math.Round((sx - lowerLeft.X) / m_diameter);
				int cy = (int)Math.Round((sy - lowerLeft.Y) / m_diameter);

				// Convert to cluster dictionary key.                    
				int ci = (cx << 16) | cy;

				// Find existing cluster                    
				if (m_orig.ContainsKey(ci))
				{
					Cluster cluster = m_orig[ci];
					// Average centroid values based on new map point.
					cluster.X = (cluster.X + sx) / 2.0;
					cluster.Y = (cluster.Y + sy) / 2.0;
					cluster.Graphics.Add(g);
				}
				else
				{
					// Not found - create a new cluster as that index.
					m_orig[ci] = new Cluster(sx, sy, cx, cy);
					m_orig[ci].Graphics.Add(g);
				}

			}
			return m_orig;
		}

		/// <summary>
		/// Determines whether the specified point is within the extent.
		/// </summary>
		/// <param name="Extent">The extent.</param>
		/// <param name="point">The point.</param>
		/// <returns>
		/// 	<c>true</c> if the specified point is contained by the instance; otherwise, <c>false</c>.
		/// </returns>
		private static bool Contains(Envelope Extent, MapPoint point)
		{
			return (point.X >= Extent.XMin && point.X <= Extent.XMax &&
				point.Y >= Extent.YMin && point.Y <= Extent.YMax);
		}

        public static readonly DependencyProperty ClustererProperty =
            DependencyProperty.RegisterAttached("Clusterer", typeof(PointClusterer), typeof(MapView), new PropertyMetadata(null));

        public static PointClusterer GetClusterer(MapView geoView)
        {
            return geoView.GetValue(ClustererProperty) as PointClusterer;
        }

        public static void SetClusterer(MapView geoView, PointClusterer clusterer)
        {
            geoView.SetValue(ClustererProperty, clusterer);
            if (double.IsNaN(geoView.UnitsPerPixel))
            {
                PropertyChangedEventHandler changedHandler = null;
                changedHandler = (o, e) =>
                {
                    if (e.PropertyName == nameof(MapView.UnitsPerPixel) && !double.IsNaN(((MapView)o).UnitsPerPixel))
                    {
                        ((INotifyPropertyChanged)geoView).PropertyChanged -= changedHandler;
                        clusterer.ClusterResolution = ((MapView)o).UnitsPerPixel;
                    }
                };
                ((INotifyPropertyChanged)geoView).PropertyChanged += changedHandler;
            }
            geoView.NavigationCompleted += MapView_NavigationCompleted;
        }

        private static void MapView_NavigationCompleted(object sender, EventArgs e)
        {
            var mapView = (MapView)sender;
            var viewpoint = mapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry).TargetGeometry as Envelope;
            var clusterer = (PointClusterer)mapView.GetValue(ClustererProperty);
            clusterer.ClusterResolution = mapView.UnitsPerPixel;
            var extent = (Envelope)mapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry).TargetGeometry;
            clusterer.UpdateBounds(extent);
        }

        #endregion Clustering algorithm

        public int Radius
		{
			get { return (int)GetValue(RadiusProperty); }
			set { SetValue(RadiusProperty, value); }
		}

		public static readonly DependencyProperty RadiusProperty =
			DependencyProperty.Register("Radius", typeof(int), typeof(PointClusterer), new PropertyMetadata(30));

		public bool IsActive
		{
			get { return (bool)GetValue(IsActiveProperty); }
			set { SetValue(IsActiveProperty, value); }
		}

		// Using a DependencyProperty as the backing store for IsActive.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IsActiveProperty =
			DependencyProperty.Register("IsActive", typeof(bool), typeof(PointClusterer), new PropertyMetadata(true, OnIsActivePropertyChanged));

		private static void OnIsActivePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			PointClusterer cl = (PointClusterer)d;
			if (cl.m_clusterResolution != cl.ClusterResolution && cl.IsActive)
				cl.ClusterGraphicsAsync();
		}

		public double ClusterResolution
		{
			get { return (double)GetValue(ClusterResolutionProperty); }
			set { SetValue(ClusterResolutionProperty, value); }
		}

		public static readonly DependencyProperty ClusterResolutionProperty =
			DependencyProperty.Register("ClusterResolution", typeof(double), typeof(PointClusterer),
			new PropertyMetadata(double.NaN, OnClusterResolutionChanged));

		private static void OnClusterResolutionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			PointClusterer cl = (PointClusterer)d;
			cl.CancelAsync();
			if (cl.IsActive)
				cl.ClusterGraphicsAsync();
		}


		public List<Graphic> Graphics
		{
			get { return (List<Graphic>)GetValue(GraphicsProperty); }
			set { SetValue(GraphicsProperty, value); }
		}

		public static readonly DependencyProperty GraphicsProperty =
			DependencyProperty.Register("Graphics", typeof(List<Graphic>), typeof(PointClusterer), new PropertyMetadata(null, OnGraphicsPropertyChanged));

		private static void OnGraphicsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			PointClusterer cl = (PointClusterer)d;
			cl.CancelAsync();
			if (cl.IsActive)
				cl.ClusterGraphicsAsync();
		}

		private Envelope currentBounds;
		public void UpdateBounds(Envelope env)
		{
			currentBounds = env;
			if (clusterThread != null)
				return;
			List<Graphic> added = new List<Graphic>();
			var cluster = GraphicClusters;
			if (cluster != null)
				foreach (var graphic in cluster)
				{
					if (Contains(env, (MapPoint)graphic.Geometry))
					{
						if (graphic.Attributes.ContainsKey("WithinBounds") && !(bool)graphic.Attributes["WithinBounds"])
						{
							graphic.Attributes["WithinBounds"] = true;
							added.Add(graphic);
						}
					}
				}
			if (cluster == GraphicClusters) //Ensure collection didn't change while processing
				if (CollectionChanged != null && added.Count > 0)
					CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, added, 0));
		
		}
		
		public IEnumerator<Graphic> GetEnumerator()
		{
			if (GraphicClusters != null)
				foreach (var graphic in GraphicClusters)
					if (!graphic.Attributes.ContainsKey("WithinBounds") ||
						(bool)graphic.Attributes["WithinBounds"])
						yield return graphic;
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public event NotifyCollectionChangedEventHandler CollectionChanged;
	}

	internal class Cluster : MapPointBuilder
	{
		/// <summary>
		/// Number of map points in the cluster 
		/// </summary>
		public int Count { get { return Graphics.Count; } }
		/// <summary>
		/// Centroid x value
		/// </summary>
		public int cx { get; set; }
		/// <summary>
		/// Centroid y value 
		/// </summary>
		public int cy { get; set; }

		public Cluster(double x, double y, int cx, int cy)
			: base(x, y)
		{
			this.cx = cx;
			this.cy = cy;
			Graphics = new ObservableCollection<Graphic>();
		}

		public ObservableCollection<Graphic> Graphics { get; set; }
	}
}
