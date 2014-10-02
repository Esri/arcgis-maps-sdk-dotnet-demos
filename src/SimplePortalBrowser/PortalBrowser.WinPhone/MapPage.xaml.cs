using Esri.ArcGISRuntime.Portal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using Windows.UI.Xaml.Controls;

namespace PortalBrowser.Phone
{
	public partial class MapPage : Page
	{
		public MapPage()
		{
			InitializeComponent();
		}
		protected override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			(Resources["mapVM"] as ViewModels.MapVM).PortalItem = (ArcGISPortalItem) e.Parameter;
		}
	}
}