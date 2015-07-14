using Esri.ArcGISRuntime.Layers;
using System;
using Windows.UI.Xaml;

namespace KmlViewer
{
	/// <summary>
	/// Set the selected index to smoothly fade to a layer from the previously selected layer.
	/// Only one layer is allowed to be on at a time, so child layers are displayed in an 'exclusive' mode.
	/// </summary>
	public class FadeGroupLayer : GroupLayer
	{
		private int m_SelectedIndex;

		/// <summary>
		/// Initializes a new instance of the <see cref="FadeGroupLayer"/> class.
		/// </summary>
		public FadeGroupLayer()
		{
			this.ChildLayers.CollectionChanged += ChildLayers_CollectionChanged;
		}

		/// <summary>
		/// Handles the CollectionChanged event of the ChildLayers control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
		private void ChildLayers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			for (int i = 0; i < ChildLayers.Count; i++)
			{
				ChildLayers[i].IsVisible = i == SelectedIndex;
			}
			if (ChildLayers.Count < SelectedIndex) //If selected index is out of range, select the nearest one
			{
				ChildLayers[ChildLayers.Count - 1].IsVisible = true;
			}
			OnPropertyChanged("SelectedLayer");
		}

		public Layer SelectedLayer
		{
			get
			{
				if (ChildLayers.Count == 0 || SelectedIndex < 0)
					return null;
				return ChildLayers[Math.Min(SelectedIndex, ChildLayers.Count - 1)];
			}
		}

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
					OnPropertyChanged("SelectedLayer");
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
			if (ChildLayers.Count < fromId || ChildLayers.Count < toId || toId < 0 || fromId < 0)
				return;
			if (tcs != null)
			{
				cancelSource.Cancel();
				await tcs.Task;
			}
			var from = ChildLayers[fromId];
			var to = ChildLayers[toId];
			if(from.Status == LayerStatus.NotInitialized || to.Status == LayerStatus.NotInitialized)
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
}
