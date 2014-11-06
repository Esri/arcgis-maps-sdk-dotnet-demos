using Esri.ArcGISRuntime.Data;
using System;
using System.Linq;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace ServiceRequestsSample
{
	public sealed partial class ServiceRequestDetailsPage : Page
	{
		private GeodatabaseFeature _serviceRequest;

		public ServiceRequestDetailsPage()
		{
			this.InitializeComponent();
		}

		/// <summary>
		/// Invoked when this page is about to be displayed in a Frame.
		/// </summary>
		protected async override void OnNavigatedTo(NavigationEventArgs e)
		{
			if (e.Parameter == null)
				return; // No feature passed

			_serviceRequest = e.Parameter as GeodatabaseFeature;
			if (_serviceRequest == null)
				return; // passed parameter wasn't a feature

			UpdateUI();

			try
			{
				// Get comments for the ServiceRequest
				var comments = await ServiceRequestDataAccess.Current.GetCommentsForServiceRequestAsync(_serviceRequest);
				
				// There are comments created for the ServiceRequest
				if (comments != null)
				{
					commentCount.Text = comments.Count().ToString();
					commentsList.ItemsSource = comments;
				}
				else
				{
					commentCount.Text = "no comments";
				}
			}
			catch (Exception ex)
			{
				var _ = new MessageDialog(ex.ToString(), "Error occurred").ShowAsync();
			}
		}

		/// <summary>
		/// Updates UI. Needs to do explicitly since DataBinding is not used.
		/// </summary>
		private void UpdateUI()
		{
			if (_serviceRequest.Attributes.ContainsKey("requestid") && _serviceRequest.Attributes["requestid"] != null)
				requestidText.Text = _serviceRequest.Attributes["requestid"].ToString();
			if (_serviceRequest.Attributes.ContainsKey("requestdate") && _serviceRequest.Attributes["requestdate"] != null)
				submittedDate.Text = _serviceRequest.Attributes["requestdate"].ToString();
			if (_serviceRequest.Attributes.ContainsKey("name") && _serviceRequest.Attributes["name"] != null)
				createdName.Text = _serviceRequest.Attributes["name"].ToString();
			if (_serviceRequest.Attributes.ContainsKey("comments") && _serviceRequest.Attributes["comments"] != null)
				comment.Text = _serviceRequest.Attributes["comments"].ToString();

			// Get "status" attributes value from CodedComainValue and use it's value
			// Get domain, get key and get value for the key from the domain
			var statusDomain = ServiceRequestDataAccess.Current.Table.ServiceInfo.Fields.First(
				field => field.Name == "status").Domain as CodedValueDomain;
			var status = _serviceRequest.Attributes["status"].ToString();
			statusText.Text = statusDomain.CodedValues.First(value => value.Key.ToString() == status).Value;
			switch (status)
			{
				case "Assigned":
					statusIndicator.Background = new SolidColorBrush(Windows.UI.Colors.Gray);
					break;
				case "Unassigned":
					statusIndicator.Background = new SolidColorBrush(Windows.UI.Colors.Red);
					break;
				case "Closed":
					statusIndicator.Background = new SolidColorBrush(Windows.UI.Colors.Green);
					break;
				default:
					break;
			}
		}

		private void AddComment_Click(object sender, RoutedEventArgs e)
		{
			// Navigate to the add comment page and pass current servicerequest 
			this.Frame.Navigate(typeof(AddCommentPage), _serviceRequest);
		}

		private async void SetDone_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				// Update attribute to Closed and invoke update
				_serviceRequest.Attributes["status"] = "Closed";
				await ServiceRequestDataAccess.Current.UpdateServiceRequestAsync(_serviceRequest);
				UpdateUI();
			}
			catch (Exception ex)
			{
				var _ = new MessageDialog(ex.ToString(), "Error occurred").ShowAsync();
			}
		}
		
		private async void SetAssigned_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				// Update attribute to Assigned and invoke update
				_serviceRequest.Attributes["status"] = "Assigned";
				await ServiceRequestDataAccess.Current.UpdateServiceRequestAsync(_serviceRequest);
				UpdateUI();
			}
			catch (Exception ex)
			{
				var _ = new MessageDialog(ex.ToString(), "Error occurred").ShowAsync();
			}
		}


		private async void Delete_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				var confirmDialog = new MessageDialog("This will delete ServiceRequest and all comments related to it. Are you really sure?", "Are you sure?");
				
				// Add commands and set their callbacks; both buttons use the same callback function instead of inline event handlers
				confirmDialog.Commands.Add(new UICommand(
					"Delete"));
				confirmDialog.Commands.Add(new UICommand(
					"Cancel"));

				// Set the command that will be invoked by default
				confirmDialog.DefaultCommandIndex = 0;

				// Set the command to be invoked when escape is pressed
				confirmDialog.CancelCommandIndex = 1;

				// Show confirm dialog and if Delete is pressed, delete ServiceRequest and all comments that are related to it.
				var command = await confirmDialog.ShowAsync();
				if (command.Label == "Delete")
				{
					await ServiceRequestDataAccess.Current.DeleteServiceRequestAsync(_serviceRequest, true);
					
					// Navigate to previous vie after all items are deleted
					Frame frame = Window.Current.Content as Frame;
					if (frame.CanGoBack)
						frame.GoBack();
				}
			}
			catch (Exception ex)
			{
				var _ = new MessageDialog(ex.ToString(), "Error occurred").ShowAsync();
			}
		}
	}
}
