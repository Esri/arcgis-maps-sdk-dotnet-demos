using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.Data;

namespace ServiceRequestsSample
{
	public static class ServiceFeatureTableExtensions
	{
		/// <summary>
		/// Returns root level service url.
		/// </summary>
		/// <remarks>
		///		ie. If service url is http://sampleserver6.arcgisonline.com/arcgis/rest/services/ServiceRequest/FeatureServer/0
		///		root service is  http://sampleserver6.arcgisonline.com/arcgis/rest/services/ServiceRequest/FeatureServer
		/// </remarks>
		public static string GetParentServiceUrl(this ServiceFeatureTable table)
		{
			var serviceUrl = table.ServiceUri.ToString();
			var indexToLastSlash = serviceUrl.LastIndexOf("/");
			var rootServiceUrl = serviceUrl.Substring(0, indexToLastSlash);
			return rootServiceUrl;
		}

		public static string GetRelationServicesUrl(this ServiceFeatureTable table, Relationship relation)
		{
			var rootServiceUrl = table.GetParentServiceUrl();
			var relationSeviceUrl = string.Format("{0}/{1}", rootServiceUrl, relation.RelatedTableID);

			return relationSeviceUrl;
		}
	}
}
