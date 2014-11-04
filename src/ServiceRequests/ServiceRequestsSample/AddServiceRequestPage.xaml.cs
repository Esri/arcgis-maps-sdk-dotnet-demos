using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace ServiceRequestsSample
{
	public sealed partial class AddServiceRequestPage : Page
	{
		private GeodatabaseFeature _newFeature;

		public AddServiceRequestPage()
		{
			this.InitializeComponent();
			
			// Filter attributes that are shown in the feature data form
			MyDataForm.Fields = new ObservableCollection<string>()
			{
				"requesttype","comments","name","phone","email","requestdate"
			};
		}

		private async void MyDataForm_ApplyCompleted(object sender, EventArgs e)
		{
			try
			{
				if (_newFeature.Geometry == null)
				{
					var _ = new MessageDialog("Add geometry", "Geometry missing").ShowAsync();
					return;
				}

				// Add ServiceRequest and commit to the FeatureService
				var updatedFeature = await ServiceRequestDataAccess.Current.AddServiceRequestAsync(_newFeature);

				// After commit, navigate to the details page
				this.Frame.Navigate(typeof(ServiceRequestDetailsPage), updatedFeature);
				Frame.BackStack.RemoveAt(Frame.BackStack.Count - 1);
			}
			catch (Exception ex)
			{
				var _ = new MessageDialog(ex.ToString(), "Error occured").ShowAsync();
			}
		}

		/// <summary>
		/// Invoked when this page is about to be displayed in a Frame.
		/// </summary>
		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			// First start getting location but don't wait it. This will be executed on the background
			GetCurrentLocationAsync();

			// Creates new GeodatabaseFeature, this doesn't add it to the table
			_newFeature = ServiceRequestDataAccess.Current.Table.CreateNew();

			// Set current date as a default
			_newFeature.Attributes["requestdate"] = DateTime.Now;

#if DEBUG
			// Populate following field for testing : "requesttype","comments","name","phone","email","requestdate"
			_newFeature.Attributes["comments"] = "Added from Windows Phone Sample app.";
			_newFeature.Attributes["name"] = "Firstname Surname";
			_newFeature.Attributes["phone"] = "";
			_newFeature.Attributes["email"] = "firstname.surname@esri.com";
#endif
			// Set feature to the data form
			MyDataForm.GeodatabaseFeature = _newFeature;
		}

		/// <summary>
		/// Get current location and show indication on statusbar.
		/// </summary>
		private async Task GetCurrentLocationAsync()
		{
			// Show status indication
			var statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
			statusBar.ProgressIndicator.Text = "Getting current location...";
			statusBar.ProgressIndicator.ProgressValue = 1;
			await statusBar.ProgressIndicator.ShowAsync();

			// Create Geolocator to access current location
			Geolocator locator = new Geolocator();
			locator.DesiredAccuracy = PositionAccuracy.High;

			// Get location and create new MapPoint from that, location is returned as a WGS84
			var location = await locator.GetGeopositionAsync().AsTask();
			var mapPointWGS84 = new MapPoint(location.Coordinate.Point.Position.Longitude, location.Coordinate.Point.Position.Latitude, SpatialReferences.Wgs84);

			// Project geometry to used projectin
			_newFeature.Geometry = GeometryEngine.Project(mapPointWGS84, SpatialReferences.WebMercator);

			// Hide status indication
			statusBar.ProgressIndicator.Text = "";
			await statusBar.ProgressIndicator.HideAsync();

			saveBtn.IsEnabled = true;
		}

		private void Save_Click(object sender, RoutedEventArgs e)
		{
			// If we can save changes execute it.
			if (MyDataForm.ApplyCommand.CanExecute(null))
			{
				MyDataForm.ApplyCommand.Execute(null);
			}
			else
			{
				// Check that request type is set, if not show message that specifies that set it
				if (!_newFeature.Attributes.ContainsKey("requesttype") || _newFeature.Attributes["requesttype"] == null)
				{
					var _ = new MessageDialog("Please select problem type for the service request.", "Define problem").ShowAsync();
				}
				else
				{
					// Show general fill details info
					var _ = new MessageDialog("Please fill details for the service request.", "Set service request values").ShowAsync();
				}
			}
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			// Reset controls
			if (MyDataForm.ResetCommand.CanExecute(null))
				MyDataForm.ResetCommand.Execute(null);

			_newFeature = null;

			// Navigate to previous view.
			Frame frame = Window.Current.Content as Frame;
			if (frame.CanGoBack)
				frame.GoBack();
		}

		private async void CurrentLocation_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				await GetCurrentLocationAsync();
			}
			catch (Exception ex)
			{
				var _ = new MessageDialog(ex.ToString(), "Couldn't get location").ShowAsync();
			}
		}
	}
}
