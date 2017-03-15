using Esri.ArcGISRuntime.Data;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ServiceRequestsSample
{
	public class ServiceRequestListTemplateSelector : DataTemplateSelector
	{
		public DataTemplate StatusAssignedTemplate{ get; set; }

		public DataTemplate StatusUnssignedTemplate { get; set; }

		public DataTemplate StatusCompletedTemplate { get; set; }

		protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
		{
			if (item == null)
				return null;

			var feature = item as Feature;
			if (feature == null)
				return null;

			// Select DataTemplate based on ServiceRequests status field.
			var status = feature.Attributes["status"].ToString();
			switch (status)
			{
				case "Assigned" :
					return StatusAssignedTemplate;
				case "Unassigned" :
					return StatusUnssignedTemplate;
				case "Closed":
					return StatusCompletedTemplate;
				default :
					return null;
			}
		}
	}
}
