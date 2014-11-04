using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace ServiceRequestsSample
{
    public sealed partial class ServiceRequestListPage : Page
    {
		public ServiceRequestListPage()
        {
            this.InitializeComponent();
			this.NavigationCacheMode = NavigationCacheMode.Required;
        }

		/// <summary>
		/// Initialized data
		/// </summary>
		private async void Initialize()
		{
			try
			{
				await InitializeStatusBarAsync();
				await LoadDataAsync();
			}
			catch (Exception ex)
			{
				var _ = new MessageDialog(ex.ToString(), "Error occured").ShowAsync();
			}
		}

		private async Task InitializeStatusBarAsync()
		{
			// Set the background color of the status bar
			var statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
			statusBar.BackgroundColor = (App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush).Color;
			statusBar.BackgroundOpacity = 1; // Remember to set opacity
			statusBar.ProgressIndicator.ProgressValue = 0; // Do not show indicator animation
			await statusBar.ProgressIndicator.ShowAsync();
		}

		private async Task LoadDataAsync()
		{
			// Load all ServiceRequests
			var serviceRequests = await ServiceRequestDataAccess.Current.GetServiceRequestsAsync();

			// Create list that order items by requested date ie. date when ServiceRequest has been created
			// This is found from attribute "requestdate"
			var results = from serviceRequest in serviceRequests
						  group serviceRequest by ((DateTime)serviceRequest.Attributes["requestdate"]).Year into groupedItems
						  orderby groupedItems.Key
						  select groupedItems;

			// Set items in reverse to the UI so newest is the first one.
			serviceRequestSource.Source = results.Reverse();
		}

        protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			Initialize();
		}

		private void serviceRequestList_ItemClick(object sender, ItemClickEventArgs e)
		{
			// Navigate to the details page and pass clicked servicerequest 
			this.Frame.Navigate(typeof(ServiceRequestDetailsPage), e.ClickedItem);
		}

		private void AddServiceRequest_Click(object sender, RoutedEventArgs e)
		{
			this.Frame.Navigate(typeof(AddServiceRequestPage), null);
		}

		private async void Refresh_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				await LoadDataAsync();
			}
			catch (Exception ex)
			{
				var _ = new MessageDialog(ex.ToString(), "Error occured").ShowAsync();
			}
		}
    }
}
