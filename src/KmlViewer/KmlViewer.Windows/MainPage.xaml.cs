using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;

namespace KmlViewer
{
	/// <summary>
	/// Main KML Viewer page
	/// </summary>
	public sealed partial class MainPage : Page
	{
		MainPageVM vm;
		public MainPage()
		{
			this.InitializeComponent();

			this.NavigationCacheMode = NavigationCacheMode.Required;
			vm = Resources["vm"] as MainPageVM;
			Day.Value = DateTime.Now.DayOfYear;
			Hour.Value = DateTime.Now.TimeOfDay.TotalHours;
		}

		/// <summary>
		/// Invoked when this page is about to be displayed in a Frame.
		/// </summary>
		/// <param name="e">Event data that describes how this page was reached.
		/// This parameter is typically used to configure the page.</param>
		protected async override void OnNavigatedTo(NavigationEventArgs e)
		{
			LoadLocation();
			if(vm.Is3D)
			{
				  Camera homeCamera = new Camera(41, -180, 31000000, 0, 0);
				  await sceneView.SetViewAsync(homeCamera, TimeSpan.Zero);
					if(position != null)
					{
						homeCamera = new Camera(position.Coordinate.Point.Position.Latitude, position.Coordinate.Point.Position.Longitude, 11000000, 0, 0);
					}
					else
					  homeCamera = new Camera(41, -92, 11000000, 0, 0);
				  await sceneView.SetViewAsync(homeCamera, new TimeSpan(0, 0, 4)); 
			}
		}

		private Windows.Devices.Geolocation.Geoposition position;

		private async void LoadLocation()
		{
			Windows.Devices.Geolocation.Geoposition l = null;
			try
			{
				var loc = new Windows.Devices.Geolocation.Geolocator();
				l = position = await loc.GetGeopositionAsync();

			}
			catch { return; }
			foreach (var coll in new GraphicsOverlayCollection[] { 
				sceneView.GraphicsOverlays , mapView.GraphicsOverlays
			})
			{
				GraphicsOverlay overlay = new GraphicsOverlay();
				coll.Add(overlay);
				Graphic g = new Graphic()
				{
					Symbol = new SimpleMarkerSymbol()
					{
						Color = Color.FromArgb(255, 0, 122, 194),
						Size = 20,
						Outline = new SimpleLineSymbol()
						{
							Width = 2,
							Color = Colors.White
						}
					},

				};
				g.Geometry = new MapPoint(l.Coordinate.Point.Position.Longitude, l.Coordinate.Point.Position.Latitude, SpatialReferences.Wgs84);
				overlay.Graphics.Add(g);
				break;
			}
		}

		public void LoadKml(Windows.ApplicationModel.Activation.FileOpenPickerActivatedEventArgs args)
		{
		}

		public async void LoadKml(Windows.ApplicationModel.Activation.FileActivatedEventArgs args)
		{
			var file = args.Files.FirstOrDefault() as Windows.Storage.StorageFile;
			var localFile = await Windows.Storage.ApplicationData.Current.TemporaryFolder.CreateFileAsync(file.Name, Windows.Storage.CreationCollisionOption.ReplaceExisting);
			await file.CopyAndReplaceAsync(localFile);
			AddKmlLayer("file:///" + localFile.Path);
		}

		private async void AddKmlLayer(string url)
		{
			Uri uri;
			try
			{
				uri = new Uri(url);
			}
			catch (System.Exception ex)
			{
				var _ = new MessageDialog("Invalid url: " + ex.Message).ShowAsync();
				return;
			}
			var layer = vm.LoadKmlLayer(uri.OriginalString.Replace("file:///", ""));
			
			try
			{
				await layer.InitializeAsync();
			}
			catch (System.Exception ex)
			{
				var _ = new MessageDialog(ex.Message).ShowAsync();
				return;
			}
			
			sidePanel.SelectedIndex = 0;
			try
			{
				await layer.InitializeAsync();
				Viewpoint viewPoint = null;
				KmlFeature root = layer.RootFeature;
				if(root == null)
				{
					TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
					System.ComponentModel.PropertyChangedEventHandler handler = null;
					handler = (s,e) => {
						if (e.PropertyName == "RootFeature")
						{
							layer.PropertyChanged -= handler;
							tcs.SetResult(null);
						}
					};
					layer.PropertyChanged += handler;
					await tcs.Task;
					root = layer.RootFeature;
				}

				if (layer.FullExtent == null && layer.RootFeature != null)
				{
					viewPoint = layer.RootFeature.Viewpoint;
				}
				if(viewPoint == null && layer.FullExtent != null)
					viewPoint = new Viewpoint(layer.FullExtent);
				if (viewPoint != null)
				{
					if (vm.Is3D)
						sceneView.SetViewAsync(viewPoint);
					else
						mapView.SetViewAsync(viewPoint);
				}
			}
			catch { }
		}

		private async void BrowseButton_Click(object sender, RoutedEventArgs e)
		{
			var openPicker = new Windows.Storage.Pickers.FileOpenPicker();
			openPicker.ViewMode = Windows.Storage.Pickers.PickerViewMode.List;
			openPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
			// Users expect to have a filtered view of their folders depending on the scenario.
			// For example, when choosing a documents folder, restrict the filetypes to documents for your application.
			openPicker.FileTypeFilter.Clear();
			openPicker.FileTypeFilter.Add(".kml");
			openPicker.FileTypeFilter.Add(".kmz");
			// Open the picker for the user to pick a file
			var file = await openPicker.PickSingleFileAsync();
			if (file != null)
			{
				//Copy file inside the sandbox
				var localFile = await Windows.Storage.ApplicationData.Current.TemporaryFolder.CreateFileAsync(file.Name, Windows.Storage.CreationCollisionOption.ReplaceExisting);
				await file.CopyAndReplaceAsync(localFile);
				AddKmlLayer("file:///" + localFile.Path); 
			}
		}

		private void LoadFromUrlButton_Click(object sender, RoutedEventArgs e)
		{
			AddKmlLayer(kmlUrl.Text);
		}

		private void LoadSampleButton_Click(object sender, RoutedEventArgs e)
		{
			var sample = (KmlSample)((FrameworkElement)sender).DataContext;
			AddKmlLayer(sample.Path);
			if(sample.InitialViewpoint != null)
			{
				ViewBase view = vm.Is3D ? (ViewBase)sceneView : (ViewBase)mapView;
				var _ = view.SetViewAsync(sample.InitialViewpoint);
			}
		}

		private void LoadKmlButton_Click(object sender, RoutedEventArgs e)
		{
			sidePanel.SelectedIndex = 3;
		}

		KmlFeature highlightedFeature;
		private void OnFeatureTapped(object sender, KmlFeature feature)
		{
			HighlightFeature(feature);
		}

		private void HighlightFeature(KmlFeature feature)
		{
			if (highlightedFeature != null)
				highlightedFeature.IsHighlighted = false;
			if (highlightedFeature == feature)
			{
				highlightedFeature = null;
				return;
			}
			if (feature != null)
				feature.IsHighlighted = true;
			
			highlightedFeature = feature;
		}

		private UIElement currentMaptip;

		private void ShowMapTip(KmlFeature feature, MapPoint location = null)
		{
			ViewBase view = vm.Is3D ? (ViewBase)sceneView : (ViewBase)mapView;
			var border = (Border)view.Overlays.Items.First();
			if (feature == null || feature.BalloonStyle == null || string.IsNullOrWhiteSpace(feature.BalloonStyle.FormattedText) ||
				feature.Viewpoint == null || feature.Viewpoint.TargetGeometry == null)
			{
				border.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
				currentMaptip = null;
				return;
			}
			var grid = (Grid)border.Child;
			var webview = grid.Children.OfType<WebView>().First();
			webview.NavigateToString(feature.BalloonStyle.FormattedText);
			ViewBase.SetViewOverlayAnchor(border, location ?? feature.Viewpoint.TargetGeometry.Extent.GetCenter());
			border.Visibility = Windows.UI.Xaml.Visibility.Visible;
			currentMaptip = border;
		}

		private void CloseTip_Clicked(object sender, RoutedEventArgs e)
		{
			ShowMapTip(null);
		}

		private void OnFeatureDoubleTapped(object sender, KmlFeature feature)
		{
			var vp = feature.Viewpoint;
			if(vp != null)
			{
				if (vm.Is3D)
				{
					sceneView.SetViewAsync(vp, 2, true);
					sceneView.Focus(FocusState.Keyboard);
				}
				else
				{
					mapView.Focus(FocusState.Keyboard);
					mapView.SetViewAsync(vp);
				}
				if (highlightedFeature != null)
					highlightedFeature.IsHighlighted = false;
				feature.IsHighlighted = true;
				highlightedFeature = feature;
			}
		}
		
		private async void Is3DCheckBox_Toggled(object sender, RoutedEventArgs e)
		{
			if (sceneView != null && mapView != null)
			{
				bool isOn = ((ToggleSwitch)sender).IsOn;
				if (isOn == vm.Is3D)
					return; //this happens the first time it loads
				HighlightFeature(null);
				if (vm.Is3D)
				{
					var c = sceneView.Camera;
					//If there's tilt or heading, reset before switching to 2D
					if (c.Pitch > 1 || Math.Abs(c.Heading) > 1)
					{
						var center = sceneView.ScreenToLocation(new Point(sceneView.ActualWidth * .5, sceneView.ActualHeight * .5));
						c = c.RotateAround(center, c.Heading, -c.Pitch).SetPitch(0).SetHeading(0);
						await sceneView.SetViewAsync(c);
					}
				}
				ViewBase from = vm.Is3D ? (ViewBase)sceneView : (ViewBase)mapView;
				ViewBase to = !vm.Is3D ? (ViewBase)sceneView : (ViewBase)mapView;
				var vp = from.GetCurrentViewpoint(ViewpointType.BoundingGeometry);
				if (vp != null)
				{
					to.SetView(vp);
				}
				vm.Is3D = !vm.Is3D;
			}
		}

		public void OnLocationPicked(object sender, Esri.ArcGISRuntime.Geometry.Geometry location)
		{
			if (vm.Is3D)
			{
				sceneView.SetViewAsync(new Viewpoint(location), 1.5, true);
				sceneView.Focus(FocusState.Keyboard);
			}
			else
			{
				mapView.Focus(FocusState.Keyboard);
				mapView.SetViewAsync(location);
			}
		}

		private async void ViewTapped(object sender, MapViewInputEventArgs e)
		{
			ViewBase view = (ViewBase)sender;
			var vp = view.GetCurrentViewpoint(ViewpointType.CenterAndScale);
			var vpstr = string.Format("Camera={0},{1},{2}\nhead={3},pitch={4},{5}\nscale={6},targetg={7}", vp.Camera.Location.X, vp.Camera.Location.Y, vp.Camera.Location.Z, vp.Camera.Heading, vp.Camera.Pitch, vp.Rotation, vp.TargetScale, vp.TargetGeometry.ToJson());
			System.Diagnostics.Debug.WriteLine(vpstr);
			KmlLayer kmlLayer = null;
			if(view is SceneView)
				kmlLayer = ((SceneView)sender).Scene.Layers.OfType<KmlLayer>().FirstOrDefault();
			else if (view is MapView)
				kmlLayer = ((MapView)sender).Map.Layers.OfType<KmlLayer>().FirstOrDefault();
			if (kmlLayer == null)
				return;
			var feature = (await kmlLayer.HitTestAsync(view, e.Position, 1)).FirstOrDefault();
			HighlightFeature(feature);
			ShowMapTip(feature, e.Location);
		}

		private void WebView_FrameNavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
		{
			if (args.Uri != null)
			{
				args.Cancel = true;
				Windows.System.Launcher.LaunchUriAsync(args.Uri);
			}
		}

		private void Month_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
		{
			if (Day == null || Hour == null) return;
			var date = new DateTime(2015, 1, 1, 0, 0, 0, DateTimeKind.Local).Date.AddDays((int)Day.Value).AddHours(Hour.Value);
			sceneView.SetSunTime(date);
		}

		private void SliderValueTick(object sender, double value)
		{
			var c = sceneView.Camera;
			if (c.Pitch + value > 0 && c.Pitch + value < 180)
			{
				c = c.SetPitch(c.Pitch + value);
				sceneView.SetView(c);
			}
		}

		private void ViewChanged(object sender, EventArgs e)
		{
			//If a maptip is open, hide it when the view starts navigating
			if (currentMaptip != null)
			{
				ShowMapTip(null);
			}
		}

		private void HelpIcon_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
		{
			aboutView.Visibility = Windows.UI.Xaml.Visibility.Visible;
		}
	}	
}