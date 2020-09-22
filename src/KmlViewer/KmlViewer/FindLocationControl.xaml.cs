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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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

		private async void Location_SuggestionsRequested(SearchBox sender, SearchBoxSuggestionsRequestedEventArgs args)
		{
			if (geocodeTcs != null)
				geocodeTcs.Cancel();
			geocodeTcs = new CancellationTokenSource();

			if (args.QueryText.Length > 3)
			{
				try
				{
					var geo = new LocatorTask(serviceUri);
					var deferral = args.Request.GetDeferral();
					var result = await geo.GeocodeAsync(args.QueryText,new GeocodeParameters()
					{
						MaxResults = 5,
                        OutputSpatialReference = SpatialReferences.Wgs84
					}, geocodeTcs.Token);
					if (result.Any() && !args.Request.IsCanceled)
					{
						var imageSource = Windows.Storage.Streams.RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/blank.png"));
						foreach (var item in result)
							args.Request.SearchSuggestionCollection.AppendResultSuggestion(item.Label, "", item.Extent.ToJson(), imageSource, "");
					}
					deferral.Complete();
				}
				catch { }
			}
		}

		private void Location_ResultSuggestionChosen(SearchBox sender, SearchBoxResultSuggestionChosenEventArgs args)
		{
			var geom = Esri.ArcGISRuntime.Geometry.Geometry.FromJson(args.Tag);
			if (LocationPicked != null)
				LocationPicked(this, geom);
		}

		public event EventHandler<Esri.ArcGISRuntime.Geometry.Geometry> LocationPicked;

		private async void Location_QuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
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
					Location.QueryText = result.First().Label;
					if (LocationPicked != null)
						LocationPicked(this, result.First().Extent);
				}
			}
			catch { }
		}
    }
}
