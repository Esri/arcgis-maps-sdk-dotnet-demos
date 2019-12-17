using Esri.ArcGISRuntime.Mapping;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace KmlViewer
{
    public class BasemapLayer
    {
        public string Name { get; set; }
        public string Thumbnail { get; set; }
        public string Source { get; set; }
        public LayerType Type { get; set; }
    }
    public enum LayerType { OpenStreetMap, ArcGISTiledLayer }
    public class LayerList : List<BasemapLayer> { }

	/// <summary>
	/// Set the selected index to smoothly fade to a layer from the previously selected layer.
	/// Only one layer is allowed to be on at a time, so child layers are displayed in an 'exclusive' mode.
	/// </summary>
	public class FadeGroupLayer : Basemap, INotifyPropertyChanged
	{
		private int m_SelectedIndex;

		/// <summary>
		/// Initializes a new instance of the <see cref="FadeGroupLayer"/> class.
		/// </summary>
		public FadeGroupLayer()
		{
			this.BaseLayers.CollectionChanged += ChildLayers_CollectionChanged;
		}

		/// <summary>
		/// Handles the CollectionChanged event of the ChildLayers control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
		private void ChildLayers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			for (int i = 0; i < BaseLayers.Count; i++)
			{
                BaseLayers[i].IsVisible = i == SelectedIndex;
			}
			if (BaseLayers.Count < SelectedIndex) //If selected index is out of range, select the nearest one
			{
                BaseLayers[BaseLayers.Count - 1].IsVisible = true;
			}
			OnPropertyChanged(nameof(SelectedLayer));
		}

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // TODO
        }

		public Layer SelectedLayer
		{
			get
			{
				if (BaseLayers.Count == 0 || SelectedIndex < 0)
					return null;
				return BaseLayers[Math.Min(SelectedIndex, BaseLayers.Count - 1)];
			}
		}

        public List<string> Thumbnails { get; set; } = new List<string>();

		/// <summary>
		/// Gets or sets the index of the active layer.
		/// </summary>
		/// <value>
		/// The index of the active layer.
		/// </value>
		public int SelectedIndex
		{
			get { return m_SelectedIndex; }
			set
			{
				if (value < 0) return;
				if (m_SelectedIndex != value)
				{
					Fade(m_SelectedIndex, value);
					m_SelectedIndex = value;
					OnPropertyChanged();
					OnPropertyChanged(nameof(SelectedLayer));
				}
			}
		}
		private System.Threading.Tasks.TaskCompletionSource<object> tcs;
		private System.Threading.CancellationTokenSource cancelSource;
		private async void Fade(int fromId, int toId)
		{
			//System.Diagnostics.Debug.WriteLine("Fade requested {0} to {1}", fromId, toId);
			if (fromId == toId)
				return;
			if (BaseLayers.Count < fromId || BaseLayers.Count < toId || toId < 0 || fromId < 0)
				return;
			if (tcs != null)
			{
				cancelSource.Cancel();
				await tcs.Task;
			}
			var from = BaseLayers[fromId];
			var to = BaseLayers[toId];
			if(from.LoadStatus != Esri.ArcGISRuntime.LoadStatus.Loaded || to.LoadStatus != Esri.ArcGISRuntime.LoadStatus.Loaded)
			{
				from.IsVisible = false;
				to.IsVisible = true;
				return;
			}
			DispatcherTimer t = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(10) };
			var step = t.Interval.TotalMilliseconds / TimeSpan.FromSeconds(.5).TotalMilliseconds;
			var localTcs = tcs = new System.Threading.Tasks.TaskCompletionSource<object>();
			cancelSource = new System.Threading.CancellationTokenSource();
			var token = cancelSource.Token;

			if (toId > fromId) //Layer on top got turned on - Fade it in
			{
				to.Opacity = 0;
				to.IsVisible = true;
				t.Tick += (s, e2) =>
				{
					to.Opacity = Math.Min(1, to.Opacity + step);

					if (token.IsCancellationRequested || to.Opacity == 1)
					{
						t.Stop();
						to.Opacity = 1;
						from.IsVisible = false;
						localTcs.SetResult(null);
					}
					//System.Diagnostics.Debug.WriteLine("Fading {0} in @ {1}, viz={2}", toId, to.Opacity, to.IsVisible);
				};
			}
			else //Layer below got turned on - Fade layer on top out
			{
				to.Opacity = 1;
				to.IsVisible = true;
				t.Tick += (s, e2) =>
				{
					from.Opacity = Math.Max(0, from.Opacity - step);
					if (token.IsCancellationRequested || from.Opacity == 0)
					{
						t.Stop();
						from.Opacity = 0;
						from.IsVisible = false;
						localTcs.SetResult(null);
					}
					//System.Diagnostics.Debug.WriteLine("Fading {0} out @ {1}, viz={2}", fromId, from.Opacity, from.IsVisible);
				};
			}
			t.Start();
		}
	}

    public class BasemapThumbnailConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if(value is Layer l)
            {
                if (l is OpenStreetMapLayer)
                    return new Uri("http://c.tile.openstreetmap.org/16/19294/24640.png");
                if (l is ArcGISTiledLayer atl)
                {
                    if(atl.Source.OriginalString.Contains("/USA_Topo_Maps/"))
                        return new Uri(atl.Source.OriginalString + "/export?bbox=-117.05,35.1,-117.0,35.11&bboxSR=4326&size=250,125&format=jpg&f=image");
                    else
                        return new Uri(atl.Source.OriginalString + "/export?bbox=-118.5,34,-118.4,34.01&bboxSR=4326&size=250,125&format=jpg&f=image");
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
