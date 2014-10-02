using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using Esri.ArcGISRuntime.Portal;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace PortalBrowser.Phone
{
	public partial class MainPage : Page
	{
		public static ArcGISPortalItem SelectedPortalItem { get; private set; }

		public MainPage()
		{
			InitializeComponent();
		}

		private void Grid_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
		{
			SelectedPortalItem = (sender as FrameworkElement).DataContext as ArcGISPortalItem;
			Frame.Navigate(typeof(MapPage), SelectedPortalItem);
		}
	}
}