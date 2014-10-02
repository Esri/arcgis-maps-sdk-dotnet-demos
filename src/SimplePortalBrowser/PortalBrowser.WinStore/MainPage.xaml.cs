using Esri.ArcGISRuntime.Portal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace PortalBrowser.WinStore
{
	public sealed partial class MainPage : Page
	{
		public MainPage()
		{
			this.InitializeComponent();
		}

		private void GridView_ItemClick(object sender, ItemClickEventArgs e)
		{
			var item = (e.ClickedItem as ArcGISPortalItem);
			base.Frame.Navigate(typeof(MapPage), item);
		}

	}
}
