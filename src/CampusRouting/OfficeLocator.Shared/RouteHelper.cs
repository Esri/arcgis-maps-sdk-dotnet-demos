using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using Esri.ArcGISRuntime.Tasks.NetworkAnalyst;

namespace OfficeLocator
{
    public static class RouteHelper
    {
		private static object LockInstance = new object();
		private static Task<RouteTask> initializeTask;

		//Thread safe initialization of the route task
		private static Task<RouteTask> InitRouterAsync()
		{
			if(initializeTask == null)
			{
				string path = System.IO.Path.Combine(MapViewModel.GetDataFolder(), "Network/IndoorNavigation.geodatabase");
				initializeTask = RouteTask.CreateAsync(path, "Transportation_ND");				
			}
			return initializeTask;
		}

		public static async Task<Route> RouteAsync(MapPoint from, MapPoint to, bool reduceOutside)
		{
			var router = await InitRouterAsync().ConfigureAwait(false);
			var languages = router.RouteTaskInfo.SupportedLanguages.ToArray();
			var parameters = await router.GenerateDefaultParametersAsync();
			var lang = router.RouteTaskInfo.SupportedLanguages.Where(l => l.ToLower().StartsWith("en")).FirstOrDefault() ?? parameters.DirectionsLanguage;
			parameters.DirectionsLanguage = lang;
			parameters.SetStops(new Stop[] { new Stop(from), new Stop(to) });
			if (reduceOutside)
			{
				var restrictions = router.RouteTaskInfo.RestrictionAttributes;
				parameters.TravelMode.RestrictionAttributeNames.Add("Avoid Outside Paths");
			}
			try
            {
				var result = await router.SolveRouteAsync(parameters).ConfigureAwait(false);
				return result.Routes.FirstOrDefault();
			}
			catch(System.Exception ex)
			{
				System.Diagnostics.Debug.Write(ex.Message + "\n" + ex.StackTrace);
				return null;
			}
		}
	}
}
