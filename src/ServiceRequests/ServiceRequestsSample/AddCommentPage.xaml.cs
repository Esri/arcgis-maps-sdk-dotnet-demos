using Esri.ArcGISRuntime.Data;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace ServiceRequestsSample
{
	public sealed partial class AddCommentPage : Page
	{
		private GeodatabaseFeature _serviceRequest;
		private GeodatabaseFeature _comment;

		public AddCommentPage()
		{
			this.InitializeComponent();
		}

		/// <summary>
		/// Invoked when this page is about to be displayed in a Frame.
		/// </summary>
		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			if (e.Parameter == null)
				return; // No feature passed

			_serviceRequest = e.Parameter as GeodatabaseFeature;
			if (_serviceRequest == null)
				return; // passed parameter wasn't a feature

			// Creates new GeodatabaseFeature, this doesn't add it to the table
			_comment = CommentDataAccess.Current.Table.CreateNew();

			// Set current date as a default
			_comment.Attributes["submitdt"] = DateTime.Now;
			_comment.Attributes["requestid"] = _serviceRequest.Attributes["requestid"];

#if DEBUG
			// Populate following field for testing : "requesttype","comments","name","phone","email","requestdate"
			_comment.Attributes["comments"] = "I think that this issues...";
#endif
			// Set feature to the data form
			MyDataForm.GeodatabaseFeature = _comment;
		}

		private async void MyDataForm_ApplyCompleted(object sender, EventArgs e)
		{
			try
			{
				// Add comment and commit to the FeatureService
				var updatedFeature = await CommentDataAccess.Current.AddCommentAsync(_comment);

				// Navigate to previous view.
				Frame frame = Window.Current.Content as Frame;
				if (frame.CanGoBack)
					frame.GoBack();
			}
			catch (Exception ex)
			{
				var _ = new MessageDialog(ex.ToString(), "Error occured").ShowAsync();
			}
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
				if (!_comment.Attributes.ContainsKey("comments") || _comment.Attributes["comments"] == null)
				{
					var _ = new MessageDialog("Please add your comment", "Comment missing").ShowAsync();
				}
				else
				{
					// Show general fill details info
					var _ = new MessageDialog("Please fill details for the comment.", "Set comment values").ShowAsync();
				}
			}
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			// Reset controls
			if (MyDataForm.ResetCommand.CanExecute(null))
				MyDataForm.ResetCommand.Execute(null);

			_comment = null;
			_serviceRequest = null;

			// Navigate to previous view.
			Frame frame = Window.Current.Content as Frame;
			if (frame.CanGoBack)
				frame.GoBack();
		}
	}
}
