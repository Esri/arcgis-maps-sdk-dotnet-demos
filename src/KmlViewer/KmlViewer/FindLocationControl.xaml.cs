using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace KmlViewer
{
	public sealed partial class FindLocationControl : UserControl
	{
		private Uri serviceUri = new Uri("https://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer", UriKind.Absolute);

		public FindLocationControl()
		{
			this.InitializeComponent();
		}

		private CancellationTokenSource geocodeTcs;

		private async void Location_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
		{
			if (geocodeTcs != null)
				geocodeTcs.Cancel();
			geocodeTcs = new CancellationTokenSource();

			if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput && sender.Text.Length > 3)
			{
				try
				{
					var geo = new LocatorTask(serviceUri);
					var result = await geo.GeocodeAsync(sender.Text, new GeocodeParameters()
					{
						MaxResults = 5,
						OutputSpatialReference = SpatialReferences.Wgs84
					}, geocodeTcs.Token);
					if (result.Any())
					{
						var imageSource = Windows.Storage.Streams.RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/blank.png"));
						sender.ItemsSource = result;
					}
				}
				catch { }
			}
		}

		private void Location_ResultSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
		{
			var result = args.SelectedItem as GeocodeResult;
            sender.Text = "";
            if (LocationPicked != null)
				LocationPicked(this, result.Extent);
		}

		public event EventHandler<Esri.ArcGISRuntime.Geometry.Geometry> LocationPicked;

		private async void Location_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
		{
			if (geocodeTcs != null)
				geocodeTcs.Cancel();
			geocodeTcs = new CancellationTokenSource();
			try
			{
				var geo = new LocatorTask(serviceUri);
				var result = await geo.GeocodeAsync(args.QueryText, new GeocodeParameters()
				{
					MaxResults = 5,
					OutputSpatialReference = SpatialReferences.Wgs84
				}, geocodeTcs.Token);
				if (result.Any())
				{
					sender.Text = result.First().Label;
					if (LocationPicked != null)
						LocationPicked(this, result.First().Extent);
				}
			}
			catch { }
		}
	}
}