using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Tasks.NetworkAnalyst;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

namespace LocalNetworkSample.Controls
{
	public sealed partial class RouteDirectionView : UserControl
	{
		public RouteDirectionView()
		{
			this.InitializeComponent();
		}



		public Esri.ArcGISRuntime.Tasks.NetworkAnalyst.RouteDirection RouteDirection
		{
			get { return (Esri.ArcGISRuntime.Tasks.NetworkAnalyst.RouteDirection)GetValue(RouteDirectionProperty); }
			set { SetValue(RouteDirectionProperty, value); }
		}

		// Using a DependencyProperty as the backing store for RouteDirection.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty RouteDirectionProperty =
			DependencyProperty.Register("RouteDirection", typeof(Esri.ArcGISRuntime.Tasks.NetworkAnalyst.RouteDirection),
			typeof(RouteDirectionView), new PropertyMetadata(null, OnRouteDirectionPropertyChanged));

		private static void OnRouteDirectionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var direction = e.NewValue as RouteDirection;
			(d as RouteDirectionView).UpdateDirection(direction);
		}

		private void UpdateDirection(RouteDirection direction)
		{
			if (direction == null)
			{
				LayoutRoot.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
				return;
			}
			LayoutRoot.Visibility = Windows.UI.Xaml.Visibility.Visible;
			LayoutRoot.DataContext = direction;
			var d = direction.GetLength(LinearUnits.Miles);
			if (d == 0)
				distance.Text = "";
			else if(d >= .25)
				distance.Text = d.ToString("0.0 mi");
			else
			{
				d = direction.GetLength(LinearUnits.Yards);
				distance.Text = d.ToString("0 yd");
			}
			if (direction.Time.TotalHours >= 1)
				time.Text = direction.Time.ToString("hh\\:mm");
			else if (direction.Time.TotalMinutes > 1)
				time.Text = direction.Time.ToString("mm\\:ss");
			else if (direction.Time.TotalSeconds > 0)
				time.Text = direction.Time.ToString("ss") + " sec";
			else 
				time.Text = "";
		}
	}
}
