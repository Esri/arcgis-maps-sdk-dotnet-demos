using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace OfficeLocator.UWP
{
    /// <summary>
    /// The main page for office location and routing
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
			this.InitializeComponent();
		}

        // Most of the logic by the app is handled by the shared MainViewModel class.
        // The code in here are mostly just forwarding UI events to the view model
        // You could use various types of behaviors to accomplish this as well
        // for a cleaner MVVM style, but to easier follow the code around
        // this has been kept as code-behind

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await VM.LoadAsync();
            loadingStatus.Visibility = Visibility.Collapsed;
            VM.RequestViewpoint += VM_RequestViewpoint;
        }

        public MapViewModel VM { get; } = new MapViewModel();

        private void VM_RequestViewpoint(object sender, Esri.ArcGISRuntime.Mapping.Viewpoint viewpoint)
		{
            CampusView.SetViewpointAsync(viewpoint, TimeSpan.FromSeconds(3));
		}
        
        private void search_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
		{
			if (sender == searchFrom)
				VM.GeocodeFromLocation(args.QueryText);
			else
				VM.GeocodeToLocation(args.QueryText);
		}

		private async void search_SuggestionsRequested(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
		{
			if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
			{
				if (string.IsNullOrWhiteSpace(sender.Text))
					sender.ItemsSource = null;
				else
				{
					var suggestions = await GeocodeHelper.SuggestAsync(sender.Text);
					sender.ItemsSource = suggestions.ToList();
				}
			}
		}

		private void search_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
		{
			var selection = args.SelectedItem as string;
			sender.Text = selection;
		}

        // This allows you to do QR scanning of office locations. Simply embed the name of the office into a QR code and scan it.
		private async void Scan_Tapped(object sender, TappedRoutedEventArgs e)
		{
			var scanner = new ZXing.Mobile.MobileBarcodeScanner();
			var overlay = new QRScanOverlay();
			overlay.OnCancel += (a, b) => { scanner.Cancel(); };
			scanner.CustomOverlay = overlay;
			scanner.UseCustomOverlay = true;
			
			var result = await scanner.Scan(new ZXing.Mobile.MobileBarcodeScanningOptions() { PossibleFormats = new List<ZXing.BarcodeFormat>(new ZXing.BarcodeFormat[] { ZXing.BarcodeFormat.QR_CODE }) });
			if (result != null)
			{
				System.Diagnostics.Debug.WriteLine($"Scanned {result.Text}");
				await Task.Delay(500);
				if(sender == ScanA)
					searchFrom.Text = result.Text ?? "";
				else
					searchTo.Text = result.Text ?? "";
				if (!string.IsNullOrWhiteSpace(result.Text))
				{
					if (sender == ScanA)
						VM.GeocodeFromLocation(result.Text);
					else
						VM.GeocodeToLocation(result.Text);
				}
			}
		}

        // Picks up the location of the next meeting in the calendar and try and geocode it
		private async void Calendar_Tapped(object sender, TappedRoutedEventArgs e)
		{
			var app = await Windows.ApplicationModel.Appointments.AppointmentManager.RequestStoreAsync(Windows.ApplicationModel.Appointments.AppointmentStoreAccessType.AllCalendarsReadOnly);
			var appmts = await app.FindAppointmentsAsync(DateTimeOffset.Now, TimeSpan.FromDays(5));
			var next = appmts.Where(a => !string.IsNullOrWhiteSpace(a.Location)).FirstOrDefault();
			if(next != null)
				searchTo.Text = next.Location;
		}
	}
}
