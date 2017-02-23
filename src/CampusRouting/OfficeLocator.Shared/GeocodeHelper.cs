using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Tasks.Geocoding;

namespace OfficeLocator
{
	public static class GeocodeHelper
	{
		private static LocatorTask locator;
		private static object LockInstance = new object();
		private static Task initializeTask;

		public static void Initialize()
		{
			var _ = InitGeocoderAsync();
		}

		//Thread safe initialization of the geocoder
		private static async Task InitGeocoderAsync()
		{
			lock (LockInstance)
			{
				if (locator != null)
					return;
				if (initializeTask == null)
				{
                    string path = System.IO.Path.Combine(MapViewModel.GetDataFolder(), @"Geocoder\CampusRooms.loc");
					var task = LocatorTask.CreateAsync(new Uri(path, UriKind.RelativeOrAbsolute)); //Using relative path makes the create never complete.
                    initializeTask = task.ContinueWith(t =>
					{
						if (t.IsFaulted)
							throw t.Exception;
						lock (LockInstance)
							locator = t.Result;
					});
				}
			}
			await initializeTask.ConfigureAwait(false);
		}

        // Suggest is used for auto completion
		public static async Task<IEnumerable<string>> SuggestAsync(string text)
		{
			await InitGeocoderAsync();
			var result = await locator.SuggestAsync(text, new SuggestParameters() { MaxResults = 15 }).ConfigureAwait(false);
			return result.Select(t => t.Label);
		}

        // Geocodes an office location
		public static async Task<MapPoint> GeocodeAsync(string text)
		{
			await InitGeocoderAsync();
			var result = await locator.GeocodeAsync(text).ConfigureAwait(false);
			return result.OrderByDescending(t => t.Score).Select(t => t.RouteLocation).FirstOrDefault();
		}
	}
}